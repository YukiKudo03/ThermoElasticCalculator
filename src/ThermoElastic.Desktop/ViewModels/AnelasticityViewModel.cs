using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    // Model selection
    [ObservableProperty] private int _selectedModelIndex; // 0=Simple, 1=Parametric, 2=Andrade, 3=ExtBurgers

    // Extended parameters
    [ObservableProperty] private double _grainSize_mm = 10.0;  // mm
    [ObservableProperty] private double _waterContent = 0.0;    // ppm H/Si
    [ObservableProperty] private double _meltFraction = 0.0;

    // Results
    [ObservableProperty] private double _qS;
    [ObservableProperty] private double _vpElastic;
    [ObservableProperty] private double _vsElastic;
    [ObservableProperty] private double _vpAnelastic;
    [ObservableProperty] private double _vsAnelastic;
    [ObservableProperty] private double _deltaVsPercent;
    [ObservableProperty] private double _deltaVpPercent;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();
    public List<string> ModelNames { get; } = new() { "Simple (Karato)", "Parametric Q(T,P,f,d)", "Andrade", "Extended Burgers" };

    [RelayCommand]
    private async Task CalculateAsync()
    {
        try
        {
            StatusMessage = "Computing...";
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];

            var result = await Task.Run(() =>
            {
                var eos = new MieGruneisenEOSOptimizer(mineral, Pressure, Temperature);
                var elastic = eos.ExecOptimize();

                var anelParams = AnelasticityDatabase.GetParamsForMineral(mineral.PaperName) with
                {
                    GrainSize_m = GrainSize_mm * 1e-3,
                    WaterContent_ppm = WaterContent,
                    MeltFraction = MeltFraction,
                };

                IAnelasticityModel model = CreateModel(SelectedModelIndex);

                return model.ApplyCorrection(
                    elastic.Vp, elastic.Vs, elastic.KS, elastic.GS,
                    Temperature, Pressure, Frequency, anelParams);
            });

            QS = result.QS;
            VpElastic = result.Vp_elastic;
            VsElastic = result.Vs_elastic;
            VpAnelastic = result.Vp_anelastic;
            VsAnelastic = result.Vs_anelastic;
            DeltaVsPercent = result.DeltaVs_percent;
            DeltaVpPercent = result.DeltaVp_percent;

            StatusMessage = $"{ModelNames[SelectedModelIndex]}: QS={QS:F1}, ΔVs={DeltaVsPercent:F2}%, ΔVp={DeltaVpPercent:F2}%";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }

    private static IAnelasticityModel CreateModel(int modelIndex)
    {
        IAnelasticityModel baseModel = modelIndex switch
        {
            0 => new AnelasticityCalculator(),
            1 => new ParametricQCalculator(),
            2 => new AndradeCalculator(),
            3 => new ExtendedBurgersCalculator(),
            _ => new AnelasticityCalculator(),
        };

        // Wrap with water and melt decorators
        return new MeltQCorrector(new WaterQCorrector(baseModel));
    }
}
