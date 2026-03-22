using ThermoElastic.Core.Calculations;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for magma ocean solidification calculations.
/// References: Hirschmann (2000), Andrault et al. (2011).
/// </summary>
public class MagmaOceanTests
{
    private readonly ITestOutputHelper _output;
    private readonly MagmaOceanCalculator _calc = new();

    public MagmaOceanTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ============================================================
    // Test 1: Peridotite solidus at 25 GPa ~ 2200-2600 K
    // Reference: Andrault et al. (2011)
    // ============================================================
    [Fact]
    public void ComputeSolidus_At25GPa_InAndraultRange()
    {
        double P = 25.0;
        double solidus = _calc.ComputeSolidus(P);

        _output.WriteLine($"Solidus at {P} GPa: {solidus:F1} K");

        Assert.InRange(solidus, 2200.0, 2600.0);
    }

    // ============================================================
    // Test 2: Liquidus is always above solidus
    // ============================================================
    [Fact]
    public void Liquidus_AlwaysAboveSolidus()
    {
        double[] pressures = { 0.0, 5.0, 10.0, 15.0, 20.0, 25.0, 30.0 };

        foreach (double P in pressures)
        {
            double solidus = _calc.ComputeSolidus(P);
            double liquidus = _calc.ComputeLiquidus(P);

            _output.WriteLine($"P={P:F0} GPa: solidus={solidus:F1} K, liquidus={liquidus:F1} K");

            Assert.True(liquidus > solidus,
                $"Liquidus ({liquidus:F1} K) should be above solidus ({solidus:F1} K) at P={P} GPa");
        }
    }

    // ============================================================
    // Test 3: Solidus increases with pressure up to ~25 GPa
    // ============================================================
    [Fact]
    public void Solidus_IncreasesWithPressure_UpTo25GPa()
    {
        double[] pressures = { 1.0, 5.0, 10.0, 15.0, 20.0, 25.0 };

        double prevSolidus = 0.0;
        foreach (double P in pressures)
        {
            double solidus = _calc.ComputeSolidus(P);
            _output.WriteLine($"P={P:F0} GPa: solidus={solidus:F1} K");

            Assert.True(solidus > prevSolidus,
                $"Solidus at {P} GPa ({solidus:F1} K) should be higher than at lower P ({prevSolidus:F1} K)");
            prevSolidus = solidus;
        }
    }

    // ============================================================
    // Test 4: Solidification sequence at 10 GPa cooling from 3000 K
    // First mineral crystallizes below liquidus (in partial melt zone)
    // ============================================================
    [Fact]
    public void SolidificationSequence_At10GPa_CoolingFrom3000K()
    {
        double P = 10.0;
        double solidus = _calc.ComputeSolidus(P);
        double liquidus = _calc.ComputeLiquidus(P);

        _output.WriteLine($"At {P} GPa: solidus={solidus:F1} K, liquidus={liquidus:F1} K");

        // At 3000 K, should be fully liquid
        Assert.Equal("Liquid", _calc.GetMeltingState(P, 3000.0));
        Assert.Equal(1.0, _calc.ComputeMeltFraction(P, 3000.0));

        // Just below liquidus, should be partial melt with high melt fraction
        double justBelowLiquidus = liquidus - 10.0;
        Assert.Equal("Partial Melt", _calc.GetMeltingState(P, justBelowLiquidus));
        double meltFraction = _calc.ComputeMeltFraction(P, justBelowLiquidus);
        _output.WriteLine($"Melt fraction just below liquidus: {meltFraction:F3}");
        Assert.True(meltFraction > 0.9, "Melt fraction just below liquidus should be > 0.9");

        // Below solidus, should be fully solid
        Assert.Equal("Solid", _calc.GetMeltingState(P, solidus - 100.0));
        Assert.Equal(0.0, _calc.ComputeMeltFraction(P, solidus - 100.0));

        // Midway between solidus and liquidus
        double midT = (solidus + liquidus) / 2.0;
        double midFraction = _calc.ComputeMeltFraction(P, midT);
        _output.WriteLine($"Melt fraction at midpoint ({midT:F1} K): {midFraction:F3}");
        Assert.InRange(midFraction, 0.4, 0.6);
    }
}
