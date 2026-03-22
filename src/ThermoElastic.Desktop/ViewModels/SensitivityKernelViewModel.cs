using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class SensitivityKernelViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _pressure = 10.0;
    [ObservableProperty] private double _temperature = 1500.0;
    [ObservableProperty] private double _dlnVp_dT;
    [ObservableProperty] private double _dlnVs_dT;
    [ObservableProperty] private double _dlnRho_dT;
    [ObservableProperty] private double _rThermal;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];
            var calculator = new SensitivityKernelCalculator();
            var kernel = calculator.ComputeThermalSensitivity(mineral, Pressure, Temperature);
            DlnVp_dT = kernel.DlnVp_dT;
            DlnVs_dT = kernel.DlnVs_dT;
            DlnRho_dT = kernel.DlnRho_dT;
            RThermal = kernel.R_thermal;
            StatusMessage = $"Sensitivity for {mineral.MineralName} at {Pressure} GPa, {Temperature} K: dlnVs/dT = {DlnVs_dT:E3}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
