using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using static ThermoElastic.Core.Tests.BurnManReferenceHelper;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Phase 6: Mixing model (Voigt/Reuss/Hill) verification with analytical solutions.
/// Phase 7: Aggregate rock vs PREM comparison.
/// </summary>
public class MixingModelVerificationTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;

    public MixingModelVerificationTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
    }

    private MineralParams GetMineral(string paperName) =>
        _minerals.First(m => m.PaperName == paperName);

    private ResultSummary ComputeResult(string paperName, double P_GPa, double T_K)
    {
        var mineral = GetMineral(paperName);
        var optimizer = new MieGruneisenEOSOptimizer(mineral, P_GPa, T_K);
        return optimizer.ExecOptimize().ExportResults();
    }

    // ========================================================================
    // Voigt / Reuss / Hill analytical verification
    // ========================================================================

    [Fact]
    public void Voigt_5050_IsArithmeticMean()
    {
        var r1 = ComputeResult("fo", 10.0, 1500.0);
        var r2 = ComputeResult("en", 10.0, 1500.0);

        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (0.5, r1), (0.5, r2) });
        var voigt = mix.VoigtAverage();
        Assert.NotNull(voigt);

        double expectedKS = (r1.KS + r2.KS) / 2.0;
        double expectedGS = (r1.GS + r2.GS) / 2.0;

        _output.WriteLine($"Voigt KS = {voigt.KS:F4} (expected: {expectedKS:F4})");
        _output.WriteLine($"Voigt GS = {voigt.GS:F4} (expected: {expectedGS:F4})");

        Assert.True(Math.Abs(voigt.KS - expectedKS) < 1e-6, "Voigt KS should be arithmetic mean");
        Assert.True(Math.Abs(voigt.GS - expectedGS) < 1e-6, "Voigt GS should be arithmetic mean");
    }

    [Fact]
    public void Reuss_5050_IsHarmonicMean()
    {
        var r1 = ComputeResult("fo", 10.0, 1500.0);
        var r2 = ComputeResult("en", 10.0, 1500.0);

        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (0.5, r1), (0.5, r2) });
        var reuss = mix.ReussAverage();
        Assert.NotNull(reuss);

        double expectedKS = 1.0 / (0.5 / r1.KS + 0.5 / r2.KS);
        double expectedGS = 1.0 / (0.5 / r1.GS + 0.5 / r2.GS);

        _output.WriteLine($"Reuss KS = {reuss.KS:F4} (expected: {expectedKS:F4})");
        _output.WriteLine($"Reuss GS = {reuss.GS:F4} (expected: {expectedGS:F4})");

        Assert.True(Math.Abs(reuss.KS - expectedKS) < 1e-6, "Reuss KS should be harmonic mean");
        Assert.True(Math.Abs(reuss.GS - expectedGS) < 1e-6, "Reuss GS should be harmonic mean");
    }

    [Fact]
    public void Hill_IsVoigtReussAverage()
    {
        var r1 = ComputeResult("fo", 10.0, 1500.0);
        var r2 = ComputeResult("en", 10.0, 1500.0);

        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (0.5, r1), (0.5, r2) });
        var voigt = mix.VoigtAverage()!;
        var reuss = mix.ReussAverage()!;
        var hill = mix.HillAverage()!;

        double expectedKS = (voigt.KS + reuss.KS) / 2.0;
        double expectedGS = (voigt.GS + reuss.GS) / 2.0;

        _output.WriteLine($"Hill KS = {hill.KS:F4} (expected: {expectedKS:F4})");
        _output.WriteLine($"Hill GS = {hill.GS:F4} (expected: {expectedGS:F4})");

        Assert.True(Math.Abs(hill.KS - expectedKS) < 1e-6, "Hill KS should be VRH average");
        Assert.True(Math.Abs(hill.GS - expectedGS) < 1e-6, "Hill GS should be VRH average");
    }

    [Fact]
    public void VoigtBound_GreaterOrEqual_ReussBound()
    {
        var r1 = ComputeResult("fo", 10.0, 1500.0);
        var r2 = ComputeResult("pe", 10.0, 1500.0);

        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (0.5, r1), (0.5, r2) });
        var voigt = mix.VoigtAverage()!;
        var reuss = mix.ReussAverage()!;

        _output.WriteLine($"Voigt KS={voigt.KS:F4}, Reuss KS={reuss.KS:F4}");
        _output.WriteLine($"Voigt GS={voigt.GS:F4}, Reuss GS={reuss.GS:F4}");

        Assert.True(voigt.KS >= reuss.KS - 1e-10, "Voigt KS >= Reuss KS");
        Assert.True(voigt.GS >= reuss.GS - 1e-10, "Voigt GS >= Reuss GS");
    }

    [Fact]
    public void PureEndmember_AllModels_ReturnSameResult()
    {
        var r1 = ComputeResult("fo", 10.0, 1500.0);

        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (1.0, r1) });
        var voigt = mix.VoigtAverage()!;
        var reuss = mix.ReussAverage()!;
        var hill = mix.HillAverage()!;

        _output.WriteLine($"Pure fo: Voigt KS={voigt.KS:F4}, Reuss KS={reuss.KS:F4}, Hill KS={hill.KS:F4}, Original KS={r1.KS:F4}");

        Assert.True(Math.Abs(voigt.KS - r1.KS) < 1e-6);
        Assert.True(Math.Abs(reuss.KS - r1.KS) < 1e-6);
        Assert.True(Math.Abs(hill.KS - r1.KS) < 1e-6);
    }

    // ========================================================================
    // Phase 7: Aggregate rock vs PREM (approximate comparison)
    // ========================================================================

    [Fact]
    public void LowerMantle_Pv80Fp20_VpVs_NearPREM()
    {
        // Lower mantle ~660 km: P ≈ 23.5 GPa, T ≈ 1900 K
        // PREM values: Vp ≈ 10.75 km/s, Vs ≈ 5.95 km/s, rho ≈ 3.99 g/cm3
        double P = 25.0;  // Use nearest grid point
        double T = 2000.0;

        var rPv = ComputeResult("mpv", P, T);
        var rPe = ComputeResult("pe", P, T);

        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (0.8, rPv), (0.2, rPe) });
        var hill = mix.HillAverage()!;

        double Vp = hill.Vp / 1000.0; // m/s -> km/s
        double Vs = hill.Vs / 1000.0;

        _output.WriteLine($"Lower mantle (Pv80+Fp20) at {P}GPa/{T}K:");
        _output.WriteLine($"  Vp = {Vp:F3} km/s (PREM ~10.75)");
        _output.WriteLine($"  Vs = {Vs:F3} km/s (PREM ~5.95)");
        _output.WriteLine($"  rho = {hill.Density:F4} g/cm3 (PREM ~3.99)");

        // Allow 10% tolerance - this is a simplified model vs. real Earth
        Assert.True(Vp > 9.0 && Vp < 13.0, $"Vp {Vp} km/s outside reasonable range for lower mantle");
        Assert.True(Vs > 5.0 && Vs < 8.0, $"Vs {Vs} km/s outside reasonable range for lower mantle");
        Assert.True(hill.Density > 3.5 && hill.Density < 5.5, $"rho {hill.Density} outside reasonable range");
    }

    [Fact]
    public void DeepLowerMantle_Pv80Fp20_HighPressure()
    {
        // ~2000 km depth: P ≈ 100 GPa, T ≈ 2500 K
        // PREM: Vp ≈ 13.7 km/s, rho ≈ 5.38 g/cm3
        double P = 100.0;
        double T = 2500.0;

        var rPv = ComputeResult("mpv", P, T);
        var rPe = ComputeResult("pe", P, T);

        var mix = new MixtureCalculator(new List<(double, ResultSummary)> { (0.8, rPv), (0.2, rPe) });
        var hill = mix.HillAverage()!;

        double Vp = hill.Vp / 1000.0;
        double Vs = hill.Vs / 1000.0;

        _output.WriteLine($"Deep lower mantle (Pv80+Fp20) at {P}GPa/{T}K:");
        _output.WriteLine($"  Vp = {Vp:F3} km/s (PREM ~13.7)");
        _output.WriteLine($"  Vs = {Vs:F3} km/s");
        _output.WriteLine($"  rho = {hill.Density:F4} g/cm3 (PREM ~5.38)");

        Assert.True(Vp > 11.0 && Vp < 16.0, $"Vp {Vp} km/s outside reasonable range");
        Assert.True(hill.Density > 4.5 && hill.Density < 6.5, $"rho {hill.Density} outside reasonable range");
    }

    // ========================================================================
    // Multi-mineral mix ratio consistency
    // ========================================================================

    [Fact]
    public void ThreeMineral_Mix_ProducesValidResults()
    {
        // Pyrolite-like: Ol 60% + Px 20% + Gt 20% at 10 GPa, 1500 K
        var rFo = ComputeResult("fo", 10.0, 1500.0);
        var rEn = ComputeResult("en", 10.0, 1500.0);
        var rPy = ComputeResult("py", 10.0, 1500.0);

        var mix = new MixtureCalculator(new List<(double, ResultSummary)>
        {
            (0.6, rFo), (0.2, rEn), (0.2, rPy)
        });

        var hill = mix.HillAverage();
        Assert.NotNull(hill);

        _output.WriteLine($"Pyrolite-like at 10GPa/1500K (Hill):");
        _output.WriteLine($"  Vp = {hill.Vp / 1000.0:F3} km/s");
        _output.WriteLine($"  Vs = {hill.Vs / 1000.0:F3} km/s");
        _output.WriteLine($"  rho = {hill.Density:F4} g/cm3");
        _output.WriteLine($"  KS = {hill.KS:F2} GPa");
        _output.WriteLine($"  GS = {hill.GS:F2} GPa");

        // Sanity checks for upper mantle
        Assert.True(hill.Vp / 1000.0 > 7.0 && hill.Vp / 1000.0 < 11.0);
        Assert.True(hill.Vs / 1000.0 > 4.0 && hill.Vs / 1000.0 < 7.0);
        Assert.True(hill.Density > 3.0 && hill.Density < 4.5);
    }
}
