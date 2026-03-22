namespace ThermoElastic.Core.Models;

/// <summary>
/// A single point on a Hugoniot curve.
/// </summary>
public record HugoniotPoint
{
    /// <summary>Volume [cm³/mol]</summary>
    public double Volume { get; init; }
    /// <summary>Pressure [GPa]</summary>
    public double Pressure { get; init; }
    /// <summary>Temperature [K]</summary>
    public double Temperature { get; init; }
    /// <summary>Density [g/cm³]</summary>
    public double Density { get; init; }
    /// <summary>Shock velocity [km/s]</summary>
    public double Us { get; init; }
    /// <summary>Particle velocity [km/s]</summary>
    public double Up { get; init; }
    /// <summary>Internal energy on Hugoniot [kJ/mol]</summary>
    public double InternalEnergy { get; init; }
    /// <summary>Volume compression ratio V/V0</summary>
    public double Compression => Volume > 0 ? Volume / _v0 : 0;
    private readonly double _v0;

    public HugoniotPoint(double v0) { _v0 = v0; }
    public HugoniotPoint() { _v0 = 1.0; }
}
