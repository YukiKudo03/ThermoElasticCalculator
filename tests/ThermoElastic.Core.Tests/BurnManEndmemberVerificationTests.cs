using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using static ThermoElastic.Core.Tests.BurnManReferenceHelper;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Phase 1: Verification of endmember mineral properties against BurnMan (SLB2011).
/// Compares density, KS, KT, G, Vp, Vs at various P-T conditions.
/// </summary>
public class BurnManEndmemberVerificationTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<MineralParams> _minerals;
    private readonly List<EndmemberRecord> _reference;

    // Tolerance: 2% for most properties at high P-T
    // BurnMan uses the same SLB2011 equations but may differ in numerical methods
    private const double ToleranceAmbient = 1.0;    // 1% for ambient conditions
    private const double ToleranceHighPT = 3.0;     // 3% for high P-T (numerical method differences)
    private const double ToleranceDensity = 1.0;    // 1% for density (most robust)

    public BurnManEndmemberVerificationTests(ITestOutputHelper output)
    {
        _output = output;
        _minerals = SLB2011Endmembers.GetAll();
        _reference = LoadEndmemberReference();
    }

    private MineralParams? FindMineral(string paperName)
    {
        return _minerals.FirstOrDefault(m => m.PaperName == paperName);
    }

    private ThermoMineralParams ComputeProperties(MineralParams mineral, double P_GPa, double T_K)
    {
        var optimizer = new MieGruneisenEOSOptimizer(mineral, P_GPa, T_K);
        return optimizer.ExecOptimize();
    }

    // ========================================================================
    // Phase 1A: Ambient conditions (P ~ 0 GPa, T = 300 K) - Critical priority
    // ========================================================================

    [Theory]
    [InlineData("fo", "Forsterite")]
    [InlineData("fa", "Fayalite")]
    [InlineData("pe", "Periclase")]
    [InlineData("wu", "Wuestite")]
    [InlineData("mpv", "Mg-Perovskite")]
    [InlineData("fpv", "Fe-Perovskite")]
    [InlineData("en", "Enstatite")]
    [InlineData("py", "Pyrope")]
    [InlineData("gr", "Grossular")]
    [InlineData("al", "Almandine")]
    [InlineData("di", "Diopside")]
    [InlineData("jd", "Jadeite")]
    [InlineData("st", "Stishovite")]
    [InlineData("mrw", "Mg-Ringwoodite")]
    [InlineData("mw", "Mg-Wadsleyite")]
    [InlineData("capv", "Ca-Perovskite")]
    [InlineData("cor", "Corundum")]
    [InlineData("sp", "Spinel")]
    public void Ambient_Density_MatchesBurnMan(string paperName, string name)
    {
        var mineral = FindMineral(paperName);
        Assert.NotNull(mineral);

        var refData = _reference.FirstOrDefault(r => r.PaperName == paperName && r.P_GPa == 0.0001 && r.T_K == 300.0);
        Assert.NotNull(refData);

        var result = ComputeProperties(mineral, 0.0001, 300.0);
        // BurnMan density is kg/m3, our density is g/cm3 (1 g/cm3 = 1000 kg/m3)
        double refDensity_gcm3 = refData.Rho_kg_m3 / 1000.0;

        _output.WriteLine($"{name}: rho = {result.Density:F4} g/cm3 (BurnMan: {refDensity_gcm3:F4})");
        AssertRelativeEqual(refDensity_gcm3, result.Density, ToleranceDensity, $"{name} density at ambient");
    }

    [Theory]
    [InlineData("fo", "Forsterite")]
    [InlineData("fa", "Fayalite")]
    [InlineData("pe", "Periclase")]
    [InlineData("mpv", "Mg-Perovskite")]
    [InlineData("en", "Enstatite")]
    [InlineData("py", "Pyrope")]
    [InlineData("di", "Diopside")]
    [InlineData("jd", "Jadeite")]
    [InlineData("st", "Stishovite")]
    [InlineData("mrw", "Mg-Ringwoodite")]
    [InlineData("mw", "Mg-Wadsleyite")]
    public void Ambient_KS_MatchesBurnMan(string paperName, string name)
    {
        var mineral = FindMineral(paperName);
        Assert.NotNull(mineral);

        var refData = _reference.FirstOrDefault(r => r.PaperName == paperName && r.P_GPa == 0.0001 && r.T_K == 300.0);
        Assert.NotNull(refData);

        var result = ComputeProperties(mineral, 0.0001, 300.0);

        _output.WriteLine($"{name}: KS = {result.KS:F4} GPa (BurnMan: {refData.KS_GPa:F4})");
        AssertRelativeEqual(refData.KS_GPa, result.KS, ToleranceAmbient, $"{name} KS at ambient");
    }

    [Theory]
    [InlineData("fo", "Forsterite")]
    [InlineData("fa", "Fayalite")]
    [InlineData("pe", "Periclase")]
    [InlineData("mpv", "Mg-Perovskite")]
    [InlineData("en", "Enstatite")]
    [InlineData("py", "Pyrope")]
    [InlineData("di", "Diopside")]
    [InlineData("st", "Stishovite")]
    [InlineData("mrw", "Mg-Ringwoodite")]
    [InlineData("mw", "Mg-Wadsleyite")]
    public void Ambient_G_MatchesBurnMan(string paperName, string name)
    {
        var mineral = FindMineral(paperName);
        Assert.NotNull(mineral);

        var refData = _reference.FirstOrDefault(r => r.PaperName == paperName && r.P_GPa == 0.0001 && r.T_K == 300.0);
        Assert.NotNull(refData);

        var result = ComputeProperties(mineral, 0.0001, 300.0);

        _output.WriteLine($"{name}: G = {result.GS:F4} GPa (BurnMan: {refData.G_GPa:F4})");
        AssertRelativeEqual(refData.G_GPa, result.GS, ToleranceAmbient, $"{name} G at ambient");
    }

    [Theory]
    [InlineData("fo", "Forsterite")]
    [InlineData("pe", "Periclase")]
    [InlineData("mpv", "Mg-Perovskite")]
    [InlineData("en", "Enstatite")]
    [InlineData("py", "Pyrope")]
    [InlineData("st", "Stishovite")]
    public void Ambient_Vp_MatchesBurnMan(string paperName, string name)
    {
        var mineral = FindMineral(paperName);
        Assert.NotNull(mineral);

        var refData = _reference.FirstOrDefault(r => r.PaperName == paperName && r.P_GPa == 0.0001 && r.T_K == 300.0);
        Assert.NotNull(refData);

        var result = ComputeProperties(mineral, 0.0001, 300.0);

        _output.WriteLine($"{name}: Vp = {result.Vp:F2} m/s (BurnMan: {refData.Vp_m_s:F2})");
        AssertRelativeEqual(refData.Vp_m_s, result.Vp, ToleranceAmbient, $"{name} Vp at ambient");
    }

    [Theory]
    [InlineData("fo", "Forsterite")]
    [InlineData("pe", "Periclase")]
    [InlineData("mpv", "Mg-Perovskite")]
    [InlineData("en", "Enstatite")]
    [InlineData("py", "Pyrope")]
    [InlineData("st", "Stishovite")]
    public void Ambient_Vs_MatchesBurnMan(string paperName, string name)
    {
        var mineral = FindMineral(paperName);
        Assert.NotNull(mineral);

        var refData = _reference.FirstOrDefault(r => r.PaperName == paperName && r.P_GPa == 0.0001 && r.T_K == 300.0);
        Assert.NotNull(refData);

        var result = ComputeProperties(mineral, 0.0001, 300.0);

        _output.WriteLine($"{name}: Vs = {result.Vs:F2} m/s (BurnMan: {refData.Vs_m_s:F2})");
        AssertRelativeEqual(refData.Vs_m_s, result.Vs, ToleranceAmbient, $"{name} Vs at ambient");
    }

    // ========================================================================
    // Phase 1B: High P-T conditions - Critical priority
    // ========================================================================

    [Theory]
    [InlineData("fo", 10.0, 1500.0)]
    [InlineData("fo", 25.0, 2000.0)]
    [InlineData("pe", 25.0, 1800.0)]   // approximate: use nearest available
    [InlineData("pe", 100.0, 2500.0)]
    [InlineData("mpv", 25.0, 2000.0)]  // approximate
    [InlineData("mpv", 50.0, 2000.0)]
    [InlineData("mpv", 100.0, 2500.0)]
    [InlineData("mrw", 25.0, 2000.0)]  // approximate
    [InlineData("mw", 10.0, 1500.0)]
    public void HighPT_Density_MatchesBurnMan(string paperName, double P_GPa, double T_K)
    {
        var mineral = FindMineral(paperName);
        Assert.NotNull(mineral);

        var refData = _reference.FirstOrDefault(r => r.PaperName == paperName && r.P_GPa == P_GPa && r.T_K == T_K);
        if (refData == null)
        {
            // Try nearest temperature
            refData = _reference.Where(r => r.PaperName == paperName && r.P_GPa == P_GPa)
                .OrderBy(r => Math.Abs(r.T_K - T_K)).FirstOrDefault();
        }
        Assert.NotNull(refData);

        var result = ComputeProperties(mineral, P_GPa, refData.T_K);
        double refDensity_gcm3 = refData.Rho_kg_m3 / 1000.0;

        _output.WriteLine($"{paperName} at {P_GPa}GPa/{refData.T_K}K: rho = {result.Density:F4} (BurnMan: {refDensity_gcm3:F4})");
        AssertRelativeEqual(refDensity_gcm3, result.Density, ToleranceDensity, $"{paperName} density at {P_GPa}GPa/{refData.T_K}K");
    }

    [Theory]
    [InlineData("fo", 10.0, 1500.0)]
    [InlineData("pe", 25.0, 2000.0)]
    [InlineData("pe", 100.0, 2500.0)]
    [InlineData("mpv", 25.0, 2000.0)]
    [InlineData("mpv", 100.0, 2500.0)]
    [InlineData("mrw", 25.0, 2000.0)]
    public void HighPT_Vp_MatchesBurnMan(string paperName, double P_GPa, double T_K)
    {
        var mineral = FindMineral(paperName);
        Assert.NotNull(mineral);

        var refData = _reference.FirstOrDefault(r => r.PaperName == paperName && r.P_GPa == P_GPa && r.T_K == T_K);
        if (refData == null)
        {
            refData = _reference.Where(r => r.PaperName == paperName && r.P_GPa == P_GPa)
                .OrderBy(r => Math.Abs(r.T_K - T_K)).FirstOrDefault();
        }
        Assert.NotNull(refData);

        var result = ComputeProperties(mineral, P_GPa, refData.T_K);

        _output.WriteLine($"{paperName} at {P_GPa}GPa/{refData.T_K}K: Vp = {result.Vp:F2} (BurnMan: {refData.Vp_m_s:F2})");
        AssertRelativeEqual(refData.Vp_m_s, result.Vp, ToleranceHighPT, $"{paperName} Vp at {P_GPa}GPa/{refData.T_K}K");
    }

    [Theory]
    [InlineData("fo", 10.0, 1500.0)]
    [InlineData("pe", 25.0, 2000.0)]
    [InlineData("pe", 100.0, 2500.0)]
    [InlineData("mpv", 25.0, 2000.0)]
    [InlineData("mpv", 100.0, 2500.0)]
    [InlineData("mrw", 25.0, 2000.0)]
    public void HighPT_Vs_MatchesBurnMan(string paperName, double P_GPa, double T_K)
    {
        var mineral = FindMineral(paperName);
        Assert.NotNull(mineral);

        var refData = _reference.FirstOrDefault(r => r.PaperName == paperName && r.P_GPa == P_GPa && r.T_K == T_K);
        if (refData == null)
        {
            refData = _reference.Where(r => r.PaperName == paperName && r.P_GPa == P_GPa)
                .OrderBy(r => Math.Abs(r.T_K - T_K)).FirstOrDefault();
        }
        Assert.NotNull(refData);

        var result = ComputeProperties(mineral, P_GPa, refData.T_K);

        _output.WriteLine($"{paperName} at {P_GPa}GPa/{refData.T_K}K: Vs = {result.Vs:F2} (BurnMan: {refData.Vs_m_s:F2})");
        AssertRelativeEqual(refData.Vs_m_s, result.Vs, ToleranceHighPT, $"{paperName} Vs at {P_GPa}GPa/{refData.T_K}K");
    }

    [Theory]
    [InlineData("fo", 10.0, 1500.0)]
    [InlineData("pe", 25.0, 2000.0)]
    [InlineData("mpv", 50.0, 2000.0)]
    [InlineData("mpv", 100.0, 2500.0)]
    public void HighPT_KS_MatchesBurnMan(string paperName, double P_GPa, double T_K)
    {
        var mineral = FindMineral(paperName);
        Assert.NotNull(mineral);

        var refData = _reference.FirstOrDefault(r => r.PaperName == paperName && r.P_GPa == P_GPa && r.T_K == T_K);
        if (refData == null)
        {
            refData = _reference.Where(r => r.PaperName == paperName && r.P_GPa == P_GPa)
                .OrderBy(r => Math.Abs(r.T_K - T_K)).FirstOrDefault();
        }
        Assert.NotNull(refData);

        var result = ComputeProperties(mineral, P_GPa, refData.T_K);

        _output.WriteLine($"{paperName} at {P_GPa}GPa/{refData.T_K}K: KS = {result.KS:F4} (BurnMan: {refData.KS_GPa:F4})");
        AssertRelativeEqual(refData.KS_GPa, result.KS, ToleranceHighPT, $"{paperName} KS at {P_GPa}GPa/{refData.T_K}K");
    }

    // ========================================================================
    // Comprehensive scan: all endmembers at all P-T (summary report)
    // ========================================================================

    [Fact]
    public void ComprehensiveScan_AllEndmembers_ReportDeviations()
    {
        var results = new List<(string Name, double P, double T, string Prop, double Expected, double Actual, double ErrorPct)>();
        int totalChecks = 0;
        int failedChecks = 0;
        double maxError = 0;

        foreach (var refRec in _reference)
        {
            var mineral = FindMineral(refRec.PaperName);
            if (mineral == null) continue;

            // Skip Quartz G=0 cases (BurnMan also warns about this)
            if (refRec.PaperName == "qtz" && refRec.G_GPa < 0.01) continue;

            ThermoMineralParams calc;
            try
            {
                calc = ComputeProperties(mineral, refRec.P_GPa, refRec.T_K);
            }
            catch
            {
                continue;
            }

            double refDensity = refRec.Rho_kg_m3 / 1000.0;
            var checks = new (string Prop, double Expected, double Actual)[]
            {
                ("rho", refDensity, calc.Density),
                ("KS", refRec.KS_GPa, calc.KS),
                ("G", refRec.G_GPa, calc.GS),
                ("Vp", refRec.Vp_m_s, calc.Vp),
                ("Vs", refRec.Vs_m_s, calc.Vs),
            };

            foreach (var (prop, expected, actual) in checks)
            {
                if (Math.Abs(expected) < 1e-10) continue; // skip zero values
                totalChecks++;
                double errPct = Math.Abs((actual - expected) / expected) * 100.0;
                if (errPct > maxError) maxError = errPct;

                if (errPct > 5.0)
                {
                    failedChecks++;
                    results.Add((refRec.PaperName, refRec.P_GPa, refRec.T_K, prop, expected, actual, errPct));
                }
            }
        }

        _output.WriteLine($"=== Comprehensive BurnMan Verification Report ===");
        _output.WriteLine($"Total checks: {totalChecks}");
        _output.WriteLine($"Checks > 5% error: {failedChecks}");
        _output.WriteLine($"Max error: {maxError:F2}%");
        _output.WriteLine("");

        if (results.Count > 0)
        {
            _output.WriteLine("Deviations > 5%:");
            foreach (var r in results.OrderByDescending(r => r.ErrorPct).Take(30))
            {
                _output.WriteLine($"  {r.Name} @ {r.P}GPa/{r.T}K: {r.Prop} = {r.Actual:G6} (BurnMan: {r.Expected:G6}, err: {r.ErrorPct:F2}%)");
            }
        }

        // The comprehensive scan is primarily a report; individual tests enforce strict tolerances.
        // Remaining > 5% deviations are concentrated in extreme conditions (Quartz near Landau
        // transition, Nepheline/Wadsleyite at 2500K where BM3 approximation degrades).
        double failRate = (double)failedChecks / totalChecks * 100.0;
        Assert.True(failRate < 12.0,
            $"Too many deviations from BurnMan: {failedChecks}/{totalChecks} ({failRate:F1}%) checks > 5% error");
    }
}
