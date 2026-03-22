using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class SpinCrossoverViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _pressure = 60.0;
    [ObservableProperty] private double _temperature = 2000.0;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private double _nLS;
    [ObservableProperty] private double _volume;
    [ObservableProperty] private double _kS;
    [ObservableProperty] private double _gS;
    [ObservableProperty] private double _vp;
    [ObservableProperty] private double _vs;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    public SpinCrossoverViewModel()
    {
        var minerals = SLB2011Endmembers.GetAll();
        SelectedMineralIndex = minerals.FindIndex(m => m.PaperName == "wu");
    }

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var hs = minerals[SelectedMineralIndex];
            var ls = SpinCrossoverCalculator.CreateLSEndmember(hs);

            var calc = new SpinCrossoverCalculator();
            var result = calc.ComputeSpinState(hs, ls, Pressure, Temperature);

            NLS = result.nLS;
            Volume = result.Volume;
            KS = result.KS;
            GS = result.GS;
            Vp = result.Vp;
            Vs = result.Vs;

            StatusMessage = $"Spin crossover: nLS = {NLS:F3} at {Pressure} GPa, {Temperature} K";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
