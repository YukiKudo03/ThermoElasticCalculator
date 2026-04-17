using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

public class SLB2024Tests
{
    private readonly ITestOutputHelper _output;

    public SLB2024Tests(ITestOutputHelper output) => _output = output;

    private static MineralParams Get(string paperName) =>
        SLB2011Endmembers.GetAll().First(m => m.PaperName == paperName);

    [Theory]
    [InlineData("mag", 44.528, 183.877)]
    [InlineData("hmag", 41.702, 172.00)]
    [InlineData("wuls", 43.400, 199.70)]
    [InlineData("hebg", 29.577, 204.251)]
    [InlineData("hlbg", 27.521, 204.251)]
    [InlineData("fabg", 27.090, 223.326)]
    [InlineData("hppv", 27.688, 176.50)]
    [InlineData("hem", 30.287, 204.245)]
    [InlineData("fea", 7.093, 163.421)]
    [InlineData("feg", 6.988, 165.00)]
    [InlineData("fee", 6.765, 165.00)]
    [InlineData("esk", 28.796, 231.00)]
    [InlineData("crpv", 25.577, 252.55)]
    [InlineData("cppv", 26.949, 247.740)]
    public void SLB2024_Endmember_ParametersMatchHeFESTo(string paperName, double expectedV0, double expectedK0)
    {
        var mineral = Get(paperName);
        Assert.Equal(expectedV0, mineral.MolarVolume, 0.01);
        Assert.Equal(expectedK0, mineral.KZero, 0.01);
    }

    [Fact]
    public void SLB2024_NativeIron_HasElectronicContribution()
    {
        Assert.True(Get("fea").BetaElectronic > 0, "alpha-Fe should have non-zero BetaElectronic");
        Assert.True(Get("feg").BetaElectronic > 0, "gamma-Fe should have non-zero BetaElectronic");
        Assert.True(Get("fee").BetaElectronic > 0, "epsilon-Fe should have non-zero BetaElectronic");
        Assert.Equal(0.0, Get("fo").BetaElectronic); // insulator
        Assert.Equal(0.0, Get("mpv").BetaElectronic); // insulator
    }

    [Fact]
    public void SLB2024_PeWu_InteractionParameterUpdated()
    {
        var entry = SLB2011Solutions.GetAll().First(e => e.EndmemberA == "pe" && e.EndmemberB == "wu");
        Assert.Equal(44.0, entry.W_kJ); // SLB2024 value, not SLB2011's 13.0
    }

    [Fact]
    public void SLB2024_FpSolutions_ContainWuls()
    {
        var fpEntries = SLB2011Solutions.GetAll().Where(e => e.SolutionName == "Ferropericlase").ToList();
        Assert.Contains(fpEntries, e => e.EndmemberA == "pe" && e.EndmemberB == "wuls");
        Assert.Contains(fpEntries, e => e.EndmemberA == "wu" && e.EndmemberB == "wuls");

        var peWuls = fpEntries.First(e => e.EndmemberA == "pe" && e.EndmemberB == "wuls");
        Assert.Equal(-87.1, peWuls.W_kJ, 0.1); // Negative = favorable
    }

    [Fact]
    public void SLB2024_BgSolutions_ContainFe3Endmembers()
    {
        var bgEntries = SLB2011Solutions.GetAll().Where(e => e.SolutionName == "Perovskite").ToList();
        Assert.Contains(bgEntries, e => e.EndmemberB == "hebg" || e.EndmemberA == "hebg");
        Assert.Contains(bgEntries, e => e.EndmemberB == "hlbg" || e.EndmemberA == "hlbg");
        Assert.Contains(bgEntries, e => e.EndmemberB == "fabg" || e.EndmemberA == "fabg");

        // hebg-hlbg should have negative W (favorable HS-LS mixing)
        var hh = bgEntries.First(e => e.EndmemberA == "hebg" && e.EndmemberB == "hlbg");
        Assert.True(hh.W_kJ < 0, "hebg-hlbg W should be negative (favorable)");
    }

    [Fact]
    public void Fe2SpinCrossover_WithExplicitWuls_Computes()
    {
        var wu = Get("wu");
        var wulsRaw = Get("wuls");

        // wuls is defined for Fe4O4 (8 atoms, 4 f.u.). Normalize to FeO basis (2 atoms, 1 f.u.)
        // by scaling extensive properties by 1/4.
        var wuls = new MineralParams
        {
            MineralName = "Wuestite-LS", PaperName = "wuls",
            NumAtoms = 2, MolarVolume = wulsRaw.MolarVolume / 4.0,
            MolarWeight = wulsRaw.MolarWeight / 4.0,
            KZero = wulsRaw.KZero, K1Prime = wulsRaw.K1Prime,
            GZero = wulsRaw.GZero, G1Prime = wulsRaw.G1Prime,
            DebyeTempZero = wulsRaw.DebyeTempZero,
            GammaZero = wulsRaw.GammaZero, QZero = wulsRaw.QZero,
            EhtaZero = wulsRaw.EhtaZero,
            F0 = wulsRaw.F0 / 4.0, RefTemp = 300.0,
        };

        var calc = new SpinCrossoverCalculator();

        // At low pressure (10 GPa), should be mostly HS (nLS ~ 0)
        var result10 = calc.ComputeFe2SpinState(wu, wuls, 10.0, 300.0);
        _output.WriteLine($"P=10 GPa: nLS={result10.nLS:F4}, Vs={result10.Vs:F0}");
        Assert.True(result10.nLS < 0.5, "At 10 GPa, Fe2+ should be mostly high-spin");

        // At high pressure (100 GPa), should transition toward LS
        var result100 = calc.ComputeFe2SpinState(wu, wuls, 100.0, 300.0);
        _output.WriteLine($"P=100 GPa: nLS={result100.nLS:F4}, Vs={result100.Vs:F0}");
        Assert.True(result100.nLS > result10.nLS, "LS fraction should increase with pressure");
    }

    [Fact]
    public void Fe3SpinCrossover_BridgmaniteHebgHlbg_Computes()
    {
        var hebg = Get("hebg");
        var hlbg = Get("hlbg");
        var calc = new SpinCrossoverCalculator();

        var result30 = calc.ComputeFe3SpinState(hebg, hlbg, 30.0, 2000.0);
        _output.WriteLine($"P=30 GPa, T=2000K: nLS={result30.nLS:F4}, Vs={result30.Vs:F0}");

        var result100 = calc.ComputeFe3SpinState(hebg, hlbg, 100.0, 2000.0);
        _output.WriteLine($"P=100 GPa, T=2000K: nLS={result100.nLS:F4}, Vs={result100.Vs:F0}");

        // Properties should be physical
        Assert.True(result30.Vs > 0 && result30.Vp > 0, "Velocities should be positive");
        Assert.True(result30.Density > 0, "Density should be positive");
    }

    [Fact]
    public void NativeIron_AlphaFe_EOS_ComputesAtAmbient()
    {
        var fea = Get("fea");
        var eos = new MieGruneisenEOSOptimizer(fea, 0.001, 300.0);
        var result = eos.ExecOptimize();

        _output.WriteLine($"alpha-Fe at 0 GPa, 300K: V={result.Volume:F4}, rho={result.Density:F4}");

        // alpha-Fe density at ambient: ~7.874 g/cm3
        Assert.InRange(result.Density, 7.0, 9.0);
    }
}
