using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Desktop.ViewModels;

public partial class MagmaOceanViewModel : ObservableObject
{
    [ObservableProperty] private double _pressure = 25.0;
    [ObservableProperty] private double _temperature = 3000.0;
    [ObservableProperty] private double _solidus;
    [ObservableProperty] private double _liquidus;
    [ObservableProperty] private double _meltFraction;
    [ObservableProperty] private string _meltingState = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var calc = new MagmaOceanCalculator();
            Solidus = calc.ComputeSolidus(Pressure);
            Liquidus = calc.ComputeLiquidus(Pressure);
            MeltFraction = calc.ComputeMeltFraction(Pressure, Temperature);
            MeltingState = calc.GetMeltingState(Pressure, Temperature);
            StatusMessage = $"Computed: Solidus={Solidus:F1} K, Liquidus={Liquidus:F1} K, State={MeltingState}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
