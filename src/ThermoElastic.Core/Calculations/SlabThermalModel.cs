namespace ThermoElastic.Core.Calculations;

using MathNet.Numerics;
using ThermoElastic.Core.Models;

/// <summary>
/// Models the thermal structure of subducting oceanic slabs.
/// Uses half-space cooling for the initial plate thermal structure
/// and kinematic advection for the slab path.
/// </summary>
public class SlabThermalModel
{
    private const double ThermalDiffusivity = 1.0e-6; // m²/s
    private const double MantleTemperature = 1600.0; // K (potential temperature)
    private const double SurfaceTemperature = 273.0; // K

    /// <summary>
    /// Compute the thermal profile of an oceanic plate before subduction
    /// using the half-space cooling model.
    /// The slab core temperature at each depth is approximated by the
    /// half-space temperature at the plate half-thickness, preserving
    /// the cold thermal anomaly as the slab subducts.
    /// </summary>
    /// <param name="plateAge_Myr">Age of the oceanic plate [Myr]</param>
    /// <param name="maxDepth_km">Maximum depth to compute [km]</param>
    /// <param name="nPoints">Number of depth points</param>
    /// <returns>List of (depth_km, T_K, P_GPa) along the slab center</returns>
    public List<(double Depth_km, double Temperature_K, double Pressure_GPa)> ComputeSlabGeotherm(
        double plateAge_Myr, double maxDepth_km = 700.0, int nPoints = 50)
    {
        double ageSeconds = plateAge_Myr * 1.0e6 * 365.25 * 24.0 * 3600.0;
        double thermalLengthScale = 2.0 * Math.Sqrt(ThermalDiffusivity * ageSeconds);

        // Plate thickness ~ 2.32 * sqrt(kappa * age) — depth where T reaches ~90% of mantle T
        double plateThickness_m = 2.32 * Math.Sqrt(ThermalDiffusivity * ageSeconds);
        double plateHalfThickness_m = plateThickness_m / 2.0;

        // Slab core temperature: half-space cooling T at the plate center
        double erfArg = plateHalfThickness_m / thermalLengthScale;
        double slabCoreTemp = SurfaceTemperature + (MantleTemperature - SurfaceTemperature) * SpecialFunctions.Erf(erfArg);

        var geotherm = new List<(double Depth_km, double Temperature_K, double Pressure_GPa)>();

        for (int i = 0; i < nPoints; i++)
        {
            double depth_km = (i == 0) ? 0.0 : maxDepth_km * i / (nPoints - 1);
            double depth_m = depth_km * 1000.0;
            double pressure = PREMModel.GetPropertiesAtDepth(depth_km).Pressure;

            double temperature;
            if (depth_km <= 0)
            {
                temperature = SurfaceTemperature;
            }
            else
            {
                // For shallow depths (within the plate), use half-space cooling directly
                double z_erf = Math.Min(depth_m, plateHalfThickness_m);
                double arg = z_erf / thermalLengthScale;
                temperature = SurfaceTemperature + (MantleTemperature - SurfaceTemperature) * SpecialFunctions.Erf(arg);

                // Once past the plate half-thickness, use the slab core temperature
                // (the cold slab interior is advected to depth)
                if (depth_m > plateHalfThickness_m)
                {
                    temperature = slabCoreTemp;
                }
            }

            geotherm.Add((depth_km, temperature, pressure));
        }

        return geotherm;
    }

    /// <summary>
    /// Compute the half-space cooling temperature at a given depth for a plate of given age.
    /// T(z) = T_surface + (T_mantle - T_surface) * erf(z / (2*sqrt(kappa*age)))
    /// </summary>
    public double HalfSpaceCoolingTemperature(double depth_km, double plateAge_Myr)
    {
        double ageSeconds = plateAge_Myr * 1.0e6 * 365.25 * 24.0 * 3600.0;
        double depth_m = depth_km * 1000.0;
        double arg = depth_m / (2.0 * Math.Sqrt(ThermalDiffusivity * ageSeconds));
        return SurfaceTemperature + (MantleTemperature - SurfaceTemperature) * SpecialFunctions.Erf(arg);
    }

    /// <summary>
    /// Compute the velocity anomaly of a slab mineral relative to ambient mantle.
    /// </summary>
    /// <param name="mineral">Mineral to evaluate</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T_slab">Slab temperature [K]</param>
    /// <param name="T_ambient">Ambient mantle temperature [K]</param>
    /// <returns>(dVp_percent, dVs_percent, dRho_percent) anomaly</returns>
    public (double dVp_percent, double dVs_percent, double dRho_percent) ComputeSlabAnomaly(
        MineralParams mineral, double P, double T_slab, double T_ambient)
    {
        var slabResult = new MieGruneisenEOSOptimizer(mineral, P, T_slab).ExecOptimize();
        var ambientResult = new MieGruneisenEOSOptimizer(mineral, P, T_ambient).ExecOptimize();

        double dVp_percent = (slabResult.Vp - ambientResult.Vp) / ambientResult.Vp * 100.0;
        double dVs_percent = (slabResult.Vs - ambientResult.Vs) / ambientResult.Vs * 100.0;
        double dRho_percent = (slabResult.Density - ambientResult.Density) / ambientResult.Density * 100.0;

        return (dVp_percent, dVs_percent, dRho_percent);
    }
}
