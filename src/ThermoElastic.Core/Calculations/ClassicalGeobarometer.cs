namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Classical geothermobarometry using phase equilibrium and exchange reactions.
/// Uses SLB2011 database for phase stability calculations.
/// </summary>
public class ClassicalGeobarometer
{
    private readonly PhaseDiagramCalculator _phaseDiagram = new();

    /// <summary>
    /// Determine which phase is stable at given P-T from a pair of polymorphs.
    /// </summary>
    /// <param name="phase1">First phase</param>
    /// <param name="phase2">Second phase</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <returns>Name of stable phase and Gibbs difference [kJ/mol]</returns>
    public (string StablePhase, double DeltaG) DetermineStablePhase(
        MineralParams phase1, MineralParams phase2, double P, double T)
    {
        var entry1 = new PhaseEntry { Name = phase1.MineralName, Mineral = phase1, Amount = 1.0 };
        var entry2 = new PhaseEntry { Name = phase2.MineralName, Mineral = phase2, Amount = 1.0 };

        double g1 = GibbsMinimizer.ComputePhaseGibbs(entry1, P, T);
        double g2 = GibbsMinimizer.ComputePhaseGibbs(entry2, P, T);

        double deltaG = g1 - g2; // positive means phase2 is more stable

        if (g1 <= g2)
            return (phase1.MineralName, deltaG);
        else
            return (phase2.MineralName, deltaG);
    }

    /// <summary>
    /// Compute a simplified pseudosection over a P-T grid.
    /// Returns which phase is stable at each grid point.
    /// </summary>
    public string[,] ComputePseudosection(MineralParams phase1, MineralParams phase2,
        double[] pressures, double[] temperatures)
    {
        int nP = pressures.Length;
        int nT = temperatures.Length;
        var grid = new string[nP, nT];

        for (int i = 0; i < nP; i++)
        {
            for (int j = 0; j < nT; j++)
            {
                var (stablePhase, _) = DetermineStablePhase(phase1, phase2, pressures[i], temperatures[j]);
                grid[i, j] = stablePhase;
            }
        }

        return grid;
    }

    /// <summary>
    /// Estimate pressure from a known phase transition at given temperature.
    /// Uses bisection via PhaseDiagramCalculator.FindPhaseBoundary.
    /// </summary>
    public double EstimatePressure(MineralParams lowPPhase, MineralParams highPPhase,
        double T, double pMin = 0.1, double pMax = 30.0)
    {
        var entry1 = new PhaseEntry { Name = lowPPhase.MineralName, Mineral = lowPPhase, Amount = 1.0 };
        var entry2 = new PhaseEntry { Name = highPPhase.MineralName, Mineral = highPPhase, Amount = 1.0 };

        return _phaseDiagram.FindPhaseBoundary(entry1, entry2, T, pMin, pMax);
    }

    /// <summary>
    /// Estimate temperature from a known phase transition at given pressure.
    /// Uses bisection via PhaseDiagramCalculator.FindBoundaryTemperature.
    /// </summary>
    public double EstimateTemperature(MineralParams phase1, MineralParams phase2,
        double P, double tMin = 300.0, double tMax = 3000.0)
    {
        var entry1 = new PhaseEntry { Name = phase1.MineralName, Mineral = phase1, Amount = 1.0 };
        var entry2 = new PhaseEntry { Name = phase2.MineralName, Mineral = phase2, Amount = 1.0 };

        return _phaseDiagram.FindBoundaryTemperature(entry1, entry2, P, tMin, tMax);
    }
}
