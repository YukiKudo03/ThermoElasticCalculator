using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class ClassicalGeobarometryViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedPhase1Index;
    [ObservableProperty] private int _selectedPhase2Index = 1;
    [ObservableProperty] private double _temperature = 1600.0;
    [ObservableProperty] private double _pMin = 10.0;
    [ObservableProperty] private double _pMax = 20.0;
    [ObservableProperty] private double _estimatedPressure;
    [ObservableProperty] private string _stablePhase = string.Empty;
    [ObservableProperty] private double _deltaG;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var phase1 = minerals[SelectedPhase1Index];
            var phase2 = minerals[SelectedPhase2Index];
            var calc = new ClassicalGeobarometer();

            EstimatedPressure = calc.EstimatePressure(phase1, phase2, Temperature, PMin, PMax);
            var (stable, dg) = calc.DetermineStablePhase(phase1, phase2, EstimatedPressure, Temperature);
            StablePhase = stable;
            DeltaG = dg;
            StatusMessage = $"Estimated P = {EstimatedPressure:F2} GPa, Stable: {StablePhase}, ΔG = {DeltaG:F2} kJ/mol";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
