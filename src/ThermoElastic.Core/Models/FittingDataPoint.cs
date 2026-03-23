namespace ThermoElastic.Core.Models;

/// <summary>
/// A single observed data point for thermoelastic parameter fitting.
/// Set Vp/Vs for velocity data, Volume for V(T,P) data, or both for combined mode.
/// Unset fields default to NaN (= not observed).
/// </summary>
public class FittingDataPoint
{
    /// <summary>Temperature [K]</summary>
    public double Temperature { get; set; }
    /// <summary>Pressure [GPa]</summary>
    public double Pressure { get; set; }

    /// <summary>P-wave velocity [m/s] (NaN = not observed)</summary>
    public double Vp { get; set; } = double.NaN;
    /// <summary>S-wave velocity [m/s] (NaN = not observed)</summary>
    public double Vs { get; set; } = double.NaN;
    /// <summary>Molar volume [cm³/mol] (NaN = not observed)</summary>
    public double Volume { get; set; } = double.NaN;

    /// <summary>Uncertainty on Vp [m/s] (NaN = not applicable)</summary>
    public double SigmaVp { get; set; } = double.NaN;
    /// <summary>Uncertainty on Vs [m/s] (NaN = not applicable)</summary>
    public double SigmaVs { get; set; } = double.NaN;
    /// <summary>Uncertainty on Volume [cm³/mol] (NaN = not applicable)</summary>
    public double SigmaVolume { get; set; } = double.NaN;

    /// <summary>True if both Vp and Vs are set (non-NaN).</summary>
    public bool HasVelocityData => !double.IsNaN(Vp) && !double.IsNaN(Vs);

    /// <summary>True if Volume is set (non-NaN).</summary>
    public bool HasVolumeData => !double.IsNaN(Volume);
}
