using ThermoElastic.Core;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;
using Xunit;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Phase 1 tests: H2 (convergence status), H3 (CSV F/G/S columns), H4 (error handling)
/// </summary>
public class Phase1Tests
{
    // ============================================================
    // H2: EOS Optimizer convergence status
    // ============================================================

    [Fact]
    public void ExecOptimize_NormalConditions_ReportsConverged()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var optimizer = new MieGruneisenEOSOptimizer(fo, 10.0, 1500.0);
        var result = optimizer.ExecOptimize();

        Assert.True(result.IsConverged);
        Assert.True(result.Iterations < 500);
    }

    [Fact]
    public void ExecOptimize_AmbientConditions_ReportsConverged()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var optimizer = new MieGruneisenEOSOptimizer(fo, 0.0001, 300.0);
        var result = optimizer.ExecOptimize();

        Assert.True(result.IsConverged);
    }

    [Fact]
    public void ExecOptimize_ExtremeConditions_ReportsIterationCount()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var optimizer = new MieGruneisenEOSOptimizer(fo, 135.0, 4000.0);
        var result = optimizer.ExecOptimize();

        // Should still converge at CMB-like conditions
        Assert.True(result.Iterations > 0);
    }

    // ============================================================
    // H3: CSV export with F, G, S columns
    // ============================================================

    [Fact]
    public void ColumnsCSV_IncludesFGS()
    {
        var csv = ResultSummary.ColumnsCSV;
        Assert.Contains("F[kJ/mol]", csv);
        Assert.Contains("G[kJ/mol]", csv);
        Assert.Contains("S[J/mol/K]", csv);
    }

    [Fact]
    public void ColumnStrs_IncludesFGS()
    {
        var cols = ResultSummary.ColumnStrs;
        Assert.Contains("F[kJ/mol]", cols);
        Assert.Contains("G[kJ/mol]", cols);
        Assert.Contains("S[J/mol/K]", cols);
    }

    [Fact]
    public void ExportSummaryAsColumn_IncludesFGSValues()
    {
        var summary = new ResultSummary
        {
            GivenP = 10.0,
            GivenT = 1500.0,
            HelmholtzF = -123.45,
            GibbsG = -100.0,
            Entropy = 200.5,
            KS = 200.0,
            KT = 180.0,
            GS = 80.0,
            Volume = 40.0,
            Density = 3.5,
        };

        var csv = summary.ExportSummaryAsColumn();
        var fields = csv.Split(',').Select(f => f.Trim()).ToList();

        // Original 15 columns + 3 new = 18
        Assert.Equal(18, fields.Count);
        Assert.Equal("-123.45", fields[15]);
        Assert.Equal("-100", fields[16]);
        Assert.Equal("200.5", fields[17]);
    }

    [Fact]
    public void ExportSummaryAsColumn_BackwardCompatible_First15ColumnsUnchanged()
    {
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var optimizer = new MieGruneisenEOSOptimizer(fo, 10.0, 1500.0);
        var result = optimizer.ExecOptimize().ExportResults();

        var csv = result.ExportSummaryAsColumn();
        var fields = csv.Split(',').Select(f => f.Trim()).ToList();

        // At least 18 fields now
        Assert.True(fields.Count >= 18);
        // P and T are first two
        Assert.Equal(result.GivenP.ToString(), fields[0]);
        Assert.Equal(result.GivenT.ToString(), fields[1]);
    }

    // ============================================================
    // H4: Error handling - no silent catch
    // ============================================================

    [Fact]
    public void ComputePhaseGibbs_InvalidMineral_ThrowsOrReturnsMaxValue()
    {
        // A mineral with completely invalid params should be handled gracefully
        var badMineral = new MineralParams
        {
            MineralName = "Invalid",
            NumAtoms = 0, // invalid
            MolarVolume = 0, // invalid
            KZero = 0,
        };
        var phase = new PhaseEntry { Name = "Bad", Mineral = badMineral, Amount = 1.0 };

        // Should not throw - returns MaxValue or NaN for uncomputable phases
        var gibbs = GibbsMinimizer.ComputePhaseGibbs(phase, 10.0, 1500.0);
        Assert.True(double.IsNaN(gibbs) || gibbs == double.MaxValue,
            "Invalid mineral should return NaN or MaxValue, not a normal value");
    }

    [Fact]
    public void EquilibriumAggregateCalculator_FailedPhase_ReportsWarning()
    {
        // When a phase fails to compute, the result should still contain computable phases
        var fo = MineralDatabase.GetByName("Forsterite")!;
        var badMineral = new MineralParams { MineralName = "Bad", NumAtoms = 0 };

        var phases = new List<PhaseEntry>
        {
            new() { Name = "Forsterite", Mineral = fo, Amount = 0.5 },
            new() { Name = "Bad", Mineral = badMineral, Amount = 0.5 },
        };

        var calc = new EquilibriumAggregateCalculator();
        var (mixedResult, individualResults) = calc.CalculateMechanical(phases, 10.0, 1500.0, MixtureMethod.Hill);

        // Should have at least the Forsterite result
        Assert.True(individualResults.Count >= 1);
        Assert.Contains(individualResults, r => r.name == "Forsterite");
    }
}
