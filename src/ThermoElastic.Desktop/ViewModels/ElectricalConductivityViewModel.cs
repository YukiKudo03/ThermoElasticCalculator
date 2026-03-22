using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Desktop.ViewModels;

public partial class ElectricalConductivityViewModel : ObservableObject
{
    [ObservableProperty] private double _temperature = 1500.0;
    [ObservableProperty] private double _pressure = 5.0;
    [ObservableProperty] private double _waterContent_ppm;
    [ObservableProperty] private double _conductivity;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var calc = new ElectricalConductivityCalculator();
            Conductivity = calc.ComputeConductivity(Temperature, Pressure, WaterContent_ppm);
            StatusMessage = $"Conductivity = {Conductivity:E4} S/m at {Temperature} K, {Pressure} GPa, {WaterContent_ppm} ppm H2O";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
