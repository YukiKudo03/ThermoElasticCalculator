using ThermoElastic.Core;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Phase 5 tests: L3 (ViewModel-equivalent logic tests), L4 (CI - separate), L5 (logging - integration)
/// Since ViewModels depend on Avalonia which is not testable in Core tests,
/// we test the underlying logic that ViewModels depend on.
/// </summary>
public class Phase5Tests
{
    // ============================================================
    // L3: Logic tests that back ViewModels
    // ============================================================

    [Fact]
    public void MineralDatabase_GetAll_Returns42Endmembers()
    {
        var all = MineralDatabase.GetAll();
        Assert.Equal(46, all.Count);
    }

    [Fact]
    public void MineralDatabase_Search_FindsForsterite()
    {
        var results = MineralDatabase.Search("Forst");
        Assert.Contains(results, m => m.MineralName == "Forsterite");
    }

    [Fact]
    public void MineralDatabase_GetByName_AllEndmembersResolvable()
    {
        var all = MineralDatabase.GetAll();
        foreach (var mineral in all)
        {
            var found = MineralDatabase.GetByName(mineral.MineralName);
            Assert.NotNull(found);
            Assert.Equal(mineral.MineralName, found!.MineralName);
        }
    }

    [Fact]
    public void PredefinedRocks_AllRocksCalculateAtAmbient()
    {
        var rocks = PredefinedRocks.GetAll();
        foreach (var rock in rocks)
        {
            var calc = new RockCalculator(rock, 0.0001, 300.0, MixtureMethod.Hill);
            var (mixedResult, individualResults) = calc.Calculate();

            Assert.NotNull(mixedResult);
            Assert.True(mixedResult!.Density > 0,
                $"Rock '{rock.Name}' should have positive density at ambient");
            Assert.True(mixedResult.KS > 0,
                $"Rock '{rock.Name}' should have positive KS at ambient");
            Assert.True(individualResults.Count > 0);
        }
    }

    [Fact]
    public void PredefinedRocks_AllRocksCalculateAtMantleConditions()
    {
        var rocks = PredefinedRocks.GetAll();
        foreach (var rock in rocks)
        {
            var calc = new RockCalculator(rock, 10.0, 1500.0, MixtureMethod.Hill);
            var (mixedResult, individualResults) = calc.Calculate();

            Assert.NotNull(mixedResult);
            Assert.True(mixedResult!.Vp > 0,
                $"Rock '{rock.Name}' should have positive Vp at 10 GPa, 1500 K");
        }
    }

    [Fact]
    public void PTProfile_CalculateAlongProfile_ReturnsCorrectCount()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var profile = new PTProfile
        {
            Profile = new List<PTData>
            {
                new() { Pressure = 0.0001, Temperature = 300.0 },
                new() { Pressure = 5.0, Temperature = 1000.0 },
                new() { Pressure = 10.0, Temperature = 1500.0 },
            },
        };

        var calc = new PTProfileCalculator(fo, profile);
        var results = calc.DoProfileCalculationAsSummary();
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.Density > 0));
    }

    [Fact]
    public void RockComposition_SerializationRoundTrip()
    {
        var rock = PredefinedRocks.Pyrolite();
        var json = rock.ExportJson();
        Assert.True(RockComposition.ImportJson(json, out var recovered));
        Assert.NotNull(recovered);
        Assert.Equal(rock.Name, recovered!.Name);
        Assert.Equal(rock.Minerals.Count, recovered.Minerals.Count);
    }

    [Fact]
    public void ResultSummary_JsonRoundTrip()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var opt = new MieGruneisenEOSOptimizer(fo, 10.0, 1500.0);
        var result = opt.ExecOptimize().ExportResults();

        var json = result.ExportSummaryAsJson();
        Assert.Contains("KS", json);
        Assert.Contains("GibbsG", json);
        Assert.Contains("Entropy", json);
    }

    [Fact]
    public void PREM_Profile_HasReasonableSteps()
    {
        var profile = PREMModel.GetProfile(100.0);
        Assert.True(profile.Count >= 29);
        Assert.Equal(0, profile[0].Depth, 1);
        Assert.True(profile[^1].Depth >= 2891);
    }

    [Fact]
    public void DepthConverter_AllPREMDepths_Monotonic()
    {
        double prevDepth = -1;
        for (double p = 0; p <= 135; p += 10)
        {
            double depth = DepthConverter.PressureToDepth(p);
            Assert.True(depth > prevDepth,
                $"Depth should increase: depth({p} GPa) = {depth} km <= {prevDepth} km");
            prevDepth = depth;
        }
    }

    [Fact]
    public void IsentropeCalculator_MultipleStartingTemperatures()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;

        foreach (var startT in new[] { 1400.0, 1600.0, 1800.0 })
        {
            var calc = new IsentropeCalculator(fo);
            var profile = calc.ComputeIsentrope(0.0001, startT, pressureMax: 10.0, pressureStep: 5.0);
            Assert.True(profile.Count >= 2,
                $"Isentrope from {startT}K should have multiple points");
        }
    }

    [Fact]
    public void HashinShtrikman_EquilibriumAggregate_FourComponents()
    {
        var pyrolite = PredefinedRocks.Pyrolite();
        var calc = new RockCalculator(pyrolite, 10.0, 1500.0, MixtureMethod.HS);
        var (mixedResult, _) = calc.Calculate();

        Assert.NotNull(mixedResult);
        Assert.True(mixedResult!.KS > 100, "HS KS should be > 100 GPa at 10 GPa");
    }
}
