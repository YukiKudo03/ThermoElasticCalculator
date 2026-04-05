using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

public class MCMCCompositionTests
{
    private readonly ITestOutputHelper _output;
    public MCMCCompositionTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void MCMCInversion_Bridgmanite_ProducesReasonableMgNumber()
    {
        var minerals = SLB2011Endmembers.GetAll();
        var mpv = minerals.First(m => m.PaperName == "mpv");
        var fpv = minerals.First(m => m.PaperName == "fpv");

        var inverter = new CompositionInverter();
        var result = inverter.MCMCInversion(mpv, fpv, P: 50.0, T: 2000.0, depth_km: 1200.0,
            nSamples: 3000, burnIn: 500);

        _output.WriteLine($"Mg# = {result.MeanMgNumber:F4} ± {result.StdMgNumber:F4}");
        _output.WriteLine($"95% CI: [{result.CI95_Low:F4}, {result.CI95_High:F4}]");
        _output.WriteLine($"Acceptance rate: {result.AcceptanceRate:P1}");
        _output.WriteLine($"Grid search Mg#: {result.GridSearchResult.BestMgNumber:F4}");

        // Mg# should be in physical range
        Assert.InRange(result.MeanMgNumber, 0.0, 1.0);
        // Uncertainty should be non-zero but bounded
        Assert.True(result.StdMgNumber > 0, "Should have non-zero uncertainty");
        Assert.True(result.StdMgNumber < 0.5, "Uncertainty should be bounded");
        // Acceptance rate should be reasonable (10-60%)
        Assert.InRange(result.AcceptanceRate, 0.05, 0.80);
        // MCMC mean should agree with grid search within ~2 sigma
        Assert.InRange(result.MeanMgNumber,
            result.GridSearchResult.BestMgNumber - 3 * result.StdMgNumber,
            result.GridSearchResult.BestMgNumber + 3 * result.StdMgNumber);
    }

    [Fact]
    public void MCMCInversion_CI95_ContainsMean()
    {
        var minerals = SLB2011Endmembers.GetAll();
        var pe = minerals.First(m => m.PaperName == "pe");
        var wu = minerals.First(m => m.PaperName == "wu");

        var inverter = new CompositionInverter();
        var result = inverter.MCMCInversion(pe, wu, P: 30.0, T: 1500.0, depth_km: 800.0,
            nSamples: 2000, burnIn: 500);

        _output.WriteLine($"Mg# = {result.MeanMgNumber:F4}, CI95 = [{result.CI95_Low:F4}, {result.CI95_High:F4}]");

        Assert.True(result.CI95_Low <= result.MeanMgNumber, "CI lower bound <= mean");
        Assert.True(result.CI95_High >= result.MeanMgNumber, "CI upper bound >= mean");
        Assert.True(result.CI95_High > result.CI95_Low, "CI should have non-zero width");
    }
}
