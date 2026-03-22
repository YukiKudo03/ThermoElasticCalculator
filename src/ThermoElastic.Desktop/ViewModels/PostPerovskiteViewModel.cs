using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class PostPerovskiteViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedPvIndex;
    [ObservableProperty] private int _selectedPpvIndex;
    [ObservableProperty] private double _temperature = 2500.0;
    [ObservableProperty] private double _pMin = 100.0;
    [ObservableProperty] private double _pMax = 140.0;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private double _boundaryPressure;
    [ObservableProperty] private double _clapeyronSlope;
    [ObservableProperty] private double _dVsPercent;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    public PostPerovskiteViewModel()
    {
        var minerals = SLB2011Endmembers.GetAll();
        SelectedPvIndex = minerals.FindIndex(m => m.PaperName == "mpv");
        SelectedPpvIndex = minerals.FindIndex(m => m.PaperName == "mppv");
    }

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var pv = minerals[SelectedPvIndex];
            var ppv = minerals[SelectedPpvIndex];

            var calc = new PostPerovskiteCalculator();
            BoundaryPressure = calc.FindBoundary(pv, ppv, Temperature, PMin, PMax);
            ClapeyronSlope = calc.GetClapeyronSlope(pv, ppv, BoundaryPressure, Temperature);

            var comparison = calc.CompareAcrossTransition(pv, ppv, BoundaryPressure, Temperature);
            DVsPercent = comparison.dVs_percent;

            StatusMessage = $"Boundary at {BoundaryPressure:F1} GPa, Clapeyron slope {ClapeyronSlope:F4} GPa/K";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
