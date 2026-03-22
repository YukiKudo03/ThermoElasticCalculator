namespace ThermoElastic.Core.Models;

/// <summary>
/// Result of anelastic correction applied to elastic velocities.
/// </summary>
public record AnelasticResult
{
    /// <summary>Shear quality factor</summary>
    public double QS { get; init; }
    /// <summary>Bulk quality factor (typically >> QS)</summary>
    public double QK { get; init; }
    /// <summary>Corrected P-wave velocity [m/s]</summary>
    public double Vp_anelastic { get; init; }
    /// <summary>Corrected S-wave velocity [m/s]</summary>
    public double Vs_anelastic { get; init; }
    /// <summary>Original elastic P-wave velocity [m/s]</summary>
    public double Vp_elastic { get; init; }
    /// <summary>Original elastic S-wave velocity [m/s]</summary>
    public double Vs_elastic { get; init; }
    /// <summary>Relative Vs reduction (negative value)</summary>
    public double DeltaVs_percent => (Vs_anelastic - Vs_elastic) / Vs_elastic * 100.0;
    /// <summary>Relative Vp reduction (negative value)</summary>
    public double DeltaVp_percent => (Vp_anelastic - Vp_elastic) / Vp_elastic * 100.0;
}
