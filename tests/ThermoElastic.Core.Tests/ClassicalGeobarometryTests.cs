using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for classical geothermobarometry using phase equilibrium.
/// References: Katsura &amp; Ito (1989), Akaogi et al. (1989).
/// </summary>
public class ClassicalGeobarometryTests
{
    private readonly ITestOutputHelper _output;
    private readonly ClassicalGeobarometer _barometer = new();
    private readonly MineralParams _fo;
    private readonly MineralParams _mw;

    public ClassicalGeobarometryTests(ITestOutputHelper output)
    {
        _output = output;
        _fo = MineralDatabase.GetByName("fo")!;
        _mw = MineralDatabase.GetByName("mw")!;
    }

    // ============================================================
    // Test 1: Olivine-wadsleyite transition as barometer
    // fo -> mw at 1600 K gives P ~ 13-14.5 GPa
    // Reference: Katsura & Ito (1989)
    // ============================================================
    [Fact]
    public void EstimatePressure_FoToMw_At1600K_InExpectedRange()
    {
        double T = 1600.0;
        double P = _barometer.EstimatePressure(_fo, _mw, T, 5.0, 25.0);

        _output.WriteLine($"fo->mw boundary at T={T} K: P={P:F2} GPa");

        Assert.False(double.IsNaN(P), "Boundary pressure should be found");
        Assert.InRange(P, 13.0, 14.5);
    }

    // ============================================================
    // Test 2: Exchange thermometer concept
    // At higher T, Gibbs difference between phases changes
    // ============================================================
    [Fact]
    public void GibbsDifference_VariesWithTemperature()
    {
        double P = 14.0; // GPa, near the fo-mw boundary

        var (_, dG1400) = _barometer.DetermineStablePhase(_fo, _mw, P, 1400.0);
        var (_, dG1800) = _barometer.DetermineStablePhase(_fo, _mw, P, 1800.0);

        _output.WriteLine($"DeltaG at 1400 K: {dG1400:F2} kJ/mol");
        _output.WriteLine($"DeltaG at 1800 K: {dG1800:F2} kJ/mol");

        // The Gibbs difference should change with temperature
        Assert.NotEqual(dG1400, dG1800);
        // The magnitude or sign of the difference should shift
        double change = Math.Abs(dG1800 - dG1400);
        _output.WriteLine($"Change in DeltaG: {change:F2} kJ/mol");
        Assert.True(change > 0.1, "Gibbs difference should vary meaningfully with T");
    }

    // ============================================================
    // Test 3: Pseudosection-like grid
    // 3x3 P-T grid, verify fo stable at low P and mw at high P
    // ============================================================
    [Fact]
    public void ComputePseudosection_FoMw_BoundaryInExpectedRange()
    {
        double[] pressures = { 10.0, 14.0, 18.0 };
        double[] temperatures = { 1400.0, 1600.0, 1800.0 };

        var grid = _barometer.ComputePseudosection(_fo, _mw, pressures, temperatures);

        // Log the grid
        for (int i = 0; i < pressures.Length; i++)
        {
            for (int j = 0; j < temperatures.Length; j++)
            {
                _output.WriteLine($"P={pressures[i]:F0} GPa, T={temperatures[j]:F0} K: {grid[i, j]}");
            }
        }

        // At 10 GPa (low P), forsterite should be stable at all temperatures
        for (int j = 0; j < temperatures.Length; j++)
        {
            Assert.Equal(_fo.MineralName, grid[0, j]);
        }

        // At 18 GPa (high P), wadsleyite should be stable at all temperatures
        for (int j = 0; j < temperatures.Length; j++)
        {
            Assert.Equal(_mw.MineralName, grid[2, j]);
        }

        // At 14 GPa (near boundary), there should be a transition
        // At least one temperature should show each phase, or all same is acceptable
        // since 14 GPa is close to boundary
    }

    // ============================================================
    // Test 4: P-T path recovery
    // Given a phase boundary P, infer T using FindBoundaryTemperature
    // ============================================================
    [Fact]
    public void EstimateTemperature_FromBoundaryPressure_Consistent()
    {
        // First find boundary P at 1600 K
        double T_known = 1600.0;
        double P_boundary = _barometer.EstimatePressure(_fo, _mw, T_known, 5.0, 25.0);
        Assert.False(double.IsNaN(P_boundary), "Boundary pressure should be found");

        _output.WriteLine($"Boundary P at T={T_known} K: {P_boundary:F2} GPa");

        // Now recover T from the boundary P
        double T_recovered = _barometer.EstimateTemperature(_fo, _mw, P_boundary, 1200.0, 2200.0);

        _output.WriteLine($"Recovered T at P={P_boundary:F2} GPa: {T_recovered:F1} K");

        Assert.False(double.IsNaN(T_recovered), "Temperature should be recovered");
        // Recovered T should be close to original
        Assert.InRange(T_recovered, T_known - 100.0, T_known + 100.0);
    }
}
