using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class ElasticTensorViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedTensorIndex;
    [ObservableProperty] private double _directionX = 1.0;
    [ObservableProperty] private double _directionY = 0.0;
    [ObservableProperty] private double _directionZ = 0.0;
    [ObservableProperty] private double _vp;
    [ObservableProperty] private double _vs1;
    [ObservableProperty] private double _vs2;
    [ObservableProperty] private double _maxAnisotropy;
    [ObservableProperty] private double _kVRH;
    [ObservableProperty] private double _gVRH;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> TensorNames { get; } = new() { "Forsterite", "Periclase" };

    private static readonly Dictionary<int, string> TensorPaperNames = new()
    {
        { 0, "fo" },
        { 1, "pe" },
    };

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var paperName = TensorPaperNames[SelectedTensorIndex];
            var tensor = SingleCrystalElasticConstants.GetTensor(paperName);
            if (tensor == null)
            {
                StatusMessage = "Error: Tensor not found.";
                return;
            }

            var calc = new ElasticTensorCalculator();
            var direction = new[] { DirectionX, DirectionY, DirectionZ };

            var (vp, vs1, vs2) = calc.SolveChristoffel(tensor, direction);
            Vp = vp;
            Vs1 = vs1;
            Vs2 = vs2;

            MaxAnisotropy = calc.ComputeMaxAnisotropy(tensor);
            var (kVrh, gVrh) = calc.ComputeVRH(tensor);
            KVRH = kVrh;
            GVRH = gVrh;

            StatusMessage = $"Vp={vp:F1} m/s, Vs1={vs1:F1} m/s, Vs2={vs2:F1} m/s, Anisotropy={MaxAnisotropy:F1}%";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
