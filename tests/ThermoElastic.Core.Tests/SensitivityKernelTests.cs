using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class SensitivityKernelTests
{
    private readonly ITestOutputHelper _output;
    private readonly SensitivityKernelCalculator _calculator;

    public SensitivityKernelTests(ITestOutputHelper output)
    {
        _output = output;
        _calculator = new SensitivityKernelCalculator();
    }

    private static MineralParams GetMineral(string paperName)
    {
        return SLB2011Endmembers.GetAll().First(m => m.PaperName == paperName);
    }

    [Fact]
    public void DlnVs_dT_Forsterite_10GPa_1500K_IsNegative()
    {
        // dlnVs/dT should be negative: velocity decreases with increasing temperature
        // Expected range: ~ -0.5 to -1.5 × 10⁻⁴ /K
        var fo = GetMineral("fo");
        var kernel = _calculator.ComputeThermalSensitivity(fo, 10.0, 1500.0);

        _output.WriteLine($"dlnVs/dT = {kernel.DlnVs_dT:E4} /K");
        Assert.True(kernel.DlnVs_dT < 0, "dlnVs/dT should be negative");
        Assert.InRange(kernel.DlnVs_dT, -1.5e-4, -0.5e-5);
    }

    [Fact]
    public void DlnVp_dT_Forsterite_10GPa_1500K_IsNegative_AndSmallerMagnitudeThanVs()
    {
        // dlnVp/dT should be negative, with smaller magnitude than dlnVs/dT
        var fo = GetMineral("fo");
        var kernel = _calculator.ComputeThermalSensitivity(fo, 10.0, 1500.0);

        _output.WriteLine($"dlnVp/dT = {kernel.DlnVp_dT:E4} /K");
        _output.WriteLine($"dlnVs/dT = {kernel.DlnVs_dT:E4} /K");
        Assert.True(kernel.DlnVp_dT < 0, "dlnVp/dT should be negative");
        Assert.True(Math.Abs(kernel.DlnVp_dT) < Math.Abs(kernel.DlnVs_dT),
            "|dlnVp/dT| should be less than |dlnVs/dT|");
    }

    [Fact]
    public void DlnRho_dT_Forsterite_10GPa_1500K_IsNegative()
    {
        // Density decreases with temperature due to thermal expansion
        var fo = GetMineral("fo");
        var kernel = _calculator.ComputeThermalSensitivity(fo, 10.0, 1500.0);

        _output.WriteLine($"dlnRho/dT = {kernel.DlnRho_dT:E4} /K");
        Assert.True(kernel.DlnRho_dT < 0, "dlnRho/dT should be negative (thermal expansion)");
    }

    [Fact]
    public void R_Thermal_Forsterite_10GPa_1500K_InExpectedRange()
    {
        // R = dlnVs/dlnVp should be ~1.5-2.5 for thermal anomalies
        // Reference: Trampert et al. (2004)
        var fo = GetMineral("fo");
        var kernel = _calculator.ComputeThermalSensitivity(fo, 10.0, 1500.0);

        _output.WriteLine($"R_thermal = {kernel.R_thermal:F3}");
        Assert.InRange(kernel.R_thermal, 1.0, 3.0);
    }

    [Fact]
    public void DlnVs_dT_MgPerovskite_50GPa_2000K_IsNegative()
    {
        // Lower mantle conditions: Mg-perovskite at 50 GPa, 2000 K
        var mpv = GetMineral("mpv");
        var kernel = _calculator.ComputeThermalSensitivity(mpv, 50.0, 2000.0);

        _output.WriteLine($"mpv dlnVs/dT = {kernel.DlnVs_dT:E4} /K");
        _output.WriteLine($"mpv dlnVp/dT = {kernel.DlnVp_dT:E4} /K");
        _output.WriteLine($"mpv dlnRho/dT = {kernel.DlnRho_dT:E4} /K");
        _output.WriteLine($"mpv R_thermal = {kernel.R_thermal:F3}");
        Assert.True(kernel.DlnVs_dT < 0, "dlnVs/dT should be negative for mpv at lower mantle conditions");
    }

    [Fact]
    public void ComputeOnPressureProfile_Returns_ValidResults()
    {
        // Batch evaluation over 5 pressure points
        var fo = GetMineral("fo");
        double[] pressures = { 5.0, 10.0, 15.0, 20.0, 25.0 };

        var kernels = _calculator.ComputeOnPressureProfile(fo, pressures, 1500.0);

        Assert.Equal(pressures.Length, kernels.Count);
        for (int i = 0; i < kernels.Count; i++)
        {
            _output.WriteLine($"P={kernels[i].Pressure:F1} GPa: dlnVs/dT={kernels[i].DlnVs_dT:E4}, " +
                              $"dlnVp/dT={kernels[i].DlnVp_dT:E4}, R={kernels[i].R_thermal:F3}");
            Assert.Equal(pressures[i], kernels[i].Pressure);
            Assert.Equal(1500.0, kernels[i].Temperature);
            Assert.True(kernels[i].DlnVs_dT < 0, $"dlnVs/dT should be negative at P={pressures[i]}");
            Assert.True(kernels[i].DlnVp_dT < 0, $"dlnVp/dT should be negative at P={pressures[i]}");
            Assert.True(kernels[i].DlnRho_dT < 0, $"dlnRho/dT should be negative at P={pressures[i]}");
            Assert.False(double.IsNaN(kernels[i].R_thermal), "R_thermal should not be NaN");
            Assert.False(double.IsInfinity(kernels[i].R_thermal), "R_thermal should not be Infinity");
        }
    }
}
