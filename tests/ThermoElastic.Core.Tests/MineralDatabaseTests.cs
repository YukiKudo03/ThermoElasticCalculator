using Xunit;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Core.Tests;

public class MineralDatabaseTests
{
    [Fact]
    public void GetAll_ReturnsExpectedCount()
    {
        var all = MineralDatabase.GetAll();
        Assert.True(all.Count >= 40);
    }

    [Fact]
    public void GetByName_Forsterite_HasCorrectV0()
    {
        var fo = MineralDatabase.GetByName("fo");
        Assert.NotNull(fo);
        Assert.Equal(43.603, fo!.MolarVolume, 3);
    }

    [Fact]
    public void GetByName_CaseInsensitive()
    {
        var fo1 = MineralDatabase.GetByName("Forsterite");
        var fo2 = MineralDatabase.GetByName("forsterite");
        Assert.NotNull(fo1);
        Assert.NotNull(fo2);
        Assert.Equal(fo1!.MineralName, fo2!.MineralName);
    }

    [Fact]
    public void Search_FindsMultipleResults()
    {
        var results = MineralDatabase.Search("Perovskite");
        Assert.True(results.Count >= 3);
    }

    [Fact]
    public void GetByName_Quartz_HasLandauParams()
    {
        var qtz = MineralDatabase.GetByName("qtz");
        Assert.NotNull(qtz);
        Assert.Equal(847.0, qtz!.Tc0);
        Assert.True(qtz.VD > 0);
        Assert.True(qtz.SD > 0);
    }

    [Fact]
    public void ForsteriteParams_JsonRoundTrip()
    {
        var fo = MineralDatabase.GetByName("fo");
        Assert.NotNull(fo);
        var json = fo!.ExportJson();
        Assert.True(ThermoElastic.Core.Models.MineralParams.ImportJson(json, out var imported));
        Assert.NotNull(imported);
        Assert.Equal(fo.MolarVolume, imported!.MolarVolume);
        Assert.Equal(fo.F0, imported.F0);
    }
}
