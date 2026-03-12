using Xunit;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class MagneticContributionTests
{
    private static MineralParams CreateForsteriteNoMagnetic()
    {
        return new MineralParams
        {
            MineralName = "Forsterite",
            PaperName = "fo",
            NumAtoms = 7,
            MolarVolume = 43.6,
            MolarWeight = 140.69,
            KZero = 128.0,
            K1Prime = 4.2,
            GZero = 82.0,
            G1Prime = 1.5,
            DebyeTempZero = 809.0,
            GammaZero = 0.99,
            QZero = 2.1,
            EhtaZero = 2.3,
            RefTemp = 300.0,
            MagneticAtomCount = 0,
            SpinQuantumNumber = 0,
        };
    }

    [Fact]
    public void MagneticFreeEnergy_WhenNoMagneticAtoms_IsZero()
    {
        var mineral = CreateForsteriteNoMagnetic();
        var optimizer = new MieGruneisenEOSOptimizer(mineral, 0.0001, 300.0);
        var result = optimizer.ExecOptimize();
        Assert.Equal(0.0, result.MagneticFreeEnergy);
    }

    [Fact]
    public void MagneticFreeEnergy_Fe2PlusHighSpin_IsCorrect()
    {
        // Fe²⁺ high spin: S=2, r=1
        var mineral = CreateForsteriteNoMagnetic();
        mineral.MineralName = "Fayalite";
        mineral.SpinQuantumNumber = 2.0;
        mineral.MagneticAtomCount = 1.0;
        double T = 300.0;

        var optimizer = new MieGruneisenEOSOptimizer(mineral, 0.0001, T);
        var result = optimizer.ExecOptimize();

        // F_mag = -T * r * R * ln(2S + 1) = -300 * 1 * 8.31477 * ln(5)
        double expected = -T * 1.0 * 8.31477 * Math.Log(5.0);
        Assert.Equal(expected, result.MagneticFreeEnergy, 1);
    }

    [Fact]
    public void ExistingTests_UnaffectedByMagneticDefaults()
    {
        // Forsterite with default (0) magnetic params should give same results
        var mineral = CreateForsteriteNoMagnetic();
        var optimizer = new MieGruneisenEOSOptimizer(mineral, 0.0001, 300.0);
        var result = optimizer.ExecOptimize();

        Assert.True(result.Density > 3.0 && result.Density < 4.0);
        Assert.True(result.KS > 100 && result.KS < 200);
    }
}
