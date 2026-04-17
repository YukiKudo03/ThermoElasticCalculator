using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ThermoElastic.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _title = "ThermoElasticCalculator v1.0.0";

    // === Experimental Data ===
    private readonly SLBFitterViewModel _slbFitterViewModel = new();

    // === Core Tools ===
    private readonly MineralEditorViewModel _mineralEditorViewModel = new();
    private readonly PTProfileViewModel _ptProfileViewModel = new();
    private readonly MixtureViewModel _mixtureViewModel = new();
    private readonly RockCalculatorViewModel _rockCalculatorViewModel = new();
    private readonly ResultsViewModel _resultsViewModel = new();
    private readonly MineralDatabaseViewModel _mineralDatabaseViewModel = new();
    private readonly ChartViewModel _chartViewModel = new();

    // === EOS & Shock ===
    private readonly HugoniotViewModel _hugoniotViewModel = new();
    private readonly EOSFitterViewModel _eosFitterViewModel = new();
    private readonly ThermoElasticFitterViewModel _thermoElasticFitterViewModel = new();
    private readonly VerificationDashboardViewModel _verificationDashboardViewModel = new();

    // === Phase Equilibria ===
    private readonly PhaseDiagramExplorerViewModel _phaseDiagramExplorerViewModel = new();
    private readonly PostPerovskiteViewModel _postPerovskiteViewModel = new();
    private readonly ClassicalGeobarometryViewModel _classicalGeobarometryViewModel = new();
    private readonly GeobarometryViewModel _geobarometryViewModel = new();

    // === Mantle & Deep Earth ===
    private readonly SensitivityKernelViewModel _sensitivityKernelViewModel = new();
    private readonly AnelasticityViewModel _anelasticityViewModel = new();
    private readonly QProfileViewModel _qProfileViewModel = new();
    private readonly LLSVPViewModel _llsvpViewModel = new();
    private readonly ULVZViewModel _ulvzViewModel = new();
    private readonly SlabModelViewModel _slabModelViewModel = new();
    private readonly PlanetaryInteriorViewModel _planetaryInteriorViewModel = new();

    // === Material Properties ===
    private readonly SpinCrossoverViewModel _spinCrossoverViewModel = new();
    private readonly ThermalConductivityViewModel _thermalConductivityViewModel = new();
    private readonly ElectricalConductivityViewModel _electricalConductivityViewModel = new();
    private readonly ElasticTensorViewModel _elasticTensorViewModel = new();
    private readonly OxygenFugacityViewModel _oxygenFugacityViewModel = new();

    // === Composition & Fluids ===
    private readonly CompositionInverterViewModel _compositionInverterViewModel = new();
    private readonly IronPartitioningViewModel _ironPartitioningViewModel = new();
    private readonly WaterContentViewModel _waterContentViewModel = new();
    private readonly MagmaOceanViewModel _magmaOceanViewModel = new();

    // === Inversion & ML ===
    private readonly MLDataViewModel _mlDataViewModel = new();
    private readonly BayesianInversionViewModel _bayesianInversionViewModel = new();
    private readonly LookupTableViewModel _lookupTableViewModel = new();

    // === Experimental Data Commands ===
    [RelayCommand] private void ShowSLBFitter() => CurrentView = _slbFitterViewModel;

    // === Core Tools Commands ===
    [RelayCommand] private void ShowMineralEditor() => CurrentView = _mineralEditorViewModel;
    [RelayCommand] private void ShowPTProfile() => CurrentView = _ptProfileViewModel;
    [RelayCommand] private void ShowMixture() => CurrentView = _mixtureViewModel;
    [RelayCommand] private void ShowRockCalculator() => CurrentView = _rockCalculatorViewModel;
    [RelayCommand] private void ShowResults() => CurrentView = _resultsViewModel;
    [RelayCommand] private void ShowDatabase() => CurrentView = _mineralDatabaseViewModel;
    [RelayCommand] private void ShowChart() => CurrentView = _chartViewModel;

    // === EOS & Shock Commands ===
    [RelayCommand] private void ShowHugoniot() => CurrentView = _hugoniotViewModel;
    [RelayCommand] private void ShowEOSFitter() => CurrentView = _eosFitterViewModel;
    [RelayCommand] private void ShowThermoElasticFitter() => CurrentView = _thermoElasticFitterViewModel;
    [RelayCommand] private void ShowVerificationDashboard() => CurrentView = _verificationDashboardViewModel;

    // === Phase Equilibria Commands ===
    [RelayCommand] private void ShowPhaseDiagram() => CurrentView = _phaseDiagramExplorerViewModel;
    [RelayCommand] private void ShowPostPerovskite() => CurrentView = _postPerovskiteViewModel;
    [RelayCommand] private void ShowClassicalGeobarometry() => CurrentView = _classicalGeobarometryViewModel;
    [RelayCommand] private void ShowGeobarometry() => CurrentView = _geobarometryViewModel;

    // === Mantle & Deep Earth Commands ===
    [RelayCommand] private void ShowSensitivityKernel() => CurrentView = _sensitivityKernelViewModel;
    [RelayCommand] private void ShowAnelasticity() => CurrentView = _anelasticityViewModel;
    [RelayCommand] private void ShowQProfile() => CurrentView = _qProfileViewModel;
    [RelayCommand] private void ShowLLSVP() => CurrentView = _llsvpViewModel;
    [RelayCommand] private void ShowULVZ() => CurrentView = _ulvzViewModel;
    [RelayCommand] private void ShowSlabModel() => CurrentView = _slabModelViewModel;
    [RelayCommand] private void ShowPlanetaryInterior() => CurrentView = _planetaryInteriorViewModel;

    // === Material Properties Commands ===
    [RelayCommand] private void ShowSpinCrossover() => CurrentView = _spinCrossoverViewModel;
    [RelayCommand] private void ShowThermalConductivity() => CurrentView = _thermalConductivityViewModel;
    [RelayCommand] private void ShowElectricalConductivity() => CurrentView = _electricalConductivityViewModel;
    [RelayCommand] private void ShowElasticTensor() => CurrentView = _elasticTensorViewModel;
    [RelayCommand] private void ShowOxygenFugacity() => CurrentView = _oxygenFugacityViewModel;

    // === Composition & Fluids Commands ===
    [RelayCommand] private void ShowCompositionInverter() => CurrentView = _compositionInverterViewModel;
    [RelayCommand] private void ShowIronPartitioning() => CurrentView = _ironPartitioningViewModel;
    [RelayCommand] private void ShowWaterContent() => CurrentView = _waterContentViewModel;
    [RelayCommand] private void ShowMagmaOcean() => CurrentView = _magmaOceanViewModel;

    // === Inversion & ML Commands ===
    [RelayCommand] private void ShowMLData() => CurrentView = _mlDataViewModel;
    [RelayCommand] private void ShowBayesianInversion() => CurrentView = _bayesianInversionViewModel;
    [RelayCommand] private void ShowLookupTable() => CurrentView = _lookupTableViewModel;
}
