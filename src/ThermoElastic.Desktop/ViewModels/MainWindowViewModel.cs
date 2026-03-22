using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ThermoElastic.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _title = "ThermoElasticCalculator v1.0.0";

    private readonly MineralEditorViewModel _mineralEditorViewModel = new();
    private readonly PTProfileViewModel _ptProfileViewModel = new();
    private readonly MixtureViewModel _mixtureViewModel = new();
    private readonly RockCalculatorViewModel _rockCalculatorViewModel = new();
    private readonly ResultsViewModel _resultsViewModel = new();
    private readonly MineralDatabaseViewModel _mineralDatabaseViewModel = new();
    private readonly ChartViewModel _chartViewModel = new();
    private readonly HugoniotViewModel _hugoniotViewModel = new();
    private readonly PhaseDiagramExplorerViewModel _phaseDiagramExplorerViewModel = new();
    private readonly LookupTableViewModel _lookupTableViewModel = new();
    private readonly SensitivityKernelViewModel _sensitivityKernelViewModel = new();
    private readonly PlanetaryInteriorViewModel _planetaryInteriorViewModel = new();

    [RelayCommand]
    private void ShowMineralEditor()
    {
        CurrentView = _mineralEditorViewModel;
    }

    [RelayCommand]
    private void ShowPTProfile()
    {
        CurrentView = _ptProfileViewModel;
    }

    [RelayCommand]
    private void ShowMixture()
    {
        CurrentView = _mixtureViewModel;
    }

    [RelayCommand]
    private void ShowRockCalculator()
    {
        CurrentView = _rockCalculatorViewModel;
    }

    [RelayCommand]
    private void ShowResults()
    {
        CurrentView = _resultsViewModel;
    }

    [RelayCommand]
    private void ShowDatabase()
    {
        CurrentView = _mineralDatabaseViewModel;
    }

    [RelayCommand]
    private void ShowChart()
    {
        CurrentView = _chartViewModel;
    }

    [RelayCommand]
    private void ShowHugoniot()
    {
        CurrentView = _hugoniotViewModel;
    }

    [RelayCommand]
    private void ShowPhaseDiagram()
    {
        CurrentView = _phaseDiagramExplorerViewModel;
    }

    [RelayCommand]
    private void ShowLookupTable()
    {
        CurrentView = _lookupTableViewModel;
    }

    [RelayCommand]
    private void ShowSensitivityKernel()
    {
        CurrentView = _sensitivityKernelViewModel;
    }

    [RelayCommand]
    private void ShowPlanetaryInterior()
    {
        CurrentView = _planetaryInteriorViewModel;
    }
}
