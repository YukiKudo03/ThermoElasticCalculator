using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class RockCalculatorViewModel : ObservableObject
{
    [ObservableProperty] private double _pressure;
    [ObservableProperty] private double _temperature = 300.0;
    [ObservableProperty] private int _selectedMethodIndex;
    [ObservableProperty] private string _rockName = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public ObservableCollection<MineralEntryViewModel> MineralEntries { get; } = new();
    public ObservableCollection<ResultSummary> Results { get; } = new();
    public string[] MethodNames => Enum.GetNames<MixtureMethod>();

    public void AddMineral(MineralParams mineral, double ratio = 0.0)
    {
        MineralEntries.Add(new MineralEntryViewModel
        {
            MineralName = mineral.ParamSymbol,
            VolumeRatio = ratio,
            Mineral = mineral,
        });
    }

    [RelayCommand]
    private void RemoveMineral(MineralEntryViewModel? entry)
    {
        if (entry != null)
        {
            MineralEntries.Remove(entry);
        }
    }

    [RelayCommand]
    private void Calculate()
    {
        if (MineralEntries.Count == 0)
        {
            StatusMessage = "No minerals added.";
            return;
        }

        try
        {
            var rock = new RockComposition
            {
                Name = string.IsNullOrWhiteSpace(RockName) ? "Rock" : RockName,
                Minerals = MineralEntries.Select(e => new RockMineralEntry
                {
                    Mineral = e.Mineral!,
                    VolumeRatio = e.VolumeRatio,
                }).ToList(),
            };

            var method = (MixtureMethod)SelectedMethodIndex;
            var calculator = new RockCalculator(rock, Pressure, Temperature, method);
            var (mixedResult, individualResults) = calculator.Calculate();

            Results.Clear();
            foreach (var (_, _, result) in individualResults)
            {
                Results.Add(result);
            }
            if (mixedResult != null)
            {
                Results.Add(mixedResult);
            }

            StatusMessage = $"Calculated rock with {individualResults.Count} minerals.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    public RockComposition ToRockComposition()
    {
        return new RockComposition
        {
            Name = RockName,
            Minerals = MineralEntries.Select(e => new RockMineralEntry
            {
                Mineral = e.Mineral!,
                VolumeRatio = e.VolumeRatio,
            }).ToList(),
        };
    }

    public void FromRockComposition(RockComposition rock)
    {
        RockName = rock.Name;
        MineralEntries.Clear();
        foreach (var entry in rock.Minerals)
        {
            AddMineral(entry.Mineral, entry.VolumeRatio);
        }
    }
}

public partial class MineralEntryViewModel : ObservableObject
{
    [ObservableProperty] private string _mineralName = string.Empty;
    [ObservableProperty] private double _volumeRatio;

    public MineralParams? Mineral { get; set; }
}
