using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Converts between depth (km) and pressure (GPa) using the PREM model.
/// Uses the PREM pressure-depth relationship with linear interpolation.
/// </summary>
public static class DepthConverter
{
    /// <summary>
    /// Convert pressure (GPa) to depth (km) using PREM.
    /// Uses bisection search on the monotonic PREM P(depth) profile.
    /// </summary>
    public static double PressureToDepth(double pressure_GPa)
    {
        if (pressure_GPa <= 0) return 0;
        if (pressure_GPa >= 135.75) return 2891.0;

        // Bisection on depth
        double lo = 0, hi = 2891.0;
        for (int i = 0; i < 50; i++)
        {
            double mid = (lo + hi) / 2.0;
            double pMid = PREMModel.GetPropertiesAtDepth(mid).Pressure;
            if (pMid < pressure_GPa)
                lo = mid;
            else
                hi = mid;
        }
        return (lo + hi) / 2.0;
    }

    /// <summary>
    /// Convert depth (km) to pressure (GPa) using PREM.
    /// </summary>
    public static double DepthToPressure(double depth_km)
    {
        return PREMModel.GetPropertiesAtDepth(depth_km).Pressure;
    }
}
