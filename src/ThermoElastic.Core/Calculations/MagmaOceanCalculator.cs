namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Calculates solidus/liquidus and solidification sequence for magma oceans.
/// Uses parameterized melting curves from experimental data.
/// </summary>
public class MagmaOceanCalculator
{
    /// <summary>
    /// Compute peridotite solidus temperature at given pressure.
    /// Parameterization from Hirschmann (2000) for P &lt; 10 GPa,
    /// Andrault et al. (2011) for P &gt;= 10 GPa.
    /// </summary>
    public double ComputeSolidus(double P_GPa)
    {
        if (P_GPa < 10.0)
        {
            // Hirschmann (2000): T_solidus = 1394 + 132.9*P - 5.1*P^2
            return 1394.0 + 132.9 * P_GPa - 5.1 * P_GPa * P_GPa;
        }
        else
        {
            // Andrault et al. (2011), simplified: T_solidus = 2038 + 19.5*P - 0.2*P^2
            return 2038.0 + 19.5 * P_GPa - 0.2 * P_GPa * P_GPa;
        }
    }

    /// <summary>
    /// Compute peridotite liquidus temperature at given pressure.
    /// Simplified as solidus + 600 K.
    /// </summary>
    public double ComputeLiquidus(double P_GPa)
    {
        return ComputeSolidus(P_GPa) + 600.0;
    }

    /// <summary>
    /// Determine if a given P-T point is above liquidus, between solidus-liquidus, or below solidus.
    /// </summary>
    /// <param name="P_GPa">Pressure in GPa</param>
    /// <param name="T_K">Temperature in K</param>
    /// <returns>"Liquid" if above liquidus, "Partial Melt" if between, "Solid" if below solidus</returns>
    public string GetMeltingState(double P_GPa, double T_K)
    {
        double solidus = ComputeSolidus(P_GPa);
        double liquidus = ComputeLiquidus(P_GPa);

        if (T_K >= liquidus)
            return "Liquid";
        else if (T_K >= solidus)
            return "Partial Melt";
        else
            return "Solid";
    }

    /// <summary>
    /// Compute melt fraction using simple linear parameterization between solidus and liquidus.
    /// Returns 0 below solidus, 1 above liquidus, linear interpolation in between.
    /// </summary>
    public double ComputeMeltFraction(double P_GPa, double T_K)
    {
        double solidus = ComputeSolidus(P_GPa);
        double liquidus = ComputeLiquidus(P_GPa);

        if (T_K <= solidus)
            return 0.0;
        if (T_K >= liquidus)
            return 1.0;

        return (T_K - solidus) / (liquidus - solidus);
    }
}
