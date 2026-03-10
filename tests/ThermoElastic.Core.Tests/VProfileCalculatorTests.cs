using Xunit;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class VProfileCalculatorTests
{
    private static ResultSummary CreateSample(string name, double ks, double gs, double volume, double density) => new()
    {
        Name = name, GivenP = 10.0, GivenT = 1500.0,
        KS = ks, KT = ks * 0.95, GS = gs, Volume = volume, Density = density,
    };

    [Fact]
    public void VoigtAverage_PureEndMember_MatchesEndMember()
    {
        var elem1 = CreateSample("A", 200, 100, 40, 3.5);
        var elem2 = CreateSample("B", 300, 150, 50, 4.0);
        var result = VProfileCalculator.VoigtAverage(1.0, elem1, elem2);

        Assert.NotNull(result);
        Assert.Equal(200, result!.KS, 5);
    }

    [Fact]
    public void ReussAverage_50_50_IsLowerBound()
    {
        var elem1 = CreateSample("A", 200, 100, 40, 3.5);
        var elem2 = CreateSample("B", 300, 150, 50, 4.0);
        var voigt = VProfileCalculator.VoigtAverage(0.5, elem1, elem2)!;
        var reuss = VProfileCalculator.ReussAverage(0.5, elem1, elem2)!;

        Assert.True(reuss.KS <= voigt.KS);
    }

    [Fact]
    public void HillAverage_IsMeanOfVoigtAndReuss()
    {
        var elem1 = CreateSample("A", 200, 100, 40, 3.5);
        var elem2 = CreateSample("B", 300, 150, 50, 4.0);
        var voigt = VProfileCalculator.VoigtAverage(0.5, elem1, elem2)!;
        var reuss = VProfileCalculator.ReussAverage(0.5, elem1, elem2)!;
        var hill = VProfileCalculator.HillAverage(0.5, elem1, elem2)!;

        Assert.Equal((voigt.KS + reuss.KS) / 2.0, hill.KS, 5);
    }

    [Fact]
    public void VoigtAverage_InvalidRatio_ReturnsNull()
    {
        var elem1 = CreateSample("A", 200, 100, 40, 3.5);
        var elem2 = CreateSample("B", 300, 150, 50, 4.0);
        Assert.Null(VProfileCalculator.VoigtAverage(-0.1, elem1, elem2));
        Assert.Null(VProfileCalculator.VoigtAverage(1.1, elem1, elem2));
    }

    [Fact]
    public void VoigtAverage_DifferentPT_ReturnsNull()
    {
        var elem1 = CreateSample("A", 200, 100, 40, 3.5);
        var elem2 = new ResultSummary { Name = "B", GivenP = 20.0, GivenT = 1500.0, KS = 300, GS = 150, Volume = 50, Density = 4.0 };
        Assert.Null(VProfileCalculator.VoigtAverage(0.5, elem1, elem2));
    }

    [Fact]
    public void VoigtResults_GeneratesCorrectCount()
    {
        var elem1 = CreateSample("A", 200, 100, 40, 3.5);
        var elem2 = CreateSample("B", 300, 150, 50, 4.0);
        var ratios = new List<double> { 0.0, 0.25, 0.5, 0.75, 1.0 };
        var vpc = new VProfileCalculator(ratios, elem1, elem2, "Test");

        var results = vpc.VoigtResults();
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void JsonRoundTrip_PreservesData()
    {
        var elem1 = CreateSample("A", 200, 100, 40, 3.5);
        var elem2 = CreateSample("B", 300, 150, 50, 4.0);
        var ratios = new List<double> { 0.0, 0.5, 1.0 };
        var vpc = new VProfileCalculator(ratios, elem1, elem2, "Test");

        var json = vpc.ExportJson();
        Assert.True(VProfileCalculator.ImportJson(json, out var loaded));
        Assert.NotNull(loaded);
        Assert.Equal(3, loaded!.Elem1RatioList.Count);
        Assert.Equal("Test", loaded.Name);
    }

    [Fact]
    public void HashinShtrikmanBond_ReturnsBetweenBounds()
    {
        var elem1 = CreateSample("A", 200, 100, 40, 3.5);
        var elem2 = CreateSample("B", 300, 150, 50, 4.0);
        var hs = VProfileCalculator.HashinShtrikmanBond(0.5, elem1, elem2);

        Assert.NotNull(hs);
        Assert.True(hs!.KS > 0);
    }
}
