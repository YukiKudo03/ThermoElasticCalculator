using Xunit;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class ChartDataTests
{
    [Fact]
    public void ResultSummary_CanExtractVpFromResults()
    {
        var results = new List<ResultSummary>
        {
            new() { Name = "test", GivenP = 0.0, GivenT = 300, KS = 130, GS = 80, Density = 3.2, Volume = 43.6 },
            new() { Name = "test", GivenP = 5.0, GivenT = 300, KS = 160, GS = 100, Density = 3.4, Volume = 41.0 },
            new() { Name = "test", GivenP = 10.0, GivenT = 300, KS = 190, GS = 120, Density = 3.6, Volume = 39.0 },
        };

        var pressures = results.Select(r => r.GivenP).ToArray();
        var vpValues = results.Select(r => r.Vp).ToArray();

        Assert.Equal(3, pressures.Length);
        Assert.Equal(3, vpValues.Length);
        Assert.True(vpValues[0] > 0);
        Assert.True(vpValues[2] > vpValues[0]); // Higher P → higher Vp
    }

    [Fact]
    public void ResultSummary_CanExtractMultipleProperties()
    {
        var r = new ResultSummary { KS = 130, GS = 80, Density = 3.2, Volume = 43.6, GivenP = 1.0, GivenT = 300 };

        Assert.True(r.Vp > 0);
        Assert.True(r.Vs > 0);
        Assert.True(r.Vb > 0);
        Assert.True(r.Vp > r.Vs);
        Assert.True(r.Vp > r.Vb);
    }

    [Fact]
    public void ResultSummary_TemperatureSeries_CanBeGenerated()
    {
        var results = Enumerable.Range(3, 10).Select(i => new ResultSummary
        {
            GivenP = 0.0001,
            GivenT = i * 100.0,
            KS = 130 - i * 2,
            GS = 80 - i,
            Density = 3.2 - i * 0.01,
            Volume = 43.6 + i * 0.1,
        }).ToList();

        var temps = results.Select(r => r.GivenT).ToArray();
        Assert.Equal(10, temps.Length);
        Assert.Equal(300.0, temps[0]);
    }
}
