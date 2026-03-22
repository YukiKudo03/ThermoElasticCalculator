using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class WaterContentViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _pressure = 14.0;
    [ObservableProperty] private double _temperature = 1600.0;
    [ObservableProperty] private double _waterContent_wt = 1.0;
    [ObservableProperty] private double _observedDlnVs = -0.02;
    [ObservableProperty] private double _vs_hydrous;
    [ObservableProperty] private double _estimatedWater;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];
            var calc = new WaterContentEstimator();

            var (_, vs, _) = calc.ComputeHydrousProperties(mineral, Pressure, Temperature, WaterContent_wt);
            Vs_hydrous = vs;
            EstimatedWater = calc.EstimateWaterContent(mineral, Pressure, Temperature, ObservedDlnVs);
            StatusMessage = $"Water content: Vs_hydrous = {Vs_hydrous:F1} m/s, Estimated H2O = {EstimatedWater:F3} wt%";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
