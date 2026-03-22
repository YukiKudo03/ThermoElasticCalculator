using Xunit;
using ThermoElastic.Desktop.ViewModels;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.E2E;

/// <summary>
/// Phase 1 E2E tests for 7 new views: functional and literature verification.
/// </summary>
public class Phase1ViewModelE2ETests
{
    // ── 1. OxygenFugacityView ──

    [Fact]
    public void Phase1_OxygenFugacityViewModel_Calculate_ProducesResult()
    {
        var vm = new OxygenFugacityViewModel();
        vm.SelectedBufferIndex = 0; // IW
        vm.Temperature = 1473.0;
        vm.Pressure = 0.0001;
        vm.CalculateCommand.Execute(null);
        Assert.NotEqual(0.0, vm.LogFO2);
        Assert.Contains("log", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Phase1_OxygenFugacity_LiteratureVerification_IW_LessThan_QFM_LessThan_NNO()
    {
        var vm = new OxygenFugacityViewModel();
        vm.Temperature = 1473.0;
        vm.Pressure = 0.0001;

        vm.SelectedBufferIndex = 0; // IW
        vm.CalculateCommand.Execute(null);
        double iwValue = vm.LogFO2;

        vm.SelectedBufferIndex = 1; // QFM
        vm.CalculateCommand.Execute(null);
        double qfmValue = vm.LogFO2;

        vm.SelectedBufferIndex = 2; // NNO
        vm.CalculateCommand.Execute(null);
        double nnoValue = vm.LogFO2;

        Assert.True(iwValue < qfmValue, $"IW ({iwValue}) should be < QFM ({qfmValue})");
        Assert.True(qfmValue < nnoValue, $"QFM ({qfmValue}) should be < NNO ({nnoValue})");
    }

    // ── 2. MagmaOceanView ──

    [Fact]
    public void Phase1_MagmaOceanViewModel_Calculate_ProducesResult()
    {
        var vm = new MagmaOceanViewModel();
        vm.Pressure = 25.0;
        vm.Temperature = 3000.0;
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.Solidus > 0, "Solidus should be positive");
        Assert.True(vm.Liquidus > vm.Solidus, "Liquidus should exceed solidus");
        Assert.NotEmpty(vm.MeltingState);
        Assert.Contains("Computed", vm.StatusMessage);
    }

    [Fact]
    public void Phase1_MagmaOcean_LiteratureVerification_Solidus25GPa()
    {
        var vm = new MagmaOceanViewModel();
        vm.Pressure = 25.0;
        vm.Temperature = 2500.0;
        vm.CalculateCommand.Execute(null);
        Assert.InRange(vm.Solidus, 2200, 2600);
    }

    // ── 3. ElectricalConductivityView ──

    [Fact]
    public void Phase1_ElectricalConductivityViewModel_Calculate_ProducesResult()
    {
        var vm = new ElectricalConductivityViewModel();
        vm.Temperature = 1500.0;
        vm.Pressure = 5.0;
        vm.WaterContent_ppm = 0.0;
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.Conductivity > 0, "Conductivity should be positive");
        Assert.Contains("Conductivity", vm.StatusMessage);
    }

    [Fact]
    public void Phase1_ElectricalConductivity_LiteratureVerification_WetGreaterThan3xDry()
    {
        var vm = new ElectricalConductivityViewModel();
        vm.Temperature = 1500.0;
        vm.Pressure = 5.0;

        vm.WaterContent_ppm = 0.0;
        vm.CalculateCommand.Execute(null);
        double dry = vm.Conductivity;

        vm.WaterContent_ppm = 1000.0;
        vm.CalculateCommand.Execute(null);
        double wet = vm.Conductivity;

        Assert.True(wet > 3.0 * dry, $"Wet ({wet}) should be > 3x dry ({3.0 * dry})");
    }

    // ── 4. ThermalConductivityView ──

    [Fact]
    public void Phase1_ThermalConductivityViewModel_Calculate_ProducesResult()
    {
        var vm = new ThermalConductivityViewModel();
        // Select Periclase (MgO)
        var minerals = SLB2011Endmembers.GetAll();
        int idx = minerals.FindIndex(m => m.PaperName == "pe");
        vm.SelectedMineralIndex = idx;
        vm.Pressure = 0.0001;
        vm.Temperature = 300.0;
        vm.K0 = 50.0;
        vm.G = 5.0;
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.Conductivity > 0, "Conductivity should be positive");
        Assert.Contains("Conductivity", vm.StatusMessage);
    }

    [Fact]
    public void Phase1_ThermalConductivity_LiteratureVerification_MgO_Ambient()
    {
        var vm = new ThermalConductivityViewModel();
        var minerals = SLB2011Endmembers.GetAll();
        int idx = minerals.FindIndex(m => m.PaperName == "pe");
        vm.SelectedMineralIndex = idx;
        vm.Pressure = 0.0001;
        vm.Temperature = 300.0;
        vm.K0 = 50.0;
        vm.G = 5.0;
        vm.CalculateCommand.Execute(null);
        Assert.InRange(vm.Conductivity, 30.0, 60.0);
    }

    // ── 5. ClassicalGeobarometryView ──

    [Fact]
    public void Phase1_ClassicalGeobarometryViewModel_Calculate_ProducesResult()
    {
        var vm = new ClassicalGeobarometryViewModel();
        var minerals = SLB2011Endmembers.GetAll();
        int foIdx = minerals.FindIndex(m => m.PaperName == "fo");
        int mwIdx = minerals.FindIndex(m => m.PaperName == "mw");
        vm.SelectedPhase1Index = foIdx;
        vm.SelectedPhase2Index = mwIdx;
        vm.Temperature = 1600.0;
        vm.PMin = 10.0;
        vm.PMax = 20.0;
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.EstimatedPressure > 0, "Estimated pressure should be positive");
        Assert.NotEmpty(vm.StablePhase);
        Assert.Contains("Estimated", vm.StatusMessage);
    }

    [Fact]
    public void Phase1_ClassicalGeobarometry_LiteratureVerification_FoMw_1600K()
    {
        var vm = new ClassicalGeobarometryViewModel();
        var minerals = SLB2011Endmembers.GetAll();
        int foIdx = minerals.FindIndex(m => m.PaperName == "fo");
        int mwIdx = minerals.FindIndex(m => m.PaperName == "mw");
        vm.SelectedPhase1Index = foIdx;
        vm.SelectedPhase2Index = mwIdx;
        vm.Temperature = 1600.0;
        vm.PMin = 10.0;
        vm.PMax = 20.0;
        vm.CalculateCommand.Execute(null);
        Assert.InRange(vm.EstimatedPressure, 13.0, 14.5);
    }

    // ── 6. WaterContentView ──

    [Fact]
    public void Phase1_WaterContentViewModel_Calculate_ProducesResult()
    {
        var vm = new WaterContentViewModel();
        var minerals = SLB2011Endmembers.GetAll();
        int foIdx = minerals.FindIndex(m => m.PaperName == "fo");
        vm.SelectedMineralIndex = foIdx;
        vm.Pressure = 14.0;
        vm.Temperature = 1600.0;
        vm.WaterContent_wt = 1.0;
        vm.ObservedDlnVs = -0.02;
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.Vs_hydrous > 0, "Hydrous Vs should be positive");
        Assert.Contains("Water", vm.StatusMessage);
    }

    [Fact]
    public void Phase1_WaterContent_LiteratureVerification_HydrousVsLessThanDry()
    {
        var vm = new WaterContentViewModel();
        var minerals = SLB2011Endmembers.GetAll();
        int foIdx = minerals.FindIndex(m => m.PaperName == "fo");
        vm.SelectedMineralIndex = foIdx;
        vm.Pressure = 14.0;
        vm.Temperature = 1600.0;
        vm.WaterContent_wt = 1.0;
        vm.ObservedDlnVs = -0.02;
        vm.CalculateCommand.Execute(null);

        // Get dry Vs for comparison
        var fo = minerals[foIdx];
        var dry = new ThermoElastic.Core.Calculations.MieGruneisenEOSOptimizer(fo, 14.0, 1600.0).ExecOptimize();
        Assert.True(vm.Vs_hydrous < dry.Vs, $"Hydrous Vs ({vm.Vs_hydrous}) should be < dry Vs ({dry.Vs})");
    }

    // ── 7. GeobarometryView (Isomeke) ──

    [Fact]
    public void Phase1_GeobarometryViewModel_Calculate_ProducesResult()
    {
        var vm = new GeobarometryViewModel();
        var minerals = SLB2011Endmembers.GetAll();
        int foIdx = minerals.FindIndex(m => m.PaperName == "fo");
        int qtzIdx = minerals.FindIndex(m => m.PaperName == "qtz");
        vm.SelectedHostIndex = foIdx;
        vm.SelectedInclusionIndex = qtzIdx;
        vm.EntrapmentP = 1.0;
        vm.EntrapmentT = 1000.0;
        vm.CalculateCommand.Execute(null);
        Assert.NotEqual(0.0, vm.ResidualPressure);
        Assert.Contains("Residual", vm.StatusMessage);
    }

    [Fact]
    public void Phase1_Geobarometry_LiteratureVerification_QuartzInForsterite_PositiveP()
    {
        var vm = new GeobarometryViewModel();
        var minerals = SLB2011Endmembers.GetAll();
        int foIdx = minerals.FindIndex(m => m.PaperName == "fo");
        int qtzIdx = minerals.FindIndex(m => m.PaperName == "qtz");
        vm.SelectedHostIndex = foIdx;
        vm.SelectedInclusionIndex = qtzIdx;
        vm.EntrapmentP = 1.0;
        vm.EntrapmentT = 1000.0;
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.ResidualPressure > 0, $"Residual pressure ({vm.ResidualPressure}) should be > 0 for qtz-in-fo");
    }
}
