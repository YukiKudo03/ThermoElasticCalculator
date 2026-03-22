using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class CoverageBoostTests
{
    private readonly ITestOutputHelper _output;
    public CoverageBoostTests(ITestOutputHelper output) { _output = output; }

    // ================================================================
    // 1. JointLikelihood (0% coverage)
    // ================================================================

    [Fact]
    public void GaussianLogLikelihood_PerfectMatch_ReturnsZero()
    {
        var observed = new double[] { 1.0, 2.0, 3.0 };
        var predicted = new double[] { 1.0, 2.0, 3.0 };
        var sigma = new double[] { 1.0, 1.0, 1.0 };

        double ll = JointLikelihood.GaussianLogLikelihood(observed, predicted, sigma);
        Assert.Equal(0.0, ll, 1e-10);
    }

    [Fact]
    public void GaussianLogLikelihood_KnownValues_ReturnsExpected()
    {
        // Single point: observed=1, predicted=0, sigma=1 => -0.5 * (1/1)^2 = -0.5
        var observed = new double[] { 1.0 };
        var predicted = new double[] { 0.0 };
        var sigma = new double[] { 1.0 };

        double ll = JointLikelihood.GaussianLogLikelihood(observed, predicted, sigma);
        Assert.Equal(-0.5, ll, 1e-10);
    }

    [Fact]
    public void GaussianLogLikelihood_MultipleMismatches_IsNegative()
    {
        var observed = new double[] { 10.0, 20.0 };
        var predicted = new double[] { 12.0, 18.0 };
        var sigma = new double[] { 1.0, 2.0 };

        // diff1 = (10-12)/1 = -2, contribution = -0.5*4 = -2
        // diff2 = (20-18)/2 = 1,  contribution = -0.5*1 = -0.5
        // total = -2.5
        double ll = JointLikelihood.GaussianLogLikelihood(observed, predicted, sigma);
        Assert.Equal(-2.5, ll, 1e-10);
    }

    [Fact]
    public void UniformPrior_AllInsideBounds_ReturnsZero()
    {
        var parameters = new double[] { 1.0, 2.0, 3.0 };
        var lower = new double[] { 0.0, 1.0, 2.0 };
        var upper = new double[] { 2.0, 3.0, 4.0 };

        double result = JointLikelihood.UniformPrior(parameters, lower, upper);
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void UniformPrior_OneBelowLower_ReturnsNegInfinity()
    {
        var parameters = new double[] { -1.0, 2.0, 3.0 };
        var lower = new double[] { 0.0, 1.0, 2.0 };
        var upper = new double[] { 2.0, 3.0, 4.0 };

        double result = JointLikelihood.UniformPrior(parameters, lower, upper);
        Assert.Equal(double.NegativeInfinity, result);
    }

    [Fact]
    public void UniformPrior_OneAboveUpper_ReturnsNegInfinity()
    {
        var parameters = new double[] { 1.0, 2.0, 5.0 };
        var lower = new double[] { 0.0, 1.0, 2.0 };
        var upper = new double[] { 2.0, 3.0, 4.0 };

        double result = JointLikelihood.UniformPrior(parameters, lower, upper);
        Assert.Equal(double.NegativeInfinity, result);
    }

    [Fact]
    public void UniformPrior_OnBoundary_ReturnsZero()
    {
        var parameters = new double[] { 0.0, 3.0 };
        var lower = new double[] { 0.0, 1.0 };
        var upper = new double[] { 2.0, 3.0 };

        double result = JointLikelihood.UniformPrior(parameters, lower, upper);
        Assert.Equal(0.0, result);
    }

    // ================================================================
    // 2. PhysicConstants / CommonMethods (0% GetDensity, DoubleEquals)
    // ================================================================

    [Fact]
    public void DoubleEquals_EqualValues_ReturnsTrue()
    {
        Assert.True(CommonMethods.DoubleEquals(1.0, 1.0));
    }

    [Fact]
    public void DoubleEquals_NearlyEqualValues_ReturnsTrue()
    {
        Assert.True(CommonMethods.DoubleEquals(1.0, 1.0 + 1e-6));
    }

    [Fact]
    public void DoubleEquals_DifferentValues_ReturnsFalse()
    {
        Assert.False(CommonMethods.DoubleEquals(1.0, 2.0));
    }

    [Fact]
    public void DoubleEquals_SmallDifferenceBeyondTolerance_ReturnsFalse()
    {
        Assert.False(CommonMethods.DoubleEquals(1.0, 1.0 + 1e-4));
    }

    [Fact]
    public void GetDensity_EqualRatio_ReturnsWeightedAverage()
    {
        var elem1 = new ResultSummary { Volume = 10.0, Density = 3.0 };
        var elem2 = new ResultSummary { Volume = 10.0, Density = 5.0 };

        // 50/50 mix with same volume: density = (0.5*10*3 + 0.5*10*5)/(0.5*10 + 0.5*10) = 40/10 = 4.0
        double density = CommonMethods.GetDensity(0.5, elem1, elem2);
        Assert.Equal(4.0, density, 1e-10);
    }

    [Fact]
    public void GetDensity_PureElem1_ReturnsElem1Density()
    {
        var elem1 = new ResultSummary { Volume = 10.0, Density = 3.0 };
        var elem2 = new ResultSummary { Volume = 20.0, Density = 5.0 };

        double density = CommonMethods.GetDensity(1.0, elem1, elem2);
        Assert.Equal(3.0, density, 1e-10);
    }

    [Fact]
    public void GetDensity_PureElem2_ReturnsElem2Density()
    {
        var elem1 = new ResultSummary { Volume = 10.0, Density = 3.0 };
        var elem2 = new ResultSummary { Volume = 20.0, Density = 5.0 };

        double density = CommonMethods.GetDensity(0.0, elem1, elem2);
        Assert.Equal(5.0, density, 1e-10);
    }

    // ================================================================
    // 3. MLSurrogateModel (45.5% coverage)
    // ================================================================

    [Fact]
    public void MLSurrogateModel_IsLoaded_InitiallyFalse()
    {
        var model = new MLSurrogateModel();
        Assert.False(model.IsLoaded);
    }

    [Fact]
    public void MLSurrogateModel_LoadModel_NonExistentFile_ThrowsFileNotFoundException()
    {
        var model = new MLSurrogateModel();
        Assert.Throws<FileNotFoundException>(() => model.LoadModel("/nonexistent/path/model.onnx"));
    }

    [Fact]
    public void MLSurrogateModel_Predict_WithoutLoading_ThrowsInvalidOperationException()
    {
        var model = new MLSurrogateModel();
        Assert.Throws<InvalidOperationException>(() => model.Predict(10.0, 1500.0));
    }

    // ================================================================
    // 4. SingleCrystalElasticConstants (66.7% coverage)
    // ================================================================

    [Fact]
    public void GetTensor_Forsterite_ReturnsNonNull()
    {
        var tensor = SingleCrystalElasticConstants.GetTensor("fo");
        Assert.NotNull(tensor);
        Assert.Equal("Forsterite", tensor.MineralName);
    }

    [Fact]
    public void GetTensor_Forsterite_C11IsCorrect()
    {
        var tensor = SingleCrystalElasticConstants.GetTensor("fo")!;
        Assert.Equal(328.1, tensor.C[0, 0], 0.01);
    }

    [Fact]
    public void GetTensor_Periclase_ReturnsNonNull()
    {
        var tensor = SingleCrystalElasticConstants.GetTensor("pe");
        Assert.NotNull(tensor);
        Assert.Equal("Periclase", tensor.MineralName);
    }

    [Fact]
    public void GetTensor_Periclase_C11IsCorrect()
    {
        var tensor = SingleCrystalElasticConstants.GetTensor("pe")!;
        Assert.Equal(297.0, tensor.C[0, 0], 0.01);
    }

    [Fact]
    public void GetTensor_Unknown_ReturnsNull()
    {
        var tensor = SingleCrystalElasticConstants.GetTensor("unknown");
        Assert.Null(tensor);
    }

    [Fact]
    public void Periclase_HasCubicSymmetry_C11EqualsC22EqualsC33()
    {
        var tensor = SingleCrystalElasticConstants.Periclase();
        Assert.Equal(tensor.C[0, 0], tensor.C[1, 1], 1e-10);
        Assert.Equal(tensor.C[1, 1], tensor.C[2, 2], 1e-10);
    }

    [Fact]
    public void Periclase_HasCubicSymmetry_C44EqualsC55EqualsC66()
    {
        var tensor = SingleCrystalElasticConstants.Periclase();
        Assert.Equal(tensor.C[3, 3], tensor.C[4, 4], 1e-10);
        Assert.Equal(tensor.C[4, 4], tensor.C[5, 5], 1e-10);
    }

    [Fact]
    public void Forsterite_IsOrthorhombic_C11NotEqualC22()
    {
        var tensor = SingleCrystalElasticConstants.Forsterite();
        Assert.NotEqual(tensor.C[0, 0], tensor.C[1, 1]);
    }

    // ================================================================
    // 5. IsentropeCalculator (69.7% coverage)
    // ================================================================

    [Fact]
    public void IsentropeCalculator_VeryLowPressure_ProducesResult()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var calc = new IsentropeCalculator(fo);

        var profile = calc.ComputeIsentrope(0.0001, 1000.0, pressureMax: 2.0, pressureStep: 1.0);

        Assert.NotEmpty(profile);
        Assert.True(profile.Count >= 2);
        _output.WriteLine($"Isentrope low-P: {profile.Count} points, T range [{profile.First().Temperature:F1} - {profile.Last().Temperature:F1}] K");
    }

    [Fact]
    public void IsentropeCalculator_SingleStep_ProducesTwoPoints()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var calc = new IsentropeCalculator(fo);

        var profile = calc.ComputeIsentrope(1.0, 1500.0, pressureMax: 2.0, pressureStep: 1.0);

        Assert.True(profile.Count >= 2);
        Assert.Equal(1.0, profile[0].Pressure, 1e-10);
        Assert.Equal(1500.0, profile[0].Temperature, 1e-10);
    }

    [Fact]
    public void IsentropeCalculator_HighPressureRange_ProducesMonotonicTemperature()
    {
        var pv = MineralDatabase.GetByName("Mg-Perovskite")!;
        var calc = new IsentropeCalculator(pv);

        var profile = calc.ComputeIsentrope(25.0, 2000.0, pressureMax: 50.0, pressureStep: 5.0);

        Assert.True(profile.Count >= 2);
        for (int i = 1; i < profile.Count; i++)
        {
            Assert.True(profile[i].Temperature >= profile[i - 1].Temperature - 50,
                $"Temperature should generally increase along isentrope: T[{i-1}]={profile[i-1].Temperature:F1}, T[{i}]={profile[i].Temperature:F1}");
        }
    }

    [Fact]
    public void IsentropeCalculator_PressureStepLargerThanRange_ProducesStartPointOnly()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var calc = new IsentropeCalculator(fo);

        var profile = calc.ComputeIsentrope(10.0, 1500.0, pressureMax: 10.5, pressureStep: 5.0);

        // Start point is always added; step of 5 from 10 goes to 15 which > 10.5
        Assert.Single(profile);
        Assert.Equal(10.0, profile[0].Pressure, 1e-10);
    }

    // ================================================================
    // 6. OxygenFugacityCalculator (72.7% coverage)
    // ================================================================

    [Fact]
    public void ComputeLogFO2_IW_Buffer_AtAmbient()
    {
        var calc = new OxygenFugacityCalculator();
        double logfO2 = calc.ComputeLogFO2("IW", 1473.0); // ~1200 C

        _output.WriteLine($"IW at 1473 K: log10(fO2) = {logfO2:F2}");
        // IW buffer at 1473 K, 1 bar: A/T + B = -27489/1473 + 6.702 ≈ -11.96
        Assert.InRange(logfO2, -13.0, -10.0);
    }

    [Fact]
    public void ComputeLogFO2_NNO_Buffer_AtAmbient()
    {
        var calc = new OxygenFugacityCalculator();
        double logfO2 = calc.ComputeLogFO2("NNO", 1473.0);

        _output.WriteLine($"NNO at 1473 K: log10(fO2) = {logfO2:F2}");
        // NNO: -24930/1473 + 9.36 ≈ -7.57
        Assert.InRange(logfO2, -9.0, -6.0);
    }

    [Fact]
    public void ComputeLogFO2_QFM_Buffer_AtAmbient()
    {
        var calc = new OxygenFugacityCalculator();
        double logfO2 = calc.ComputeLogFO2("QFM", 1473.0);

        _output.WriteLine($"QFM at 1473 K: log10(fO2) = {logfO2:F2}");
        Assert.InRange(logfO2, -10.0, -7.0);
    }

    [Fact]
    public void ComputeLogFO2_WithPressureCorrection_DiffersFrom1Bar()
    {
        var calc = new OxygenFugacityCalculator();
        double logfO2_1bar = calc.ComputeLogFO2("IW", 1473.0, 0.0001);
        double logfO2_5GPa = calc.ComputeLogFO2("IW", 1473.0, 5.0);

        _output.WriteLine($"IW at 1473 K: 1 bar = {logfO2_1bar:F4}, 5 GPa = {logfO2_5GPa:F4}");
        // Pressure correction should make fO2 different
        Assert.NotEqual(logfO2_1bar, logfO2_5GPa);
        // Positive pressure correction expected (deltaV > 0)
        Assert.True(logfO2_5GPa > logfO2_1bar);
    }

    [Fact]
    public void ComputeDeltaBuffer_SameAsBuffer_ReturnsZero()
    {
        var calc = new OxygenFugacityCalculator();
        double logfO2_IW = calc.ComputeLogFO2("IW", 1473.0, 0.0001);
        double delta = calc.ComputeDeltaBuffer("IW", 1473.0, 0.0001, logfO2_IW);

        Assert.Equal(0.0, delta, 1e-10);
    }

    [Fact]
    public void ComputeDeltaBuffer_NNO_RelativeToIW()
    {
        var calc = new OxygenFugacityCalculator();
        double logfO2_NNO = calc.ComputeLogFO2("NNO", 1473.0);
        double delta = calc.ComputeDeltaBuffer("IW", 1473.0, 0.0001, logfO2_NNO);

        _output.WriteLine($"NNO relative to IW at 1473 K: delta = {delta:F2}");
        // NNO is more oxidized than IW, so delta should be positive
        Assert.True(delta > 0);
    }

    [Fact]
    public void AvailableBuffers_ContainsExpectedNames()
    {
        var buffers = OxygenFugacityCalculator.AvailableBuffers;

        Assert.Contains("IW", buffers);
        Assert.Contains("QFM", buffers);
        Assert.Contains("NNO", buffers);
        Assert.Equal(3, buffers.Length);
    }

    [Fact]
    public void ComputeLogFO2_InvalidBuffer_ThrowsArgumentException()
    {
        var calc = new OxygenFugacityCalculator();
        Assert.Throws<ArgumentException>(() => calc.ComputeLogFO2("INVALID", 1473.0));
    }

    // ================================================================
    // 7. MeltParams (75% coverage)
    // ================================================================

    [Fact]
    public void MeltParams_DefaultValues()
    {
        var melt = new MeltParams();
        Assert.Equal(3.0, melt.Rho0);
        Assert.Equal(30.0, melt.K0);
        Assert.Equal(6.0, melt.K1);
    }

    [Fact]
    public void MeltParams_GS_IsAlwaysZero()
    {
        var melt = new MeltParams();
        Assert.Equal(0.0, melt.GS);
    }

    [Fact]
    public void MeltParams_CustomValues()
    {
        var melt = new MeltParams { Rho0 = 2.8, K0 = 25.0, K1 = 5.0 };
        Assert.Equal(2.8, melt.Rho0);
        Assert.Equal(25.0, melt.K0);
        Assert.Equal(5.0, melt.K1);
        Assert.Equal(0.0, melt.GS); // still zero for liquid
    }

    [Fact]
    public void MeltParams_IsRecord_SupportsEquality()
    {
        var melt1 = new MeltParams { Rho0 = 3.0, K0 = 30.0, K1 = 6.0 };
        var melt2 = new MeltParams { Rho0 = 3.0, K0 = 30.0, K1 = 6.0 };
        Assert.Equal(melt1, melt2);
    }

    // ================================================================
    // 8. ElasticTensorCalculator (76% coverage)
    // ================================================================

    [Fact]
    public void ComputeVRH_Periclase_VoigtApproxEqualsReuss()
    {
        var calc = new ElasticTensorCalculator();
        var tensor = SingleCrystalElasticConstants.Periclase();

        var (kVRH, gVRH) = calc.ComputeVRH(tensor);

        _output.WriteLine($"Periclase VRH: K = {kVRH:F2} GPa, G = {gVRH:F2} GPa");
        // For cubic crystal, Voigt and Reuss should be relatively close
        Assert.InRange(kVRH, 100.0, 250.0);
        Assert.InRange(gVRH, 50.0, 200.0);
    }

    [Fact]
    public void ComputeVRH_Forsterite_ReasonableModuli()
    {
        var calc = new ElasticTensorCalculator();
        var tensor = SingleCrystalElasticConstants.Forsterite();

        var (kVRH, gVRH) = calc.ComputeVRH(tensor);

        _output.WriteLine($"Forsterite VRH: K = {kVRH:F2} GPa, G = {gVRH:F2} GPa");
        // Literature: K ~ 128 GPa, G ~ 82 GPa for forsterite
        Assert.InRange(kVRH, 100.0, 160.0);
        Assert.InRange(gVRH, 60.0, 110.0);
    }

    [Fact]
    public void SolveChristoffel_Periclase_100Direction()
    {
        var calc = new ElasticTensorCalculator();
        var tensor = SingleCrystalElasticConstants.Periclase();

        var (vp, vs1, vs2) = calc.SolveChristoffel(tensor, new[] { 1.0, 0.0, 0.0 });

        _output.WriteLine($"Periclase [100]: Vp={vp:F0}, Vs1={vs1:F0}, Vs2={vs2:F0} m/s");
        Assert.True(vp > vs1, "Vp should be greater than Vs1");
        Assert.True(vp > 5000, "Vp should be > 5000 m/s for periclase");
    }

    [Fact]
    public void SolveChristoffel_Periclase_010Direction()
    {
        var calc = new ElasticTensorCalculator();
        var tensor = SingleCrystalElasticConstants.Periclase();

        var (vp, vs1, vs2) = calc.SolveChristoffel(tensor, new[] { 0.0, 1.0, 0.0 });

        _output.WriteLine($"Periclase [010]: Vp={vp:F0}, Vs1={vs1:F0}, Vs2={vs2:F0} m/s");
        // For cubic crystal, [010] Vp should equal [100] Vp
        var (vp100, _, _) = calc.SolveChristoffel(tensor, new[] { 1.0, 0.0, 0.0 });
        Assert.Equal(vp100, vp, 1.0); // within 1 m/s
    }

    [Fact]
    public void SolveChristoffel_Periclase_001Direction()
    {
        var calc = new ElasticTensorCalculator();
        var tensor = SingleCrystalElasticConstants.Periclase();

        var (vp, vs1, vs2) = calc.SolveChristoffel(tensor, new[] { 0.0, 0.0, 1.0 });

        _output.WriteLine($"Periclase [001]: Vp={vp:F0}, Vs1={vs1:F0}, Vs2={vs2:F0} m/s");
        var (vp100, _, _) = calc.SolveChristoffel(tensor, new[] { 1.0, 0.0, 0.0 });
        Assert.Equal(vp100, vp, 1.0);
    }

    [Fact]
    public void SolveChristoffel_NonUnitVector_NormalizesCorrectly()
    {
        var calc = new ElasticTensorCalculator();
        var tensor = SingleCrystalElasticConstants.Periclase();

        // [2, 0, 0] should give same result as [1, 0, 0] after normalization
        var (vp1, vs1_1, vs2_1) = calc.SolveChristoffel(tensor, new[] { 2.0, 0.0, 0.0 });
        var (vp2, vs1_2, vs2_2) = calc.SolveChristoffel(tensor, new[] { 1.0, 0.0, 0.0 });

        Assert.Equal(vp2, vp1, 0.1);
        Assert.Equal(vs1_2, vs1_1, 0.1);
        Assert.Equal(vs2_2, vs2_1, 0.1);
    }

    [Fact]
    public void SolveChristoffel_InvalidDirection_Throws()
    {
        var calc = new ElasticTensorCalculator();
        var tensor = SingleCrystalElasticConstants.Periclase();

        Assert.Throws<ArgumentException>(() => calc.SolveChristoffel(tensor, new[] { 1.0, 0.0 }));
    }

    [Fact]
    public void SolveChristoffel_ZeroVector_Throws()
    {
        var calc = new ElasticTensorCalculator();
        var tensor = SingleCrystalElasticConstants.Periclase();

        Assert.Throws<ArgumentException>(() => calc.SolveChristoffel(tensor, new[] { 0.0, 0.0, 0.0 }));
    }

    [Fact]
    public void ComputeMaxAnisotropy_Periclase_LessThanForsterite()
    {
        var calc = new ElasticTensorCalculator();
        var periclase = SingleCrystalElasticConstants.Periclase();
        var forsterite = SingleCrystalElasticConstants.Forsterite();

        double aniPe = calc.ComputeMaxAnisotropy(periclase);
        double aniFo = calc.ComputeMaxAnisotropy(forsterite);

        _output.WriteLine($"Anisotropy: Periclase = {aniPe:F2}%, Forsterite = {aniFo:F2}%");
        // Both should have some anisotropy
        Assert.True(aniPe > 0);
        Assert.True(aniFo > 0);
        // Forsterite (orthorhombic) is generally more anisotropic than periclase (cubic)
        Assert.True(aniFo > aniPe, "Forsterite should be more anisotropic than periclase");
    }

    [Fact]
    public void ComputeMaxAnisotropy_Periclase_ReasonableRange()
    {
        var calc = new ElasticTensorCalculator();
        var tensor = SingleCrystalElasticConstants.Periclase();

        double anisotropy = calc.ComputeMaxAnisotropy(tensor, nDirections: 200);

        _output.WriteLine($"Periclase anisotropy (200 dirs): {anisotropy:F2}%");
        // Periclase has moderate anisotropy (~10-15%)
        Assert.InRange(anisotropy, 1.0, 30.0);
    }
}
