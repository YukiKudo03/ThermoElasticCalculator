using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for WaterContentEstimator: hydrous velocity corrections and water content inversion.
/// References:
///   Mao et al. (2012), Inoue et al. (2010).
/// </summary>
public class WaterContentTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;

    public WaterContentTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
    }

    private MineralParams GetMineral(string paperName) =>
        _minerals.First(m => m.PaperName == paperName);

    /// <summary>
    /// Test 1: Adding 1 wt% water should reduce Vs by 0.5–2% at 15 GPa, 1500 K.
    /// </summary>
    [Fact]
    public void HydrousProperties_1WtPercentWater_ReducesVsByExpectedAmount()
    {
        var mw = GetMineral("mw");
        var estimator = new WaterContentEstimator();

        var (_, Vs_wet, _) = estimator.ComputeHydrousProperties(mw, 15.0, 1500.0, 1.0);

        // Compute dry for comparison
        var dry = new MieGruneisenEOSOptimizer(mw, 15.0, 1500.0).ExecOptimize();
        double reductionPercent = (dry.Vs - Vs_wet) / dry.Vs * 100.0;

        _output.WriteLine($"Dry Vs: {dry.Vs:F1} m/s");
        _output.WriteLine($"Wet Vs (1 wt% H2O): {Vs_wet:F1} m/s");
        _output.WriteLine($"Reduction: {reductionPercent:F2}%");

        Assert.InRange(reductionPercent, 0.5, 2.0);
    }

    /// <summary>
    /// Test 2: Hydration should slightly change density.
    /// </summary>
    [Fact]
    public void HydrousProperties_1WtPercentWater_ChangesDensity()
    {
        var mw = GetMineral("mw");
        var estimator = new WaterContentEstimator();

        var (_, _, Rho_wet) = estimator.ComputeHydrousProperties(mw, 15.0, 1500.0, 1.0);

        var dry = new MieGruneisenEOSOptimizer(mw, 15.0, 1500.0).ExecOptimize();

        _output.WriteLine($"Dry density: {dry.Density:F4} g/cm³");
        _output.WriteLine($"Wet density: {Rho_wet:F4} g/cm³");

        // Density should change but not by much
        Assert.NotEqual(dry.Density, Rho_wet);
        double changePercent = Math.Abs(dry.Density - Rho_wet) / dry.Density * 100.0;
        Assert.InRange(changePercent, 0.1, 1.0);
    }

    /// <summary>
    /// Test 3: Zero water content should give original (dry) properties.
    /// </summary>
    [Fact]
    public void HydrousProperties_ZeroWater_GivesOriginalProperties()
    {
        var mw = GetMineral("mw");
        var estimator = new WaterContentEstimator();

        var (Vp_wet, Vs_wet, Rho_wet) = estimator.ComputeHydrousProperties(mw, 15.0, 1500.0, 0.0);

        var dry = new MieGruneisenEOSOptimizer(mw, 15.0, 1500.0).ExecOptimize();

        _output.WriteLine($"Dry Vp={dry.Vp:F1}, Vs={dry.Vs:F1}, Rho={dry.Density:F4}");
        _output.WriteLine($"Wet(0%) Vp={Vp_wet:F1}, Vs={Vs_wet:F1}, Rho={Rho_wet:F4}");

        Assert.Equal(dry.Vp, Vp_wet, 5);
        Assert.Equal(dry.Vs, Vs_wet, 5);
        Assert.Equal(dry.Density, Rho_wet, 5);
    }

    /// <summary>
    /// Test 4: Estimate water content from a 1% Vs reduction.
    /// Should give 0.5–3 wt% water.
    /// </summary>
    [Fact]
    public void EstimateWaterContent_1PercentVsReduction_InExpectedRange()
    {
        var mw = GetMineral("mw");
        var estimator = new WaterContentEstimator();

        // -1% Vs reduction = dlnVs = -0.01
        double waterContent = estimator.EstimateWaterContent(mw, 15.0, 1500.0, -0.01);

        _output.WriteLine($"Estimated water content for -1% Vs: {waterContent:F2} wt%");

        Assert.InRange(waterContent, 0.5, 3.0);
    }
}
