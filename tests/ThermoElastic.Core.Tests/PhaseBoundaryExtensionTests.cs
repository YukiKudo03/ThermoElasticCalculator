using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for multi-phase boundary extension of PhaseDiagramCalculator.
/// References: Katsura & Ito (1989), Akaogi et al. (1989), Ito & Takahashi (1989).
/// </summary>
public class PhaseBoundaryExtensionTests
{
    private readonly PhaseDiagramCalculator _calc = new();

    private static PhaseEntry MakePhase(string name)
    {
        var mineral = MineralDatabase.GetByName(name)!;
        return new PhaseEntry { Name = mineral.MineralName, Mineral = mineral, Amount = 1.0 };
    }

    // ============================================================
    // Test 1: 410 km olivine→wadsleyite boundary
    // ============================================================
    [Fact]
    public void FindPhaseBoundary_410km_FoToMw_At1600K()
    {
        // Reference: Katsura & Ito (1989)
        var fo = MakePhase("fo");
        var mw = MakePhase("mw");

        double boundary = _calc.FindPhaseBoundary(fo, mw, 1600.0, 5.0, 25.0);

        Assert.False(double.IsNaN(boundary), "Boundary should be found");
        Assert.InRange(boundary, 13.0, 14.5);
    }

    // ============================================================
    // Test 2: 410 km Clapeyron slope (positive)
    // ============================================================
    [Fact]
    public void ClapeyronSlope_410km_FoToMw_Positive()
    {
        // Reference: Akaogi et al. (1989) report ~3.6 MPa/K
        var fo = MakePhase("fo");
        var mw = MakePhase("mw");

        double pAt1400 = _calc.FindPhaseBoundary(fo, mw, 1400.0, 5.0, 25.0);
        double pAt1800 = _calc.FindPhaseBoundary(fo, mw, 1800.0, 5.0, 25.0);

        Assert.False(double.IsNaN(pAt1400), "Boundary at 1400K should be found");
        Assert.False(double.IsNaN(pAt1800), "Boundary at 1800K should be found");

        double dPdT = (pAt1800 - pAt1400) / (1800.0 - 1400.0);
        Assert.InRange(dPdT, 0.002, 0.004); // 2-4 MPa/K, positive
    }

    // ============================================================
    // Test 3: 660 km multi-phase boundary
    // ============================================================
    [Fact]
    public void FindMultiPhaseBoundary_660km_MrwToMpvPe_At1900K()
    {
        // Reaction: Mg2SiO4 (ringwoodite) → MgSiO3 (bridgmanite) + MgO (periclase)
        // Reference: Ito & Takahashi (1989)
        var mrw = MakePhase("mrw");
        var mpv = MakePhase("mpv");
        var pe = MakePhase("pe");

        var reactants = new List<(PhaseEntry Phase, double Stoichiometry)> { (mrw, 1.0) };
        var products = new List<(PhaseEntry Phase, double Stoichiometry)> { (mpv, 1.0), (pe, 1.0) };

        double boundary = _calc.FindMultiPhaseBoundary(reactants, products, 1900.0, 15.0, 30.0);

        Assert.False(double.IsNaN(boundary), "660 km boundary should be found");
        Assert.InRange(boundary, 22.0, 25.0);
    }

    // ============================================================
    // Test 4: 660 km Clapeyron slope (negative)
    // ============================================================
    [Fact]
    public void ClapeyronSlope_660km_MrwToMpvPe_Negative()
    {
        // Reference: Ito & Takahashi (1989), dP/dT ~ -1 to -3 MPa/K
        var mrw = MakePhase("mrw");
        var mpv = MakePhase("mpv");
        var pe = MakePhase("pe");

        var reactants = new List<(PhaseEntry Phase, double Stoichiometry)> { (mrw, 1.0) };
        var products = new List<(PhaseEntry Phase, double Stoichiometry)> { (mpv, 1.0), (pe, 1.0) };

        double pAt1700 = _calc.FindMultiPhaseBoundary(reactants, products, 1700.0, 15.0, 30.0);
        double pAt2100 = _calc.FindMultiPhaseBoundary(reactants, products, 2100.0, 15.0, 30.0);

        Assert.False(double.IsNaN(pAt1700), "Boundary at 1700K should be found");
        Assert.False(double.IsNaN(pAt2100), "Boundary at 2100K should be found");

        double dPdT = (pAt2100 - pAt1700) / (2100.0 - 1700.0);
        Assert.InRange(dPdT, -0.003, -0.001); // negative slope
    }

    // ============================================================
    // Test 5: Temperature inversion from boundary pressure
    // ============================================================
    [Fact]
    public void FindBoundaryTemperature_410km_FoMw_At13_8GPa()
    {
        var fo = MakePhase("fo");
        var mw = MakePhase("mw");

        double T = _calc.FindBoundaryTemperature(fo, mw, 13.8, 1200.0, 2200.0);

        Assert.False(double.IsNaN(T), "Temperature should be found");
        Assert.InRange(T, 1500.0, 1700.0);
    }

    // ============================================================
    // Test 6: Phase boundary tracing
    // ============================================================
    [Fact]
    public void TracePhaseBoundary_FoMw_SmoothPositiveSlope()
    {
        var fo = MakePhase("fo");
        var mw = MakePhase("mw");

        var curve = _calc.TracePhaseBoundary(fo, mw, 1200.0, 2200.0, 10, 5.0, 25.0);

        // Should have 10 points
        Assert.Equal(10, curve.Count);

        // All pressures should be valid (not NaN)
        foreach (var (p, t) in curve)
        {
            Assert.False(double.IsNaN(p), $"Pressure at T={t} should not be NaN");
            Assert.InRange(p, 5.0, 25.0);
        }

        // Positive Clapeyron slope: P should increase with T
        for (int i = 1; i < curve.Count; i++)
        {
            Assert.True(curve[i].P >= curve[i - 1].P,
                $"P should increase with T: P({curve[i].T})={curve[i].P} < P({curve[i - 1].T})={curve[i - 1].P}");
        }
    }

    // ============================================================
    // Test 7: Clapeyron slope via Clausius-Clapeyron
    // ============================================================
    [Fact]
    public void ComputeClapeyronSlope_FoMw_At1600K_MatchesNumerical()
    {
        var fo = MakePhase("fo");
        var mw = MakePhase("mw");

        // Find boundary pressure at 1600K
        double pBoundary = _calc.FindPhaseBoundary(fo, mw, 1600.0, 5.0, 25.0);
        Assert.False(double.IsNaN(pBoundary), "Boundary should be found");

        // Compute Clapeyron slope analytically: dP/dT = ΔS/ΔV
        double analyticalSlope = _calc.ComputeClapeyronSlope(fo, mw, pBoundary, 1600.0);

        // Compute numerical slope from two nearby temperatures
        double dT = 100.0;
        double pPlus = _calc.FindPhaseBoundary(fo, mw, 1600.0 + dT, 5.0, 25.0);
        double pMinus = _calc.FindPhaseBoundary(fo, mw, 1600.0 - dT, 5.0, 25.0);
        double numericalSlope = (pPlus - pMinus) / (2.0 * dT);

        // Should match within 20%
        Assert.True(analyticalSlope > 0, "Clapeyron slope for fo→mw should be positive");
        double ratio = analyticalSlope / numericalSlope;
        Assert.InRange(ratio, 0.8, 1.2);
    }
}
