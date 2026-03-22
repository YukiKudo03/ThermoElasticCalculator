using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class MLDataViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _pMin = 0.001;
    [ObservableProperty] private double _pMax = 25.0;
    [ObservableProperty] private double _tMin = 300.0;
    [ObservableProperty] private double _tMax = 2000.0;
    [ObservableProperty] private int _nSamples = 50;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public ObservableCollection<TrainingDataPoint> TrainingData { get; } = new();
    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];

            var generator = new TrainingDataGenerator();
            var data = generator.Generate(mineral, PMin, PMax, TMin, TMax, NSamples);

            TrainingData.Clear();
            foreach (var point in data)
                TrainingData.Add(point);

            StatusMessage = $"Generated {TrainingData.Count} training points for {mineral.MineralName}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
