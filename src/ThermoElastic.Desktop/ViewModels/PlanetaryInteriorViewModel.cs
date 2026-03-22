using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class PlanetaryInteriorViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedPlanetIndex;
    [ObservableProperty] private double _momentOfInertiaFactor;
    [ObservableProperty] private double _totalMass;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public ObservableCollection<ProfilePointViewModel> ProfilePoints { get; } = new();
    public List<string> PlanetNames { get; } = new() { "Earth", "Mars" };

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            PlanetaryConfig config;
            if (SelectedPlanetIndex == 1)
            {
                config = MarsInteriorModel.GetDefaultConfig();
            }
            else
            {
                // Earth default config
                var minerals = SLB2011Endmembers.GetAll();
                config = new PlanetaryConfig
                {
                    Name = "Earth",
                    Radius_km = 6371.0,
                    CoreRadius_km = 3480.0,
                    CoreDensity = 11.0,
                    SurfaceTemperature = 300.0,
                    PotentialTemperature = 1600.0,
                    MantleMineral = minerals.First(m => m.PaperName == "fo"),
                };
            }

            var solver = new PlanetaryInteriorSolver();
            var profile = solver.Solve(config);

            MomentOfInertiaFactor = profile.MomentOfInertiaFactor;
            TotalMass = profile.TotalMass;

            ProfilePoints.Clear();
            for (int i = 0; i < profile.Radius_km.Length; i++)
            {
                ProfilePoints.Add(new ProfilePointViewModel
                {
                    Radius_km = profile.Radius_km[i],
                    Depth_km = profile.Depth_km[i],
                    Pressure_GPa = profile.Pressure_GPa[i],
                    Temperature_K = profile.Temperature_K[i],
                    Density = profile.Density[i],
                    Vp = profile.Vp[i],
                    Vs = profile.Vs[i],
                });
            }

            StatusMessage = $"Computed {config.Name} interior: MoI = {MomentOfInertiaFactor:F4}, Mass = {TotalMass:E3} kg, {ProfilePoints.Count} points";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}

public class ProfilePointViewModel
{
    public double Radius_km { get; set; }
    public double Depth_km { get; set; }
    public double Pressure_GPa { get; set; }
    public double Temperature_K { get; set; }
    public double Density { get; set; }
    public double Vp { get; set; }
    public double Vs { get; set; }
}
