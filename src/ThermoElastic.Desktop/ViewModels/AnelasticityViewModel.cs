using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class AnelasticityViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _pressure = 5.0;
    [ObservableProperty] private double _temperature = 1400.0;
    [ObservableProperty] private double _frequency = 1.0;
    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private double _qS;
    [ObservableProperty] private double _vpElastic;
    [ObservableProperty] private double _vsElastic;
    [ObservableProperty] private double _vpAnelastic;
    [ObservableProperty] private double _vsAnelastic;
    [ObservableProperty] private double _deltaVsPercent;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];

            var eos = new MieGruneisenEOSOptimizer(mineral, Pressure, Temperature);
            var elastic = eos.ExecOptimize();

            var anelCalc = new AnelasticityCalculator();
            var result = anelCalc.ApplyCorrection(
                elastic.Vp, elastic.Vs, elastic.KS, elastic.GS,
                Temperature, Pressure, Frequency);

            QS = result.QS;
            VpElastic = result.Vp_elastic;
            VsElastic = result.Vs_elastic;
            VpAnelastic = result.Vp_anelastic;
            VsAnelastic = result.Vs_anelastic;
            DeltaVsPercent = result.DeltaVs_percent;

            StatusMessage = $"Computed anelastic correction for {mineral.MineralName} at {Pressure} GPa, {Temperature} K";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
