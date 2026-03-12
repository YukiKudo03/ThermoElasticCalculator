using Xunit;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Core.Tests;

public class EquilibriumAggregateTests
{
    [Fact]
    public void NoSolidSolution_MatchesMechanicalMixing()
    {
        var fo = MineralDatabase.GetByName("fo")!;
        var rock = new RockComposition
        {
            Name = "Pure Fo",
            Minerals = new List<RockMineralEntry>
            {
                new RockMineralEntry { Mineral = fo, VolumeRatio = 1.0 }
            }
        };

        var calcMech = new RockCalculator(rock, 5.0, 1500.0, MixtureMethod.Hill);
        var (mechResult, mechIndiv) = calcMech.Calculate();

        var calcEquil = new RockCalculator(rock, 5.0, 1500.0, MixtureMethod.Hill) { UseEquilibrium = true };
        var (equilResult, equilIndiv) = calcEquil.Calculate();

        Assert.NotNull(mechResult);
        Assert.NotNull(equilResult);
        Assert.Equal(mechResult!.KS, equilResult!.KS, 1);
        Assert.Equal(mechResult.Vp, equilResult.Vp, 0);
    }

    [Fact]
    public void MassBalance_IsPreserved()
    {
        var fo = MineralDatabase.GetByName("fo")!;
        var pe = MineralDatabase.GetByName("pe")!;

        var phases = new List<PhaseEntry>
        {
            new PhaseEntry { Name = "Forsterite", Mineral = fo, Amount = 0.7 },
            new PhaseEntry { Name = "Periclase", Mineral = pe, Amount = 0.3 },
        };

        var calc = new EquilibriumAggregateCalculator();
        var (mixed, individual) = calc.CalculateMechanical(phases, 5.0, 1500.0, MixtureMethod.Hill);

        double totalRatio = individual.Sum(r => r.ratio);
        Assert.Equal(1.0, totalRatio, 4);
    }

    [Fact]
    public void ExistingRockCalculator_StillWorks()
    {
        // Use the original test Forsterite params (not database) for compatibility
        var fo = new MineralParams
        {
            MineralName = "Forsterite", PaperName = "fo",
            NumAtoms = 7, MolarVolume = 43.6, MolarWeight = 140.69,
            KZero = 128.0, K1Prime = 4.2, GZero = 82.0, G1Prime = 1.5,
            DebyeTempZero = 809.0, GammaZero = 0.99, QZero = 2.1, EhtaZero = 2.3,
            RefTemp = 300.0,
        };
        var en = new MineralParams
        {
            MineralName = "Enstatite", PaperName = "en",
            NumAtoms = 10, MolarVolume = 62.68, MolarWeight = 200.78,
            KZero = 107.0, K1Prime = 7.0, GZero = 77.0, G1Prime = 1.5,
            DebyeTempZero = 812.0, GammaZero = 0.78, QZero = 3.4, EhtaZero = 1.6,
            RefTemp = 300.0,
        };

        var rock = new RockComposition
        {
            Name = "Test Rock",
            Minerals = new List<RockMineralEntry>
            {
                new RockMineralEntry { Mineral = fo, VolumeRatio = 0.7 },
                new RockMineralEntry { Mineral = en, VolumeRatio = 0.3 },
            }
        };

        var calc = new RockCalculator(rock, 5.0, 1000.0, MixtureMethod.Hill);
        var (mixedResult, individualResults) = calc.Calculate();

        Assert.Equal(2, individualResults.Count);
        Assert.NotNull(mixedResult);
        Assert.True(mixedResult!.Vp > 0);
    }

    [Fact]
    public void EquilibriumAggregate_AlongProfile_ProducesResults()
    {
        var fo = MineralDatabase.GetByName("fo")!;

        var assemblage = new PhaseAssemblage
        {
            Phases = new List<PhaseEntry>
            {
                new PhaseEntry { Name = "Forsterite", Mineral = fo, Amount = 1.0 }
            }
        };

        var profile = new PTProfile
        {
            Name = "Test",
            Profile = new List<PTData>
            {
                new PTData { Pressure = 5.0, Temperature = 1500.0 },
                new PTData { Pressure = 10.0, Temperature = 1600.0 },
            }
        };

        var calc = new EquilibriumAggregateCalculator();
        var results = calc.Calculate(assemblage, profile, MixtureMethod.Hill);

        Assert.Equal(2, results.Count);
        Assert.NotNull(results[0].mixedResult);
        Assert.NotNull(results[1].mixedResult);
    }
}
