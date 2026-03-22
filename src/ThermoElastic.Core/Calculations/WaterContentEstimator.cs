using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Estimates water content in transition zone minerals from velocity reductions.
/// </summary>
public class WaterContentEstimator
{
    // Empirical parameters from Mao et al. (2012): 1 wt% H2O reduces:
    private const double DVs_per_wtPercent = -0.012; // -1.2% per wt% H2O
    private const double DVp_per_wtPercent = -0.005; // -0.5% per wt% H2O
    private const double DRho_per_wtPercent = -0.003; // -0.3% per wt% H2O

    /// <summary>
    /// Compute hydrous mineral properties using empirical scaling.
    /// </summary>
    /// <param name="mineral">Dry mineral parameters</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="waterContent_wt">Water content [wt%]</param>
    /// <returns>Tuple of (Vp [m/s], Vs [m/s], Density [g/cm³])</returns>
    public (double Vp, double Vs, double Density) ComputeHydrousProperties(
        MineralParams mineral, double P, double T, double waterContent_wt)
    {
        // Compute dry properties at (P, T) using EOS
        var dry = new MieGruneisenEOSOptimizer(mineral, P, T).ExecOptimize();

        double Vp_dry = dry.Vp;
        double Vs_dry = dry.Vs;
        double Rho_dry = dry.Density;

        // Apply empirical corrections
        double Vp_wet = Vp_dry * (1.0 + DVp_per_wtPercent * waterContent_wt);
        double Vs_wet = Vs_dry * (1.0 + DVs_per_wtPercent * waterContent_wt);
        double Rho_wet = Rho_dry * (1.0 + DRho_per_wtPercent * waterContent_wt);

        return (Vp_wet, Vs_wet, Rho_wet);
    }

    /// <summary>
    /// Estimate water content from observed velocity reduction.
    /// </summary>
    /// <param name="mineral">Dry mineral</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="observedDlnVs">Observed ∂ln(Vs) relative to dry (negative)</param>
    /// <returns>Estimated water content [wt%]</returns>
    public double EstimateWaterContent(MineralParams mineral, double P, double T, double observedDlnVs)
    {
        // Invert: waterContent = observedDlnVs / DVs_per_wtPercent
        return observedDlnVs / DVs_per_wtPercent;
    }
}
