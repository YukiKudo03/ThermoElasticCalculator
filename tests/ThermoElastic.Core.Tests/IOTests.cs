using Xunit;
using ThermoElastic.Core.IO;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class IOTests
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
    public void JsonFileIO_RoundTrip_PreservesData()
    {
        var mineral = CreateForsterite();
        var tempFile = Path.GetTempFileName();
        try
        {
            JsonFileIO.Save(tempFile, mineral);
            var loaded = JsonFileIO.Load<MineralParams>(tempFile);

            Assert.NotNull(loaded);
            Assert.Equal(mineral.MineralName, loaded!.MineralName);
            Assert.Equal(mineral.KZero, loaded.KZero);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void MineralCsvIO_ExportAndImport_RoundTrips()
    {
        var minerals = new List<MineralParams> { CreateForsterite() };
        var tempFile = Path.GetTempFileName() + ".csv";
        try
        {
            MineralCsvIO.Export(tempFile, minerals);
            var loaded = MineralCsvIO.Import(tempFile);

            Assert.Single(loaded);
            Assert.Equal("Forsterite", loaded[0].MineralName);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void MineralCsvIO_ExportResults_WritesCorrectly()
    {
        var results = new List<ResultSummary>
        {
            new() { Name = "Test", GivenP = 5.0, GivenT = 1000.0, KS = 150, GS = 80, Volume = 42, Density = 3.5 }
        };
        var tempFile = Path.GetTempFileName() + ".csv";
        try
        {
            MineralCsvIO.ExportResults(tempFile, results);
            var lines = File.ReadAllLines(tempFile);

            Assert.Equal(2, lines.Length);
            Assert.Contains("P[GPa]", lines[0]);
            Assert.Contains("5", lines[1]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void PTProfile_JsonRoundTrip()
    {
        var profile = new PTProfile
        {
            Name = "TestProfile",
            Profile = new List<PTData>
            {
                new() { Pressure = 5.0, Temperature = 1000.0 },
                new() { Pressure = 10.0, Temperature = 1500.0 },
            }
        };

        var json = profile.ExportJson();
        Assert.True(PTProfile.ImportJson(json, out var loaded));
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded!.Profile.Count);
        Assert.Equal("TestProfile", loaded.Name);
    }

    [Fact]
    public void RockComposition_JsonRoundTrip()
    {
        var rock = new RockComposition
        {
            Name = "TestRock",
            Minerals = new List<RockMineralEntry>
            {
                new() { Mineral = CreateForsterite(), VolumeRatio = 0.7 },
            }
        };

        var json = rock.ExportJson();
        Assert.True(RockComposition.ImportJson(json, out var loaded));
        Assert.NotNull(loaded);
        Assert.Equal("TestRock", loaded!.Name);
        Assert.Single(loaded.Minerals);
        Assert.Equal(0.7, loaded.Minerals[0].VolumeRatio);
    }

    [Fact]
    public void ResultSummary_ExportSummaryAsColumn_ContainsValues()
    {
        var result = new ResultSummary
        {
            GivenP = 5.0, GivenT = 1000.0,
            KS = 150, KT = 145, GS = 80, Volume = 42, Density = 3.5,
        };

        var csv = result.ExportSummaryAsColumn();
        Assert.Contains("5", csv);
        Assert.Contains("1000", csv);
    }

    [Fact]
    public void ResultSummary_ExportSummaryAsJson_IsValidJson()
    {
        var result = new ResultSummary
        {
            Name = "Test", GivenP = 5.0, GivenT = 1000.0,
            KS = 150, GS = 80, Volume = 42, Density = 3.5,
        };

        var json = result.ExportSummaryAsJson();
        Assert.Contains("\"Name\"", json);
        Assert.Contains("\"GivenP\"", json);
    }
}
