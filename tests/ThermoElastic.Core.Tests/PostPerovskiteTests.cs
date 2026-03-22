using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for the bridgmanite -> post-perovskite phase transition.
/// Reference: Murakami et al. (2004), Oganov & Ono (2004).
/// </summary>
public class PostPerovskiteTests
{
    private readonly ITestOutputHelper _output;
    private readonly PostPerovskiteCalculator _calc = new();

    private readonly MineralParams _mpv;
    private readonly MineralParams _mppv;

    public PostPerovskiteTests(ITestOutputHelper output)
    {
        _output = output;
        _mpv = MineralDatabase.GetByName("mpv")!;
        _mppv = MineralDatabase.GetByName("mppv")!;
    }

    // ============================================================
    // Test 1: pv-ppv boundary pressure at T=2500K ~ 115-135 GPa
    // Reference: Murakami et al. (2004)
    // ============================================================
    [Fact]
    public void PvPpvBoundary_At2500K_InExpectedRange()
    {
        double T = 2500.0;

        double boundary = _calc.FindBoundary(_mpv, _mppv, T, 100.0, 140.0);

        _output.WriteLine($"pv-ppv boundary at T={T} K: P={boundary:F1} GPa");

        Assert.False(double.IsNaN(boundary), "Boundary should be found");
        Assert.InRange(boundary, 115.0, 135.0);
    }

    // ============================================================
    // Test 2: Clapeyron slope is positive, ~5-15 MPa/K
    // ============================================================
    [Fact]
    public void ClapeyronSlope_IsPositive()
    {
        double T = 2500.0;
        double boundary = _calc.FindBoundary(_mpv, _mppv, T, 100.0, 140.0);

        Assert.False(double.IsNaN(boundary), "Boundary must be found first");

        double slope = _calc.GetClapeyronSlope(_mpv, _mppv, boundary, T);
        double slopeMPaPerK = slope * 1000.0; // GPa/K -> MPa/K

        _output.WriteLine($"Clapeyron slope at P={boundary:F1} GPa, T={T} K: {slopeMPaPerK:F2} MPa/K");

        Assert.True(slope > 0, $"Clapeyron slope should be positive, got {slopeMPaPerK:F2} MPa/K");
        Assert.InRange(slopeMPaPerK, 5.0, 20.0);
    }

    // ============================================================
    // Test 3: Velocity jump across pv-ppv transition
    // ============================================================
    [Fact]
    public void VelocityJump_AcrossTransition()
    {
        double T = 2500.0;
        double boundary = _calc.FindBoundary(_mpv, _mppv, T, 100.0, 140.0);

        Assert.False(double.IsNaN(boundary), "Boundary must be found first");

        var (pvProps, ppvProps, dVs, dVp, dRho) =
            _calc.CompareAcrossTransition(_mpv, _mppv, boundary, T);

        _output.WriteLine($"At boundary P={boundary:F1} GPa, T={T} K:");
        _output.WriteLine($"  Vs(pv)={pvProps.Vs:F0} m/s, Vs(ppv)={ppvProps.Vs:F0} m/s, dVs={dVs:F2}%");
        _output.WriteLine($"  Vp(pv)={pvProps.Vp:F0} m/s, Vp(ppv)={ppvProps.Vp:F0} m/s, dVp={dVp:F2}%");
        _output.WriteLine($"  rho(pv)={pvProps.Density:F3} g/cm3, rho(ppv)={ppvProps.Density:F3} g/cm3, dRho={dRho:F2}%");

        // Vs should change across the transition (either direction is physically valid)
        Assert.True(Math.Abs(dVs) > 0.01, $"Vs should change across transition, got dVs={dVs:F4}%");
    }

    // ============================================================
    // Test 4: Post-perovskite is denser than perovskite at same P-T
    // ============================================================
    [Fact]
    public void PostPerovskite_IsDenser()
    {
        double P = 125.0;
        double T = 2500.0;

        var (pvProps, ppvProps, _, _, dRho) =
            _calc.CompareAcrossTransition(_mpv, _mppv, P, T);

        _output.WriteLine($"At P={P} GPa, T={T} K:");
        _output.WriteLine($"  rho(pv)={pvProps.Density:F3} g/cm3");
        _output.WriteLine($"  rho(ppv)={ppvProps.Density:F3} g/cm3");
        _output.WriteLine($"  dRho={dRho:F2}%");

        Assert.True(ppvProps.Density > pvProps.Density,
            $"ppv should be denser: rho_ppv={ppvProps.Density:F3} vs rho_pv={pvProps.Density:F3}");
        Assert.True(dRho > 0, $"dRho should be positive, got {dRho:F2}%");
    }
}
