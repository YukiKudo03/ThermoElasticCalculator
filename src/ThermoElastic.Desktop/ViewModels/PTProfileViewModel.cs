using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class PTProfileViewModel : ObservableObject
{
    [ObservableProperty] private string _mineralName = "(No mineral loaded)";
    [ObservableProperty] private string _statusMessage = string.Empty;

    private MineralParams? _mineral;

    public ObservableCollection<PTData> PTDataList { get; } = new();
    public ObservableCollection<ResultSummary> Results { get; } = new();

    public void LoadMineral(MineralParams mineral)
    {
        _mineral = mineral;
        MineralName = mineral.ParamSymbol;
    }

    [RelayCommand]
    private void AddPTData()
    {
        PTDataList.Add(new PTData { Pressure = 0, Temperature = 300 });
    }

    [RelayCommand]
    private void Calculate()
    {
        if (_mineral == null)
        {
            StatusMessage = "No mineral loaded.";
            return;
        }

        try
        {
            var profile = new PTProfile { Name = "Profile", Profile = PTDataList.ToList() };
            var calculator = new PTProfileCalculator(_mineral, profile);
            var results = calculator.DoProfileCalculationAsSummary();
            Results.Clear();
            foreach (var r in results)
            {
                Results.Add(r);
            }
            StatusMessage = $"Calculated {results.Count} points.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
