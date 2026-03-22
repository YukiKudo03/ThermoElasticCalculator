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

    /// <summary>
    /// Find phase boundary for a multi-phase reaction: reactants → products.
    /// Finds P where sum(G_products) = sum(G_reactants) at given T.
    /// </summary>
    /// <param name="reactants">Reactant phases with stoichiometric amounts</param>
    /// <param name="products">Product phases with stoichiometric amounts</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="pMin">Minimum pressure for search [GPa]</param>
    /// <param name="pMax">Maximum pressure for search [GPa]</param>
    /// <param name="tolerance">Convergence tolerance [kJ/mol]</param>
    /// <returns>Boundary pressure [GPa], or NaN if not found</returns>
    public double FindMultiPhaseBoundary(
        List<(PhaseEntry Phase, double Stoichiometry)> reactants,
        List<(PhaseEntry Phase, double Stoichiometry)> products,
        double T, double pMin, double pMax, double tolerance = 0.01)
    {
        double dgLow = ComputeMultiPhaseGDiff(reactants, products, pMin, T);
        double dgHigh = ComputeMultiPhaseGDiff(reactants, products, pMax, T);

        if (dgLow * dgHigh > 0) return double.NaN;

        for (int i = 0; i < 50; i++)
        {
            double pMid = (pMin + pMax) / 2.0;
            double dgMid = ComputeMultiPhaseGDiff(reactants, products, pMid, T);

            if (Math.Abs(dgMid) < tolerance) return pMid;

            if (dgLow * dgMid < 0)
                pMax = pMid;
            else
            {
                pMin = pMid;
                dgLow = dgMid;
            }
        }

        return (pMin + pMax) / 2.0;
    }

    /// <summary>
    /// Find the temperature at which a two-phase boundary occurs at given pressure.
    /// </summary>
    public double FindBoundaryTemperature(PhaseEntry phase1, PhaseEntry phase2,
        double P, double tMin, double tMax, double tolerance = 0.5)
    {
        double gDiffLow = ComputeGDiff(phase1, phase2, P, tMin);
        double gDiffHigh = ComputeGDiff(phase1, phase2, P, tMax);

        if (gDiffLow * gDiffHigh > 0) return double.NaN;

        for (int i = 0; i < 50; i++)
        {
            double tMid = (tMin + tMax) / 2.0;
            double gDiffMid = ComputeGDiff(phase1, phase2, P, tMid);

            if (Math.Abs(gDiffMid) < tolerance) return tMid;

            if (gDiffLow * gDiffMid < 0)
                tMax = tMid;
            else
            {
                tMin = tMid;
                gDiffLow = gDiffMid;
            }
        }

        return (tMin + tMax) / 2.0;
    }

    /// <summary>
    /// Trace a phase boundary curve across a temperature range, returning list of (P, T) points.
    /// </summary>
    public List<(double P, double T)> TracePhaseBoundary(PhaseEntry phase1, PhaseEntry phase2,
        double tMin, double tMax, int nPoints, double pMin = 0.1, double pMax = 30.0)
    {
        if (nPoints < 2)
            throw new ArgumentOutOfRangeException(nameof(nPoints), "Must be >= 2.");

        var result = new List<(double P, double T)>();
        double dT = (tMax - tMin) / (nPoints - 1);

        for (int i = 0; i < nPoints; i++)
        {
            double t = tMin + i * dT;
            double p = FindPhaseBoundary(phase1, phase2, t, pMin, pMax);
            result.Add((p, t));
        }

        return result;
    }

    /// <summary>
    /// Compute Clapeyron slope dP/dT at a given phase boundary point.
    /// Uses Clausius-Clapeyron: dP/dT = ΔS/ΔV
    /// </summary>
    public double ComputeClapeyronSlope(PhaseEntry phase1, PhaseEntry phase2, double P, double T)
    {
        var eos1 = new MieGruneisenEOSOptimizer(phase1.Mineral, P, T);
        var th1 = eos1.ExecOptimize();

        var eos2 = new MieGruneisenEOSOptimizer(phase2.Mineral, P, T);
        var th2 = eos2.ExecOptimize();

        double deltaS = th2.Entropy - th1.Entropy; // J/mol/K
        double deltaV = th2.Volume - th1.Volume;    // cm³/mol

        if (Math.Abs(deltaV) < 1e-10) return double.NaN;

        // dP/dT [GPa/K] = ΔS [J/mol/K] / ΔV [cm³/mol] * 0.001
        // Because 1 J = 1e-3 kJ, and 1 kJ/(cm³/mol) = 1 GPa
        // So 1 J/(cm³/mol) = 0.001 GPa
        return deltaS / deltaV * 0.001;
    }

    private static double ComputeGDiff(PhaseEntry p1, PhaseEntry p2, double P, double T)
    {
        double g1 = GibbsMinimizer.ComputePhaseGibbs(p1, P, T);
        double g2 = GibbsMinimizer.ComputePhaseGibbs(p2, P, T);
        return g1 - g2;
    }

    private static double ComputeMultiPhaseGDiff(
        List<(PhaseEntry Phase, double Stoichiometry)> reactants,
        List<(PhaseEntry Phase, double Stoichiometry)> products,
        double P, double T)
    {
        double gProducts = 0;
        foreach (var (phase, stoich) in products)
            gProducts += stoich * GibbsMinimizer.ComputePhaseGibbs(phase, P, T);

        double gReactants = 0;
        foreach (var (phase, stoich) in reactants)
            gReactants += stoich * GibbsMinimizer.ComputePhaseGibbs(phase, P, T);

        return gProducts - gReactants;
    }
}
