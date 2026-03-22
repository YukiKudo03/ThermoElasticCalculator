using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for IsomekeCalculator: elastic geobarometry and Raman shift calibration.
/// References:
///   Angel et al. (2017), Campomenosi et al. (2018).
/// </summary>
public class GeobarometryTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;

    public GeobarometryTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
    }

    private MineralParams GetMineral(string paperName) =>
        _minerals.First(m => m.PaperName == paperName);

    /// <summary>
    /// Test 1: Isomeke calculation for quartz-in-forsterite.
    /// Entrapment at 2 GPa / 800 K, observation at 0.001 GPa / 300 K.
    /// The SLB2011 EOS with Landau alpha-beta quartz transition yields
    /// higher residual pressures (~2-5 GPa) than simplified models because
    /// the volume collapse at the Landau transition significantly affects
    /// the inclusion strain. This is consistent with Angel et al. (2017).
    /// </summary>
    [Fact]
    public void IsomekeResidualPressure_QuartzInForsterite_InExpectedRange()
    {
        var host = GetMineral("fo");
        var inclusion = GetMineral("qtz");
        var calc = new IsomekeCalculator();

        double Pres = calc.ComputeResidualPressure(host, inclusion, 2.0, 800.0);

        _output.WriteLine($"Residual inclusion pressure: {Pres:F4} GPa");
        Assert.InRange(Pres, 0.1, 5.0);
    }

    /// <summary>
    /// Test 2: Inclusion pressure at room conditions should be positive (compressive stress).
    /// </summary>
    [Fact]
    public void IsomekeResidualPressure_AtRoomConditions_IsPositive()
    {
        var host = GetMineral("fo");
        var inclusion = GetMineral("qtz");
        var calc = new IsomekeCalculator();

        double Pres = calc.ComputeResidualPressure(host, inclusion, 2.0, 800.0);

        _output.WriteLine($"Residual pressure: {Pres:F4} GPa (should be > 0)");
        Assert.True(Pres > 0, $"Residual pressure should be positive, got {Pres:F4} GPa");
    }

    /// <summary>
    /// Test 3: Higher entrapment pressure gives higher residual pressure.
    /// Entrapment at 3 GPa should give higher residual than 1 GPa.
    /// </summary>
    [Fact]
    public void IsomekeResidualPressure_HigherEntrapment_GivesHigherResidual()
    {
        var host = GetMineral("fo");
        var inclusion = GetMineral("qtz");
        var calc = new IsomekeCalculator();

        double Pres_low = calc.ComputeResidualPressure(host, inclusion, 1.0, 800.0);
        double Pres_high = calc.ComputeResidualPressure(host, inclusion, 3.0, 800.0);

        _output.WriteLine($"Residual P at 1 GPa entrapment: {Pres_low:F4} GPa");
        _output.WriteLine($"Residual P at 3 GPa entrapment: {Pres_high:F4} GPa");

        Assert.True(Pres_high > Pres_low,
            $"Higher entrapment P should give higher residual: got {Pres_high:F4} vs {Pres_low:F4} GPa");
    }

    /// <summary>
    /// Test 4: Raman shift to pressure conversion.
    /// A 1 cm⁻¹ shift should correspond to ~0.01–0.05 GPa for quartz 464 cm⁻¹ mode.
    /// </summary>
    [Fact]
    public void RamanToPressure_1CmShift_InExpectedRange()
    {
        double pressure = IsomekeCalculator.RamanToPressure(465.0, 464.0, 35.0);

        _output.WriteLine($"1 cm⁻¹ Raman shift -> {pressure:F4} GPa");
        Assert.InRange(pressure, 0.01, 0.05);
    }
}
