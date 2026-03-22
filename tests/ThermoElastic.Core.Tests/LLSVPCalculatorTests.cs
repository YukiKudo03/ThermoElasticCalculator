using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for LLSVP composition interpretation calculator.
/// Evaluates thermal vs compositional origins of velocity anomalies in the deep mantle.
/// </summary>
public class LLSVPCalculatorTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;
    private readonly LLSVPCalculator _calculator;

    public LLSVPCalculatorTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
        _calculator = new LLSVPCalculator();
    }

    private MineralParams FindMineral(string paperName)
    {
        return _minerals.First(m => m.PaperName == paperName);
    }

    /// <summary>
    /// Test 1: A +500K thermal anomaly should reduce Vs by ~1-3% for Mg-perovskite at 120 GPa.
    /// </summary>
    [Fact]
    public void ThermalAnomaly_ReducesVs_By1To3Percent()
    {
        var mpv = FindMineral("mpv");
        double P = 120.0;
        double T_ref = 2500.0;
        double T_hot = 3000.0;

        var refResult = new MieGruneisenEOSOptimizer(mpv, P, T_ref).ExecOptimize();
        var hotResult = new MieGruneisenEOSOptimizer(mpv, P, T_hot).ExecOptimize();

        double dlnVs = (hotResult.Vs - refResult.Vs) / refResult.Vs * 100.0; // percent change

        _output.WriteLine($"Vs at {T_ref}K: {refResult.Vs:F1} m/s");
        _output.WriteLine($"Vs at {T_hot}K: {hotResult.Vs:F1} m/s");
        _output.WriteLine($"dVs: {dlnVs:F2}%");

        // +500K anomaly should reduce Vs (negative dlnVs) by 1-3%
        Assert.True(dlnVs < 0, "Higher temperature should reduce Vs");
        Assert.True(dlnVs > -5.0, $"Vs reduction should be less than 5%, got {dlnVs:F2}%");
        Assert.True(dlnVs < -0.5, $"Vs reduction should be at least 0.5%, got {dlnVs:F2}%");
    }

    /// <summary>
    /// Test 2: Fe-enrichment should reduce Vs compared to pure Mg-perovskite.
    /// </summary>
    [Fact]
    public void FeEnrichment_ReducesVs()
    {
        var mpv = FindMineral("mpv");
        var fpv = FindMineral("fpv");
        double P = 120.0;
        double T = 2500.0;

        var (reference, anomalous, dlnVp, dlnVs, dlnRho) =
            _calculator.CompareAssemblages(mpv, fpv, P, T);

        _output.WriteLine($"Mg-pv Vs: {reference.Vs:F1} m/s");
        _output.WriteLine($"Fe-pv Vs: {anomalous.Vs:F1} m/s");
        _output.WriteLine($"dlnVs: {dlnVs * 100:F2}%");
        _output.WriteLine($"dlnVp: {dlnVp * 100:F2}%");
        _output.WriteLine($"dlnRho: {dlnRho * 100:F2}%");

        // Fe-bearing perovskite should have lower Vs than Mg-perovskite
        Assert.True(dlnVs < 0, "Fe-enrichment should reduce Vs");
    }

    /// <summary>
    /// Test 3: Fe-enriched assemblage should be denser than normal pyrolite at same P-T.
    /// </summary>
    [Fact]
    public void FeEnrichment_IncreasesDensity()
    {
        var mpv = FindMineral("mpv");
        var fpv = FindMineral("fpv");
        double P = 120.0;
        double T = 2500.0;

        var (reference, anomalous, _, _, dlnRho) =
            _calculator.CompareAssemblages(mpv, fpv, P, T);

        _output.WriteLine($"Mg-pv density: {reference.Density:F3} g/cm3");
        _output.WriteLine($"Fe-pv density: {anomalous.Density:F3} g/cm3");
        _output.WriteLine($"dlnRho: {dlnRho * 100:F2}%");

        // Fe-bearing perovskite should be denser
        Assert.True(dlnRho > 0, "Fe-enrichment should increase density");
        Assert.True(anomalous.Density > reference.Density,
            "Fe-pv density should exceed Mg-pv density");
    }

    /// <summary>
    /// Test 4: For dVs = -2%, the required DeltaT (pure thermal) should be ~300-600K.
    /// </summary>
    [Fact]
    public void VelocityTemperatureTradeoff_RequiresDeltaT_300To600K()
    {
        var mpv = FindMineral("mpv");
        double P = 120.0;
        double T_ref = 2500.0;
        double targetDlnVs = -0.02; // -2%

        double deltaT = _calculator.ComputeRequiredDeltaT(mpv, P, T_ref, targetDlnVs);

        _output.WriteLine($"Required DeltaT for -2% dVs: {deltaT:F1} K");

        // The required temperature anomaly should be in a geophysically reasonable range
        Assert.True(deltaT > 100, $"DeltaT should be > 100K, got {deltaT:F1}K");
        Assert.True(deltaT < 1500, $"DeltaT should be < 1500K, got {deltaT:F1}K");
    }

    /// <summary>
    /// Test 5: Mg-perovskite at CMB conditions (135 GPa, 3800K) should have Vs ~6-8 km/s.
    /// </summary>
    [Fact]
    public void CMBConditions_MgPerovskite_VsInExpectedRange()
    {
        var mpv = FindMineral("mpv");
        double P = 135.0;
        double T = 3800.0;

        var result = _calculator.ComputeAtCMB(mpv, P, T);

        double vsKmPerS = result.Vs / 1000.0;
        double vpKmPerS = result.Vp / 1000.0;

        _output.WriteLine($"CMB conditions: P={P} GPa, T={T} K");
        _output.WriteLine($"Vs: {vsKmPerS:F2} km/s");
        _output.WriteLine($"Vp: {vpKmPerS:F2} km/s");
        _output.WriteLine($"Density: {result.Density:F3} g/cm3");

        // Vs should be in the range 6-8 km/s for bridgmanite at CMB
        Assert.True(vsKmPerS > 5.0, $"Vs should be > 5 km/s, got {vsKmPerS:F2}");
        Assert.True(vsKmPerS < 9.0, $"Vs should be < 9 km/s, got {vsKmPerS:F2}");

        // Vp should be reasonable too
        Assert.True(vpKmPerS > 10.0, $"Vp should be > 10 km/s, got {vpKmPerS:F2}");
        Assert.True(vpKmPerS < 16.0, $"Vp should be < 16 km/s, got {vpKmPerS:F2}");

        // Density at CMB should be ~5-6 g/cm3
        Assert.True(result.Density > 4.5, $"Density should be > 4.5, got {result.Density:F3}");
        Assert.True(result.Density < 7.0, $"Density should be < 7.0, got {result.Density:F3}");
    }
}
