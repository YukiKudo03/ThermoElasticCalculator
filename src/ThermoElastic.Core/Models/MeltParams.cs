namespace ThermoElastic.Core.Models;

/// <summary>
/// Simplified parametric EOS for silicate melt at extreme pressures.
/// </summary>
public record MeltParams
{
    /// <summary>Reference density [g/cm³]</summary>
    public double Rho0 { get; init; } = 3.0;
    /// <summary>Bulk modulus [GPa]</summary>
    public double K0 { get; init; } = 30.0;
    /// <summary>K' pressure derivative</summary>
    public double K1 { get; init; } = 6.0;
    /// <summary>Shear modulus is zero for liquid</summary>
    public double GS => 0.0;
}
