using Xunit;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class MineralParamsTests
{
    private static MineralParams CreateForsterite()
    {
        return new MineralParams
        {
            MineralName = "Forsterite",
            PaperName = "fo",
            NumAtoms = 7,
            MolarVolume = 43.6,
            MolarWeight = 140.69,
            KZero = 128.0,
            K1Prime = 4.2,
            K2Prime = 0,
            GZero = 82.0,
            G1Prime = 1.5,
            G2Prime = 0,
            DebyeTempZero = 809.0,
            GammaZero = 0.99,
            QZero = 2.1,
            EhtaZero = 2.3,
            RefTemp = 300.0,
        };
    }

    [Fact]
    public void AveMolarWeight_ReturnsCorrectValue()
    {
        var mineral = CreateForsterite();
        Assert.Equal(140.69 / 7.0, mineral.AveMolarWeight, 5);
    }

    [Fact]
    public void JsonRoundTrip_PreservesAllProperties()
    {
        var original = CreateForsterite();
        var json = original.ExportJson();
        Assert.True(MineralParams.ImportJson(json, out var loaded));
        Assert.NotNull(loaded);
        Assert.Equal(original.MineralName, loaded!.MineralName);
        Assert.Equal(original.KZero, loaded.KZero);
        Assert.Equal(original.GammaZero, loaded.GammaZero);
    }

    [Fact]
    public void CsvRoundTrip_PreservesAllProperties()
    {
        var original = CreateForsterite();
        var csvRow = original.ExportCsvRow();
        Assert.True(MineralParams.ImportCsvRow(csvRow, out var loaded));
        Assert.NotNull(loaded);
        Assert.Equal(original.MineralName, loaded!.MineralName);
        Assert.Equal(original.NumAtoms, loaded.NumAtoms);
        Assert.Equal(original.KZero, loaded.KZero);
    }

    [Fact]
    public void GetPressure_AtZeroFinite_ReturnsZero()
    {
        var mineral = CreateForsterite();
        Assert.Equal(0.0, mineral.GetPressure(0.0), 10);
    }

    [Fact]
    public void FiniteToVolume_AtZeroFinite_ReturnsMolarVolume()
    {
        var mineral = CreateForsterite();
        Assert.Equal(mineral.MolarVolume, mineral.FiniteToVolume(0.0), 10);
    }
}
