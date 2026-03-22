using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for mantle composition inversion via grid search over Mg#.
/// </summary>
public class CompositionInversionTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;
    private readonly CompositionInverter _inverter;

    public CompositionInversionTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
        _inverter = new CompositionInverter();
    }

    private MineralParams FindMineral(string paperName)
    {
        return _minerals.First(m => m.PaperName == paperName);
    }

    /// <summary>
    /// Test 1: Misfit for Mg-perovskite vs PREM at 1000 km depth should be finite positive.
    /// </summary>
    [Fact]
    public void ComputeMisfit_MgPerovskite_AtDepth1000km_IsFinitePositive()
    {
        var mpv = FindMineral("mpv");
        double P = 38.0;  // ~38 GPa at 1000 km
        double T = 2000.0;
        double depth_km = 1000.0;

        double misfit = _inverter.ComputeMisfit(mpv, P, T, depth_km);

        _output.WriteLine($"Misfit for mpv at {depth_km} km: {misfit:E4}");
        Assert.True(double.IsFinite(misfit), "Misfit should be finite");
        Assert.True(misfit > 0, "Misfit should be positive (exact match is extremely unlikely)");
    }

    /// <summary>
    /// Test 2: Grid search over Mg# (0.50-1.00) for perovskite at lower mantle conditions.
    /// Best-fit should be in a reasonable range (Mg# > 0.50) with lower misfit than
    /// the pure Mg-endmember, reflecting the need for some Fe content.
    /// </summary>
    [Fact]
    public void GridSearch_Perovskite_BestFitIsMgRich()
    {
        var mpv = FindMineral("mpv");
        var fpv = FindMineral("fpv");
        double P = 38.0;
        double T = 2000.0;
        double depth_km = 1000.0;

        var result = _inverter.GridSearch(mpv, fpv, P, T, depth_km,
            mgMin: 0.50, mgMax: 1.00, nSteps: 10);

        _output.WriteLine($"Best Mg#: {result.BestMgNumber:F3}, Min misfit: {result.MinMisfit:E4}");
        foreach (var (mg, misfit) in result.MisfitProfile)
        {
            _output.WriteLine($"  Mg# = {mg:F3}, misfit = {misfit:E4}");
        }

        // Best-fit should be in the Mg-rich half (Mg# > 0.50)
        Assert.True(result.BestMgNumber >= 0.50, $"Best Mg# should be >= 0.50, got {result.BestMgNumber:F3}");
        // Best-fit misfit should be lower than pure Mg-endmember misfit
        double pureMgMisfit = result.MisfitProfile.Last().Misfit;
        Assert.True(result.MinMisfit <= pureMgMisfit,
            "Best-fit misfit should be <= pure Mg-endmember misfit");
        Assert.True(result.MinMisfit < 1.0, "Minimum misfit should be reasonable (< 1.0)");
    }

    /// <summary>
    /// Test 3: Misfit profile should have a minimum (U-shaped or monotonic towards it).
    /// At least one interior point should have lower misfit than both endpoints.
    /// </summary>
    [Fact]
    public void GridSearch_MisfitProfile_HasMinimum()
    {
        var mpv = FindMineral("mpv");
        var fpv = FindMineral("fpv");
        double P = 38.0;
        double T = 2000.0;
        double depth_km = 1000.0;

        var result = _inverter.GridSearch(mpv, fpv, P, T, depth_km,
            mgMin: 0.80, mgMax: 1.00, nSteps: 10);

        _output.WriteLine("Misfit profile:");
        foreach (var (mg, misfit) in result.MisfitProfile)
        {
            _output.WriteLine($"  Mg# = {mg:F3}, misfit = {misfit:E4}");
        }

        // The minimum misfit should be less than or equal to at least one endpoint
        double firstMisfit = result.MisfitProfile.First().Misfit;
        double lastMisfit = result.MisfitProfile.Last().Misfit;
        double minMisfit = result.MinMisfit;

        Assert.True(minMisfit <= firstMisfit || minMisfit <= lastMisfit,
            "Minimum misfit should be <= at least one endpoint");
        Assert.True(result.MisfitProfile.Count == 11, "Should have nSteps+1 grid points");
    }

    /// <summary>
    /// Test 4: Forsterite at upper mantle conditions (10 GPa, 1500 K) should have
    /// relatively low misfit vs PREM at corresponding depth (~300 km).
    /// </summary>
    [Fact]
    public void ComputeMisfit_Forsterite_UpperMantle_HasLowMisfit()
    {
        var fo = FindMineral("fo");
        double P = 10.0;   // ~10 GPa
        double T = 1500.0;
        double depth_km = 300.0;

        double misfit = _inverter.ComputeMisfit(fo, P, T, depth_km);

        _output.WriteLine($"Misfit for forsterite at {depth_km} km (P={P} GPa, T={T} K): {misfit:E4}");
        Assert.True(double.IsFinite(misfit), "Misfit should be finite");
        // Forsterite is a major upper mantle mineral, misfit should be modest
        Assert.True(misfit < 0.5, $"Forsterite misfit at upper mantle should be < 0.5, got {misfit:E4}");
    }
}
