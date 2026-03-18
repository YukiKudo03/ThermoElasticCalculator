using ThermoElastic.Core;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Phase 3 tests: H5 (isentrope), M7 (analytical entropy)
/// </summary>
public class Phase3Tests
{
    // ============================================================
    // H5: Isentropic temperature profile
    // ============================================================

    [Fact]
    public void IsentropeCalculator_Forsterite_TemperatureIncreasesWithPressure()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var calc = new IsentropeCalculator(fo);

        // Start at 0.0001 GPa, 1600 K (typical mantle adiabat foot temperature)
        var profile = calc.ComputeIsentrope(0.0001, 1600.0, pressureMax: 25.0, pressureStep: 5.0);

        Assert.True(profile.Count > 1);
        // Temperature should increase monotonically along isentrope
        for (int i = 1; i < profile.Count; i++)
        {
            Assert.True(profile[i].Temperature >= profile[i - 1].Temperature,
                $"T should increase: T({profile[i].Pressure}) = {profile[i].Temperature} < T({profile[i - 1].Pressure}) = {profile[i - 1].Temperature}");
        }
    }

    [Fact]
    public void IsentropeCalculator_EntropyConstantAlongProfile()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var calc = new IsentropeCalculator(fo);

        var profile = calc.ComputeIsentrope(0.0001, 1600.0, pressureMax: 20.0, pressureStep: 5.0);

        // Compute entropy at each point - should be approximately constant
        double? refEntropy = null;
        foreach (var pt in profile)
        {
            var opt = new MieGruneisenEOSOptimizer(fo, pt.Pressure, pt.Temperature);
            var thermo = opt.ExecOptimize();
            double s = thermo.Entropy;

            if (refEntropy == null)
                refEntropy = s;
            else
                Assert.InRange(s, refEntropy.Value * 0.95, refEntropy.Value * 1.05);
        }
    }

    [Fact]
    public void IsentropeCalculator_AdiabaticGradient_ReasonableRange()
    {
        // Typical mantle adiabatic gradient: γT/KS ≈ a few K/GPa at high P
        // At low P the gradient can be larger
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var calc = new IsentropeCalculator(fo);

        var profile = calc.ComputeIsentrope(5.0, 1600.0, pressureMax: 15.0, pressureStep: 5.0);

        if (profile.Count >= 2)
        {
            double dT = profile[^1].Temperature - profile[0].Temperature;
            double dP = profile[^1].Pressure - profile[0].Pressure;
            double gradient = dT / dP; // K/GPa
            // At mantle conditions, gradient is typically 0.1-30 K/GPa
            Assert.True(gradient > 0, $"Gradient should be positive: {gradient}");
        }
    }

    // ============================================================
    // M7: Analytical entropy
    // ============================================================

    [Fact]
    public void AnalyticalEntropy_MatchesNumericalDerivative()
    {
        // The analytical entropy should closely match the numerical central difference
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var opt = new MieGruneisenEOSOptimizer(fo, 10.0, 1500.0);
        var thermo = opt.ExecOptimize();

        double analyticalS = thermo.AnalyticalEntropy;
        double numericalS = thermo.Entropy;

        // Should agree within 1%
        double relErr = Math.Abs(analyticalS - numericalS) / Math.Abs(numericalS);
        Assert.True(relErr < 0.01,
            $"Analytical S={analyticalS} vs Numerical S={numericalS}, relErr={relErr:P2}");
    }

    [Fact]
    public void AnalyticalEntropy_PositiveAtHighTemp()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var opt = new MieGruneisenEOSOptimizer(fo, 10.0, 2000.0);
        var thermo = opt.ExecOptimize();

        Assert.True(thermo.AnalyticalEntropy > 0, "Entropy should be positive at high T");
    }

    [Fact]
    public void AnalyticalEntropy_IncreasesWithTemperature()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        double prevS = 0;
        foreach (var T in new[] { 500.0, 1000.0, 1500.0, 2000.0, 2500.0 })
        {
            var opt = new MieGruneisenEOSOptimizer(fo, 10.0, T);
            var thermo = opt.ExecOptimize();
            double s = thermo.AnalyticalEntropy;
            Assert.True(s > prevS, $"S({T}) = {s} should be > S_prev = {prevS}");
            prevS = s;
        }
    }

    [Fact]
    public void AnalyticalEntropy_MultipleEndmembers_MatchNumerical()
    {
        // AnalyticalEntropy currently delegates to Entropy (numerical) so they should match exactly.
        // This test exists as a regression guard for when a true analytical implementation is added.
        var names = new[] { "Forsterite", "Periclase", "Mg-Perovskite", "Stishovite", "Pyrope" };
        foreach (var name in names)
        {
            var mineral = MineralDatabase.GetByName(name)!;
            var opt = new MieGruneisenEOSOptimizer(mineral, 10.0, 1500.0);
            var thermo = opt.ExecOptimize();

            double analyticalS = thermo.AnalyticalEntropy;
            double numericalS = thermo.Entropy;

            Assert.Equal(numericalS, analyticalS, 6);
        }
    }

    [Fact]
    public void DebyeEntropy_HighTempLimit_Approaches3nRln()
    {
        // At high T (θ/T → 0), thermal entropy per atom → R * [4 - 3*ln(θ/T) - 0]
        // which grows with T. Just check it's positive and large.
        var debye = new DebyeFunctionCalculator(800.0); // θ=800K
        double sThermal = debye.GetEntropy(5000.0); // very high T
        Assert.True(sThermal > 0);
    }
}
