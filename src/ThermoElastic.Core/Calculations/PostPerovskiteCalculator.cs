namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Calculator for the bridgmanite -> post-perovskite phase transition near the CMB.
/// </summary>
public class PostPerovskiteCalculator
{
    private readonly PhaseDiagramCalculator _phaseDiagram = new();

    /// <summary>
    /// Find the pv-ppv phase boundary pressure at given temperature.
    /// </summary>
    public double FindBoundary(MineralParams pv, MineralParams ppv, double T,
        double pMin = 100.0, double pMax = 140.0)
    {
        var pvPhase = MakePhase("pv", pv);
        var ppvPhase = MakePhase("ppv", ppv);

        return _phaseDiagram.FindPhaseBoundary(pvPhase, ppvPhase, T, pMin, pMax);
    }

    /// <summary>
    /// Compute the Clapeyron slope of the pv-ppv transition [GPa/K].
    /// </summary>
    public double GetClapeyronSlope(MineralParams pv, MineralParams ppv, double P, double T)
    {
        var pvPhase = MakePhase("pv", pv);
        var ppvPhase = MakePhase("ppv", ppv);

        return _phaseDiagram.ComputeClapeyronSlope(pvPhase, ppvPhase, P, T);
    }

    /// <summary>
    /// Compare properties across the transition at given P-T.
    /// </summary>
    public (ThermoMineralParams pv_props, ThermoMineralParams ppv_props, double dVs_percent, double dVp_percent, double dRho_percent)
        CompareAcrossTransition(MineralParams pv, MineralParams ppv, double P, double T)
    {
        var eosPv = new MieGruneisenEOSOptimizer(pv, P, T);
        var pvProps = eosPv.ExecOptimize();

        var eosPpv = new MieGruneisenEOSOptimizer(ppv, P, T);
        var ppvProps = eosPpv.ExecOptimize();

        double dVs = (ppvProps.Vs - pvProps.Vs) / pvProps.Vs * 100.0;
        double dVp = (ppvProps.Vp - pvProps.Vp) / pvProps.Vp * 100.0;
        double dRho = (ppvProps.Density - pvProps.Density) / pvProps.Density * 100.0;

        return (pvProps, ppvProps, dVs, dVp, dRho);
    }

    private static PhaseEntry MakePhase(string name, MineralParams mineral)
    {
        return new PhaseEntry { Name = name, Mineral = mineral, Amount = 1.0 };
    }
}
