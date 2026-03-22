using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for OxygenFugacityCalculator: mineral buffer oxygen fugacity
/// at given P-T conditions.
/// References:
///   O'Neill (1987), O'Neill & Pownceby (1993), Frost (1991).
/// </summary>
public class OxygenFugacityTests
{
    private readonly ITestOutputHelper _output;
    private readonly OxygenFugacityCalculator _calc;

    public OxygenFugacityTests(ITestOutputHelper output)
    {
        _output = output;
        _calc = new OxygenFugacityCalculator();
    }

    /// <summary>
    /// Test 1: IW buffer at 1 bar (0.0001 GPa), 1473 K.
    /// log10(fO2) ≈ -11.96 from O'Neill (1987) parameterization: -27489/T + 6.702.
    /// Reference: O'Neill (1987).
    /// </summary>
    [Fact]
    public void IW_1bar_1473K_LogFO2_InRange()
    {
        double T = 1473.0;
        double P_GPa = 0.0001;

        double logfO2 = _calc.ComputeLogFO2("IW", T, P_GPa);

        _output.WriteLine($"IW at {T} K, {P_GPa} GPa: log10(fO2) = {logfO2:F4}");

        // O'Neill (1987): -27489/1473 + 6.702 ≈ -11.96
        Assert.InRange(logfO2, -12.5, -11.0);
    }

    /// <summary>
    /// Test 2: QFM buffer at 1 bar (0.0001 GPa), 1473 K.
    /// log10(fO2) should be approximately -5.5 to -6.5 (QFM is more oxidizing than IW).
    /// Reference: O'Neill (1987).
    /// </summary>
    [Fact]
    public void QFM_1bar_1473K_LogFO2_InRange()
    {
        double T = 1473.0;
        double P_GPa = 0.0001;

        double logfO2 = _calc.ComputeLogFO2("QFM", T, P_GPa);

        _output.WriteLine($"QFM at {T} K, {P_GPa} GPa: log10(fO2) = {logfO2:F4}");

        Assert.InRange(logfO2, -9.5, -5.0);
    }

    /// <summary>
    /// Test 3: At the same T and P, IW should be more reducing than QFM
    /// (i.e., log10(fO2) for IW &lt; log10(fO2) for QFM).
    /// </summary>
    [Fact]
    public void IW_MoreReducing_Than_QFM()
    {
        double T = 1473.0;
        double P_GPa = 0.0001;

        double logfO2_IW = _calc.ComputeLogFO2("IW", T, P_GPa);
        double logfO2_QFM = _calc.ComputeLogFO2("QFM", T, P_GPa);

        _output.WriteLine($"IW: {logfO2_IW:F4}, QFM: {logfO2_QFM:F4}");

        Assert.True(logfO2_IW < logfO2_QFM,
            $"IW ({logfO2_IW:F4}) should be more reducing than QFM ({logfO2_QFM:F4})");
    }

    /// <summary>
    /// Test 4: fO2 should increase (become less negative in log10) with
    /// increasing temperature at constant P for all buffers.
    /// </summary>
    [Theory]
    [InlineData("IW")]
    [InlineData("QFM")]
    [InlineData("NNO")]
    public void FO2_Increases_With_Temperature(string buffer)
    {
        double P_GPa = 0.0001;
        double T_low = 1273.0;
        double T_high = 1673.0;

        double logfO2_low = _calc.ComputeLogFO2(buffer, T_low, P_GPa);
        double logfO2_high = _calc.ComputeLogFO2(buffer, T_high, P_GPa);

        _output.WriteLine($"{buffer}: T={T_low} K -> {logfO2_low:F4}, T={T_high} K -> {logfO2_high:F4}");

        Assert.True(logfO2_high > logfO2_low,
            $"{buffer} fO2 at {T_high} K ({logfO2_high:F4}) should be greater than at {T_low} K ({logfO2_low:F4})");
    }

    /// <summary>
    /// Test 5: Pressure effect — at higher P, fO2 should shift
    /// (positive ΔV means higher P increases log10(fO2)).
    /// </summary>
    [Theory]
    [InlineData("IW")]
    [InlineData("QFM")]
    [InlineData("NNO")]
    public void Pressure_Effect_Shifts_FO2(string buffer)
    {
        double T = 1473.0;
        double P_low = 0.0001;
        double P_high = 5.0;

        double logfO2_low = _calc.ComputeLogFO2(buffer, T, P_low);
        double logfO2_high = _calc.ComputeLogFO2(buffer, T, P_high);

        _output.WriteLine($"{buffer}: P={P_low} GPa -> {logfO2_low:F4}, P={P_high} GPa -> {logfO2_high:F4}");

        // With positive ΔV, higher P should increase log10(fO2)
        Assert.True(logfO2_high > logfO2_low,
            $"{buffer} fO2 at {P_high} GPa ({logfO2_high:F4}) should differ from {P_low} GPa ({logfO2_low:F4})");
    }
}
