using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Calculates residual inclusion pressures for elastic geobarometry.
/// Based on the isomeke (equal volume change) concept.
/// </summary>
public class IsomekeCalculator
{
    /// <summary>
    /// Compute residual pressure in an inclusion at observation conditions.
    /// The inclusion pressure is found by matching the volume strain of host and inclusion
    /// from entrapment to observation conditions.
    /// </summary>
    /// <param name="host">Host mineral (e.g., garnet/forsterite)</param>
    /// <param name="inclusion">Inclusion mineral (e.g., quartz)</param>
    /// <param name="P_entrap">Entrapment pressure [GPa]</param>
    /// <param name="T_entrap">Entrapment temperature [K]</param>
    /// <param name="P_obs">Observation pressure [GPa] (typically 0.001)</param>
    /// <param name="T_obs">Observation temperature [K] (typically 300)</param>
    /// <returns>Residual inclusion pressure [GPa]</returns>
    public double ComputeResidualPressure(MineralParams host, MineralParams inclusion,
        double P_entrap, double T_entrap, double P_obs = 0.001, double T_obs = 300.0)
    {
        // At entrapment: compute volumes for host and inclusion
        var hostEntrap = new MieGruneisenEOSOptimizer(host, P_entrap, T_entrap).ExecOptimize();
        var incEntrap = new MieGruneisenEOSOptimizer(inclusion, P_entrap, T_entrap).ExecOptimize();
        double V_host_entrap = hostEntrap.Volume;
        double V_inc_entrap = incEntrap.Volume;

        // At observation: compute host volume at ambient conditions
        var hostObs = new MieGruneisenEOSOptimizer(host, P_obs, T_obs).ExecOptimize();
        double V_host_obs = hostObs.Volume;

        // Host volumetric strain from entrapment to observation
        double eps_host = (V_host_obs - V_host_entrap) / V_host_entrap;

        // Bisection: find P_inc such that inclusion strain matches host strain
        double P_lo = -1.0;
        double P_hi = 5.0;
        int maxIter = 200;
        double tol = 1e-6;

        for (int i = 0; i < maxIter; i++)
        {
            double P_mid = (P_lo + P_hi) / 2.0;
            var incObs = new MieGruneisenEOSOptimizer(inclusion, P_mid, T_obs).ExecOptimize();
            double V_inc_obs = incObs.Volume;
            double eps_inc = (V_inc_obs - V_inc_entrap) / V_inc_entrap;

            double diff = eps_inc - eps_host;

            if (Math.Abs(diff) < tol)
                return P_mid;

            // Higher pressure -> smaller volume -> smaller (more negative) strain
            // If eps_inc > eps_host, inclusion is too expanded -> need higher pressure
            if (diff > 0)
                P_lo = P_mid;
            else
                P_hi = P_mid;
        }

        return (P_lo + P_hi) / 2.0;
    }

    /// <summary>
    /// Convert Raman wavenumber shift to pressure using calibration.
    /// </summary>
    /// <param name="observedShift">Observed Raman shift [cm⁻¹]</param>
    /// <param name="referenceShift">Reference shift at 1 atm [cm⁻¹]</param>
    /// <param name="calibrationFactor">dν/dP [cm⁻¹/GPa], default for quartz 464 mode</param>
    /// <returns>Pressure [GPa]</returns>
    public static double RamanToPressure(double observedShift, double referenceShift = 464.0,
        double calibrationFactor = 35.0)
    {
        return (observedShift - referenceShift) / calibrationFactor;
    }
}
