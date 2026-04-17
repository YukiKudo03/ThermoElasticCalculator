using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class QProfileViewModel : ObservableObject
{
    [ObservableProperty] private double _potentialTemp = 1600.0;
    [ObservableProperty] private double _frequency = 1.0;
    [ObservableProperty] private double _grainSize_mm = 10.0;
    [ObservableProperty] private double _maxDepth = 800.0;
    [ObservableProperty] private int _selectedModelIndex = 1; // Parametric Q default
    [ObservableProperty] private string _statusMessage = string.Empty;

    public ObservableCollection<QProfilePoint> ProfilePoints { get; } = new();
    public List<string> ModelNames { get; } = new() { "Simple (Karato)", "Parametric Q", "Andrade", "Extended Burgers" };

    [RelayCommand]
    private async Task CalculateAsync()
    {
        try
        {
            StatusMessage = "Computing Q profile...";
            ProfilePoints.Clear();

            var points = await Task.Run(() =>
            {
                IAnelasticityModel model = SelectedModelIndex switch
                {
                    0 => new AnelasticityCalculator(),
                    1 => new ParametricQCalculator(),
                    2 => new AndradeCalculator(),
                    3 => new ExtendedBurgersCalculator(),
                    _ => new ParametricQCalculator(),
                };

                var builder = new QProfileBuilder();
                return builder.Build(model, PotentialTemp, Frequency,
                    GrainSize_mm * 1e-3, MaxDepth, 25.0);
            });

            foreach (var pt in points)
                ProfilePoints.Add(pt);

            StatusMessage = $"Computed {ProfilePoints.Count} depth points, Tp={PotentialTemp:F0} K";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
