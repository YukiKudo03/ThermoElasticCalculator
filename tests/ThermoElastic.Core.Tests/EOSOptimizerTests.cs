using Xunit;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class EOSOptimizerTests
{
    private static MineralParams CreateForsterite()
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
            K2Prime = 0,
            GZero = 82.0,
            G1Prime = 1.5,
            G2Prime = 0,
            DebyeTempZero = 809.0,
            GammaZero = 0.99,
            QZero = 2.1,
            EhtaZero = 2.3,
            RefTemp = 300.0,
        };
    }

    [Fact]
    public void ExecOptimize_AtAmbientConditions_ReturnsReasonableValues()
    {
        var mineral = CreateForsterite();
        var optimizer = new MieGruneisenEOSOptimizer(mineral, 0.0001, 300.0);
        var result = optimizer.ExecOptimize();

        Assert.True(result.Density > 3.0 && result.Density < 4.0);
        Assert.True(result.KS > 100 && result.KS < 200);
        Assert.True(result.GS > 50 && result.GS < 150);
        Assert.True(result.Vp > 5000 && result.Vp < 12000);
        Assert.True(result.Vs > 3000 && result.Vs < 8000);
    }

    [Fact]
    public void ExecOptimize_HighPressure_IncreasesVelocity()
    {
        var mineral = CreateForsterite();
        var lowP = new MieGruneisenEOSOptimizer(mineral, 1.0, 300.0).ExecOptimize();
        var highP = new MieGruneisenEOSOptimizer(mineral, 10.0, 300.0).ExecOptimize();

        Assert.True(highP.Vp > lowP.Vp);
        Assert.True(highP.Density > lowP.Density);
    }
}
