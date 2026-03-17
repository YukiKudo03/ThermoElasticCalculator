using Xunit;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Core.Tests;

public class GibbsFreeEnergyTests
{
    private static MineralParams CreateForsterite()
    {
        return MineralDatabase.GetByName("fo")!;
    }

    [Fact]
    public void FCold_AtZeroStrain_IsZero()
    {
        var mineral = CreateForsterite();
        var th = new ThermoMineralParams(0.0, 300.0, mineral);
        Assert.Equal(0.0, th.FCold, 6);
    }

    [Fact]
    public void FThermal_AtRefTemp_IsNearZero()
    {
        var mineral = CreateForsterite();
        // At zero strain and reference temperature, thermal contribution should be near zero
        var th = new ThermoMineralParams(0.0, mineral.RefTemp, mineral);
        // Not exactly zero because Debye temp changes with strain,
        // but at f=0, vibrational Debye temp = DebyeTempZero, so it should be very close
        Assert.True(Math.Abs(th.FThermal) < 0.1, $"FThermal at RefTemp should be near 0, got {th.FThermal}");
    }

    [Fact]
    public void GibbsG_EqualsF_Plus_PV()
    {
        var mineral = CreateForsterite();
        var optimizer = new MieGruneisenEOSOptimizer(mineral, 10.0, 1500.0);
        var th = optimizer.ExecOptimize();

        double expectedG = th.HelmholtzF + th.Pressure * th.Volume;
        Assert.Equal(expectedG, th.GibbsG, 6);
    }

    [Fact]
    public void Entropy_IsNegativeDerivativeOfF()
    {
        var mineral = CreateForsterite();
        var optimizer = new MieGruneisenEOSOptimizer(mineral, 5.0, 1000.0);
        var th = optimizer.ExecOptimize();

        // S should be positive at T>0
        Assert.True(th.Entropy > 0, $"Entropy should be positive, got {th.Entropy}");
    }

    [Fact]
    public void Entropy_NumericalDerivativeCheck()
    {
        var mineral = CreateForsterite();
        double P = 5.0;
        double T = 1000.0;
        double dT = 1.0;

        var thCenter = new MieGruneisenEOSOptimizer(mineral, P, T).ExecOptimize();
        var thPlus = new MieGruneisenEOSOptimizer(mineral, P, T + dT).ExecOptimize();
        var thMinus = new MieGruneisenEOSOptimizer(mineral, P, T - dT).ExecOptimize();

        double numericalS = -(thPlus.HelmholtzF - thMinus.HelmholtzF) / (2.0 * dT) * 1000.0;

        // Within 5% tolerance (different finite strains at different P-T points)
        double relError = Math.Abs(thCenter.Entropy - numericalS) / Math.Abs(numericalS);
        Assert.True(relError < 0.05, $"Entropy mismatch: {thCenter.Entropy} vs numerical {numericalS}, relError={relError}");
    }

    [Fact]
    public void Forsterite_AtAmbient_GibbsConsistentWithF0()
    {
        var mineral = CreateForsterite();
        var optimizer = new MieGruneisenEOSOptimizer(mineral, 0.0001, 300.0);
        var th = optimizer.ExecOptimize();

        // At ambient conditions, G ≈ F0 + small corrections
        // F0 for forsterite is -2055.403 kJ/mol
        // G should be close to F0 since P≈0 and T=T_ref
        double diff = Math.Abs(th.GibbsG - mineral.F0);
        Assert.True(diff < 50, $"G at ambient ({th.GibbsG}) should be close to F0 ({mineral.F0}), diff={diff}");
    }

    [Fact]
    public void LandauMagnetic_Zero_NoEffectOnGibbs()
    {
        // Forsterite has no Landau/magnetic params
        var mineral = CreateForsterite();
        Assert.Equal(0.0, mineral.Tc0);
        Assert.Equal(0.0, mineral.MagneticAtomCount);

        var optimizer = new MieGruneisenEOSOptimizer(mineral, 5.0, 1000.0);
        var th = optimizer.ExecOptimize();
        Assert.Equal(0.0, th.LandauFreeEnergy);
        Assert.Equal(0.0, th.MagneticFreeEnergy);
    }

    [Fact]
    public void F0_Zero_ExistingCalcUnaffected()
    {
        // Original forsterite params without F0
        var mineral = new MineralParams
        {
            MineralName = "Forsterite", PaperName = "fo",
            NumAtoms = 7, MolarVolume = 43.6, MolarWeight = 140.69,
            KZero = 128.0, K1Prime = 4.2, GZero = 82.0, G1Prime = 1.5,
            DebyeTempZero = 809.0, GammaZero = 0.99, QZero = 2.1, EhtaZero = 2.3,
            RefTemp = 300.0,
            F0 = 0, // default
        };

        var optimizer = new MieGruneisenEOSOptimizer(mineral, 0.0001, 300.0);
        var result = optimizer.ExecOptimize();

        // Existing properties should still be reasonable
        Assert.True(result.Density > 3.0 && result.Density < 4.0);
        Assert.True(result.KS > 100 && result.KS < 200);
    }
}
