using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for AnelasticityCalculator: frequency-dependent Q corrections
/// to convert elastic to anelastic (seismic) velocities.
/// References:
///   Karato (1993), Jackson &amp; Faul (2010), Faul &amp; Jackson (2005),
///   Anderson &amp; Given (1982).
/// </summary>
public class AnelasticityCalculatorTests
{
    private readonly ITestOutputHelper _output;
    private readonly AnelasticityCalculator _calc;

    public AnelasticityCalculatorTests(ITestOutputHelper output)
    {
        _output = output;
        _calc = new AnelasticityCalculator();
    }

    /// <summary>
    /// Test 1: QS for olivine at 1400 K, 5 GPa, 1 Hz should be ~80-300.
    /// Reference: Faul &amp; Jackson (2005).
    /// </summary>
    [Fact]
    public void QS_Olivine_1400K_5GPa_1Hz_InRange80To300()
    {
        double T = 1400.0;  // K
        double P = 5.0;     // GPa
        double freq = 1.0;  // Hz

        double qs = _calc.ComputeQS(T, P, freq);

        _output.WriteLine($"QS at T={T} K, P={P} GPa, f={freq} Hz: {qs:F1}");

        Assert.InRange(qs, 80.0, 300.0);
    }

    /// <summary>
    /// Test 2: QS increases with pressure (less attenuation at depth).
    /// At 10 GPa vs 5 GPa (same T=1400 K), QS should increase.
    /// </summary>
    [Fact]
    public void QS_IncreasesWithPressure_10GPa_GreaterThan_5GPa()
    {
        double T = 1400.0;
        double freq = 1.0;

        double qs_5GPa = _calc.ComputeQS(T, 5.0, freq);
        double qs_10GPa = _calc.ComputeQS(T, 10.0, freq);

        _output.WriteLine($"QS at 5 GPa: {qs_5GPa:F1}");
        _output.WriteLine($"QS at 10 GPa: {qs_10GPa:F1}");

        // At higher pressure, T/T_solidus is lower (solidus increases with P),
        // so premelting effect is weaker, and QS should be higher.
        Assert.True(qs_10GPa > qs_5GPa,
            $"QS at 10 GPa ({qs_10GPa:F1}) should be greater than at 5 GPa ({qs_5GPa:F1})");
    }

    /// <summary>
    /// Test 3: QS decreases near solidus (T/Tm > 0.9), QS should drop dramatically (&lt;50).
    /// </summary>
    [Fact]
    public void QS_NearSolidus_DropsBelow50()
    {
        double P = 5.0;
        double T_solidus = AnelasticityCalculator.GetSolidusTemperature(P);
        double T = 0.95 * T_solidus;  // T/Tm = 0.95
        double freq = 1.0;

        double qs = _calc.ComputeQS(T, P, freq);

        _output.WriteLine($"Solidus at P={P} GPa: {T_solidus:F0} K");
        _output.WriteLine($"T = {T:F0} K (T/Tm = 0.95)");
        _output.WriteLine($"QS near solidus: {qs:F1}");

        Assert.True(qs < 50.0,
            $"QS near solidus ({qs:F1}) should be < 50");
    }

    /// <summary>
    /// Test 4: Velocity correction magnitude — for typical mantle QS,
    /// Vs reduction should be ~0.5-1.5%.
    /// </summary>
    [Fact]
    public void VelocityCorrection_VsReduction_Between05And15Percent()
    {
        double Vp_el = 8500.0;  // m/s (typical upper mantle)
        double Vs_el = 4800.0;  // m/s
        double KS = 130.0;      // GPa
        double GS = 80.0;       // GPa
        double T = 1400.0;
        double P = 5.0;

        var result = _calc.ApplyCorrection(Vp_el, Vs_el, KS, GS, T, P);

        _output.WriteLine($"QS = {result.QS:F1}");
        _output.WriteLine($"Vs elastic = {result.Vs_elastic:F1} m/s");
        _output.WriteLine($"Vs anelastic = {result.Vs_anelastic:F1} m/s");
        _output.WriteLine($"Delta Vs = {result.DeltaVs_percent:F3}%");

        double reduction = -result.DeltaVs_percent;  // positive value
        Assert.InRange(reduction, 0.5, 1.5);
    }

    /// <summary>
    /// Test 5: Vp correction should be smaller than Vs correction because QK >> QS.
    /// </summary>
    [Fact]
    public void VpCorrection_SmallerThan_VsCorrection()
    {
        double Vp_el = 8500.0;
        double Vs_el = 4800.0;
        double KS = 130.0;
        double GS = 80.0;
        double T = 1400.0;
        double P = 5.0;

        var result = _calc.ApplyCorrection(Vp_el, Vs_el, KS, GS, T, P);

        _output.WriteLine($"Delta Vp = {result.DeltaVp_percent:F3}%");
        _output.WriteLine($"Delta Vs = {result.DeltaVs_percent:F3}%");

        // Both are negative; Vp reduction magnitude should be smaller
        Assert.True(Math.Abs(result.DeltaVp_percent) < Math.Abs(result.DeltaVs_percent),
            $"|DeltaVp| ({Math.Abs(result.DeltaVp_percent):F3}%) should be < |DeltaVs| ({Math.Abs(result.DeltaVs_percent):F3}%)");
    }

    /// <summary>
    /// Test 6: Anelastic Vs is always less than elastic Vs.
    /// </summary>
    [Fact]
    public void AnelasticVs_AlwaysLessThan_ElasticVs()
    {
        double Vp_el = 8500.0;
        double Vs_el = 4800.0;
        double KS = 130.0;
        double GS = 80.0;

        // Test across a range of conditions
        double[] temperatures = { 1000, 1200, 1400, 1600, 1800 };
        double[] pressures = { 2, 5, 10, 20 };

        foreach (double T in temperatures)
        {
            foreach (double P in pressures)
            {
                var result = _calc.ApplyCorrection(Vp_el, Vs_el, KS, GS, T, P);

                _output.WriteLine($"T={T} K, P={P} GPa: Vs_el={Vs_el:F1}, Vs_an={result.Vs_anelastic:F1}");

                Assert.True(result.Vs_anelastic < result.Vs_elastic,
                    $"At T={T}, P={P}: Vs_anelastic ({result.Vs_anelastic:F1}) should be < Vs_elastic ({result.Vs_elastic:F1})");
            }
        }
    }

    /// <summary>
    /// Test 7: Frequency dependence — at 0.01 Hz (lower frequency),
    /// QS should be lower than at 1 Hz (more attenuation at lower frequency).
    /// </summary>
    [Fact]
    public void FrequencyDependence_LowerFreq_LowerQS()
    {
        double T = 1400.0;
        double P = 5.0;

        double qs_1Hz = _calc.ComputeQS(T, P, 1.0);
        double qs_001Hz = _calc.ComputeQS(T, P, 0.01);

        _output.WriteLine($"QS at 1 Hz: {qs_1Hz:F1}");
        _output.WriteLine($"QS at 0.01 Hz: {qs_001Hz:F1}");

        Assert.True(qs_001Hz < qs_1Hz,
            $"QS at 0.01 Hz ({qs_001Hz:F1}) should be < QS at 1 Hz ({qs_1Hz:F1})");
    }
}
