using Xunit;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class PTProfileCalculatorTests
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

    [Fact]
    public void DoProfileCalculation_SinglePoint_ReturnsSingleResult()
    {
        var mineral = CreateForsterite();
        var profile = new PTProfile
        {
            Name = "Test",
            Profile = new List<PTData> { new() { Pressure = 5.0, Temperature = 1000.0 } }
        };
        var calc = new PTProfileCalculator(mineral, profile);
        var results = calc.DoProfileCalculation();

        Assert.Single(results);
        Assert.True(results[0].Density > 3.0);
    }

    [Fact]
    public void DoProfileCalculation_MultiplePoints_ReturnsCorrectCount()
    {
        var mineral = CreateForsterite();
        var profile = new PTProfile
        {
            Name = "Test",
            Profile = new List<PTData>
            {
                new() { Pressure = 1.0, Temperature = 500.0 },
                new() { Pressure = 5.0, Temperature = 1000.0 },
                new() { Pressure = 10.0, Temperature = 1500.0 },
            }
        };
        var calc = new PTProfileCalculator(mineral, profile);
        var results = calc.DoProfileCalculation();

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void DoProfileCalculationAsSummary_ReturnsSummaries()
    {
        var mineral = CreateForsterite();
        var profile = new PTProfile
        {
            Name = "Test",
            Profile = new List<PTData>
            {
                new() { Pressure = 5.0, Temperature = 1000.0 },
                new() { Pressure = 10.0, Temperature = 1500.0 },
            }
        };
        var calc = new PTProfileCalculator(mineral, profile);
        var results = calc.DoProfileCalculationAsSummary();

        Assert.Equal(2, results.Count);
        Assert.True(results[0].KS > 0);
        Assert.True(results[1].KS > 0);
    }

    [Fact]
    public void DoProfileCalculationAsCSV_IncludesHeader()
    {
        var mineral = CreateForsterite();
        var profile = new PTProfile
        {
            Name = "Test",
            Profile = new List<PTData> { new() { Pressure = 5.0, Temperature = 1000.0 } }
        };
        var calc = new PTProfileCalculator(mineral, profile);
        var csv = calc.DoProfileCalculationAsCSV();

        Assert.Equal(2, csv.Count);
        Assert.Contains("P[GPa]", csv[0]);
    }

    [Fact]
    public void HigherPressure_IncreasesModuli()
    {
        var mineral = CreateForsterite();
        var profile = new PTProfile
        {
            Name = "Test",
            Profile = new List<PTData>
            {
                new() { Pressure = 1.0, Temperature = 300.0 },
                new() { Pressure = 20.0, Temperature = 300.0 },
            }
        };
        var calc = new PTProfileCalculator(mineral, profile);
        var results = calc.DoProfileCalculationAsSummary();

        Assert.True(results[1].KS > results[0].KS);
        Assert.True(results[1].Density > results[0].Density);
    }
}
