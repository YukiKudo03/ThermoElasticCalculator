using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for ULVZ (Ultra-Low Velocity Zone) calculator.
/// Validates partial melt effects on seismic velocities at CMB conditions.
/// </summary>
public class ULVZTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;
    private readonly ULVZCalculator _calculator;

    public ULVZTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
        _calculator = new ULVZCalculator();
    }

    private MineralParams FindMineral(string paperName)
    {
        return _minerals.First(m => m.PaperName == paperName);
    }

    /// <summary>
    /// Test 1: 10% partial melt at 135 GPa, 3800K should reduce Vs by more than 5%.
    /// Melt has GS=0 (liquid), so shear wave velocity drops significantly.
    /// </summary>
    [Fact]
    public void PartialMelt_ReducesVs_ByMoreThan5Percent()
    {
        var mpv = FindMineral("mpv");
        double P = 135.0;
        double T = 3800.0;
        double meltFraction = 0.10;

        var result = _calculator.ComputeMeltMixture(mpv, meltFraction, P, T);

        _output.WriteLine($"Vp_mix = {result.Vp:F1} m/s, Vs_mix = {result.Vs:F1} m/s");
        _output.WriteLine($"dVp = {result.dVp_pct:F2}%, dVs = {result.dVs_pct:F2}%");
        _output.WriteLine($"Density_mix = {result.Density:F3} g/cm³");

        // 10% melt should reduce Vs by more than 5%
        Assert.True(result.dVs_pct < -5.0,
            $"Expected dVs < -5%, got {result.dVs_pct:F2}%");
    }

    /// <summary>
    /// Test 2: Partial melt reduces Vp less than Vs.
    /// Melt has finite K but zero G, so Vp is less affected than Vs.
    /// </summary>
    [Fact]
    public void PartialMelt_ReducesVp_LessThanVs()
    {
        var mpv = FindMineral("mpv");
        double P = 135.0;
        double T = 3800.0;
        double meltFraction = 0.10;

        var result = _calculator.ComputeMeltMixture(mpv, meltFraction, P, T);

        _output.WriteLine($"dVp = {result.dVp_pct:F2}%, dVs = {result.dVs_pct:F2}%");

        // |dVp| should be smaller than |dVs|
        Assert.True(Math.Abs(result.dVp_pct) < Math.Abs(result.dVs_pct),
            $"Expected |dVp| < |dVs|, got |dVp|={Math.Abs(result.dVp_pct):F2}%, |dVs|={Math.Abs(result.dVs_pct):F2}%");
    }

    /// <summary>
    /// Test 3: Fe-enriched solid (wustite) at 135 GPa should be denser than periclase.
    /// </summary>
    [Fact]
    public void FeEnrichedSolid_IncreaseDensity()
    {
        var pe = FindMineral("pe");
        var wu = FindMineral("wu");
        double P = 135.0;
        double T = 3000.0;

        var peResult = new MieGruneisenEOSOptimizer(pe, P, T).ExecOptimize();
        var wuResult = new MieGruneisenEOSOptimizer(wu, P, T).ExecOptimize();

        _output.WriteLine($"Periclase density = {peResult.Density:F3} g/cm³");
        _output.WriteLine($"Wustite density   = {wuResult.Density:F3} g/cm³");

        Assert.True(wuResult.Density > peResult.Density,
            $"Expected wustite denser than periclase, got {wuResult.Density:F3} vs {peResult.Density:F3}");
    }

    /// <summary>
    /// Test 4: Zero melt fraction should return unchanged solid properties.
    /// </summary>
    [Fact]
    public void ZeroMelt_ReturnsOriginalProperties()
    {
        var mpv = FindMineral("mpv");
        double P = 135.0;
        double T = 3800.0;

        var solidResult = new MieGruneisenEOSOptimizer(mpv, P, T).ExecOptimize();
        var mixResult = _calculator.ComputeMeltMixture(mpv, 0.0, P, T);

        _output.WriteLine($"Solid Vp = {solidResult.Vp:F1}, Mix Vp = {mixResult.Vp:F1}");
        _output.WriteLine($"Solid Vs = {solidResult.Vs:F1}, Mix Vs = {mixResult.Vs:F1}");
        _output.WriteLine($"dVp = {mixResult.dVp_pct:F4}%, dVs = {mixResult.dVs_pct:F4}%");

        Assert.Equal(0.0, mixResult.dVp_pct);
        Assert.Equal(0.0, mixResult.dVs_pct);
        Assert.Equal(solidResult.Vp, mixResult.Vp, 1);
        Assert.Equal(solidResult.Vs, mixResult.Vs, 1);
    }
}
