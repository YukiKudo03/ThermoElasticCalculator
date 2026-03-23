using System.Threading.Tasks;
using Xunit;
using ThermoElastic.Desktop.ViewModels;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.E2E;

/// <summary>
/// Phase 2 E2E tests for mineral-pair UI ViewModels.
/// Tests the full stack: ViewModel -> Calculator -> EOS -> Database.
/// </summary>
public class Phase2ViewModelE2ETests
{
    // ==================== AnelasticityView ====================

    [Fact]
    public async Task Phase2_Anelasticity_QS_InRange_At1400K_5GPa()
    {
        var vm = new AnelasticityViewModel();
        vm.SelectedMineralIndex = 0; // Forsterite
        vm.Pressure = 5.0;
        vm.Temperature = 1400.0;
        vm.Frequency = 1.0;
        await vm.CalculateCommand.ExecuteAsync(null);

        Assert.True(vm.QS > 0, $"QS should be positive, got {vm.QS}");
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public async Task Phase2_Anelasticity_AnelasticVs_LessThan_ElasticVs()
    {
        var vm = new AnelasticityViewModel();
        vm.SelectedMineralIndex = 0; // Forsterite
        vm.Pressure = 5.0;
        vm.Temperature = 1400.0;
        vm.Frequency = 1.0;
        await vm.CalculateCommand.ExecuteAsync(null);

        Assert.True(vm.VsAnelastic < vm.VsElastic,
            $"Anelastic Vs ({vm.VsAnelastic:F1}) should be less than elastic Vs ({vm.VsElastic:F1})");
        Assert.True(vm.DeltaVsPercent < 0, "DeltaVs should be negative");
    }

    // ==================== PostPerovskiteView ====================

    [Fact]
    public void Phase2_PostPerovskite_BoundaryPressure_InRange_At2500K()
    {
        var vm = new PostPerovskiteViewModel();
        // Constructor sets default mpv/mppv indices
        vm.Temperature = 2500.0;
        vm.PMin = 100.0;
        vm.PMax = 140.0;
        vm.CalculateCommand.Execute(null);

        Assert.InRange(vm.BoundaryPressure, 115, 140);
        Assert.Contains("Boundary", vm.StatusMessage);
    }

    [Fact]
    public void Phase2_PostPerovskite_PositiveClapeyronSlope()
    {
        var vm = new PostPerovskiteViewModel();
        vm.Temperature = 2500.0;
        vm.PMin = 100.0;
        vm.PMax = 140.0;
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.ClapeyronSlope > 0,
            $"Clapeyron slope ({vm.ClapeyronSlope:F4}) should be positive");
    }

    // ==================== LLSVPView ====================

    [Fact]
    public void Phase2_LLSVP_DeltaT_InRange_ForMinus2Percent()
    {
        var minerals = SLB2011Endmembers.GetAll();
        int mpvIdx = minerals.FindIndex(m => m.PaperName == "mpv");

        var vm = new LLSVPViewModel();
        vm.SelectedMineralIndex = mpvIdx;
        vm.Pressure = 120.0;
        vm.ReferenceTemperature = 2500.0;
        vm.TargetDlnVs = -0.02;
        vm.CalculateCommand.Execute(null);

        Assert.InRange(vm.RequiredDeltaT, 100, 1500);
        Assert.Contains("Required", vm.StatusMessage);
    }

    // ==================== SlabModelView ====================

    [Fact]
    public void Phase2_SlabModel_ColdSlab_PositiveDVs()
    {
        var vm = new SlabModelViewModel();
        vm.SelectedMineralIndex = 0; // Forsterite
        vm.PlateAgeMyr = 80.0;
        vm.Pressure = 13.0;
        vm.SlabT = 1000.0;
        vm.AmbientT = 1700.0;
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.DVsPercent > 0,
            $"Cold slab should have positive dVs anomaly, got {vm.DVsPercent:F2}%");
        Assert.True(vm.GeothermResults.Count > 0, "Should produce geotherm points");
        Assert.Contains("dVs", vm.StatusMessage);
    }

    // ==================== ULVZView ====================

    [Fact]
    public void Phase2_ULVZ_10PercentMelt_DVs_LessThanMinus5()
    {
        var minerals = SLB2011Endmembers.GetAll();
        int mpvIdx = minerals.FindIndex(m => m.PaperName == "mpv");

        var vm = new ULVZViewModel();
        vm.SelectedMineralIndex = mpvIdx;
        vm.MeltFraction = 0.10;
        vm.Pressure = 135.0;
        vm.Temperature = 3800.0;
        vm.CalculateCommand.Execute(null);

        Assert.True(vm.DVsPercent < -5.0,
            $"10% melt should give dVs < -5%, got {vm.DVsPercent:F2}%");
        Assert.Contains("ULVZ", vm.StatusMessage);
    }

    // ==================== SpinCrossoverView ====================

    [Fact]
    public void Phase2_SpinCrossover_NLS_IncreasesWithPressure()
    {
        var minerals = SLB2011Endmembers.GetAll();
        int wuIdx = minerals.FindIndex(m => m.PaperName == "wu");

        // Low pressure
        var vmLow = new SpinCrossoverViewModel();
        vmLow.SelectedMineralIndex = wuIdx;
        vmLow.Pressure = 10.0;
        vmLow.Temperature = 2000.0;
        vmLow.CalculateCommand.Execute(null);

        // Medium pressure
        var vmMid = new SpinCrossoverViewModel();
        vmMid.SelectedMineralIndex = wuIdx;
        vmMid.Pressure = 60.0;
        vmMid.Temperature = 2000.0;
        vmMid.CalculateCommand.Execute(null);

        // High pressure
        var vmHigh = new SpinCrossoverViewModel();
        vmHigh.SelectedMineralIndex = wuIdx;
        vmHigh.Pressure = 120.0;
        vmHigh.Temperature = 2000.0;
        vmHigh.CalculateCommand.Execute(null);

        Assert.True(vmMid.NLS > vmLow.NLS,
            $"nLS at 60 GPa ({vmMid.NLS:F4}) should be > nLS at 10 GPa ({vmLow.NLS:F4})");
        Assert.True(vmHigh.NLS > vmMid.NLS,
            $"nLS at 120 GPa ({vmHigh.NLS:F4}) should be > nLS at 60 GPa ({vmMid.NLS:F4})");
        Assert.Contains("Spin crossover", vmHigh.StatusMessage);
    }
}
