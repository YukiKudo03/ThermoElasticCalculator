using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class MixtureViewModel : ObservableObject
{
    [ObservableProperty] private string _mineral1Name = "(No mineral loaded)";
    [ObservableProperty] private string _mineral2Name = "(No mineral loaded)";
    [ObservableProperty] private double _pressure;
    [ObservableProperty] private double _temperature = 300.0;
    [ObservableProperty] private int _selectedMethodIndex;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private double _ratioStart;
    [ObservableProperty] private double _ratioEnd = 1.0;
    [ObservableProperty] private double _ratioStep = 0.1;

    private MineralParams? _mineral1;
    private MineralParams? _mineral2;

    public ObservableCollection<double> RatioList { get; } = new();
    public ObservableCollection<ResultSummary> Results { get; } = new();

    public string[] MethodNames => Enum.GetNames<MixtureMethod>();

    [RelayCommand]
    private void GenerateRatioList()
    {
        RatioList.Clear();
        if (RatioStep <= 0)
        {
            StatusMessage = "Step must be > 0.";
            return;
        }
        for (double r = RatioStart; r <= RatioEnd + 1e-10; r += RatioStep)
        {
            RatioList.Add(Math.Round(r, 10));
        }
        StatusMessage = $"Generated {RatioList.Count} ratio values.";
    }

    public void LoadMineral1(MineralParams mineral)
    {
        _mineral1 = mineral;
        Mineral1Name = mineral.ParamSymbol;
    }

    public void LoadMineral2(MineralParams mineral)
    {
        _mineral2 = mineral;
        Mineral2Name = mineral.ParamSymbol;
    }

    public VProfileCalculator? CreateVProfileCalculator()
    {
        if (_mineral1 == null || _mineral2 == null || RatioList.Count == 0) return null;

        var pt = new PTData { Pressure = Pressure, Temperature = Temperature };
        var res1 = new MieGruneisenEOSOptimizer(_mineral1, pt).ExecOptimize().ExportResults();
        var res2 = new MieGruneisenEOSOptimizer(_mineral2, pt).ExecOptimize().ExportResults();

        return new VProfileCalculator(RatioList.ToList(), res1, res2, $"{_mineral1.MineralName}-{_mineral2.MineralName}");
    }

    public void LoadFromVProfile(VProfileCalculator vpc)
    {
        RatioList.Clear();
        foreach (var r in vpc.Elem1RatioList)
        {
            RatioList.Add(r);
        }
        StatusMessage = $"Loaded VProfile '{vpc.Name}' with {vpc.Elem1RatioList.Count} ratios.";
    }

    [RelayCommand]
    private void Calculate()
    {
        if (_mineral1 == null || _mineral2 == null)
        {
            StatusMessage = "Please load both minerals.";
            return;
        }

        try
        {
            var pt = new PTData { Pressure = Pressure, Temperature = Temperature };
            var res1 = new MieGruneisenEOSOptimizer(_mineral1, pt).ExecOptimize().ExportResults();
            var res2 = new MieGruneisenEOSOptimizer(_mineral2, pt).ExecOptimize().ExportResults();

            var method = (MixtureMethod)SelectedMethodIndex;
            var ratioList = RatioList.ToList();
            var vpc = new VProfileCalculator(ratioList, res1, res2, $"{_mineral1.MineralName}-{_mineral2.MineralName}");

            var results = method switch
            {
                MixtureMethod.Voigt => vpc.VoigtResults(),
                MixtureMethod.Reuss => vpc.ReussResults(),
                MixtureMethod.HS => vpc.HSResults(),
                _ => vpc.HillResults(),
            };

            Results.Clear();
            foreach (var (_, ret) in results)
            {
                Results.Add(ret);
            }
            StatusMessage = $"Calculated {results.Count} compositions.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
