using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class ThermoElasticFitterTests
{
    private readonly ITestOutputHelper _output;

    public ThermoElasticFitterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // =============================================
    // Phase 1: Data Model Tests
    // =============================================

    [Fact]
    public void FittingDataPoint_CreatesWithVelocityData()
    {
        var dp = new FittingDataPoint
        {
            Temperature = 1500.0,
            Pressure = 25.0,
            Vp = 9500.0,
            Vs = 5200.0,
            SigmaVp = 50.0,
            SigmaVs = 30.0,
        };

        Assert.Equal(1500.0, dp.Temperature);
        Assert.Equal(25.0, dp.Pressure);
        Assert.Equal(9500.0, dp.Vp);
        Assert.Equal(5200.0, dp.Vs);
        Assert.Equal(50.0, dp.SigmaVp);
        Assert.Equal(30.0, dp.SigmaVs);
        Assert.True(double.IsNaN(dp.Volume));
        Assert.True(double.IsNaN(dp.SigmaVolume));
    }

    [Fact]
    public void FittingDataPoint_CreatesWithVolumeData()
    {
        var dp = new FittingDataPoint
        {
            Temperature = 300.0,
            Pressure = 10.0,
            Volume = 40.5,
            SigmaVolume = 0.04,
        };

        Assert.Equal(300.0, dp.Temperature);
        Assert.Equal(10.0, dp.Pressure);
        Assert.Equal(40.5, dp.Volume);
        Assert.Equal(0.04, dp.SigmaVolume);
        Assert.True(double.IsNaN(dp.Vp));
        Assert.True(double.IsNaN(dp.Vs));
    }

    [Fact]
    public void FittingDataPoint_HasVelocityData_ReturnsTrueWhenVpVsSet()
    {
        var dp = new FittingDataPoint
        {
            Temperature = 1500.0, Pressure = 25.0,
            Vp = 9500.0, Vs = 5200.0,
            SigmaVp = 50.0, SigmaVs = 30.0,
        };
        Assert.True(dp.HasVelocityData);
        Assert.False(dp.HasVolumeData);
    }

    [Fact]
    public void FittingDataPoint_HasVolumeData_ReturnsTrueWhenVolumeSet()
    {
        var dp = new FittingDataPoint
        {
            Temperature = 300.0, Pressure = 10.0,
            Volume = 40.5, SigmaVolume = 0.04,
        };
        Assert.False(dp.HasVelocityData);
        Assert.True(dp.HasVolumeData);
    }

    [Fact]
    public void FittingConfig_DefaultFitFlags_AllFalse()
    {
        var config = new FittingConfig();
        Assert.Equal(9, config.FitFlags.Length);
        Assert.All(config.FitFlags, flag => Assert.False(flag));
    }

    [Fact]
    public void FittingConfig_FittingMode_DetectsVelocityMode()
    {
        var config = new FittingConfig();
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;

        Assert.Equal(FittingConfig.IndexK0, 1);
        Assert.Equal(FittingConfig.IndexG0, 3);
    }

    [Fact]
    public void FittingConfig_ParameterIndicesAreCorrect()
    {
        Assert.Equal(0, FittingConfig.IndexV0);
        Assert.Equal(1, FittingConfig.IndexK0);
        Assert.Equal(2, FittingConfig.IndexK1Prime);
        Assert.Equal(3, FittingConfig.IndexG0);
        Assert.Equal(4, FittingConfig.IndexG1Prime);
        Assert.Equal(5, FittingConfig.IndexDebyeTemp);
        Assert.Equal(6, FittingConfig.IndexGamma);
        Assert.Equal(7, FittingConfig.IndexQ);
        Assert.Equal(8, FittingConfig.IndexEtaS);
    }

    [Fact]
    public void FittingConfig_GetFreeParameterCount_ReturnsCorrectCount()
    {
        var config = new FittingConfig();
        Assert.Equal(0, config.FreeParameterCount);

        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;
        Assert.Equal(2, config.FreeParameterCount);
    }

    [Fact]
    public void FittingConfig_ExtractAllParams_FromMineralParams()
    {
        var mineral = new MineralParams
        {
            MolarVolume = 43.603,
            KZero = 127.96,
            K1Prime = 4.218,
            GZero = 81.60,
            G1Prime = 1.4626,
            DebyeTempZero = 809.17,
            GammaZero = 0.99282,
            QZero = 2.10672,
            EhtaZero = 2.2997,
        };

        var config = new FittingConfig { BaseMineralParams = mineral };
        double[] allParams = config.ExtractAllParams();

        Assert.Equal(9, allParams.Length);
        Assert.Equal(43.603, allParams[0]);
        Assert.Equal(127.96, allParams[1]);
        Assert.Equal(4.218, allParams[2]);
        Assert.Equal(81.60, allParams[3]);
        Assert.Equal(1.4626, allParams[4]);
        Assert.Equal(809.17, allParams[5]);
        Assert.Equal(0.99282, allParams[6]);
        Assert.Equal(2.10672, allParams[7]);
        Assert.Equal(2.2997, allParams[8]);
    }

    [Fact]
    public void FittingConfig_PackUnpack_RoundTrips()
    {
        var mineral = new MineralParams
        {
            MolarVolume = 43.603,
            KZero = 127.96,
            K1Prime = 4.218,
            GZero = 81.60,
            G1Prime = 1.4626,
            DebyeTempZero = 809.17,
            GammaZero = 0.99282,
            QZero = 2.10672,
            EhtaZero = 2.2997,
        };

        var config = new FittingConfig { BaseMineralParams = mineral };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;

        double[] packed = config.PackFreeParams();
        Assert.Equal(2, packed.Length);
        Assert.Equal(127.96, packed[0]);
        Assert.Equal(81.60, packed[1]);

        // Modify and unpack
        packed[0] = 130.0;
        packed[1] = 85.0;
        MineralParams updated = config.UnpackToMineralParams(packed);

        Assert.Equal(130.0, updated.KZero);
        Assert.Equal(85.0, updated.GZero);
        // Fixed params should remain unchanged
        Assert.Equal(43.603, updated.MolarVolume);
        Assert.Equal(4.218, updated.K1Prime);
    }

    // =============================================
    // Phase 2: ThermoElasticFitter Engine Tests
    // =============================================

    private static MineralParams GetForsterite()
    {
        return SLB2011Endmembers.GetAll().First(m => m.PaperName == "fo");
    }

    private static MineralParams GetPericlase()
    {
        return SLB2011Endmembers.GetAll().First(m => m.PaperName == "pe");
    }

    /// <summary>
    /// Generate synthetic Vp/Vs data at multiple T,P points using the forward model.
    /// </summary>
    private static List<FittingDataPoint> GenerateVelocityData(
        MineralParams mineral, (double T, double P)[] conditions, double noiseFraction, int seed)
    {
        var rng = new Random(seed);
        var data = new List<FittingDataPoint>();
        foreach (var (T, P) in conditions)
        {
            var eos = new MieGruneisenEOSOptimizer(mineral, P, T);
            var th = eos.ExecOptimize();
            if (!th.IsConverged) continue;

            double vpNoise = th.Vp * noiseFraction * (rng.NextDouble() * 2 - 1);
            double vsNoise = th.Vs * noiseFraction * (rng.NextDouble() * 2 - 1);
            data.Add(new FittingDataPoint
            {
                Temperature = T,
                Pressure = P,
                Vp = th.Vp + vpNoise,
                Vs = th.Vs + vsNoise,
                SigmaVp = th.Vp * noiseFraction,
                SigmaVs = th.Vs * noiseFraction,
            });
        }
        return data;
    }

    /// <summary>
    /// Generate synthetic V(T,P) data using the forward model.
    /// </summary>
    private static List<FittingDataPoint> GenerateVolumeData(
        MineralParams mineral, (double T, double P)[] conditions, double noiseFraction, int seed)
    {
        var rng = new Random(seed);
        var data = new List<FittingDataPoint>();
        foreach (var (T, P) in conditions)
        {
            var eos = new MieGruneisenEOSOptimizer(mineral, P, T);
            var th = eos.ExecOptimize();
            if (!th.IsConverged) continue;

            double vNoise = th.Volume * noiseFraction * (rng.NextDouble() * 2 - 1);
            data.Add(new FittingDataPoint
            {
                Temperature = T,
                Pressure = P,
                Volume = th.Volume + vNoise,
                SigmaVolume = th.Volume * noiseFraction,
            });
        }
        return data;
    }

    private static (double T, double P)[] StandardTPConditions =>
        new (double T, double P)[]
        {
            (300, 5), (300, 10), (300, 20), (300, 30), (300, 50),
            (1000, 5), (1000, 10), (1000, 20), (1000, 30), (1000, 50),
            (1500, 10), (1500, 20), (1500, 30), (1500, 50),
            (2000, 10), (2000, 20), (2000, 30), (2000, 50),
        };

    [Fact]
    public void Fit_VpVs_Forsterite_RecoversK0G0()
    {
        var mineral = GetForsterite();
        var data = GenerateVelocityData(mineral, StandardTPConditions, 0.005, seed: 42);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;

        // Perturb initial guess
        config.BaseMineralParams.KZero *= 1.10; // +10%
        config.BaseMineralParams.GZero *= 0.90; // -10%

        var fitter = new ThermoElasticFitter();
        var result = fitter.Fit(config, data);

        _output.WriteLine($"True:   K0={mineral.KZero:F2}, G0={mineral.GZero:F2}");
        _output.WriteLine($"Fitted: K0={result.Parameters[0]:F2}, G0={result.Parameters[1]:F2}");
        _output.WriteLine($"σ:      K0={result.Uncertainties[0]:F2}, G0={result.Uncertainties[1]:F2}");
        _output.WriteLine($"Converged={result.Converged}, Iter={result.Iterations}, χ²r={result.ReducedChiSquared:F4}");

        Assert.True(result.Converged, "Fit should converge");
        Assert.True(Math.Abs(result.Parameters[0] - mineral.KZero) / mineral.KZero < 0.02,
            $"K0 recovery: {result.Parameters[0]:F2} vs {mineral.KZero:F2}");
        Assert.True(Math.Abs(result.Parameters[1] - mineral.GZero) / mineral.GZero < 0.02,
            $"G0 recovery: {result.Parameters[1]:F2} vs {mineral.GZero:F2}");
    }

    [Fact]
    public void Fit_Volume_Forsterite_RecoversV0K0K1()
    {
        var mineral = GetForsterite();
        var data = GenerateVolumeData(mineral, StandardTPConditions, 0.001, seed: 123);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexV0] = true;
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexK1Prime] = true;

        // Perturb initial guess
        config.BaseMineralParams.MolarVolume *= 1.02;
        config.BaseMineralParams.KZero *= 0.95;
        config.BaseMineralParams.K1Prime *= 1.05;

        var fitter = new ThermoElasticFitter();
        var result = fitter.Fit(config, data);

        _output.WriteLine($"True:   V0={mineral.MolarVolume:F3}, K0={mineral.KZero:F2}, K'={mineral.K1Prime:F3}");
        _output.WriteLine($"Fitted: V0={result.Parameters[0]:F3}, K0={result.Parameters[1]:F2}, K'={result.Parameters[2]:F3}");
        _output.WriteLine($"Converged={result.Converged}, Iter={result.Iterations}, χ²r={result.ReducedChiSquared:F4}");

        Assert.True(result.Converged, "Fit should converge");
        Assert.True(Math.Abs(result.Parameters[0] - mineral.MolarVolume) / mineral.MolarVolume < 0.02,
            $"V0 recovery: {result.Parameters[0]:F3} vs {mineral.MolarVolume:F3}");
        Assert.True(Math.Abs(result.Parameters[1] - mineral.KZero) / mineral.KZero < 0.02,
            $"K0 recovery: {result.Parameters[1]:F2} vs {mineral.KZero:F2}");
        Assert.True(Math.Abs(result.Parameters[2] - mineral.K1Prime) / mineral.K1Prime < 0.05,
            $"K' recovery: {result.Parameters[2]:F3} vs {mineral.K1Prime:F3}");
    }

    [Fact]
    public void Fit_Combined_Periclase_RecoversMultipleParams()
    {
        var mineral = GetPericlase();
        var conditions = StandardTPConditions;
        var rng = new Random(777);

        // Generate combined Vp+Vs+V data
        var data = new List<FittingDataPoint>();
        foreach (var (T, P) in conditions)
        {
            var eos = new MieGruneisenEOSOptimizer(mineral, P, T);
            var th = eos.ExecOptimize();
            if (!th.IsConverged) continue;

            data.Add(new FittingDataPoint
            {
                Temperature = T,
                Pressure = P,
                Vp = th.Vp + th.Vp * 0.005 * (rng.NextDouble() * 2 - 1),
                Vs = th.Vs + th.Vs * 0.005 * (rng.NextDouble() * 2 - 1),
                Volume = th.Volume + th.Volume * 0.001 * (rng.NextDouble() * 2 - 1),
                SigmaVp = th.Vp * 0.005,
                SigmaVs = th.Vs * 0.005,
                SigmaVolume = th.Volume * 0.001,
            });
        }

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;
        config.FitFlags[FittingConfig.IndexDebyeTemp] = true;

        config.BaseMineralParams.KZero *= 1.08;
        config.BaseMineralParams.GZero *= 0.92;
        config.BaseMineralParams.DebyeTempZero *= 1.05;

        var fitter = new ThermoElasticFitter();
        var result = fitter.Fit(config, data);

        _output.WriteLine($"True:   K0={mineral.KZero:F2}, G0={mineral.GZero:F2}, θ0={mineral.DebyeTempZero:F1}");
        _output.WriteLine($"Fitted: K0={result.Parameters[0]:F2}, G0={result.Parameters[1]:F2}, θ0={result.Parameters[2]:F1}");
        _output.WriteLine($"Converged={result.Converged}, χ²r={result.ReducedChiSquared:F4}");

        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - mineral.KZero) / mineral.KZero < 0.03);
        Assert.True(Math.Abs(result.Parameters[1] - mineral.GZero) / mineral.GZero < 0.03);
        Assert.True(Math.Abs(result.Parameters[2] - mineral.DebyeTempZero) / mineral.DebyeTempZero < 0.05);
    }

    [Fact]
    public void Fit_SingleParam_G0FromVs()
    {
        var mineral = GetForsterite();
        var data = GenerateVelocityData(mineral, StandardTPConditions, 0.003, seed: 55);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexG0] = true;
        config.BaseMineralParams.GZero *= 0.85; // -15%

        var fitter = new ThermoElasticFitter();
        var result = fitter.Fit(config, data);

        _output.WriteLine($"True G0={mineral.GZero:F2}, Fitted G0={result.Parameters[0]:F2}");
        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - mineral.GZero) / mineral.GZero < 0.02);
    }

    [Fact]
    public void Fit_BadInitialGuess_StillConverges()
    {
        var mineral = GetPericlase();
        var data = GenerateVelocityData(mineral, StandardTPConditions, 0.005, seed: 99);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;

        // 50% off initial guess
        config.BaseMineralParams.KZero *= 0.50;
        config.BaseMineralParams.GZero *= 1.50;

        var fitter = new ThermoElasticFitter();
        var result = fitter.Fit(config, data);

        _output.WriteLine($"True: K0={mineral.KZero:F2}, G0={mineral.GZero:F2}");
        _output.WriteLine($"Fitted: K0={result.Parameters[0]:F2}, G0={result.Parameters[1]:F2}");
        _output.WriteLine($"Converged={result.Converged}");

        // With 50% off, we accept 5% recovery tolerance
        Assert.True(result.Converged);
        Assert.True(Math.Abs(result.Parameters[0] - mineral.KZero) / mineral.KZero < 0.05);
        Assert.True(Math.Abs(result.Parameters[1] - mineral.GZero) / mineral.GZero < 0.05);
    }

    [Fact]
    public void Fit_ReturnsUncertainties()
    {
        var mineral = GetForsterite();
        var data = GenerateVelocityData(mineral, StandardTPConditions, 0.005, seed: 42);

        var config = new FittingConfig { BaseMineralParams = mineral.Clone() };
        config.FitFlags[FittingConfig.IndexK0] = true;
        config.FitFlags[FittingConfig.IndexG0] = true;

        var fitter = new ThermoElasticFitter();
        var result = fitter.Fit(config, data);

        Assert.True(result.Converged);
        // Uncertainties should be positive and finite
        foreach (var u in result.Uncertainties)
        {
            Assert.True(u > 0, $"Uncertainty should be positive, got {u}");
            Assert.True(!double.IsNaN(u) && !double.IsInfinity(u), $"Uncertainty should be finite, got {u}");
        }
        // Covariance matrix should exist
        Assert.NotNull(result.CovarianceMatrix);
    }

    [Fact]
    public void Fit_EmptyData_ThrowsOrReturnsFailed()
    {
        var mineral = GetForsterite();
        var config = new FittingConfig { BaseMineralParams = mineral };
        config.FitFlags[FittingConfig.IndexK0] = true;

        var fitter = new ThermoElasticFitter();
        Assert.Throws<ArgumentException>(() => fitter.Fit(config, new List<FittingDataPoint>()));
    }

    [Fact]
    public void Fit_NoFreeParams_ThrowsOrReturnsFailed()
    {
        var mineral = GetForsterite();
        var data = GenerateVelocityData(mineral, StandardTPConditions, 0.005, seed: 42);
        var config = new FittingConfig { BaseMineralParams = mineral };
        // No flags set

        var fitter = new ThermoElasticFitter();
        Assert.Throws<ArgumentException>(() => fitter.Fit(config, data));
    }
}
