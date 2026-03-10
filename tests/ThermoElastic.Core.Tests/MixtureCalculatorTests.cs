using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class MixtureCalculatorTests
{
    private static ResultSummary CreateSample(string name, double ks, double gs, double volume, double density)
    {
        return new ResultSummary
        {
            Name = name,
            GivenP = 10.0,
            GivenT = 1500.0,
            KS = ks,
            KT = ks * 0.95,
            GS = gs,
            Volume = volume,
            Density = density,
        };
    }

    [Fact]
    public void VoigtAverage_50_50_ReturnsArithmeticMean()
    {
        var r1 = CreateSample("A", 200, 100, 40, 3.5);
        var r2 = CreateSample("B", 300, 150, 50, 4.0);
        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (0.5, r1), (0.5, r2) });
        var result = mix.VoigtAverage();

        Assert.NotNull(result);
        Assert.Equal(250, result!.KS, 5);
        Assert.Equal(125, result.GS, 5);
    }

    [Fact]
    public void ReussAverage_50_50_ReturnsHarmonicMean()
    {
        var r1 = CreateSample("A", 200, 100, 40, 3.5);
        var r2 = CreateSample("B", 200, 100, 50, 4.0);
        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (0.5, r1), (0.5, r2) });
        var result = mix.ReussAverage();

        Assert.NotNull(result);
        Assert.Equal(200, result!.KS, 5);
    }

    [Fact]
    public void HillAverage_IsBetweenVoigtAndReuss()
    {
        var r1 = CreateSample("A", 200, 100, 40, 3.5);
        var r2 = CreateSample("B", 300, 150, 50, 4.0);
        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (0.5, r1), (0.5, r2) });
        var voigt = mix.VoigtAverage()!;
        var reuss = mix.ReussAverage()!;
        var hill = mix.HillAverage()!;

        Assert.True(hill.KS >= Math.Min(voigt.KS, reuss.KS));
        Assert.True(hill.KS <= Math.Max(voigt.KS, reuss.KS));
    }
}
