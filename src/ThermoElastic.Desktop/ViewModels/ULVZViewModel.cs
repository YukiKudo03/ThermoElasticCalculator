using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class ULVZViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _meltFraction = 0.10;
    [ObservableProperty] private double _pressure = 135.0;
    [ObservableProperty] private double _temperature = 3800.0;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private double _vp;
    [ObservableProperty] private double _vs;
    [ObservableProperty] private double _density;
    [ObservableProperty] private double _dVpPercent;
    [ObservableProperty] private double _dVsPercent;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];

            var calc = new ULVZCalculator();
            var result = calc.ComputeMeltMixture(mineral, MeltFraction, Pressure, Temperature);

            Vp = result.Vp;
            Vs = result.Vs;
            Density = result.Density;
            DVpPercent = result.dVp_pct;
            DVsPercent = result.dVs_pct;

            StatusMessage = $"ULVZ mixture: {MeltFraction * 100:F0}% melt at {Pressure} GPa, {Temperature} K";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
