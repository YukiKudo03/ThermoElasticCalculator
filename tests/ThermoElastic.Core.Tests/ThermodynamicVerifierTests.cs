using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for ThermodynamicVerifier: self-consistent thermodynamic verification.
/// </summary>
public class ThermodynamicVerifierTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;
    private readonly ThermodynamicVerifier _verifier;

    public ThermodynamicVerifierTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
        _verifier = new ThermodynamicVerifier();
    }

    private MineralParams GetMineral(string paperName) =>
        _minerals.First(m => m.PaperName == paperName);

    // ========================================================================
    // Maxwell relation: (dS/dP)_T = -(dV/dT)_P
    // ========================================================================

    [Theory]
    [InlineData("fo", 10.0, 1500.0)]
    [InlineData("fo", 25.0, 2000.0)]
    [InlineData("pe", 10.0, 1000.0)]
    [InlineData("pe", 50.0, 2000.0)]
    [InlineData("mpv", 25.0, 2000.0)]
    [InlineData("mpv", 80.0, 2500.0)]
    public void CheckMaxwellRelation_SatisfiedWithin2Percent(string paperName, double P, double T)
    {
        var mineral = GetMineral(paperName);
        var result = _verifier.Verify(mineral, P, T);

        _output.WriteLine($"{paperName} at {P}GPa/{T}K: Maxwell residual = {result.MaxwellResidual:E4}");
        Assert.True(result.MaxwellResidual < 0.02,
            $"{paperName} Maxwell residual {result.MaxwellResidual:E4} exceeds 2% tolerance");
    }

    // ========================================================================
    // Gibbs-Helmholtz: G = F + PV
    // ========================================================================

    [Theory]
    [InlineData("fo", 0.0001, 300.0)]
    [InlineData("fo", 10.0, 1500.0)]
    [InlineData("pe", 25.0, 2000.0)]
    [InlineData("mpv", 50.0, 2000.0)]
    public void CheckGibbsHelmholtz_ExactWithin1e6(string paperName, double P, double T)
    {
        var mineral = GetMineral(paperName);
        var result = _verifier.Verify(mineral, P, T);

        _output.WriteLine($"{paperName} at {P}GPa/{T}K: Gibbs-Helmholtz residual = {result.GibbsHelmholtzResidual:E4}");
        Assert.True(result.GibbsHelmholtzResidual < 1e-6,
            $"{paperName} Gibbs-Helmholtz residual {result.GibbsHelmholtzResidual:E4} exceeds 1e-6 tolerance");
    }

    // ========================================================================
    // Entropy consistency: S = -dF/dT
    // ========================================================================

    [Theory]
    [InlineData("fo", 0.0001, 1000.0)]
    [InlineData("fo", 10.0, 1500.0)]
    [InlineData("pe", 0.0001, 1000.0)]
    [InlineData("pe", 25.0, 2000.0)]
    [InlineData("mpv", 25.0, 2000.0)]
    [InlineData("mpv", 80.0, 2500.0)]
    public void CheckEntropyConsistency_Within5Percent(string paperName, double P, double T)
    {
        var mineral = GetMineral(paperName);
        var result = _verifier.Verify(mineral, P, T);

        _output.WriteLine($"{paperName} at {P}GPa/{T}K: Entropy residual = {result.EntropyResidual:E4}");
        Assert.True(result.EntropyResidual < 0.05,
            $"{paperName} Entropy residual {result.EntropyResidual:E4} exceeds 5% tolerance");
    }

    // ========================================================================
    // Bulk modulus: KT = -V * (dP/dV)_T
    // ========================================================================

    [Theory]
    [InlineData("fo", 10.0, 1500.0)]
    [InlineData("fo", 25.0, 2000.0)]
    [InlineData("pe", 10.0, 1000.0)]
    [InlineData("pe", 50.0, 2000.0)]
    [InlineData("mpv", 25.0, 2000.0)]
    [InlineData("mpv", 80.0, 2500.0)]
    public void CheckBulkModulus_Within2Percent(string paperName, double P, double T)
    {
        var mineral = GetMineral(paperName);
        var result = _verifier.Verify(mineral, P, T);

        _output.WriteLine($"{paperName} at {P}GPa/{T}K: Bulk modulus residual = {result.BulkModulusResidual:E4}");
        Assert.True(result.BulkModulusResidual < 0.03,
            $"{paperName} Bulk modulus residual {result.BulkModulusResidual:E4} exceeds 3% tolerance");
    }

    // ========================================================================
    // KS-KT identity: KS = KT * (1 + alpha * gamma * T)
    // ========================================================================

    [Theory]
    [InlineData("fo", 0.0001, 300.0)]
    [InlineData("fo", 10.0, 1500.0)]
    [InlineData("pe", 0.0001, 300.0)]
    [InlineData("pe", 25.0, 2000.0)]
    [InlineData("mpv", 25.0, 2000.0)]
    [InlineData("mpv", 100.0, 2500.0)]
    public void CheckKsKtIdentity_Within1Percent(string paperName, double P, double T)
    {
        var mineral = GetMineral(paperName);
        var result = _verifier.Verify(mineral, P, T);

        _output.WriteLine($"{paperName} at {P}GPa/{T}K: KS-KT residual = {result.KsKtResidual:E4}");
        Assert.True(result.KsKtResidual < 0.01,
            $"{paperName} KS-KT residual {result.KsKtResidual:E4} exceeds 1% tolerance");
    }

    // ========================================================================
    // Full verification over grid: 5 minerals × 4 P-T points
    // ========================================================================

    [Fact]
    public void RunFullVerification_AllMineralsPass()
    {
        var mineralNames = new[] { "fo", "pe", "mpv", "mw", "mrw" };
        var pressures = new[] { 10.0, 25.0, 50.0, 80.0 };
        var temperatures = new[] { 1000.0, 1500.0, 2000.0, 2500.0 };

        foreach (var name in mineralNames)
        {
            var mineral = GetMineral(name);
            var results = _verifier.VerifyOverGrid(mineral, pressures, temperatures);

            int total = results.Count;
            int passed = results.Count(r => r.IsValid);
            _output.WriteLine($"{name}: {passed}/{total} P-T points passed verification");

            foreach (var r in results)
            {
                Assert.True(r.IsValid,
                    $"{name}: Maxwell={r.MaxwellResidual:E3}, GH={r.GibbsHelmholtzResidual:E3}, " +
                    $"S={r.EntropyResidual:E3}, KT={r.BulkModulusResidual:E3}, KsKt={r.KsKtResidual:E3}");
            }
        }
    }
}
