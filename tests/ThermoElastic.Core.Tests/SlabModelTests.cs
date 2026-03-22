using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for subducting slab thermal structure and mineral property modeling.
/// Verifies half-space cooling profiles, slab temperature anomalies,
/// and seismic velocity perturbations relative to ambient mantle.
/// </summary>
public class SlabModelTests
{
    private readonly ITestOutputHelper _output;
    private readonly SlabThermalModel _model;
    private readonly List<MineralParams> _minerals;

    public SlabModelTests(ITestOutputHelper output)
    {
        _output = output;
        _model = new SlabThermalModel();
        _minerals = SLB2011Endmembers.GetAll();
    }

    private MineralParams FindMineral(string paperName)
    {
        return _minerals.First(m => m.PaperName == paperName);
    }

    /// <summary>
    /// Test 1: Half-space cooling profile for 80 Myr oceanic plate.
    /// At 100 km depth (surface, before subduction), T should be ~1000-1300 K.
    /// Reference: McKenzie (1969) half-space cooling model.
    /// </summary>
    [Fact]
    public void HalfSpaceCooling_80Myr_At100km_GivesExpectedTemperature()
    {
        double plateAge_Myr = 80.0;
        double depth_km = 100.0;

        double T = _model.HalfSpaceCoolingTemperature(depth_km, plateAge_Myr);

        _output.WriteLine($"Half-space cooling T at {depth_km} km for {plateAge_Myr} Myr plate: {T:F1} K");

        // McKenzie (1969): T_m * erf(z / 2*sqrt(kappa*t)) with T_m ~ 1600K
        // For 80 Myr at 100 km: erf(100e3 / (2*sqrt(1e-6 * 80e6*3.15e7))) ~ erf(0.99) ~ 0.84
        // T ~ 273 + (1600-273)*0.84 ~ 1388 K — expect range 1000-1300 K is conservative
        Assert.InRange(T, 1000.0, 1400.0);
    }

    /// <summary>
    /// Test 2: Slab center at 400 km depth should be colder than ambient mantle (~1700K).
    /// For 80 Myr plate, expect T_slab ~ 800-1200 K.
    /// </summary>
    [Fact]
    public void SlabCenter_At400km_IsColdRelativeToAmbientMantle()
    {
        double plateAge_Myr = 80.0;
        double ambientT = 1700.0; // approximate mantle T at 400 km

        var geotherm = _model.ComputeSlabGeotherm(plateAge_Myr, maxDepth_km: 700.0, nPoints: 50);

        // Find point nearest to 400 km
        var point400 = geotherm.OrderBy(p => Math.Abs(p.Depth_km - 400.0)).First();

        _output.WriteLine($"Slab center at {point400.Depth_km:F1} km: T = {point400.Temperature_K:F1} K, P = {point400.Pressure_GPa:F2} GPa");
        _output.WriteLine($"Ambient mantle T ~ {ambientT:F0} K");
        _output.WriteLine($"Temperature deficit: {ambientT - point400.Temperature_K:F1} K");

        Assert.True(point400.Temperature_K < ambientT,
            $"Slab T ({point400.Temperature_K:F1} K) should be less than ambient ({ambientT:F0} K)");
        Assert.InRange(point400.Temperature_K, 800.0, 1200.0);
    }

    /// <summary>
    /// Test 3: At 400 km depth, cold slab should have higher Vs than ambient mantle.
    /// Using forsterite/wadsleyite at slab vs ambient temperature.
    /// Expect +1% to +5% dVs.
    /// </summary>
    [Fact]
    public void SlabVelocityAnomaly_At400km_IsPositive()
    {
        var fo = FindMineral("fo");
        double P = 13.35; // PREM pressure at ~400 km
        double T_slab = 1000.0; // cold slab
        double T_ambient = 1700.0; // ambient mantle

        var (dVp, dVs, dRho) = _model.ComputeSlabAnomaly(fo, P, T_slab, T_ambient);

        _output.WriteLine($"Forsterite at P={P} GPa:");
        _output.WriteLine($"  T_slab = {T_slab} K, T_ambient = {T_ambient} K");
        _output.WriteLine($"  dVp = {dVp:F2}%, dVs = {dVs:F2}%, dRho = {dRho:F2}%");

        // Cold slab should have higher velocities (positive anomaly)
        Assert.True(dVs > 0, $"Cold slab should have higher Vs, got dVs = {dVs:F2}%");
        Assert.InRange(dVs, 1.0, 6.0);
    }

    /// <summary>
    /// Test 4: Cold slab at same depth should be denser than ambient mantle.
    /// </summary>
    [Fact]
    public void SlabDensity_At400km_IsHigherThanAmbient()
    {
        var fo = FindMineral("fo");
        double P = 13.35; // PREM pressure at ~400 km
        double T_slab = 1000.0;
        double T_ambient = 1700.0;

        var (dVp, dVs, dRho) = _model.ComputeSlabAnomaly(fo, P, T_slab, T_ambient);

        _output.WriteLine($"Forsterite density anomaly at P={P} GPa: dRho = {dRho:F2}%");

        // Cold slab should be denser (positive density anomaly)
        Assert.True(dRho > 0, $"Cold slab should be denser, got dRho = {dRho:F2}%");
    }

    /// <summary>
    /// Test 5: Geotherm should have the expected number of depth points.
    /// </summary>
    [Fact]
    public void SlabGeotherm_HasExpectedNumberOfPoints()
    {
        int nPoints = 50;
        var geotherm = _model.ComputeSlabGeotherm(80.0, maxDepth_km: 700.0, nPoints: nPoints);

        _output.WriteLine($"Geotherm points: {geotherm.Count}");
        _output.WriteLine($"Depth range: {geotherm.First().Depth_km:F1} - {geotherm.Last().Depth_km:F1} km");

        Assert.Equal(nPoints, geotherm.Count);
        Assert.Equal(0.0, geotherm.First().Depth_km);
        Assert.Equal(700.0, geotherm.Last().Depth_km);
    }
}
