namespace ThermoElastic.Core.Models;

/// <summary>Configuration for planetary interior model.</summary>
public class PlanetaryConfig
{
    /// <summary>Planet name</summary>
    public string Name { get; set; } = "Earth";
    /// <summary>Total radius [km]</summary>
    public double Radius_km { get; set; } = 6371.0;
    /// <summary>Core radius [km]</summary>
    public double CoreRadius_km { get; set; } = 3480.0;
    /// <summary>Core density [g/cm3] (simplified uniform core)</summary>
    public double CoreDensity { get; set; } = 11.0;
    /// <summary>Surface temperature [K]</summary>
    public double SurfaceTemperature { get; set; } = 300.0;
    /// <summary>Potential temperature [K] for adiabatic mantle</summary>
    public double PotentialTemperature { get; set; } = 1600.0;
    /// <summary>Mantle mineral for simplified single-mineral model</summary>
    public MineralParams? MantleMineral { get; set; }
}
