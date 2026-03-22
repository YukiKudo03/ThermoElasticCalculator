namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Mars interior model using PlanetaryInteriorSolver.
/// Mars: R=3389.5 km, core R~1830 km, Mg#~0.75-0.80.
/// </summary>
public class MarsInteriorModel
{
    /// <summary>Create a default Mars configuration.</summary>
    public static PlanetaryConfig GetDefaultConfig()
    {
        var minerals = Database.SLB2011Endmembers.GetAll();
        return new PlanetaryConfig
        {
            Name = "Mars",
            Radius_km = 3389.5,
            CoreRadius_km = 1830.0,
            CoreDensity = 6.5,  // Fe-S core
            SurfaceTemperature = 220.0,
            PotentialTemperature = 1600.0,
            MantleMineral = minerals.First(m => m.PaperName == "fo"),  // simplified olivine mantle
        };
    }

    /// <summary>Compute Mars interior profile.</summary>
    public RadialProfile Compute(PlanetaryConfig? config = null)
    {
        config ??= GetDefaultConfig();
        var solver = new PlanetaryInteriorSolver();
        return solver.Solve(config);
    }
}
