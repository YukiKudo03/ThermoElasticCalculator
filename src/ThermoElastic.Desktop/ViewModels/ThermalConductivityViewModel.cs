using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class ThermalConductivityViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _pressure = 0.0001;
    [ObservableProperty] private double _temperature = 300.0;
    [ObservableProperty] private double _k0 = 50.0;
    [ObservableProperty] private double _g = 5.0;
    [ObservableProperty] private double _conductivity;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];
            var calc = new ThermalConductivityCalculator();
            Conductivity = calc.ComputeLatticeConductivity(mineral, Pressure, Temperature, K0, G);
            StatusMessage = $"Conductivity = {Conductivity:F4} W/m/K for {mineral.MineralName}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
