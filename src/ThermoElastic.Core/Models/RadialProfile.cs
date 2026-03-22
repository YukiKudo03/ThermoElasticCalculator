namespace ThermoElastic.Core.Models;

/// <summary>Radial profile of planetary interior properties.</summary>
public class RadialProfile
{
    public string PlanetName { get; set; } = string.Empty;
    public double[] Radius_km { get; set; } = Array.Empty<double>();
    public double[] Depth_km { get; set; } = Array.Empty<double>();
    public double[] Pressure_GPa { get; set; } = Array.Empty<double>();
    public double[] Temperature_K { get; set; } = Array.Empty<double>();
    public double[] Density { get; set; } = Array.Empty<double>();
    public double[] Gravity { get; set; } = Array.Empty<double>();
    public double[] Vp { get; set; } = Array.Empty<double>();
    public double[] Vs { get; set; } = Array.Empty<double>();
    /// <summary>Total mass [kg]</summary>
    public double TotalMass { get; set; }
    /// <summary>Moment of inertia factor I/(MR^2)</summary>
    public double MomentOfInertiaFactor { get; set; }
}
