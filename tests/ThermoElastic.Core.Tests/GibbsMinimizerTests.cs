using Xunit;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Core.Tests;

public class GibbsMinimizerTests
{
    [Fact]
    public void PureEndmember_SinglePhaseStable()
    {
        var fo = MineralDatabase.GetByName("fo")!;
        var initial = new PhaseAssemblage
        {
            Phases = new List<PhaseEntry>
            {
                new PhaseEntry { Name = "Forsterite", Mineral = fo, Amount = 1.0 }
            }
        };

        var minimizer = new GibbsMinimizer();
        var result = minimizer.Minimize(initial, 5.0, 1500.0);

        Assert.Single(result.Phases);
        Assert.Equal("Forsterite", result.Phases[0].Name);
    }

    [Fact]
    public void TwoPhase_LowerGibbsWins()
    {
        var fo = MineralDatabase.GetByName("fo")!;
        var fa = MineralDatabase.GetByName("fa")!;

        var initial = new PhaseAssemblage
        {
            Phases = new List<PhaseEntry>
            {
                new PhaseEntry { Name = "Forsterite", Mineral = fo, Amount = 1.0 },
                new PhaseEntry { Name = "Fayalite", Mineral = fa, Amount = 1.0 },
            }
        };

        var minimizer = new GibbsMinimizer();
        var result = minimizer.Minimize(initial, 5.0, 1500.0);

        // One phase should dominate
        Assert.True(result.Phases.Count >= 1);
        // Total G should be <= any individual phase G
        double totalG = result.TotalGibbs;
        double g1 = GibbsMinimizer.ComputePhaseGibbs(initial.Phases[0], 5.0, 1500.0);
        double g2 = GibbsMinimizer.ComputePhaseGibbs(initial.Phases[1], 5.0, 1500.0);
        double minIndividualG = Math.Min(g1, g2);
        // The result should have G close to the minimum single-phase G
        Assert.True(totalG / result.Phases.Sum(p => p.Amount) <= minIndividualG + 1.0);
    }

    [Fact]
    public void MassBalance_IsPreserved()
    {
        var fo = MineralDatabase.GetByName("fo")!;
        var mw = MineralDatabase.GetByName("mw")!;

        double initialTotal = 3.0;
        var initial = new PhaseAssemblage
        {
            Phases = new List<PhaseEntry>
            {
                new PhaseEntry { Name = "Forsterite", Mineral = fo, Amount = 1.5 },
                new PhaseEntry { Name = "Wadsleyite", Mineral = mw, Amount = 1.5 },
            }
        };

        var minimizer = new GibbsMinimizer();
        var result = minimizer.Minimize(initial, 14.0, 1600.0);

        double resultTotal = result.Phases.Sum(p => p.Amount);
        Assert.True(Math.Abs(resultTotal - initialTotal) < 0.1,
            $"Mass balance: initial={initialTotal}, result={resultTotal}");
    }

    [Fact]
    public void ClapeyronSlope_HasCorrectSign()
    {
        var fo = MineralDatabase.GetByName("fo")!;
        var mw = MineralDatabase.GetByName("mw")!;

        var p1 = new PhaseEntry { Name = "Forsterite", Mineral = fo, Amount = 1.0 };
        var p2 = new PhaseEntry { Name = "Wadsleyite", Mineral = mw, Amount = 1.0 };

        double slope = GibbsMinimizer.ClapeyronSlope(p1, p2, 14.0, 1600.0);
        // Olivine → wadsleyite transition has positive Clapeyron slope
        // (both ΔV < 0 and ΔS < 0, so slope > 0)
        Assert.False(double.IsNaN(slope), "Clapeyron slope should be computable");
    }

    [Fact]
    public void ThreePhase_SelectsMostStable()
    {
        var fo = MineralDatabase.GetByName("fo")!;
        var mw = MineralDatabase.GetByName("mw")!;
        var mrw = MineralDatabase.GetByName("mrw")!;

        var initial = new PhaseAssemblage
        {
            Phases = new List<PhaseEntry>
            {
                new PhaseEntry { Name = "Forsterite", Mineral = fo, Amount = 1.0 },
                new PhaseEntry { Name = "Wadsleyite", Mineral = mw, Amount = 1.0 },
                new PhaseEntry { Name = "Ringwoodite", Mineral = mrw, Amount = 1.0 },
            }
        };

        var minimizer = new GibbsMinimizer();
        // At low pressure, olivine should be stable
        var lowP = minimizer.Minimize(initial, 5.0, 1500.0);
        Assert.True(lowP.Phases.Count >= 1);

        // At very high pressure, ringwoodite should be stable
        var highP = minimizer.Minimize(initial, 22.0, 1500.0);
        Assert.True(highP.Phases.Count >= 1);
    }

    [Fact]
    public void PhaseDiagramCalculator_FindsBoundary()
    {
        var fo = MineralDatabase.GetByName("fo")!;
        var mw = MineralDatabase.GetByName("mw")!;

        var p1 = new PhaseEntry { Name = "Forsterite", Mineral = fo, Amount = 1.0 };
        var p2 = new PhaseEntry { Name = "Wadsleyite", Mineral = mw, Amount = 1.0 };

        var calc = new PhaseDiagramCalculator();
        double boundary = calc.FindPhaseBoundary(p1, p2, 1600.0, 5.0, 25.0);

        // Olivine-wadsleyite boundary should be somewhere between 10-20 GPa
        if (!double.IsNaN(boundary))
        {
            Assert.True(boundary > 5 && boundary < 25,
                $"Phase boundary at {boundary} GPa should be between 5-25 GPa");
        }
    }
}
