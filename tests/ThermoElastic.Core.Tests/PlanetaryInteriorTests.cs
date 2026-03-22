using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for PlanetaryInteriorSolver, MarsInteriorModel, and ThermalConductivityCalculator.
/// </summary>
public class PlanetaryInteriorTests
{
    private readonly ITestOutputHelper _output;

    public PlanetaryInteriorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ============================================================
    // Planetary Interior Solver Tests
    // ============================================================

    private static PlanetaryConfig GetEarthLikeConfig()
    {
        var minerals = SLB2011Endmembers.GetAll();
        return new PlanetaryConfig
        {
            Name = "Earth",
            Radius_km = 6371.0,
            CoreRadius_km = 3480.0,
            CoreDensity = 11.0,
            SurfaceTemperature = 300.0,
            PotentialTemperature = 1600.0,
            MantleMineral = minerals.First(m => m.PaperName == "fo"),
        };
    }

    [Fact]
    public void PlanetaryInterior_EarthLike_CMBPressure_InRange()
    {
        // Earth CMB pressure should be ~100-140 GPa
        var config = GetEarthLikeConfig();
        var solver = new PlanetaryInteriorSolver();
        var profile = solver.Solve(config, nPoints: 200);

        // Find pressure at core-mantle boundary (~3480 km radius)
        double cmbPressure = 0;
        for (int i = 0; i < profile.Radius_km.Length; i++)
        {
            if (profile.Radius_km[i] >= config.CoreRadius_km)
            {
                cmbPressure = profile.Pressure_GPa[i];
                break;
            }
        }

        _output.WriteLine($"Earth-like CMB pressure: {cmbPressure:F1} GPa");
        Assert.InRange(cmbPressure, 100.0, 140.0);
    }

    [Fact]
    public void PlanetaryInterior_EarthLike_SurfaceGravity_InRange()
    {
        // Earth surface gravity should be ~8.5-11 m/s^2
        // (simplified single-mineral model slightly underestimates mantle density)
        var config = GetEarthLikeConfig();
        var solver = new PlanetaryInteriorSolver();
        var profile = solver.Solve(config, nPoints: 200);

        double surfaceG = profile.Gravity[^1];
        _output.WriteLine($"Earth-like surface gravity: {surfaceG:F2} m/s^2");
        Assert.InRange(surfaceG, 8.5, 11.0);
    }

    [Fact]
    public void PlanetaryInterior_EarthLike_MoIFactor_InRange()
    {
        // Differentiated planet MoI factor should be ~0.25-0.40
        var config = GetEarthLikeConfig();
        var solver = new PlanetaryInteriorSolver();
        var profile = solver.Solve(config, nPoints: 200);

        _output.WriteLine($"Earth-like MoI factor: {profile.MomentOfInertiaFactor:F4}");
        Assert.InRange(profile.MomentOfInertiaFactor, 0.25, 0.40);
    }

    [Fact]
    public void PlanetaryInterior_EarthLike_PressureMonotonicallyIncreases()
    {
        // Pressure should increase from surface to center
        var config = GetEarthLikeConfig();
        var solver = new PlanetaryInteriorSolver();
        var profile = solver.Solve(config, nPoints: 100);

        // Profile is from center (i=0) to surface (i=n-1)
        // So pressure should decrease from center to surface
        for (int i = 1; i < profile.Pressure_GPa.Length; i++)
        {
            Assert.True(profile.Pressure_GPa[i] <= profile.Pressure_GPa[i - 1],
                $"Pressure should decrease from center to surface: P[{i}]={profile.Pressure_GPa[i]:F2} > P[{i - 1}]={profile.Pressure_GPa[i - 1]:F2}");
        }

        // Surface pressure should be ~0
        Assert.True(profile.Pressure_GPa[^1] < 1.0,
            $"Surface pressure should be near 0, got {profile.Pressure_GPa[^1]:F2} GPa");
    }

    // ============================================================
    // Mars Interior Model Tests
    // ============================================================

    [Fact]
    public void MarsInterior_MoIFactor_InRange()
    {
        // Mars MoI factor: Konopliv et al. (2020) gives 0.3639
        // Allow range 0.33-0.38
        var mars = new MarsInteriorModel();
        var profile = mars.Compute();

        _output.WriteLine($"Mars MoI factor: {profile.MomentOfInertiaFactor:F4}");
        Assert.InRange(profile.MomentOfInertiaFactor, 0.33, 0.38);
    }

    [Fact]
    public void MarsInterior_SurfaceGravity_InRange()
    {
        // Mars surface gravity should be ~3.5-4.0 m/s^2
        var mars = new MarsInteriorModel();
        var profile = mars.Compute();

        double surfaceG = profile.Gravity[^1];
        _output.WriteLine($"Mars surface gravity: {surfaceG:F2} m/s^2");
        Assert.InRange(surfaceG, 3.5, 4.0);
    }

    [Fact]
    public void MarsInterior_CMBPressure_InRange()
    {
        // Mars CMB pressure should be ~15-25 GPa for core radius ~1830 km
        // (simplified single-mineral forsterite model; real Mars has denser phases)
        var mars = new MarsInteriorModel();
        var config = MarsInteriorModel.GetDefaultConfig();
        var profile = mars.Compute();

        double cmbPressure = 0;
        for (int i = 0; i < profile.Radius_km.Length; i++)
        {
            if (profile.Radius_km[i] >= config.CoreRadius_km)
            {
                cmbPressure = profile.Pressure_GPa[i];
                break;
            }
        }

        _output.WriteLine($"Mars CMB pressure: {cmbPressure:F1} GPa");
        Assert.InRange(cmbPressure, 15.0, 25.0);
    }

    // ============================================================
    // Thermal Conductivity Tests
    // ============================================================

    [Fact]
    public void ThermalConductivity_MgO_Ambient_InRange()
    {
        // MgO (periclase) at ambient: k_lat ~ 30-60 W/m/K. Hofmeister (1999).
        var minerals = SLB2011Endmembers.GetAll();
        var pe = minerals.First(m => m.PaperName == "pe");

        var calc = new ThermalConductivityCalculator();
        double k = calc.ComputeLatticeConductivity(pe, 0.0001, 300.0, k0: 50.0, g: 5.0);

        _output.WriteLine($"MgO k_lat at ambient: {k:F1} W/m/K");
        Assert.InRange(k, 30.0, 60.0);
    }

    [Fact]
    public void ThermalConductivity_IncreasesWithPressure()
    {
        // Higher pressure -> higher density -> higher k_lat
        var minerals = SLB2011Endmembers.GetAll();
        var pe = minerals.First(m => m.PaperName == "pe");

        var calc = new ThermalConductivityCalculator();
        double k_lowP = calc.ComputeLatticeConductivity(pe, 0.0001, 300.0);
        double k_highP = calc.ComputeLatticeConductivity(pe, 50.0, 300.0);

        _output.WriteLine($"k_lat at 0 GPa: {k_lowP:F1}, at 50 GPa: {k_highP:F1}");
        Assert.True(k_highP > k_lowP,
            $"k_lat should increase with pressure: k(50 GPa)={k_highP:F1} <= k(0 GPa)={k_lowP:F1}");
    }

    [Fact]
    public void ThermalConductivity_DecreasesWithTemperature()
    {
        // k_lat proportional to 1/T
        var minerals = SLB2011Endmembers.GetAll();
        var pe = minerals.First(m => m.PaperName == "pe");

        var calc = new ThermalConductivityCalculator();
        double k_lowT = calc.ComputeLatticeConductivity(pe, 10.0, 300.0);
        double k_highT = calc.ComputeLatticeConductivity(pe, 10.0, 1500.0);

        _output.WriteLine($"k_lat at 300 K: {k_lowT:F1}, at 1500 K: {k_highT:F1}");
        Assert.True(k_lowT > k_highT,
            $"k_lat should decrease with temperature: k(300K)={k_lowT:F1} <= k(1500K)={k_highT:F1}");
    }
}
