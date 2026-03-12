using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Calculates phase diagrams over P-T grids using Gibbs minimization.
/// </summary>
public class PhaseDiagramCalculator
{
    private readonly GibbsMinimizer _minimizer = new();

    /// <summary>
    /// Calculate a phase diagram over a P-T grid.
    /// </summary>
    /// <param name="candidatePhases">All candidate phases to consider</param>
    /// <param name="pressures">Pressure points [GPa]</param>
    /// <param name="temperatures">Temperature points [K]</param>
    /// <returns>2D array of equilibrium assemblages [P_index, T_index]</returns>
    public PhaseAssemblage[,] CalculateDiagram(
        List<PhaseEntry> candidatePhases,
        double[] pressures,
        double[] temperatures)
    {
        int nP = pressures.Length;
        int nT = temperatures.Length;
        var grid = new PhaseAssemblage[nP, nT];

        for (int i = 0; i < nP; i++)
        {
            for (int j = 0; j < nT; j++)
            {
                var initial = new PhaseAssemblage
                {
                    Pressure = pressures[i],
                    Temperature = temperatures[j],
                    Phases = candidatePhases.Select(p => new PhaseEntry
                    {
                        Name = p.Name,
                        Mineral = p.Mineral,
                        Amount = 1.0, // equal initial amounts
                        Solution = p.Solution,
                        Composition = p.Composition,
                    }).ToList(),
                };

                grid[i, j] = _minimizer.Minimize(initial, pressures[i], temperatures[j]);
            }
        }

        return grid;
    }

    /// <summary>
    /// Find approximate phase boundary between two phases along a pressure profile at fixed temperature.
    /// Uses bisection to find where G1 = G2.
    /// </summary>
    public double FindPhaseBoundary(PhaseEntry phase1, PhaseEntry phase2, double T,
        double pMin, double pMax, double tolerance = 0.01)
    {
        double gDiffLow = ComputeGDiff(phase1, phase2, pMin, T);
        double gDiffHigh = ComputeGDiff(phase1, phase2, pMax, T);

        if (gDiffLow * gDiffHigh > 0) return double.NaN; // no crossing in range

        for (int i = 0; i < 50; i++)
        {
            double pMid = (pMin + pMax) / 2.0;
            double gDiffMid = ComputeGDiff(phase1, phase2, pMid, T);

            if (Math.Abs(gDiffMid) < tolerance) return pMid;

            if (gDiffLow * gDiffMid < 0)
                pMax = pMid;
            else
            {
                pMin = pMid;
                gDiffLow = gDiffMid;
            }
        }

        return (pMin + pMax) / 2.0;
    }

    private static double ComputeGDiff(PhaseEntry p1, PhaseEntry p2, double P, double T)
    {
        double g1 = GibbsMinimizer.ComputePhaseGibbs(p1, P, T);
        double g2 = GibbsMinimizer.ComputePhaseGibbs(p2, P, T);
        return g1 - g2;
    }
}
