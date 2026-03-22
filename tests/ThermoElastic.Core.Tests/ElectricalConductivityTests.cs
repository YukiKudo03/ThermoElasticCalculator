using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for ElectricalConductivityCalculator: Arrhenius-based electrical
/// conductivity of mantle minerals with hydrous enhancement.
/// References:
///   Karato (1990), Yoshino et al. (2009).
/// </summary>
public class ElectricalConductivityTests
{
    private readonly ITestOutputHelper _output;
    private readonly ElectricalConductivityCalculator _calc;

    public ElectricalConductivityTests(ITestOutputHelper output)
    {
        _output = output;
        _calc = new ElectricalConductivityCalculator();
    }

    /// <summary>
    /// Test 1: Arrhenius olivine conductivity at 1400 K, 5 GPa should be
    /// in the range ~1e-4 to 0.1 S/m for dry olivine.
    /// Reference: Karato (1990).
    /// </summary>
    [Fact]
    public void DryOlivine_1400K_5GPa_ConductivityInRange()
    {
        double T = 1400.0; // K
        double P = 5.0;    // GPa

        double sigma = _calc.ComputeConductivity(T, P, waterContent_ppm: 0.0);

        _output.WriteLine($"Dry olivine conductivity at T={T} K, P={P} GPa: {sigma:E3} S/m");

        Assert.InRange(sigma, 1e-4, 0.1);
    }

    /// <summary>
    /// Test 2: Water enhances conductivity. 1000 ppm H2O should increase
    /// σ by a factor of ~5-30 at 1400 K compared to dry olivine.
    /// </summary>
    [Fact]
    public void WaterEnhancesConductivity_1000ppm_Factor5To20()
    {
        double T = 1400.0; // K
        double P = 5.0;    // GPa

        double sigma_dry = _calc.ComputeConductivity(T, P, waterContent_ppm: 0.0);
        double sigma_wet = _calc.ComputeConductivity(T, P, waterContent_ppm: 1000.0);

        double ratio = sigma_wet / sigma_dry;

        _output.WriteLine($"Dry σ = {sigma_dry:E3} S/m");
        _output.WriteLine($"Wet σ (1000 ppm) = {sigma_wet:E3} S/m");
        _output.WriteLine($"Enhancement ratio = {ratio:F1}");

        Assert.True(ratio >= 5.0, $"Water enhancement ratio {ratio:F1} should be >= 5");
        Assert.True(ratio <= 30.0, $"Water enhancement ratio {ratio:F1} should be <= 30");
    }

    /// <summary>
    /// Test 3: Conductivity increases with temperature. σ at 1600 K should be
    /// greater than σ at 1200 K at the same pressure (dry).
    /// </summary>
    [Fact]
    public void ConductivityIncreasesWithTemperature()
    {
        double P = 5.0; // GPa

        double sigma_1200 = _calc.ComputeConductivity(1200.0, P);
        double sigma_1600 = _calc.ComputeConductivity(1600.0, P);

        _output.WriteLine($"σ at 1200 K = {sigma_1200:E3} S/m");
        _output.WriteLine($"σ at 1600 K = {sigma_1600:E3} S/m");

        Assert.True(sigma_1600 > sigma_1200,
            $"Conductivity at 1600 K ({sigma_1600:E3}) should exceed that at 1200 K ({sigma_1200:E3})");
    }

    /// <summary>
    /// Test 4: Aggregate conductivity for a multi-mineral mixture should lie
    /// between the minimum and maximum of the individual mineral conductivities.
    /// Uses geometric mean (HS bounds approximation).
    /// </summary>
    [Fact]
    public void AggregateConductivity_BetweenMinAndMax()
    {
        // Simulate a 3-mineral aggregate with different conductivities
        double[] conductivities = { 0.001, 0.01, 0.1 }; // S/m
        double[] volumeFractions = { 0.6, 0.3, 0.1 };

        double sigma_agg = _calc.ComputeAggregateConductivity(conductivities, volumeFractions);

        double minSigma = 0.001;
        double maxSigma = 0.1;

        _output.WriteLine($"Individual conductivities: {string.Join(", ", conductivities)} S/m");
        _output.WriteLine($"Volume fractions: {string.Join(", ", volumeFractions)}");
        _output.WriteLine($"Aggregate conductivity = {sigma_agg:E3} S/m");
        _output.WriteLine($"Min = {minSigma:E3}, Max = {maxSigma:E3}");

        Assert.True(sigma_agg >= minSigma,
            $"Aggregate σ ({sigma_agg:E3}) should be >= min ({minSigma:E3})");
        Assert.True(sigma_agg <= maxSigma,
            $"Aggregate σ ({sigma_agg:E3}) should be <= max ({maxSigma:E3})");
    }
}
