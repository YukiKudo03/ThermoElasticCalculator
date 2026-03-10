using Xunit;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class RockCalculatorTests
{
    private static MineralParams CreateForsterite() => new()
    {
        MineralName = "Forsterite", PaperName = "fo", NumAtoms = 7,
        MolarVolume = 43.6, MolarWeight = 140.69,
        KZero = 128.0, K1Prime = 4.2, K2Prime = 0,
        GZero = 82.0, G1Prime = 1.5, G2Prime = 0,
        DebyeTempZero = 809.0, GammaZero = 0.99,
        QZero = 2.1, EhtaZero = 2.3, RefTemp = 300.0,
    };

    private static MineralParams CreateWadsleyite() => new()
    {
        MineralName = "Wadsleyite", PaperName = "wa", NumAtoms = 7,
        MolarVolume = 40.52, MolarWeight = 140.69,
        KZero = 169.0, K1Prime = 4.3, K2Prime = 0,
        GZero = 112.0, G1Prime = 1.4, G2Prime = 0,
        DebyeTempZero = 853.0, GammaZero = 1.21,
        QZero = 2.0, EhtaZero = 2.6, RefTemp = 300.0,
    };

    [Fact]
    public void Calculate_SingleMineral_ReturnsMixedResult()
    {
        var rock = new RockComposition
        {
            Name = "Pure Fo",
            Minerals = new List<RockMineralEntry>
            {
                new() { Mineral = CreateForsterite(), VolumeRatio = 1.0 }
            }
        };

        var calc = new RockCalculator(rock, 5.0, 1000.0, MixtureMethod.Hill);
        var (mixed, individual) = calc.Calculate();

        Assert.NotNull(mixed);
        Assert.Single(individual);
        Assert.Equal("Pure Fo", mixed!.Name);
    }

    [Fact]
    public void Calculate_TwoMinerals_ReturnsAllResults()
    {
        var rock = new RockComposition
        {
            Name = "Fo-Wa",
            Minerals = new List<RockMineralEntry>
            {
                new() { Mineral = CreateForsterite(), VolumeRatio = 0.6 },
                new() { Mineral = CreateWadsleyite(), VolumeRatio = 0.4 },
            }
        };

        var calc = new RockCalculator(rock, 5.0, 1000.0, MixtureMethod.Hill);
        var (mixed, individual) = calc.Calculate();

        Assert.NotNull(mixed);
        Assert.Equal(2, individual.Count);
        Assert.True(mixed!.KS > 0);
    }

    [Fact]
    public void Calculate_VoigtMethod_ReturnsResult()
    {
        var rock = new RockComposition
        {
            Name = "Test",
            Minerals = new List<RockMineralEntry>
            {
                new() { Mineral = CreateForsterite(), VolumeRatio = 0.5 },
                new() { Mineral = CreateWadsleyite(), VolumeRatio = 0.5 },
            }
        };

        var calc = new RockCalculator(rock, 5.0, 1000.0, MixtureMethod.Voigt);
        var (mixed, _) = calc.Calculate();

        Assert.NotNull(mixed);
    }

    [Fact]
    public void Calculate_ReussMethod_ReturnsResult()
    {
        var rock = new RockComposition
        {
            Name = "Test",
            Minerals = new List<RockMineralEntry>
            {
                new() { Mineral = CreateForsterite(), VolumeRatio = 0.5 },
                new() { Mineral = CreateWadsleyite(), VolumeRatio = 0.5 },
            }
        };

        var calc = new RockCalculator(rock, 5.0, 1000.0, MixtureMethod.Reuss);
        var (mixed, _) = calc.Calculate();

        Assert.NotNull(mixed);
    }
}
