using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for Fe-Mg partitioning between bridgmanite and ferropericlase.
/// Reference: Irifune et al. (2010), Kobayashi et al. (2005).
/// </summary>
public class IronPartitioningTests
{
    private readonly ITestOutputHelper _output;
    private readonly IronPartitioningSolver _solver = new();

    private readonly MineralParams _mgPv;
    private readonly MineralParams _fePv;
    private readonly MineralParams _mgFp;
    private readonly MineralParams _feFp;

    public IronPartitioningTests(ITestOutputHelper output)
    {
        _output = output;
        _mgPv = MineralDatabase.GetByName("mpv")!;
        _fePv = MineralDatabase.GetByName("fpv")!;
        _mgFp = MineralDatabase.GetByName("pe")!;
        _feFp = MineralDatabase.GetByName("wu")!;
    }

    // ============================================================
    // Test 1: K_D at 25 GPa, 2000 K should be ~0.1-0.8
    // Reference: Irifune et al. (2010)
    // ============================================================
    [Fact]
    public void KD_At25GPa_2000K_InExpectedRange()
    {
        double P = 25.0;
        double T = 2000.0;
        double bulkXFe = 0.10;

        var (xFePv, xFeFp, kd) = _solver.SolvePartitioning(_mgPv, _fePv, _mgFp, _feFp, bulkXFe, P, T);

        _output.WriteLine($"P={P} GPa, T={T} K, bulkXFe={bulkXFe}");
        _output.WriteLine($"X_Fe_pv={xFePv:F4}, X_Fe_fp={xFeFp:F4}, K_D={kd:F4}");

        Assert.InRange(kd, 0.1, 0.8);
    }

    // ============================================================
    // Test 2: Mass balance conservation
    // For 1 mol pv + 1 mol fp, total Fe = bulkXFe * 2
    // ============================================================
    [Fact]
    public void MassBalance_IsConserved()
    {
        double P = 25.0;
        double T = 2000.0;
        double bulkXFe = 0.10;

        var (xFePv, xFeFp, _) = _solver.SolvePartitioning(_mgPv, _fePv, _mgFp, _feFp, bulkXFe, P, T);

        // Mass balance: X_Fe_pv * 1 mol + X_Fe_fp * 1 mol = bulkXFe * 2 mol
        double totalFe = xFePv + xFeFp;
        double expected = 2.0 * bulkXFe;

        _output.WriteLine($"X_Fe_pv={xFePv:F4}, X_Fe_fp={xFeFp:F4}");
        _output.WriteLine($"Total Fe = {totalFe:F4}, Expected = {expected:F4}");

        Assert.Equal(expected, totalFe, 3); // tolerance 0.001
    }

    // ============================================================
    // Test 3: K_D varies with pressure (25 GPa vs 80 GPa)
    // ============================================================
    [Fact]
    public void KD_VariesWithPressure()
    {
        double T = 2000.0;
        double bulkXFe = 0.10;

        var (_, _, kd25) = _solver.SolvePartitioning(_mgPv, _fePv, _mgFp, _feFp, bulkXFe, 25.0, T);
        var (_, _, kd80) = _solver.SolvePartitioning(_mgPv, _fePv, _mgFp, _feFp, bulkXFe, 80.0, T);

        _output.WriteLine($"K_D at 25 GPa = {kd25:F4}");
        _output.WriteLine($"K_D at 80 GPa = {kd80:F4}");

        // K_D should be different at different pressures
        Assert.NotEqual(kd25, kd80, 3);
    }

    // ============================================================
    // Test 4: Fe preferentially enters ferropericlase -> K_D < 1
    // ============================================================
    [Fact]
    public void Fe_EnrichesFerropericlase_KD_LessThanOne()
    {
        double P = 25.0;
        double T = 2000.0;
        double bulkXFe = 0.10;

        var (xFePv, xFeFp, kd) = _solver.SolvePartitioning(_mgPv, _fePv, _mgFp, _feFp, bulkXFe, P, T);

        _output.WriteLine($"X_Fe_pv={xFePv:F4}, X_Fe_fp={xFeFp:F4}, K_D={kd:F4}");

        Assert.True(xFeFp > xFePv, $"Fe should enrich ferropericlase: X_Fe_fp={xFeFp:F4} > X_Fe_pv={xFePv:F4}");
        Assert.True(kd < 1.0, $"K_D should be < 1, got {kd:F4}");
    }
}
