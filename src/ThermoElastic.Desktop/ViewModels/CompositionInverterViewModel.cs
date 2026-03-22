using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public class MisfitPoint
{
    public double MgNumber { get; set; }
    public double Misfit { get; set; }
}

public partial class CompositionInverterViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMgEndmemberIndex;
    [ObservableProperty] private int _selectedFeEndmemberIndex;
    [ObservableProperty] private double _pressure = 38.0;
    [ObservableProperty] private double _temperature = 2000.0;
    [ObservableProperty] private double _depthKm = 1000.0;
    [ObservableProperty] private double _mgMin = 0.5;
    [ObservableProperty] private double _mgMax = 1.0;
    [ObservableProperty] private int _nSteps = 20;
    [ObservableProperty] private double _bestMgNumber;
    [ObservableProperty] private double _minMisfit;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public ObservableCollection<MisfitPoint> MisfitProfile { get; } = new();
    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.PaperName).ToList();

    public CompositionInverterViewModel()
    {
        var minerals = SLB2011Endmembers.GetAll();
        SelectedMgEndmemberIndex = minerals.FindIndex(m => m.PaperName == "mpv");
        SelectedFeEndmemberIndex = minerals.FindIndex(m => m.PaperName == "fpv");
    }

    [RelayCommand]
    private void Calculate()
    {
        if (NSteps <= 0) { StatusMessage = "Error: Number of steps must be > 0."; return; }
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mgEnd = minerals[SelectedMgEndmemberIndex];
            var feEnd = minerals[SelectedFeEndmemberIndex];

            var inverter = new CompositionInverter();
            var result = inverter.GridSearch(mgEnd, feEnd, Pressure, Temperature, DepthKm, MgMin, MgMax, NSteps);

            BestMgNumber = result.BestMgNumber;
            MinMisfit = result.MinMisfit;

            MisfitProfile.Clear();
            foreach (var (mg, misfit) in result.MisfitProfile)
                MisfitProfile.Add(new MisfitPoint { MgNumber = mg, Misfit = misfit });

            StatusMessage = $"Best Mg#={BestMgNumber:F3}, Min misfit={MinMisfit:E3}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
