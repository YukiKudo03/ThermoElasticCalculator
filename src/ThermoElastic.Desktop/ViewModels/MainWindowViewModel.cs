using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ThermoElastic.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _title = "ThermoElasticCalculator v0.5.0";

    private readonly MineralEditorViewModel _mineralEditorViewModel = new();
    private readonly PTProfileViewModel _ptProfileViewModel = new();
    private readonly MixtureViewModel _mixtureViewModel = new();
    private readonly RockCalculatorViewModel _rockCalculatorViewModel = new();
    private readonly ResultsViewModel _resultsViewModel = new();
    private readonly MineralDatabaseViewModel _mineralDatabaseViewModel = new();
    private readonly ChartViewModel _chartViewModel = new();

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
}
