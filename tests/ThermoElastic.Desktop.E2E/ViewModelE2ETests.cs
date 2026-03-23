using Xunit;
using ThermoElastic.Desktop.ViewModels;

namespace ThermoElastic.Desktop.E2E;

/// <summary>
/// End-to-end tests verifying ViewModel functionality.
/// Tests the full stack: ViewModel -> Calculator -> EOS -> Database.
/// </summary>
public class ViewModelE2ETests
{
    [Fact]
    public void MainWindow_AllNavigation_NoExceptions()
    {
        var vm = new MainWindowViewModel();

        // Core Tools (7)
        vm.ShowMineralEditorCommand.Execute(null);
        Assert.IsType<MineralEditorViewModel>(vm.CurrentView);
        vm.ShowPTProfileCommand.Execute(null);
        Assert.IsType<PTProfileViewModel>(vm.CurrentView);
        vm.ShowMixtureCommand.Execute(null);
        Assert.IsType<MixtureViewModel>(vm.CurrentView);
        vm.ShowRockCalculatorCommand.Execute(null);
        Assert.IsType<RockCalculatorViewModel>(vm.CurrentView);
        vm.ShowResultsCommand.Execute(null);
        Assert.IsType<ResultsViewModel>(vm.CurrentView);
        vm.ShowDatabaseCommand.Execute(null);
        Assert.IsType<MineralDatabaseViewModel>(vm.CurrentView);
        vm.ShowChartCommand.Execute(null);
        Assert.IsType<ChartViewModel>(vm.CurrentView);

        // EOS & Shock (4)
        vm.ShowHugoniotCommand.Execute(null);
        Assert.IsType<HugoniotViewModel>(vm.CurrentView);
        vm.ShowEOSFitterCommand.Execute(null);
        Assert.IsType<EOSFitterViewModel>(vm.CurrentView);
        vm.ShowThermoElasticFitterCommand.Execute(null);
        Assert.IsType<ThermoElasticFitterViewModel>(vm.CurrentView);
        vm.ShowVerificationDashboardCommand.Execute(null);
        Assert.IsType<VerificationDashboardViewModel>(vm.CurrentView);

        // Phase Equilibria (4)
        vm.ShowPhaseDiagramCommand.Execute(null);
        Assert.IsType<PhaseDiagramExplorerViewModel>(vm.CurrentView);
        vm.ShowPostPerovskiteCommand.Execute(null);
        Assert.IsType<PostPerovskiteViewModel>(vm.CurrentView);
        vm.ShowClassicalGeobarometryCommand.Execute(null);
        Assert.IsType<ClassicalGeobarometryViewModel>(vm.CurrentView);
        vm.ShowGeobarometryCommand.Execute(null);
        Assert.IsType<GeobarometryViewModel>(vm.CurrentView);

        // Mantle & Deep Earth (6)
        vm.ShowSensitivityKernelCommand.Execute(null);
        Assert.IsType<SensitivityKernelViewModel>(vm.CurrentView);
        vm.ShowAnelasticityCommand.Execute(null);
        Assert.IsType<AnelasticityViewModel>(vm.CurrentView);
        vm.ShowLLSVPCommand.Execute(null);
        Assert.IsType<LLSVPViewModel>(vm.CurrentView);
        vm.ShowULVZCommand.Execute(null);
        Assert.IsType<ULVZViewModel>(vm.CurrentView);
        vm.ShowSlabModelCommand.Execute(null);
        Assert.IsType<SlabModelViewModel>(vm.CurrentView);
        vm.ShowPlanetaryInteriorCommand.Execute(null);
        Assert.IsType<PlanetaryInteriorViewModel>(vm.CurrentView);

        // Material Properties (5)
        vm.ShowSpinCrossoverCommand.Execute(null);
        Assert.IsType<SpinCrossoverViewModel>(vm.CurrentView);
        vm.ShowThermalConductivityCommand.Execute(null);
        Assert.IsType<ThermalConductivityViewModel>(vm.CurrentView);
        vm.ShowElectricalConductivityCommand.Execute(null);
        Assert.IsType<ElectricalConductivityViewModel>(vm.CurrentView);
        vm.ShowElasticTensorCommand.Execute(null);
        Assert.IsType<ElasticTensorViewModel>(vm.CurrentView);
        vm.ShowOxygenFugacityCommand.Execute(null);
        Assert.IsType<OxygenFugacityViewModel>(vm.CurrentView);

        // Composition & Fluids (4)
        vm.ShowCompositionInverterCommand.Execute(null);
        Assert.IsType<CompositionInverterViewModel>(vm.CurrentView);
        vm.ShowIronPartitioningCommand.Execute(null);
        Assert.IsType<IronPartitioningViewModel>(vm.CurrentView);
        vm.ShowWaterContentCommand.Execute(null);
        Assert.IsType<WaterContentViewModel>(vm.CurrentView);
        vm.ShowMagmaOceanCommand.Execute(null);
        Assert.IsType<MagmaOceanViewModel>(vm.CurrentView);

        // Inversion & ML (3)
        vm.ShowMLDataCommand.Execute(null);
        Assert.IsType<MLDataViewModel>(vm.CurrentView);
        vm.ShowBayesianInversionCommand.Execute(null);
        Assert.IsType<BayesianInversionViewModel>(vm.CurrentView);
        vm.ShowLookupTableCommand.Execute(null);
        Assert.IsType<LookupTableViewModel>(vm.CurrentView);
    }

    [Fact]
    public void HugoniotViewModel_Calculate_ProducesResults()
    {
        var vm = new HugoniotViewModel();
        vm.SelectedMineralIndex = 0; // Forsterite
        vm.NumPoints = 10;
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.Results.Count > 0, "Should produce Hugoniot points");
        Assert.Contains("Computed", vm.StatusMessage);
    }

    [Fact]
    public void SensitivityKernelViewModel_Calculate_ProducesResults()
    {
        var vm = new SensitivityKernelViewModel();
        vm.SelectedMineralIndex = 0;
        vm.Pressure = 10.0;
        vm.Temperature = 1500.0;
        vm.CalculateCommand.Execute(null);
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void PlanetaryInteriorViewModel_ComputeEarth_ProducesProfile()
    {
        var vm = new PlanetaryInteriorViewModel();
        vm.SelectedPlanetIndex = 0; // Earth
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.ProfilePoints.Count > 0);
    }

    [Fact]
    public void PlanetaryInteriorViewModel_ComputeMars_ProducesProfile()
    {
        var vm = new PlanetaryInteriorViewModel();
        vm.SelectedPlanetIndex = 1; // Mars
        vm.CalculateCommand.Execute(null);
        Assert.True(vm.ProfilePoints.Count > 0);
    }

    [Fact]
    public void PhaseDiagramExplorerViewModel_FindBoundary_Works()
    {
        var vm = new PhaseDiagramExplorerViewModel();
        // fo -> mw transition
        var minerals = ThermoElastic.Core.Database.SLB2011Endmembers.GetAll();
        int foIdx = minerals.FindIndex(m => m.PaperName == "fo");
        int mwIdx = minerals.FindIndex(m => m.PaperName == "mw");
        vm.SelectedPhase1Index = foIdx;
        vm.SelectedPhase2Index = mwIdx;
        vm.Temperature = 1600;
        vm.PMin = 10.0;
        vm.PMax = 20.0;
        vm.CalculateCommand.Execute(null);
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void LookupTableViewModel_Generate_Works()
    {
        var vm = new LookupTableViewModel();
        vm.SelectedMineralIndex = 0;
        vm.NPressure = 3;
        vm.NTemperature = 3;
        vm.GenerateCommand.Execute(null);
        Assert.NotEmpty(vm.StatusMessage);
    }

    // Literature verification E2E tests
    [Fact]
    public void LiteratureVerification_Forsterite_AmbientVp()
    {
        // SLB2011 Table A1: Forsterite at ambient should have Vp ~ 8100 m/s
        var minerals = ThermoElastic.Core.Database.SLB2011Endmembers.GetAll();
        var fo = minerals.First(m => m.PaperName == "fo");
        var result = new ThermoElastic.Core.Calculations.MieGruneisenEOSOptimizer(fo, 0.0001, 300.0).ExecOptimize();
        Assert.InRange(result.Vp, 7500, 8700);
    }

    [Fact]
    public void LiteratureVerification_MgPerovskite_LowerMantle()
    {
        // Mg-Perovskite at 25 GPa, 2000K: Vs ~ 5000-7000 m/s
        var minerals = ThermoElastic.Core.Database.SLB2011Endmembers.GetAll();
        var mpv = minerals.First(m => m.PaperName == "mpv");
        var result = new ThermoElastic.Core.Calculations.MieGruneisenEOSOptimizer(mpv, 25.0, 2000.0).ExecOptimize();
        Assert.InRange(result.Vs, 5000, 7000);
    }

    [Fact]
    public void LiteratureVerification_PhaseBoundary410()
    {
        // 410 km boundary: fo -> mw at 1600K should be ~13-14.5 GPa
        var minerals = ThermoElastic.Core.Database.SLB2011Endmembers.GetAll();
        var fo = minerals.First(m => m.PaperName == "fo");
        var mw = minerals.First(m => m.PaperName == "mw");
        var pd = new ThermoElastic.Core.Calculations.PhaseDiagramCalculator();
        var p1 = new ThermoElastic.Core.Models.PhaseEntry { Name = "fo", Mineral = fo };
        var p2 = new ThermoElastic.Core.Models.PhaseEntry { Name = "mw", Mineral = mw };
        double boundary = pd.FindPhaseBoundary(p1, p2, 1600.0, 10.0, 20.0);
        Assert.InRange(boundary, 13.0, 14.5);
    }

    [Fact]
    public void LiteratureVerification_MarsInterior_MoI()
    {
        // Mars I/(MR^2) should be ~0.33-0.38
        var mars = new ThermoElastic.Core.Calculations.MarsInteriorModel();
        var profile = mars.Compute();
        Assert.InRange(profile.MomentOfInertiaFactor, 0.33, 0.38);
    }
}
