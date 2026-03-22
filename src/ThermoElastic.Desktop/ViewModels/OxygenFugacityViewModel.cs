using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Desktop.ViewModels;

public partial class OxygenFugacityViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedBufferIndex;
    [ObservableProperty] private double _temperature = 1473.0;
    [ObservableProperty] private double _pressure = 0.0001;
    [ObservableProperty] private double _logFO2;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> BufferNames { get; } = new(OxygenFugacityCalculator.AvailableBuffers);

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var buffer = BufferNames[SelectedBufferIndex];
            var calc = new OxygenFugacityCalculator();
            LogFO2 = calc.ComputeLogFO2(buffer, Temperature, Pressure);
            StatusMessage = $"log(fO2) = {LogFO2:F4} for {buffer} at {Temperature} K, {Pressure} GPa";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
