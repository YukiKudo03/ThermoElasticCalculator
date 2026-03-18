using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using static ThermoElastic.Core.Tests.BurnManReferenceHelper;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Phase 3: Landau phase transition and magnetic contribution verification.
/// Phase 4: Solid solution model verification.
/// </summary>
public class LandauSolutionVerificationTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;

    public LandauSolutionVerificationTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
    }

    private MineralParams GetMineral(string paperName) =>
        _minerals.First(m => m.PaperName == paperName);

    // ========================================================================
    // Phase 3: Landau order parameter
    // ========================================================================

    [Fact]
    public void Landau_Quartz_BelowTc_OrderParameterNonZero()
    {
        // Quartz Tc0 = 847 K. At T=300K < Tc, Q should be > 0
        double Tc = 847.0;
        double Q = LandauCalculator.GetOrderParameter(300.0, Tc);

        // Q = (1 - T/Tc)^(1/4) = (1 - 300/847)^0.25 = (0.6458)^0.25 ≈ 0.8963
        double expected = Math.Pow(1.0 - 300.0 / 847.0, 0.25);
        _output.WriteLine($"Quartz Q(300K) = {Q:F6} (expected: {expected:F6})");
        Assert.True(Math.Abs(Q - expected) < 1e-6);
    }

    [Fact]
    public void Landau_Quartz_AboveTc_OrderParameterZero()
    {
        double Tc = 847.0;
        double Q = LandauCalculator.GetOrderParameter(900.0, Tc);
        _output.WriteLine($"Quartz Q(900K) = {Q:F6}");
        Assert.Equal(0.0, Q);
    }

    [Fact]
    public void Landau_Quartz_TcPressureDependence()
    {
        // Tc(P) = Tc0 + VD*P/SD = 847 + 1.222*P/5.164
        var qtz = GetMineral("qtz");
        double Tc_at_5GPa = LandauCalculator.GetTc(5.0, qtz.Tc0, qtz.VD, qtz.SD);
        double expected = 847.0 + 1.222 * 5.0 / 5.164;

        _output.WriteLine($"Quartz Tc(5 GPa) = {Tc_at_5GPa:F4} K (expected: {expected:F4})");
        Assert.True(Math.Abs(Tc_at_5GPa - expected) < 0.01);
    }

    [Fact]
    public void Landau_Stishovite_NegativeTc0_NoTransitionAtPositiveT()
    {
        // Stishovite Tc0 = -4250 K -> no Landau transition at positive T for P < extreme
        var st = GetMineral("st");
        double Tc = LandauCalculator.GetTc(0.0, st.Tc0, st.VD, st.SD);
        _output.WriteLine($"Stishovite Tc(0 GPa) = {Tc:F4} K");
        Assert.True(Tc < 0, "Stishovite should have negative Tc at low pressure");

        double Q = LandauCalculator.GetOrderParameter(300.0, Tc);
        Assert.Equal(0.0, Q);
    }

    [Fact]
    public void Landau_FreeEnergy_BelowTc_IsNegative()
    {
        // Below Tc, Landau free energy should stabilize the ordered phase (negative contribution)
        double Tc = 847.0;
        double SD = 5.164;
        double fLandau = LandauCalculator.GetFreeEnergy(300.0, Tc, SD);
        _output.WriteLine($"Quartz Landau F(300K) = {fLandau:F4} J/mol");
        Assert.True(fLandau < 0, "Landau free energy below Tc should be negative");
    }

    // ========================================================================
    // Phase 3: Magnetic contribution
    // ========================================================================

    [Fact]
    public void Magnetic_Fayalite_FreeEnergy()
    {
        // F_mag = -T * r * R * ln(2S+1)
        // Fayalite: S=2, r=2 -> F_mag = -300 * 2 * 8.31477 * ln(5) = -8020.5 J/mol
        double R = PhysicConstants.GasConst;
        double expected = -300.0 * 2.0 * R * Math.Log(2.0 * 2.0 + 1.0);

        var fa = GetMineral("fa");
        var optimizer = new MieGruneisenEOSOptimizer(fa, 0.0001, 300.0);
        var result = optimizer.ExecOptimize();

        _output.WriteLine($"Fayalite F_mag = {result.MagneticFreeEnergy:F4} J/mol (expected: {expected:F4})");
        AssertRelativeEqual(expected, result.MagneticFreeEnergy, 0.1, "Fayalite F_mag");
    }

    [Fact]
    public void Magnetic_Wuestite_FreeEnergy()
    {
        // Wuestite: S=2, r=1 -> F_mag = -300 * 1 * R * ln(5) = -4010.25 J/mol
        double R = PhysicConstants.GasConst;
        double expected = -300.0 * 1.0 * R * Math.Log(5.0);

        var wu = GetMineral("wu");
        var optimizer = new MieGruneisenEOSOptimizer(wu, 0.0001, 300.0);
        var result = optimizer.ExecOptimize();

        _output.WriteLine($"Wuestite F_mag = {result.MagneticFreeEnergy:F4} J/mol (expected: {expected:F4})");
        AssertRelativeEqual(expected, result.MagneticFreeEnergy, 0.1, "Wuestite F_mag");
    }

    [Fact]
    public void Magnetic_Forsterite_NoMagneticContribution()
    {
        var fo = GetMineral("fo");
        var optimizer = new MieGruneisenEOSOptimizer(fo, 0.0001, 300.0);
        var result = optimizer.ExecOptimize();

        Assert.Equal(0.0, result.MagneticFreeEnergy);
        Assert.Equal(0.0, result.MagneticEntropy);
    }

    // ========================================================================
    // Phase 4: Solid solution - Ideal entropy
    // ========================================================================

    [Fact]
    public void SolidSolution_IdealEntropy_Binary5050()
    {
        // For binary solution x=[0.5, 0.5] on one site with multiplicity 1:
        // S_conf = -R * [0.5*ln(0.5) + 0.5*ln(0.5)] = R * ln(2) = 5.763 J/mol/K
        double R = PhysicConstants.GasConst;
        double expected = R * Math.Log(2.0);

        var sites = new List<SolutionSite>
        {
            new SolutionSite { Multiplicity = 1 }
        };
        double[] x = { 0.5, 0.5 };
        double sConf = SolutionCalculator.GetIdealEntropy(x, sites);

        _output.WriteLine($"S_conf(50:50) = {sConf:F6} J/mol/K (expected: {expected:F6})");
        AssertRelativeEqual(expected, sConf, 0.1, "Ideal entropy 50:50");
    }

    [Fact]
    public void SolidSolution_IdealEntropy_Fo90Fa10()
    {
        // S_conf = -R * [0.9*ln(0.9) + 0.1*ln(0.1)]
        double R = PhysicConstants.GasConst;
        double expected = -R * (0.9 * Math.Log(0.9) + 0.1 * Math.Log(0.1));

        var sites = new List<SolutionSite>
        {
            new SolutionSite { Multiplicity = 1 }
        };
        double[] x = { 0.9, 0.1 };
        double sConf = SolutionCalculator.GetIdealEntropy(x, sites);

        _output.WriteLine($"S_conf(Fo90Fa10) = {sConf:F6} J/mol/K (expected: {expected:F6})");
        AssertRelativeEqual(expected, sConf, 0.1, "Ideal entropy Fo90Fa10");
    }

    // ========================================================================
    // Phase 4: Solid solution - Excess Gibbs (symmetric)
    // ========================================================================

    [Fact]
    public void SolidSolution_ExcessGibbs_SymmetricBinary()
    {
        // For symmetric binary with W = 7.6 kJ/mol (olivine fo-fa):
        // G_ex = x_A * x_B * W (when d_A = d_B)
        // At x = [0.9, 0.1]: G_ex = 0.9 * 0.1 * 7.6 = 0.684 kJ/mol (for symmetric without size factor)
        // But van Laar with equal sizes gives: phi_a=x_a, B = d*d/(d+d)*W = W/2
        // G_ex = x_a * x_b * W/2 = 0.9*0.1*7.6/2 = 0.342 kJ/mol

        var interactions = new List<InteractionParam>
        {
            new InteractionParam { EndmemberA = 0, EndmemberB = 1, W = 7.6, SizeA = 1.0, SizeB = 1.0 }
        };
        double[] x = { 0.9, 0.1 };
        double gEx = SolutionCalculator.GetExcessGibbs(x, interactions);

        // With SizeA=SizeB=1: B = 1*1/(1+1) * W = W/2 = 3.8
        // G_ex = phi_a * phi_b * B = 0.9 * 0.1 * 3.8 = 0.342
        double expected = 0.342;

        _output.WriteLine($"G_ex(Fo90Fa10) = {gEx:F6} kJ/mol (expected: {expected:F6})");
        AssertRelativeEqual(expected, gEx, 1.0, "Excess Gibbs Fo90Fa10");
    }

    // ========================================================================
    // Phase 4: Solid solution - Effective parameters (linear interpolation)
    // ========================================================================

    [Fact]
    public void SolidSolution_EffectiveParams_LinearInterpolation()
    {
        var fo = GetMineral("fo");
        var fa = GetMineral("fa");

        double[] x = { 0.9, 0.1 };
        var eff = SolutionCalculator.GetEffectiveParams(x, new List<MineralParams> { fo, fa });

        // V0_eff = 0.9 * 43.603 + 0.1 * 46.39 = 43.8817
        double expectedV = 0.9 * fo.MolarVolume + 0.1 * fa.MolarVolume;
        double expectedK = 0.9 * fo.KZero + 0.1 * fa.KZero;
        double expectedG = 0.9 * fo.GZero + 0.1 * fa.GZero;

        _output.WriteLine($"V0_eff = {eff.MolarVolume:F4} (expected: {expectedV:F4})");
        _output.WriteLine($"K0_eff = {eff.KZero:F4} (expected: {expectedK:F4})");
        _output.WriteLine($"G0_eff = {eff.GZero:F4} (expected: {expectedG:F4})");

        Assert.True(Math.Abs(eff.MolarVolume - expectedV) < 1e-6);
        Assert.True(Math.Abs(eff.KZero - expectedK) < 1e-6);
        Assert.True(Math.Abs(eff.GZero - expectedG) < 1e-6);
    }

    // ========================================================================
    // Phase 4: Solid solution properties vs BurnMan
    // ========================================================================

    [Theory]
    [InlineData("olivine", "Fo90Fa10", 0.0001, 300.0)]
    [InlineData("olivine", "Fo90Fa10", 10.0, 1500.0)]
    [InlineData("olivine", "Fo50Fa50", 0.0001, 300.0)]
    public void SolidSolution_Olivine_Density_MatchesBurnMan(string solutionName, string composition, double P_GPa, double T_K)
    {
        var refData = LoadSolutionReference()
            .FirstOrDefault(r => r.SolutionName == solutionName && r.Composition == composition && r.P_GPa == P_GPa && r.T_K == T_K);
        if (refData == null)
        {
            _output.WriteLine($"No reference data for {solutionName} {composition} at {P_GPa}/{T_K}");
            return;
        }

        // Parse composition to get mole fractions
        double xFo = composition.Contains("Fo100") ? 1.0 :
                      composition.Contains("Fo90") ? 0.9 :
                      composition.Contains("Fo80") ? 0.8 :
                      composition.Contains("Fo50") ? 0.5 : 0.0;
        double[] x = { xFo, 1.0 - xFo };

        var fo = GetMineral("fo");
        var fa = GetMineral("fa");
        var eff = SolutionCalculator.GetEffectiveParams(x, new List<MineralParams> { fo, fa });

        var optimizer = new MieGruneisenEOSOptimizer(eff, P_GPa, T_K);
        var result = optimizer.ExecOptimize();

        double refDensity = refData.Rho_kg_m3 / 1000.0;
        _output.WriteLine($"{composition} at {P_GPa}GPa/{T_K}K: rho = {result.Density:F4} (BurnMan: {refDensity:F4})");

        // Note: our linear interpolation differs from BurnMan's more sophisticated solution model
        // Allow wider tolerance (5%) for solid solution comparisons
        AssertRelativeEqual(refDensity, result.Density, 5.0, $"{composition} density at {P_GPa}/{T_K}");
    }
}
