namespace ThermoElastic.Core.Models;

/// <summary>
/// A single point in a depth-dependent Q profile.
/// </summary>
public record QProfilePoint
{
    public double Depth_km { get; init; }
    public double Pressure_GPa { get; init; }
    public double Temperature_K { get; init; }
    public double QS { get; init; }
    public double QS_PREM { get; init; }
    public double Vp_elastic { get; init; }
    public double Vs_elastic { get; init; }
    public double Vp_anelastic { get; init; }
    public double Vs_anelastic { get; init; }
    public double DeltaVs_percent => Vs_elastic > 0 ? (Vs_anelastic - Vs_elastic) / Vs_elastic * 100.0 : 0;
    public double DeltaVp_percent => Vp_elastic > 0 ? (Vp_anelastic - Vp_elastic) / Vp_elastic * 100.0 : 0;
    public string DominantPhase { get; init; } = "";
}
