using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class HugoniotViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private int _numPoints = 20;
    [ObservableProperty] private double _maxCompression = 0.65;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public ObservableCollection<HugoniotPoint> Results { get; } = new();
    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];
            var calculator = new HugoniotCalculator(mineral);
            var points = calculator.ComputeHugoniot(NumPoints, MaxCompression);
            Results.Clear();
            foreach (var p in points) Results.Add(p);
            StatusMessage = $"Computed {points.Count} Hugoniot points for {mineral.MineralName}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
