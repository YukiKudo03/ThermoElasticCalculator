using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class IronPartitioningViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMgPvIndex;
    [ObservableProperty] private int _selectedFePvIndex;
    [ObservableProperty] private int _selectedMgFpIndex;
    [ObservableProperty] private int _selectedFeFpIndex;
    [ObservableProperty] private double _bulkXFe = 0.10;
    [ObservableProperty] private double _pressure = 25.0;
    [ObservableProperty] private double _temperature = 2000.0;
    [ObservableProperty] private double _xFePv;
    [ObservableProperty] private double _xFeFp;
    [ObservableProperty] private double _kD;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.PaperName).ToList();

    public IronPartitioningViewModel()
    {
        var minerals = SLB2011Endmembers.GetAll();
        SelectedMgPvIndex = minerals.FindIndex(m => m.PaperName == "mpv");
        SelectedFePvIndex = minerals.FindIndex(m => m.PaperName == "fpv");
        SelectedMgFpIndex = minerals.FindIndex(m => m.PaperName == "pe");
        SelectedFeFpIndex = minerals.FindIndex(m => m.PaperName == "wu");
    }

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mgPv = minerals[SelectedMgPvIndex];
            var fePv = minerals[SelectedFePvIndex];
            var mgFp = minerals[SelectedMgFpIndex];
            var feFp = minerals[SelectedFeFpIndex];

            var solver = new IronPartitioningSolver();
            var (xFePv, xFeFp, kd) = solver.SolvePartitioning(mgPv, fePv, mgFp, feFp, BulkXFe, Pressure, Temperature);

            XFePv = xFePv;
            XFeFp = xFeFp;
            KD = kd;
            StatusMessage = $"Partitioning solved: KD={kd:F4}, XFe_pv={xFePv:F4}, XFe_fp={xFeFp:F4}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
