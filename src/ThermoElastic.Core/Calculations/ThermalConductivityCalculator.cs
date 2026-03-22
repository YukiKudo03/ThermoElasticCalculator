namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Calculates lattice thermal conductivity using the phonon gas model.
/// k_lat(P,T) = k0 * (rho/rho0)^g * (T0/T)
/// Reference: Hofmeister (1999).
/// </summary>
public class ThermalConductivityCalculator
{
    /// <summary>
    /// Compute lattice thermal conductivity at given P-T.
    /// </summary>
    /// <param name="mineral">Mineral parameters</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="k0">Reference conductivity at ambient [W/m/K]</param>
    /// <param name="g">Density exponent (typically 4-6)</param>
    /// <returns>Thermal conductivity [W/m/K]</returns>
    public double ComputeLatticeConductivity(MineralParams mineral, double P, double T,
        double k0 = 50.0, double g = 5.0)
    {
        // Reference state: near ambient (0.0001 GPa, 300 K)
        var ref0 = new MieGruneisenEOSOptimizer(mineral, 0.0001, 300.0).ExecOptimize();
        double rho0 = ref0.Density;

        // Current state
        var current = new MieGruneisenEOSOptimizer(mineral, P, T).ExecOptimize();
        double rho = current.Density;

        // k = k0 * (rho/rho0)^g * (300/T)
        double ratioRho = rho / rho0;
        double k = k0 * Math.Pow(ratioRho, g) * (300.0 / T);

        return k;
    }
}
