using ThermoElastic.Core;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Phase 2 tests: H1 (HS N-component), H6 (PhaseDiagram), M2 (PREM), M3 (predefined rocks), M4 (depth axis)
/// </summary>
public class Phase2Tests
{
    // ============================================================
    // H1: Hashin-Shtrikman bounds for N-component mixtures
    // ============================================================

    [Fact]
    public void HashinShtrikmanLower_TwoComponents_BetweenReussAndVoigt()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var en = MineralDatabase.GetByName("Enstatite")!;

        var optFo = new MieGruneisenEOSOptimizer(fo, 10.0, 1500.0);
        var optEn = new MieGruneisenEOSOptimizer(en, 10.0, 1500.0);
        var resFo = optFo.ExecOptimize().ExportResults();
        var resEn = optEn.ExecOptimize().ExportResults();

        var inputs = new List<(double ratio, ResultSummary result)>
        {
            (0.6, resFo), (0.4, resEn)
        };
        var mixer = new MixtureCalculator(inputs);

        var hs = mixer.HashinShtrikmanAverage();
        var voigt = mixer.VoigtAverage();
        var reuss = mixer.ReussAverage();

        Assert.NotNull(hs);
        // HS should be between Reuss and Voigt
        Assert.True(hs!.KS >= reuss!.KS && hs.KS <= voigt!.KS,
            $"HS KS {hs.KS} should be between Reuss {reuss.KS} and Voigt {voigt.KS}");
        Assert.True(hs.GS >= reuss.GS && hs.GS <= voigt.GS,
            $"HS GS {hs.GS} should be between Reuss {reuss.GS} and Voigt {voigt.GS}");
    }

    [Fact]
    public void HashinShtrikmanLower_ThreeComponents_BetweenReussAndVoigt()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var en = MineralDatabase.GetByName("Enstatite")!;
        var di = MineralDatabase.GetByName("Diopside")!;

        var optFo = new MieGruneisenEOSOptimizer(fo, 10.0, 1500.0);
        var optEn = new MieGruneisenEOSOptimizer(en, 10.0, 1500.0);
        var optDi = new MieGruneisenEOSOptimizer(di, 10.0, 1500.0);
        var resFo = optFo.ExecOptimize().ExportResults();
        var resEn = optEn.ExecOptimize().ExportResults();
        var resDi = optDi.ExecOptimize().ExportResults();

        var inputs = new List<(double ratio, ResultSummary result)>
        {
            (0.5, resFo), (0.3, resEn), (0.2, resDi)
        };
        var mixer = new MixtureCalculator(inputs);

        var hs = mixer.HashinShtrikmanAverage();
        var voigt = mixer.VoigtAverage();
        var reuss = mixer.ReussAverage();

        Assert.NotNull(hs);
        Assert.True(hs!.KS >= reuss!.KS && hs.KS <= voigt!.KS);
        Assert.True(hs.GS >= reuss.GS && hs.GS <= voigt.GS);
    }

    [Fact]
    public void HashinShtrikman_SingleComponent_EqualsOriginal()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var opt = new MieGruneisenEOSOptimizer(fo, 10.0, 1500.0);
        var res = opt.ExecOptimize().ExportResults();

        var inputs = new List<(double ratio, ResultSummary result)> { (1.0, res) };
        var mixer = new MixtureCalculator(inputs);

        var hs = mixer.HashinShtrikmanAverage();
        Assert.NotNull(hs);
        Assert.Equal(res.KS, hs!.KS, 3);
        Assert.Equal(res.GS, hs.GS, 3);
    }

    // ============================================================
    // M2: PREM reference model
    // ============================================================

    [Fact]
    public void PREM_GetProperties_At400km()
    {
        // 400 km depth: P ~ 13.3 GPa, Vs ~ 4.93 km/s (Dziewonski & Anderson, 1981)
        var prem = PREMModel.GetPropertiesAtDepth(400.0);
        Assert.InRange(prem.Pressure, 13.0, 14.0);
        Assert.InRange(prem.Vs / 1000.0, 4.5, 5.5); // km/s
        Assert.InRange(prem.Density, 3.5, 4.0); // g/cm3
    }

    [Fact]
    public void PREM_GetProperties_AtSurface()
    {
        var prem = PREMModel.GetPropertiesAtDepth(0.0);
        Assert.Equal(0.0, prem.Pressure, 1);
    }

    [Fact]
    public void PREM_GetProperties_At2891km_CMB()
    {
        // CMB at ~2891 km, P ~ 135.75 GPa
        var prem = PREMModel.GetPropertiesAtDepth(2891.0);
        Assert.InRange(prem.Pressure, 130.0, 140.0);
    }

    [Fact]
    public void PREM_DepthToPressure_Monotonic()
    {
        double prevP = 0;
        for (double depth = 0; depth <= 2891; depth += 100)
        {
            var props = PREMModel.GetPropertiesAtDepth(depth);
            Assert.True(props.Pressure >= prevP,
                $"Pressure should increase with depth: P({depth}) = {props.Pressure} < {prevP}");
            prevP = props.Pressure;
        }
    }

    // ============================================================
    // M3: Predefined rock compositions
    // ============================================================

    [Fact]
    public void PredefinedRocks_Pyrolite_HasExpectedMinerals()
    {
        var pyrolite = PredefinedRocks.Pyrolite();
        Assert.NotNull(pyrolite);
        Assert.Equal("Pyrolite", pyrolite.Name);
        Assert.True(pyrolite.Minerals.Count >= 3, "Pyrolite should have at least 3 minerals");
        Assert.InRange(pyrolite.TotalRatio, 0.99, 1.01);
    }

    [Fact]
    public void PredefinedRocks_Harzburgite_HasExpectedMinerals()
    {
        var harz = PredefinedRocks.Harzburgite();
        Assert.NotNull(harz);
        Assert.Equal("Harzburgite", harz.Name);
        Assert.True(harz.Minerals.Count >= 2);
        Assert.InRange(harz.TotalRatio, 0.99, 1.01);
    }

    [Fact]
    public void PredefinedRocks_MORB_HasExpectedMinerals()
    {
        var morb = PredefinedRocks.MORB();
        Assert.NotNull(morb);
        Assert.Equal("MORB", morb.Name);
        Assert.True(morb.Minerals.Count >= 2);
        Assert.InRange(morb.TotalRatio, 0.99, 1.01);
    }

    [Fact]
    public void PredefinedRocks_GetAll_ReturnsMultiple()
    {
        var all = PredefinedRocks.GetAll();
        Assert.True(all.Count >= 3);
    }

    // ============================================================
    // M4: Depth axis (P-to-depth conversion)
    // ============================================================

    [Fact]
    public void DepthConverter_PressureToDepth_At13GPa_Returns400km()
    {
        double depth = DepthConverter.PressureToDepth(13.3);
        Assert.InRange(depth, 350, 450);
    }

    [Fact]
    public void DepthConverter_DepthToPressure_At400km_Returns13GPa()
    {
        double pressure = DepthConverter.DepthToPressure(400.0);
        Assert.InRange(pressure, 12.5, 14.5);
    }

    [Fact]
    public void DepthConverter_RoundTrip()
    {
        double originalP = 25.0;
        double depth = DepthConverter.PressureToDepth(originalP);
        double recoveredP = DepthConverter.DepthToPressure(depth);
        Assert.Equal(originalP, recoveredP, 1);
    }
}
