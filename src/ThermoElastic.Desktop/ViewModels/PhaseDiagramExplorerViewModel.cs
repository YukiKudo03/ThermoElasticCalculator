using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class PhaseDiagramExplorerViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedPhase1Index;
    [ObservableProperty] private int _selectedPhase2Index = 2; // Mg-Wadsleyite (mw)
    [ObservableProperty] private double _temperature = 1600.0;
    [ObservableProperty] private double _pMin = 5.0;
    [ObservableProperty] private double _pMax = 25.0;
    [ObservableProperty] private double _boundaryPressure = double.NaN;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var m1 = minerals[SelectedPhase1Index];
            var m2 = minerals[SelectedPhase2Index];
            var pd = new PhaseDiagramCalculator();
            var p1 = new PhaseEntry { Name = m1.PaperName, Mineral = m1 };
            var p2 = new PhaseEntry { Name = m2.PaperName, Mineral = m2 };
            BoundaryPressure = pd.FindPhaseBoundary(p1, p2, Temperature, PMin, PMax);
            if (double.IsNaN(BoundaryPressure))
                StatusMessage = $"No phase boundary found between {m1.MineralName} and {m2.MineralName} at {Temperature} K in [{PMin}, {PMax}] GPa";
            else
                StatusMessage = $"Phase boundary: {m1.MineralName} -> {m2.MineralName} at {BoundaryPressure:F2} GPa, {Temperature} K";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
