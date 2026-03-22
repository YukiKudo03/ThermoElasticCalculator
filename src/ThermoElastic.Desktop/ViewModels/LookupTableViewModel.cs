using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class LookupTableViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _pMin = 0.0001;
    [ObservableProperty] private double _pMax = 25.0;
    [ObservableProperty] private int _nPressure = 5;
    [ObservableProperty] private double _tMin = 300.0;
    [ObservableProperty] private double _tMax = 2500.0;
    [ObservableProperty] private int _nTemperature = 5;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public LookupTable? GeneratedTable { get; private set; }
    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Generate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];
            var generator = new LookupTableGenerator();
            GeneratedTable = generator.Generate(mineral, PMin, PMax, NPressure, TMin, TMax, NTemperature);
            StatusMessage = $"Generated {NPressure}x{NTemperature} lookup table for {mineral.MineralName}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
