using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;

namespace ThermoElastic.Desktop.ViewModels;

public partial class VerificationDashboardViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private double _pressure = 10.0;
    [ObservableProperty] private double _temperature = 1500.0;
    [ObservableProperty] private double _maxwellResidual;
    [ObservableProperty] private double _gibbsHelmholtzResidual;
    [ObservableProperty] private double _entropyResidual;
    [ObservableProperty] private double _bulkModulusResidual;
    [ObservableProperty] private double _ksKtResidual;
    [ObservableProperty] private bool _isValid;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public List<string> MineralNames { get; } = SLB2011Endmembers.GetAll().Select(m => m.MineralName).ToList();

    public VerificationDashboardViewModel()
    {
        var minerals = SLB2011Endmembers.GetAll();
        SelectedMineralIndex = minerals.FindIndex(m => m.PaperName == "fo");
    }

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var minerals = SLB2011Endmembers.GetAll();
            var mineral = minerals[SelectedMineralIndex];

            var verifier = new ThermodynamicVerifier();
            var result = verifier.Verify(mineral, Pressure, Temperature);

            MaxwellResidual = result.MaxwellResidual;
            GibbsHelmholtzResidual = result.GibbsHelmholtzResidual;
            EntropyResidual = result.EntropyResidual;
            BulkModulusResidual = result.BulkModulusResidual;
            KsKtResidual = result.KsKtResidual;
            IsValid = result.IsValid;

            StatusMessage = $"Verification {(IsValid ? "PASSED" : "FAILED")} for {mineral.MineralName} at {Pressure} GPa, {Temperature} K";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }
}
