using ThermoElastic.Core;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Literature verification tests: compare computed properties against known SLB2011 Table A1 values
/// and verify that new features (HS bounds, PREM, predefined rocks, isentrope) produce
/// physically reasonable results.
/// </summary>
public class LiteratureVerificationTests
{
    private readonly ITestOutputHelper _output;

    public LiteratureVerificationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private ResultSummary Compute(string mineralName, double P_GPa, double T_K)
    {
        var mineral = MineralDatabase.GetByName(mineralName)!;
        var optimizer = new MieGruneisenEOSOptimizer(mineral, P_GPa, T_K);
        return optimizer.ExecOptimize().ExportResults();
    }

    private static void AssertWithinPercent(double expected, double actual, double tolerancePct, string label)
    {
        double relErr = Math.Abs((actual - expected) / expected) * 100.0;
        Assert.True(relErr < tolerancePct,
            $"{label}: expected {expected:G6}, got {actual:G6}, error {relErr:F2}% (tolerance {tolerancePct}%)");
    }

    // ================================================================
    // Section 1: Ambient conditions (0.0001 GPa, 300 K) density from V0/M
    // Density = MolarWeight / V0, so this checks internal consistency and
    // that the thermal correction at 300K is small.
    // ================================================================

    [Theory]
    [InlineData("Forsterite",    140.693, 43.603)]
    [InlineData("Periclase",      40.304, 11.244)]
    [InlineData("Mg-Perovskite", 100.389, 24.445)]
    [InlineData("Stishovite",     60.084, 14.017)]
    [InlineData("Pyrope",        403.127, 113.08)]
    public void Ambient_Density_MatchesV0overM(string name, double M, double V0)
    {
        var result = Compute(name, 0.0001, 300.0);
        double expectedDensity = M / V0;

        _output.WriteLine($"{name}: rho = {result.Density:F4} g/cm3, M/V0 = {expectedDensity:F4}");
        AssertWithinPercent(expectedDensity, result.Density, 0.5, $"{name} ambient density vs M/V0");
    }

    [Theory]
    [InlineData("Forsterite",    128.0)]
    [InlineData("Periclase",     161.0)]
    [InlineData("Mg-Perovskite", 251.0)]
    [InlineData("Stishovite",    306.0)]
    [InlineData("Pyrope",        170.0)]
    public void Ambient_KS_WithinFivePercentOfK0(string name, double K0)
    {
        var result = Compute(name, 0.0001, 300.0);

        _output.WriteLine($"{name}: KS = {result.KS:F2} GPa, K0 = {K0:F2} GPa");
        // KS differs from K0 due to thermal contribution: KS = KT + gamma^2 * Cv * T / V
        // At 300K this is typically a few percent above K0
        AssertWithinPercent(K0, result.KS, 5.0, $"{name} ambient KS vs K0");
    }

    // ================================================================
    // Section 2: Comprehensive table at ambient and mantle conditions
    // ================================================================

    [Fact]
    public void ComprehensiveTable_AmbientAndMantleConditions()
    {
        var minerals = new[] { "Forsterite", "Periclase", "Mg-Perovskite", "Stishovite", "Pyrope" };
        var conditions = new[] { (0.0001, 300.0), (25.0, 1800.0) };

        _output.WriteLine("==========================================================================");
        _output.WriteLine("  LITERATURE VERIFICATION TABLE: SLB2011 Mineral Properties");
        _output.WriteLine("==========================================================================");
        _output.WriteLine($"{"Mineral",-16} {"P(GPa)",8} {"T(K)",7} {"rho",8} {"KS",8} {"KT",8} {"G",8} {"Vp",8} {"Vs",8}");
        _output.WriteLine($"{"",      -16} {"",      8} {"",   7} {"g/cm3",8} {"GPa", 8} {"GPa", 8} {"GPa",8} {"m/s", 8} {"m/s", 8}");
        _output.WriteLine("--------------------------------------------------------------------------");

        foreach (var name in minerals)
        {
            foreach (var (P, T) in conditions)
            {
                var r = Compute(name, P, T);
                _output.WriteLine($"{name,-16} {P,8:F4} {T,7:F0} {r.Density,8:F4} {r.KS,8:F2} {r.KT,8:F2} {r.GS,8:F2} {r.Vp,8:F0} {r.Vs,8:F0}");
            }
        }

        _output.WriteLine("==========================================================================");
        _output.WriteLine("");
        _output.WriteLine("SLB2011 Table A1 reference values at ambient (0.0001 GPa, 300 K):");
        _output.WriteLine("  Forsterite:    rho~3.222, KS~129, G~82");
        _output.WriteLine("  Periclase:     rho~3.585, KS~163, G~131");
        _output.WriteLine("  Mg-Perovskite: rho~4.104, KS~253, G~175");
        _output.WriteLine("  Stishovite:    rho~4.287, KS~316, G~220");
        _output.WriteLine("  Pyrope:        rho~3.565, KS~175, G~92");

        // Verify all computed values are physically reasonable
        foreach (var name in minerals)
        {
            foreach (var (P, T) in conditions)
            {
                var r = Compute(name, P, T);
                Assert.True(r.Density > 2.0 && r.Density < 8.0,
                    $"{name} density {r.Density} out of range at {P} GPa, {T} K");
                Assert.True(r.KS > 50 && r.KS < 2000,
                    $"{name} KS {r.KS} out of range at {P} GPa, {T} K");
                Assert.True(r.GS > 10 && r.GS < 1000,
                    $"{name} G {r.GS} out of range at {P} GPa, {T} K");
                Assert.True(r.Vp > 3000 && r.Vp < 20000,
                    $"{name} Vp {r.Vp} out of range at {P} GPa, {T} K");
                Assert.True(r.Vs > 2000 && r.Vs < 15000,
                    $"{name} Vs {r.Vs} out of range at {P} GPa, {T} K");
            }
        }
    }

    [Fact]
    public void HighPressure_DensityIncreasesOverLargeRange()
    {
        // Verify density at 50 GPa is significantly higher than at ambient
        var minerals = new[] { "Forsterite", "Periclase", "Mg-Perovskite", "Stishovite", "Pyrope" };
        foreach (var name in minerals)
        {
            var rLow = Compute(name, 0.0001, 1500.0);
            var rHigh = Compute(name, 50.0, 1500.0);
            Assert.True(rHigh.Density > rLow.Density * 1.05,
                $"{name}: density at 50 GPa ({rHigh.Density:F4}) should be >5% more than at ambient ({rLow.Density:F4})");
        }
    }

    [Fact]
    public void HighTemp_DensityDecreasesOverLargeRange()
    {
        // Verify density at 2500K is lower than at 300K
        var minerals = new[] { "Forsterite", "Periclase", "Mg-Perovskite" };
        foreach (var name in minerals)
        {
            var rCold = Compute(name, 10.0, 300.0);
            var rHot = Compute(name, 10.0, 2500.0);
            Assert.True(rHot.Density < rCold.Density,
                $"{name}: density at 2500K ({rHot.Density:F4}) should be less than at 300K ({rCold.Density:F4})");
        }
    }

    // ================================================================
    // Section 3: New feature verification - physically reasonable values
    // ================================================================

    [Fact]
    public void HashinShtrikman_BoundsAreOrderedCorrectly()
    {
        // HS bounds should bracket Hill average: Reuss <= Hill <= Voigt
        // and HS should also be between Reuss and Voigt
        var pyrolite = PredefinedRocks.Pyrolite();
        double P = 10.0, T = 1500.0;

        var reussCalc = new RockCalculator(pyrolite, P, T, MixtureMethod.Reuss);
        var (reuss, _) = reussCalc.Calculate();

        var hillCalc = new RockCalculator(pyrolite, P, T, MixtureMethod.Hill);
        var (hill, _) = hillCalc.Calculate();

        var voigtCalc = new RockCalculator(pyrolite, P, T, MixtureMethod.Voigt);
        var (voigt, _) = voigtCalc.Calculate();

        var hsCalc = new RockCalculator(pyrolite, P, T, MixtureMethod.HS);
        var (hs, _) = hsCalc.Calculate();

        _output.WriteLine("=== Mixing Method Comparison for Pyrolite at 10 GPa, 1500 K ===");
        _output.WriteLine($"  Reuss: KS={reuss!.KS:F2}, G={reuss.GS:F2}, Vp={reuss.Vp:F0}");
        _output.WriteLine($"  Hill:  KS={hill!.KS:F2}, G={hill.GS:F2}, Vp={hill.Vp:F0}");
        _output.WriteLine($"  Voigt: KS={voigt!.KS:F2}, G={voigt.GS:F2}, Vp={voigt.Vp:F0}");
        _output.WriteLine($"  HS:    KS={hs!.KS:F2}, G={hs.GS:F2}, Vp={hs.Vp:F0}");

        // Reuss <= Hill <= Voigt for both KS and G
        Assert.True(reuss.KS <= hill.KS + 0.01, "Reuss KS should be <= Hill KS");
        Assert.True(hill.KS <= voigt.KS + 0.01, "Hill KS should be <= Voigt KS");
        Assert.True(reuss.GS <= hill.GS + 0.01, "Reuss G should be <= Hill G");
        Assert.True(hill.GS <= voigt.GS + 0.01, "Hill G should be <= Voigt G");

        // HS should be between Reuss and Voigt
        Assert.True(hs.KS >= reuss.KS - 0.01 && hs.KS <= voigt.KS + 0.01,
            "HS KS should be between Reuss and Voigt");
        Assert.True(hs.GS >= reuss.GS - 0.01 && hs.GS <= voigt.GS + 0.01,
            "HS G should be between Reuss and Voigt");
    }

    [Fact]
    public void PREM_Profile_PressureAndDensityArePhysical()
    {
        var profile = PREMModel.GetProfile(100.0);

        _output.WriteLine("=== PREM Profile Spot Check ===");
        _output.WriteLine($"  Points: {profile.Count}");
        _output.WriteLine($"  Surface: depth={profile[0].Depth:F0} km, P={profile[0].Pressure:F2} GPa");
        _output.WriteLine($"  CMB:     depth={profile[^1].Depth:F0} km, P={profile[^1].Pressure:F2} GPa");

        // Surface pressure ~0
        Assert.True(profile[0].Pressure < 1.0, "Surface pressure should be near 0");
        // CMB pressure ~136 GPa
        Assert.True(profile[^1].Pressure > 100, "CMB pressure should be > 100 GPa");
        // Depth should be monotonically increasing
        for (int i = 1; i < profile.Count; i++)
        {
            Assert.True(profile[i].Depth > profile[i - 1].Depth,
                $"PREM depth should increase: {profile[i].Depth} <= {profile[i - 1].Depth}");
        }
    }

    [Fact]
    public void PredefinedRocks_PyroliteAtMantle_ReasonableVelocities()
    {
        // At 10 GPa (~300 km depth), upper mantle Vp ~ 8.5-9.5 km/s, Vs ~ 4.5-5.2 km/s
        var pyrolite = PredefinedRocks.Pyrolite();
        var calc = new RockCalculator(pyrolite, 10.0, 1500.0, MixtureMethod.Hill);
        var (result, individuals) = calc.Calculate();

        _output.WriteLine("=== Pyrolite at 10 GPa, 1500 K (Hill average) ===");
        _output.WriteLine($"  rho = {result!.Density:F4} g/cm3");
        _output.WriteLine($"  KS  = {result.KS:F2} GPa");
        _output.WriteLine($"  G   = {result.GS:F2} GPa");
        _output.WriteLine($"  Vp  = {result.Vp:F0} m/s ({result.Vp / 1000.0:F2} km/s)");
        _output.WriteLine($"  Vs  = {result.Vs:F0} m/s ({result.Vs / 1000.0:F2} km/s)");

        // PREM at 300 km: Vp~8648, Vs~4672, rho~3.466
        Assert.InRange(result.Vp, 7500, 10500);
        Assert.InRange(result.Vs, 4000, 6000);
        Assert.InRange(result.Density, 3.0, 4.5);

        _output.WriteLine("  Individual phases:");
        foreach (var (name, ratio, r) in individuals)
        {
            _output.WriteLine($"    {name} ({ratio:P0}): rho={r.Density:F4}, KS={r.KS:F2}, G={r.GS:F2}");
        }
    }

    [Fact]
    public void Isentrope_Forsterite_AdiabaticGradientMatchesExpected()
    {
        // The adiabatic gradient dT/dP = gamma * T / KS
        // For forsterite at shallow mantle: gamma ~ 1.1, T ~ 1600, KS ~ 130 => dT/dP ~ 13.5 K/GPa
        // At depth this decreases as KS increases faster than gamma*T
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var calc = new IsentropeCalculator(fo);
        var profile = calc.ComputeIsentrope(0.0001, 1600.0, pressureMax: 25.0, pressureStep: 1.0);

        _output.WriteLine("=== Forsterite Isentrope from 1600 K ===");
        _output.WriteLine($"{"P(GPa)",8} {"T(K)",8}");
        foreach (var pt in profile)
        {
            _output.WriteLine($"{pt.Pressure,8:F2} {pt.Temperature,8:F1}");
        }

        // Temperature should increase along isentrope
        Assert.True(profile[^1].Temperature > profile[0].Temperature,
            "Temperature should increase along isentrope");

        // Total temperature rise over 25 GPa should be roughly 100-400 K
        double dT = profile[^1].Temperature - profile[0].Temperature;
        _output.WriteLine($"  Total dT = {dT:F1} K over 25 GPa");
        Assert.InRange(dT, 50, 600);
    }

    [Fact]
    public void DepthConverter_25GPa_GivesAbout750km()
    {
        // PREM: 25 GPa corresponds to roughly 750 km depth
        double depth = DepthConverter.PressureToDepth(25.0);

        _output.WriteLine($"  25 GPa -> {depth:F1} km depth");
        Assert.InRange(depth, 600, 900);
    }

    [Fact]
    public void DepthConverter_135GPa_GivesCMB()
    {
        double depth = DepthConverter.PressureToDepth(135.0);

        _output.WriteLine($"  135 GPa -> {depth:F1} km depth (CMB ~ 2891 km)");
        Assert.InRange(depth, 2800, 2900);
    }
}
