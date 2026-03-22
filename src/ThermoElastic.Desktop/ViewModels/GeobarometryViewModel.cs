using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class GeobarometryViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedHostIndex;
    [ObservableProperty] private int _selectedInclusionIndex = 1;
    [ObservableProperty] private double _entrapmentP = 1.0;
    [ObservableProperty] private double _entrapmentT = 1000.0;
    [ObservableProperty] private double _residualPressure;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var host = minerals[SelectedHostIndex];
            var inclusion = minerals[SelectedInclusionIndex];
            var calc = new IsomekeCalculator();
            ResidualPressure = calc.ComputeResidualPressure(host, inclusion, EntrapmentP, EntrapmentT);
            StatusMessage = $"Residual pressure = {ResidualPressure:F4} GPa for {inclusion.MineralName} in {host.MineralName}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
