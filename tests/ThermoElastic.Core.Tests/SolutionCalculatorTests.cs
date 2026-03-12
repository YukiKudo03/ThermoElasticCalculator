using Xunit;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Core.Tests;

public class SolutionCalculatorTests
{
    private static List<SolutionSite> SimpleOneSite()
    {
        return new List<SolutionSite>
        {
            new SolutionSite { SiteName = "M", Multiplicity = 1.0 }
        };
    }

    [Fact]
    public void IdealEntropy_Binary5050_EqualsRln2()
    {
        double[] x = { 0.5, 0.5 };
        var sites = SimpleOneSite();

        double S = SolutionCalculator.GetIdealEntropy(x, sites);
        double expected = PhysicConstants.GasConst * Math.Log(2.0);
        Assert.Equal(expected, S, 4);
    }

    [Fact]
    public void ExcessGibbs_PureEndmember_IsZero()
    {
        double[] x = { 1.0, 0.0 };
        var interactions = new List<InteractionParam>
        {
            new InteractionParam { EndmemberA = 0, EndmemberB = 1, W = 7.6 }
        };

        double gEx = SolutionCalculator.GetExcessGibbs(x, interactions);
        Assert.Equal(0.0, gEx, 6);
    }

    [Fact]
    public void ExcessGibbs_SymmetricModel_ReducesToMargules()
    {
        // When d_a = d_b = 1, van Laar reduces to symmetric Margules: G_ex = x_a * x_b * W
        double[] x = { 0.3, 0.7 };
        var interactions = new List<InteractionParam>
        {
            new InteractionParam { EndmemberA = 0, EndmemberB = 1, W = 7.6, SizeA = 1.0, SizeB = 1.0 }
        };

        double gEx = SolutionCalculator.GetExcessGibbs(x, interactions);
        // For symmetric: φ_a = x_a, φ_b = x_b, B = d*d/(2d)*W = W/2
        // G_ex = x_a * x_b * W/2 ... wait, with van Laar formula:
        // φ_a = x_a, φ_b = x_b, B_ab = 1*1/(1+1)*W = W/2
        // G_ex = φ_a*φ_b*B_ab = x_a*x_b*W/2
        double expected = 0.3 * 0.7 * 7.6 / 2.0;
        Assert.Equal(expected, gEx, 4);
    }

    [Fact]
    public void ChemicalPotential_PureEndmember_EqualsGibbsG()
    {
        double[] x = { 1.0, 0.0 };
        var interactions = new List<InteractionParam>
        {
            new InteractionParam { EndmemberA = 0, EndmemberB = 1, W = 7.6 }
        };

        double gibbsI = -2055.0; // kJ/mol
        double mu = SolutionCalculator.GetChemicalPotential(0, x, interactions, gibbsI, 1000.0);
        Assert.Equal(gibbsI, mu, 2);
    }

    [Fact]
    public void Olivine_FoFa_W76_ExcessGibbs()
    {
        // Olivine fo-fa with W = 7.6 kJ/mol at fo90fa10
        double[] x = { 0.9, 0.1 };
        var interactions = new List<InteractionParam>
        {
            new InteractionParam { EndmemberA = 0, EndmemberB = 1, W = 7.6, SizeA = 1.0, SizeB = 1.0 }
        };

        double gEx = SolutionCalculator.GetExcessGibbs(x, interactions);
        // Symmetric: G_ex = 0.9 * 0.1 * 7.6/2 = 0.342 kJ/mol
        Assert.True(gEx > 0);
        Assert.Equal(0.9 * 0.1 * 7.6 / 2.0, gEx, 3);
    }

    [Fact]
    public void SolidSolution_JsonRoundTrip()
    {
        var ss = new SolidSolution
        {
            Name = "Olivine",
            Endmembers = new List<MineralParams>
            {
                MineralDatabase.GetByName("fo")!,
                MineralDatabase.GetByName("fa")!,
            },
            InteractionParams = new List<InteractionParam>
            {
                new InteractionParam { EndmemberA = 0, EndmemberB = 1, W = 7.6 }
            },
        };

        var json = ss.ExportJson();
        Assert.True(SolidSolution.ImportJson(json, out var imported));
        Assert.NotNull(imported);
        Assert.Equal("Olivine", imported!.Name);
        Assert.Equal(2, imported.Endmembers.Count);
    }

    [Fact]
    public void ValidateComposition_InvalidSum_ReturnsFalse()
    {
        double[] x = { 0.3, 0.5 };
        Assert.False(SolutionCalculator.ValidateComposition(x));

        double[] valid = { 0.5, 0.5 };
        Assert.True(SolutionCalculator.ValidateComposition(valid));

        double[] negative = { 1.5, -0.5 };
        Assert.False(SolutionCalculator.ValidateComposition(negative));
    }
}
