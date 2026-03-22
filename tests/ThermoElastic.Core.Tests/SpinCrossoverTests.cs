using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for iron spin crossover (high-spin to low-spin) transition calculator.
/// Validates spin state fractions and elastic property changes in ferropericlase.
/// </summary>
public class SpinCrossoverTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;
    private readonly SpinCrossoverCalculator _calculator;

    public SpinCrossoverTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
        _calculator = new SpinCrossoverCalculator();
    }

    private MineralParams FindMineral(string paperName)
    {
        return _minerals.First(m => m.PaperName == paperName);
    }

    /// <summary>
    /// Test 1: At 60 GPa, 2000K the LS fraction should be in the crossover region (~0.3-0.9).
    /// Reference: Sturhahn et al. (2005).
    /// </summary>
    [Fact]
    public void LSFraction_At60GPa_InCrossoverRegion()
    {
        var wu = FindMineral("wu");
        var lsWu = SpinCrossoverCalculator.CreateLSEndmember(wu);

        var result = _calculator.ComputeSpinState(wu, lsWu, 60.0, 2000.0);

        _output.WriteLine($"n_LS = {result.nLS:F4} at 60 GPa, 2000 K");
        _output.WriteLine($"V_eff = {result.Volume:F4}, KS_eff = {result.KS:F2}, GS_eff = {result.GS:F2}");

        // Should be in the crossover region
        Assert.True(result.nLS >= 0.3 && result.nLS <= 0.9,
            $"Expected n_LS in [0.3, 0.9], got {result.nLS:F4}");
    }

    /// <summary>
    /// Test 2: At low pressure (10 GPa), LS fraction should be near 0 (mostly high-spin).
    /// </summary>
    [Fact]
    public void LSFraction_AtLowPressure_NearZero()
    {
        var wu = FindMineral("wu");
        var lsWu = SpinCrossoverCalculator.CreateLSEndmember(wu);

        var result = _calculator.ComputeSpinState(wu, lsWu, 10.0, 2000.0);

        _output.WriteLine($"n_LS = {result.nLS:F4} at 10 GPa, 2000 K");

        Assert.True(result.nLS < 0.2,
            $"Expected n_LS < 0.2 at low P, got {result.nLS:F4}");
    }

    /// <summary>
    /// Test 3: At high pressure (120 GPa), LS fraction should be near 1 (mostly low-spin).
    /// </summary>
    [Fact]
    public void LSFraction_AtHighPressure_NearOne()
    {
        var wu = FindMineral("wu");
        var lsWu = SpinCrossoverCalculator.CreateLSEndmember(wu);

        var result = _calculator.ComputeSpinState(wu, lsWu, 120.0, 2000.0);

        _output.WriteLine($"n_LS = {result.nLS:F4} at 120 GPa, 2000 K");

        Assert.True(result.nLS > 0.8,
            $"Expected n_LS > 0.8 at high P, got {result.nLS:F4}");
    }

    /// <summary>
    /// Test 4: Effective volume with spin crossover at 60 GPa should be less than pure HS volume.
    /// LS state has smaller volume, so mixed state should have reduced volume.
    /// </summary>
    [Fact]
    public void EffectiveVolume_WithCrossover_LessThanPureHS()
    {
        var wu = FindMineral("wu");
        var lsWu = SpinCrossoverCalculator.CreateLSEndmember(wu);

        double P = 60.0;
        double T = 2000.0;

        var hsResult = new MieGruneisenEOSOptimizer(wu, P, T).ExecOptimize();
        var spinResult = _calculator.ComputeSpinState(wu, lsWu, P, T);

        _output.WriteLine($"HS Volume = {hsResult.Volume:F4}, Effective Volume = {spinResult.Volume:F4}");
        _output.WriteLine($"n_LS = {spinResult.nLS:F4}");

        Assert.True(spinResult.Volume < hsResult.Volume,
            $"Expected V_eff < V_HS, got {spinResult.Volume:F4} vs {hsResult.Volume:F4}");
    }

    /// <summary>
    /// Test 5: In the crossover region, effective KS may show softening relative to pure HS.
    /// The mixing of HS and LS states can produce anomalous elastic behavior.
    /// </summary>
    [Fact]
    public void KSAnomaly_InCrossoverRegion()
    {
        var wu = FindMineral("wu");
        var lsWu = SpinCrossoverCalculator.CreateLSEndmember(wu);

        double P = 60.0;
        double T = 2000.0;

        var hsResult = new MieGruneisenEOSOptimizer(wu, P, T).ExecOptimize();
        var lsResult = new MieGruneisenEOSOptimizer(lsWu, P, T).ExecOptimize();
        var spinResult = _calculator.ComputeSpinState(wu, lsWu, P, T);

        _output.WriteLine($"HS KS = {hsResult.KS:F2}, LS KS = {lsResult.KS:F2}, Eff KS = {spinResult.KS:F2}");
        _output.WriteLine($"n_LS = {spinResult.nLS:F4}");

        // In crossover region, KS_eff should differ from pure HS KS
        // The effective KS is a linear mix, so it should be between HS and LS values
        double minKS = Math.Min(hsResult.KS, lsResult.KS);
        double maxKS = Math.Max(hsResult.KS, lsResult.KS);

        Assert.True(spinResult.KS >= minKS - 1.0 && spinResult.KS <= maxKS + 1.0,
            $"Expected KS_eff between {minKS:F2} and {maxKS:F2}, got {spinResult.KS:F2}");

        // With nonzero n_LS, KS should differ from pure HS
        Assert.NotEqual(hsResult.KS, spinResult.KS, 1);
    }
}
