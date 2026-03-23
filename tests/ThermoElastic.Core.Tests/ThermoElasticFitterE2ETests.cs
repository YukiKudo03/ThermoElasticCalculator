using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// End-to-end tests that validate the thermoelastic parameter fitting feature
/// by generating synthetic data from SLB2011 literature values and verifying
/// that the fitter recovers the original parameters within tolerance.
///
/// These tests serve as integration tests for the entire pipeline:
///   SLB2011 params → forward model (MieGruneisenEOS) → synthetic data → fitter → recovered params
/// </summary>
public class ThermoElasticFitterE2ETests
{
    private readonly ITestOutputHelper _output;

    public ThermoElasticFitterE2ETests(ITestOutputHelper output)
    {
        _output = output;
    }

    // Standard T,P conditions spanning upper mantle to transition zone
    private static (double T, double P)[] MantleConditions =>
        new (double T, double P)[]
        {
            // Room temperature high-P (lab experiments)
            (300, 1), (300, 5), (300, 10), (300, 15), (300, 20), (300, 30), (300, 40), (300, 50),
            // High-T moderate-P (mantle geotherm)
            (500, 5), (500, 10), (500, 20), (500, 30),
            (1000, 5), (1000, 10), (1000, 20), (1000, 30), (1000, 40),
            (1500, 10), (1500, 20), (1500, 30), (1500, 40), (1500, 50),
            (2000, 10), (2000, 20), (2000, 30), (2000, 40), (2000, 50),
        };

    /// <summary>
    /// Helper: generate synthetic Vp/Vs data from a mineral at given T,P conditions.
    /// </summary>
    private static List<FittingDataPoint> GenerateVelocityData(
        MineralParams mineral, (double T, double P)[] conditions, double noiseFrac, int seed)
    {
        var rng = new Random(seed);
        var data = new List<FittingDataPoint>();
        foreach (var (T, P) in conditions)
        {
            var eos = new MieGruneisenEOSOptimizer(mineral, P, T);
            var th = eos.ExecOptimize();
            if (!th.IsConverged) continue;
            data.Add(new FittingDataPoint
            {
                Temperature = T, Pressure = P,
                Vp = th.Vp + th.Vp * noiseFrac * (rng.NextDouble() * 2 - 1),
                Vs = th.Vs + th.Vs * noiseFrac * (rng.NextDouble() * 2 - 1),
                SigmaVp = th.Vp * noiseFrac,
                SigmaVs = th.Vs * noiseFrac,
            });
        }
        return data;
    }

    /// <summary>
    /// Helper: generate synthetic V(T,P) data from a mineral.
    /// </summary>
    private static List<FittingDataPoint> GenerateVolumeData(
        MineralParams mineral, (double T, double P)[] conditions, double noiseFrac, int seed)
    {
        var rng = new Random(seed);
        var data = new List<FittingDataPoint>();
        foreach (var (T, P) in conditions)
        {
            var eos = new MieGruneisenEOSOptimizer(mineral, P, T);
            var th = eos.ExecOptimize();
            if (!th.IsConverged) continue;
            data.Add(new FittingDataPoint
            {
                Temperature = T, Pressure = P,
                Volume = th.Volume + th.Volume * noiseFrac * (rng.NextDouble() * 2 - 1),
                SigmaVolume = th.Volume * noiseFrac,
            });
        }
        return data;
    }

    /// <summary>
    /// Helper: generate combined Vp+Vs+V data.
    /// </summary>
    private static List<FittingDataPoint> GenerateCombinedData(
        MineralParams mineral, (double T, double P)[] conditions,
        double velNoise, double volNoise, int seed)
    {
        var rng = new Random(seed);
        var data = new List<FittingDataPoint>();
        foreach (var (T, P) in conditions)
        {
            var eos = new MieGruneisenEOSOptimizer(mineral, P, T);
            var th = eos.ExecOptimize();
            if (!th.IsConverged) continue;
            data.Add(new FittingDataPoint
            {
                Temperature = T, Pressure = P,
                Vp = th.Vp + th.Vp * velNoise * (rng.NextDouble() * 2 - 1),
                Vs = th.Vs + th.Vs * velNoise * (rng.NextDouble() * 2 - 1),
                Volume = th.Volume + th.Volume * volNoise * (rng.NextDouble() * 2 - 1),
                SigmaVp = th.Vp * velNoise,
                SigmaVs = th.Vs * velNoise,
                SigmaVolume = th.Volume * volNoise,
            });
        }
        return data;
    }

    private void PrintResult(string label, string[] paramNames, double[] trueVals,
        OptimizationResult result)
    {
        _output.WriteLine($"\n=== {label} ===");
        _output.WriteLine($"Converged: {result.Converged}, Iterations: {result.Iterations}");
        _output.WriteLine($"χ² = {result.ChiSquared:F4}, χ²r = {result.ReducedChiSquared:F4}");
        for (int i = 0; i < paramNames.Length; i++)
        {
            double pctErr = Math.Abs(result.Parameters[i] - trueVals[i]) / trueVals[i] * 100;
            _output.WriteLine($"  {paramNames[i]}: true={trueVals[i]:G6}, fitted={result.Parameters[i]:G6} ± {result.Uncertainties[i]:G4} ({pctErr:F2}%)");
        }
    }

    // ================================================================
    // E2E Test 1: Forsterite (fo) — K0, G0 from Vp/Vs
    // SLB2011: K0=127.96 GPa, G0=81.60 GPa
    // ================================================================
    [Fact]
    public void E2E_Forsterite_VpVs_FitsK0G0()
    {
        var mineral = SLB2011Endmembers.GetAll().First(m => m.PaperName == "fo");
        var data = GenerateVelocityData(mineral, MantleConditions, 0.005, seed: 1001);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;
        // Perturb initial values by +10%
        config.BaseMineralParams.KZero *= 1.10;
        config.BaseMineralParams.GZero *= 1.10;

        var result = new ThermoElasticFitter().Fit(config, data);
        PrintResult("Forsterite Vp/Vs → K0, G0",
            new[] { "K0", "G0" },
            new[] { mineral.KZero, mineral.GZero },
            result);

        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - mineral.KZero) / mineral.KZero < 0.02,
            $"K0: {result.Parameters[0]:F2} vs lit. {mineral.KZero:F2}");
        Assert.True(Math.Abs(result.Parameters[1] - mineral.GZero) / mineral.GZero < 0.02,
            $"G0: {result.Parameters[1]:F2} vs lit. {mineral.GZero:F2}");
    }

    // ================================================================
    // E2E Test 2: Periclase (pe) — V0, K0, K' from V(T,P)
    // SLB2011: V0=11.244 cm³/mol, K0=160.20 GPa, K'=3.9900
    // ================================================================
    [Fact]
    public void E2E_Periclase_Volume_FitsV0K0Kprime()
    {
        var mineral = SLB2011Endmembers.GetAll().First(m => m.PaperName == "pe");
        var data = GenerateVolumeData(mineral, MantleConditions, 0.001, seed: 2002);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexV0] = true;
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexK1Prime] = true;
        config.BaseMineralParams.MolarVolume *= 1.03;
        config.BaseMineralParams.KZero *= 0.95;
        config.BaseMineralParams.K1Prime *= 1.05;

        var result = new ThermoElasticFitter().Fit(config, data);
        PrintResult("Periclase V(T,P) → V0, K0, K'",
            new[] { "V0", "K0", "K'" },
            new[] { mineral.MolarVolume, mineral.KZero, mineral.K1Prime },
            result);

        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - mineral.MolarVolume) / mineral.MolarVolume < 0.02,
            $"V0: {result.Parameters[0]:F3} vs lit. {mineral.MolarVolume:F3}");
        Assert.True(Math.Abs(result.Parameters[1] - mineral.KZero) / mineral.KZero < 0.03,
            $"K0: {result.Parameters[1]:F2} vs lit. {mineral.KZero:F2}");
        Assert.True(Math.Abs(result.Parameters[2] - mineral.K1Prime) / mineral.K1Prime < 0.05,
            $"K': {result.Parameters[2]:F3} vs lit. {mineral.K1Prime:F3}");
    }

    // ================================================================
    // E2E Test 3: Stishovite (st) — K0, G0, G' from Vp/Vs
    // SLB2011: K0=314.00 GPa, G0=220.00 GPa, G'=1.5710
    // ================================================================
    [Fact]
    public void E2E_Stishovite_VpVs_FitsK0G0Gprime()
    {
        var mineral = SLB2011Endmembers.GetAll().First(m => m.PaperName == "st");
        var data = GenerateVelocityData(mineral, MantleConditions, 0.003, seed: 3003);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;
        config.FitFlags[FittingConfig.IndexG1Prime] = true;
        config.BaseMineralParams.KZero *= 1.08;
        config.BaseMineralParams.GZero *= 0.92;
        config.BaseMineralParams.G1Prime *= 1.10;

        var result = new ThermoElasticFitter().Fit(config, data);
        PrintResult("Stishovite Vp/Vs → K0, G0, G'",
            new[] { "K0", "G0", "G'" },
            new[] { mineral.KZero, mineral.GZero, mineral.G1Prime },
            result);

        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - mineral.KZero) / mineral.KZero < 0.02);
        Assert.True(Math.Abs(result.Parameters[1] - mineral.GZero) / mineral.GZero < 0.02);
        Assert.True(Math.Abs(result.Parameters[2] - mineral.G1Prime) / mineral.G1Prime < 0.05);
    }

    // ================================================================
    // E2E Test 4: Forsterite — Combined Vp+Vs+V → K0, G0, θ0
    // Full data → simultaneous elastic + volumetric fitting
    // ================================================================
    [Fact]
    public void E2E_Forsterite_Combined_FitsK0G0DebyeTemp()
    {
        var mineral = SLB2011Endmembers.GetAll().First(m => m.PaperName == "fo");
        var data = GenerateCombinedData(mineral, MantleConditions, 0.005, 0.001, seed: 4004);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;
        config.FitFlags[FittingConfig.IndexDebyeTemp] = true;
        config.BaseMineralParams.KZero *= 1.08;
        config.BaseMineralParams.GZero *= 0.92;
        config.BaseMineralParams.DebyeTempZero *= 1.05;

        var result = new ThermoElasticFitter().Fit(config, data);
        PrintResult("Forsterite Combined → K0, G0, θ0",
            new[] { "K0", "G0", "θ0" },
            new[] { mineral.KZero, mineral.GZero, mineral.DebyeTempZero },
            result);

        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - mineral.KZero) / mineral.KZero < 0.03);
        Assert.True(Math.Abs(result.Parameters[1] - mineral.GZero) / mineral.GZero < 0.03);
        Assert.True(Math.Abs(result.Parameters[2] - mineral.DebyeTempZero) / mineral.DebyeTempZero < 0.05);
    }

    // ================================================================
    // E2E Test 5: Mg-Bridgmanite (mgpv) — deep mantle mineral
    // SLB2011: K0=250.50 GPa, G0=172.90 GPa
    // ================================================================
    [Fact]
    public void E2E_MgBridgmanite_VpVs_FitsK0G0()
    {
        var mineral = SLB2011Endmembers.GetAll().First(m => m.PaperName == "mpv");

        // Deep mantle conditions for bridgmanite
        var conditions = new (double T, double P)[]
        {
            (300, 25), (300, 40), (300, 60), (300, 80), (300, 100), (300, 120),
            (1500, 25), (1500, 40), (1500, 60), (1500, 80), (1500, 100),
            (2000, 30), (2000, 50), (2000, 70), (2000, 90), (2000, 110),
            (2500, 40), (2500, 60), (2500, 80), (2500, 100), (2500, 120),
        };

        var data = GenerateVelocityData(mineral, conditions, 0.005, seed: 5005);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;
        config.BaseMineralParams.KZero *= 1.10;
        config.BaseMineralParams.GZero *= 0.90;

        var result = new ThermoElasticFitter().Fit(config, data);
        PrintResult("Mg-Bridgmanite Vp/Vs → K0, G0",
            new[] { "K0", "G0" },
            new[] { mineral.KZero, mineral.GZero },
            result);

        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - mineral.KZero) / mineral.KZero < 0.03,
            $"K0: {result.Parameters[0]:F2} vs lit. {mineral.KZero:F2}");
        Assert.True(Math.Abs(result.Parameters[1] - mineral.GZero) / mineral.GZero < 0.03,
            $"G0: {result.Parameters[1]:F2} vs lit. {mineral.GZero:F2}");
    }

    // ================================================================
    // E2E Test 6: Ringwoodite (mgri) — transition zone mineral
    // SLB2011: V0=39.49, K0=184.22, K'=4.222, G0=122.60, G'=1.3928
    // Five-parameter simultaneous fit from combined data
    // ================================================================
    [Fact]
    public void E2E_Ringwoodite_Combined_FitsMultipleParams()
    {
        var mineral = SLB2011Endmembers.GetAll().First(m => m.PaperName == "mrw");

        var conditions = new (double T, double P)[]
        {
            (300, 5), (300, 10), (300, 15), (300, 20), (300, 25), (300, 30),
            (800, 5), (800, 10), (800, 15), (800, 20), (800, 25),
            (1200, 10), (1200, 15), (1200, 20), (1200, 25),
            (1600, 10), (1600, 15), (1600, 20), (1600, 25),
            (2000, 10), (2000, 15), (2000, 20),
        };

        var data = GenerateCombinedData(mineral, conditions, 0.003, 0.001, seed: 6006);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexV0] = true;
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;
        config.BaseMineralParams.MolarVolume *= 1.02;
        config.BaseMineralParams.KZero *= 1.05;
        config.BaseMineralParams.GZero *= 0.95;

        var result = new ThermoElasticFitter().Fit(config, data);
        PrintResult("Ringwoodite Combined → V0, K0, G0",
            new[] { "V0", "K0", "G0" },
            new[] { mineral.MolarVolume, mineral.KZero, mineral.GZero },
            result);

        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - mineral.MolarVolume) / mineral.MolarVolume < 0.02);
        Assert.True(Math.Abs(result.Parameters[1] - mineral.KZero) / mineral.KZero < 0.03);
        Assert.True(Math.Abs(result.Parameters[2] - mineral.GZero) / mineral.GZero < 0.03);
    }
}
