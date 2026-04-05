using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

public class SLBParameterFitterTests
{
    private readonly ITestOutputHelper _output;

    public SLBParameterFitterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Round-trip test: generate synthetic data from known forsterite parameters,
    /// then verify the fitter recovers those parameters.
    /// </summary>
    [Fact]
    public void RoundTrip_Forsterite_RecoverKnownParameters()
    {
        // Known forsterite parameters
        var fo = SLB2011Endmembers.GetAll().First(m => m.PaperName == "fo");

        // Generate synthetic data at 10 P-T points
        var data = new ExperimentalDataset { Name = "Synthetic Forsterite", MineralName = "fo" };
        var pressures = new[] { 0.001, 5.0, 10.0, 15.0, 20.0, 25.0, 30.0, 35.0, 40.0, 50.0 };
        foreach (var p in pressures)
        {
            var eos = new MieGruneisenEOSOptimizer(fo, p, 300.0);
            var result = eos.ExecOptimize();
            data.Data.Add(new ExperimentalDataPoint
            {
                Pressure = p,
                Temperature = 300.0,
                Vp = result.Vp,
                Vs = result.Vs,
                Density = result.Density,
                SigmaVp = result.Vp * 0.001, // 0.1% uncertainty
                SigmaVs = result.Vs * 0.001,
                SigmaDensity = result.Density * 0.001,
            });
        }

        // Perturb initial guess (5% off on K0 and G0)
        var guess = SLB2011Endmembers.GetAll().First(m => m.PaperName == "fo");
        guess.KZero *= 1.05;
        guess.GZero *= 0.95;

        var fitter = new SLBParameterFitter();
        var options = new FittingOptions
        {
            FitV0 = false,      // fix V0 (well constrained by density)
            FitK0 = true,
            FitK0Prime = false, // fix K' (reduces parameter space)
            FitG0 = true,
            FitG0Prime = false,
            Target = FitTarget.Joint,
        };

        var result2 = fitter.Fit(data, guess, options);

        _output.WriteLine($"Converged: {result2.Optimization.Converged}");
        _output.WriteLine($"Iterations: {result2.Optimization.Iterations}");
        _output.WriteLine($"Chi²: {result2.Optimization.ChiSquared:E3}");
        _output.WriteLine($"K0: true={fo.KZero:F2}, fitted={result2.FittedMineral.KZero:F2}, " +
                          $"err={result2.Optimization.Uncertainties[0]:F3}");
        _output.WriteLine($"G0: true={fo.GZero:F2}, fitted={result2.FittedMineral.GZero:F2}, " +
                          $"err={result2.Optimization.Uncertainties[1]:F3}");

        Assert.True(result2.Optimization.Converged, "Optimizer should converge on synthetic data");
        Assert.Equal(fo.KZero, result2.FittedMineral.KZero, 0.5); // within 0.5 GPa
        Assert.Equal(fo.GZero, result2.FittedMineral.GZero, 0.5);
    }

    [Fact]
    public void RoundTrip_VsOnly_RecoverG0()
    {
        var pe = SLB2011Endmembers.GetAll().First(m => m.PaperName == "pe");

        var data = new ExperimentalDataset { Name = "Periclase Vs", MineralName = "pe" };
        foreach (var p in new[] { 0.001, 10.0, 20.0, 30.0, 50.0, 80.0, 100.0 })
        {
            var eos = new MieGruneisenEOSOptimizer(pe, p, 300.0);
            var result = eos.ExecOptimize();
            data.Data.Add(new ExperimentalDataPoint
            {
                Pressure = p, Temperature = 300.0,
                Vs = result.Vs, SigmaVs = result.Vs * 0.005,
            });
        }

        var guess = SLB2011Endmembers.GetAll().First(m => m.PaperName == "pe");
        guess.GZero *= 0.90; // 10% off

        var fitter = new SLBParameterFitter();
        var result2 = fitter.Fit(data, guess, new FittingOptions
        {
            FitV0 = false, FitK0 = false, FitK0Prime = false,
            FitG0 = true, FitG0Prime = false,
            Target = FitTarget.VsOnly,
        });

        _output.WriteLine($"G0: true={pe.GZero:F2}, fitted={result2.FittedMineral.GZero:F2}");
        Assert.True(result2.Optimization.Converged);
        Assert.Equal(pe.GZero, result2.FittedMineral.GZero, 1.0);
    }

    [Fact]
    public void ParseCsv_ValidData()
    {
        var csv = @"P,T,Vp,Vs,Density
0.001,300,8500,4900,3.22
10,300,9200,5100,3.45
20,300,9800,5300,3.65";

        var dataset = ExperimentalDataset.ParseCsv(csv, "Test");
        Assert.Equal(3, dataset.Data.Count);
        Assert.Equal(0.001, dataset.Data[0].Pressure);
        Assert.Equal(8500, dataset.Data[0].Vp);
    }

    [Fact]
    public void ParseCsv_TabDelimited()
    {
        var csv = "P\tT\tVp\tVs\n0.001\t300\t8500\t4900\n10\t300\t9200\t5100";
        var dataset = ExperimentalDataset.ParseCsv(csv);
        Assert.Equal(2, dataset.Data.Count);
    }

    [Fact]
    public void ParseCsv_MissingColumns()
    {
        var csv = "P,T,Vp\n10,300,9200\n20,300,9800";
        var dataset = ExperimentalDataset.ParseCsv(csv);
        Assert.Equal(2, dataset.Data.Count);
        Assert.Null(dataset.Data[0].Vs);
        Assert.Null(dataset.Data[0].Density);
    }

    [Fact]
    public void ParseCsv_EmptyThrows()
    {
        Assert.Throws<FormatException>(() => ExperimentalDataset.ParseCsv(""));
    }

    [Fact]
    public void ParseCsv_NegativePressureThrows()
    {
        var csv = "P,T,Vp\n-5,300,8500";
        Assert.Throws<FormatException>(() => ExperimentalDataset.ParseCsv(csv));
    }

    [Fact]
    public void Fit_NoParamsSelected_Throws()
    {
        var data = new ExperimentalDataset
        {
            Data = { new ExperimentalDataPoint { Pressure = 10, Temperature = 300, Vp = 9000 } }
        };
        var mineral = SLB2011Endmembers.GetAll().First();
        var fitter = new SLBParameterFitter();

        var ex = Record.Exception(() =>
            fitter.Fit(data, mineral, new FittingOptions
            {
                FitV0 = false, FitK0 = false, FitK0Prime = false,
                FitG0 = false, FitG0Prime = false,
            }));
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void Fit_EmptyDataset_Throws()
    {
        var data = new ExperimentalDataset();
        var mineral = SLB2011Endmembers.GetAll().First();
        var fitter = new SLBParameterFitter();

        var ex = Record.Exception(() =>
            fitter.Fit(data, mineral, new FittingOptions()));
        Assert.IsType<ArgumentException>(ex);
    }
}
