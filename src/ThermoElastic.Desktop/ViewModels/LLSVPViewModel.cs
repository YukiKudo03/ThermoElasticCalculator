using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class LLSVPViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _pressure = 120.0;
    [ObservableProperty] private double _referenceTemperature = 2500.0;
    [ObservableProperty] private double _targetDlnVs = -0.02;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private double _requiredDeltaT;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];

            var calc = new LLSVPCalculator();
            RequiredDeltaT = calc.ComputeRequiredDeltaT(mineral, Pressure, ReferenceTemperature, TargetDlnVs);

            StatusMessage = $"Required DeltaT = {RequiredDeltaT:F1} K for dln(Vs) = {TargetDlnVs:F3} at {Pressure} GPa";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
