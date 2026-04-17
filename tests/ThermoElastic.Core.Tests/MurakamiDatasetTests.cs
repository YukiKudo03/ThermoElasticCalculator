using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Integration tests that fit the SLB Parameter Fitter to the transcribed
/// Murakami lab datasets in samples/. These are real experimental Vs
/// measurements, so fitted parameters must fall within physically reasonable
/// ranges for the respective minerals.
/// </summary>
public class MurakamiDatasetTests
{
    private readonly ITestOutputHelper _output;

    public MurakamiDatasetTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Fit_Murakami2012_Bridgmanite_RecoversPhysicalG0()
    {
        var dataset = LoadSampleCsv("murakami2012_bridgmanite.csv");
        Assert.Equal(20, dataset.Data.Count);
        Assert.All(dataset.Data, d => Assert.True(d.Vs.HasValue, "Every Murakami 2012 point should have Vs."));

        // Initial guess: SLB2011 Mg-perovskite (which encodes Murakami's own measurements).
        // Keep K0, K' fixed — with Vs-only data, they cannot be fit independently.
        var guess = SLB2011Endmembers.GetAll().First(m => m.PaperName == "mpv");
        guess.GZero *= 0.90;   // perturb 10% low so the fitter has real work
        guess.G1Prime *= 1.10; // perturb 10% high

        var fitter = new SLBParameterFitter();
        var result = fitter.Fit(dataset, guess, new FittingOptions
        {
            FitV0 = false, FitK0 = false, FitK0Prime = false,
            FitG0 = true, FitG0Prime = true,
            Target = FitTarget.VsOnly,
        });

        _output.WriteLine($"Converged: {result.Optimization.Converged} ({result.Optimization.Iterations} iter)");
        _output.WriteLine($"Chi² = {result.Optimization.ChiSquared:E3}");
        _output.WriteLine($"G0 fitted  = {result.FittedMineral.GZero:F2} GPa  (±{result.Optimization.Uncertainties[0]:F2})");
        _output.WriteLine($"G' fitted  = {result.FittedMineral.G1Prime:F3}      (±{result.Optimization.Uncertainties[1]:F3})");

        Assert.True(result.Optimization.Converged);

        // Murakami 2007 reports G0 = 172.9 ± 1.5 GPa, G' = 1.56 ± 0.04 for MgSiO3 Pv.
        // SLB2011 lists G0 = 172.90, G' = 1.6904 (same lineage).
        // A Vs-only fit to 2012 high-P data should recover G0 in a physically sensible range.
        Assert.InRange(result.FittedMineral.GZero, 150.0, 200.0);  // ±15% of literature
        Assert.InRange(result.FittedMineral.G1Prime, 1.0, 2.5);    // literature 1.56-1.69
    }

    [Fact]
    public void Fit_Murakami_Periclase_RecoversPhysicalG0()
    {
        var dataset = LoadSampleCsv("murakami_periclase.csv");
        Assert.Equal(17, dataset.Data.Count);
        Assert.All(dataset.Data, d => Assert.True(d.Vs.HasValue));

        // Pure MgO (periclase) from SLB2011 database.
        var guess = SLB2011Endmembers.GetAll().First(m => m.PaperName == "pe");
        guess.GZero *= 0.90;
        guess.G1Prime *= 1.10;

        var fitter = new SLBParameterFitter();
        var result = fitter.Fit(dataset, guess, new FittingOptions
        {
            FitV0 = false, FitK0 = false, FitK0Prime = false,
            FitG0 = true, FitG0Prime = true,
            Target = FitTarget.VsOnly,
        });

        _output.WriteLine($"Periclase G0 fitted = {result.FittedMineral.GZero:F2} GPa  (±{result.Optimization.Uncertainties[0]:F2})");
        _output.WriteLine($"Periclase G' fitted = {result.FittedMineral.G1Prime:F3}      (±{result.Optimization.Uncertainties[1]:F3})");

        Assert.True(result.Optimization.Converged);
        // Published MgO G0 ~ 130 GPa (Karki et al., Jackson et al., SLB2011).
        Assert.InRange(result.FittedMineral.GZero, 100.0, 160.0);
        Assert.InRange(result.FittedMineral.G1Prime, 1.5, 3.5);
    }

    private static ExperimentalDataset LoadSampleCsv(string fileName)
    {
        var repoRoot = FindRepoRoot();
        var path = Path.Combine(repoRoot, "samples", fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Sample CSV not found: {path}");
        return ExperimentalDataset.ParseCsv(File.ReadAllText(path), fileName);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ThermoElasticCalculator.sln")))
            dir = dir.Parent;
        if (dir == null)
            throw new DirectoryNotFoundException("Could not locate repo root (no ThermoElasticCalculator.sln found).");
        return dir.FullName;
    }
}
