using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class MineralEditorViewModel : ObservableObject
{
    [ObservableProperty] private string _mineralName = string.Empty;
    [ObservableProperty] private string _paperName = string.Empty;
    [ObservableProperty] private int _numAtoms;
    [ObservableProperty] private double _molarVolume;
    [ObservableProperty] private double _molarWeight;
    [ObservableProperty] private double _kZero;
    [ObservableProperty] private double _k1Prime;
    [ObservableProperty] private double _k2Prime;
    [ObservableProperty] private double _gZero;
    [ObservableProperty] private double _g1Prime;
    [ObservableProperty] private double _g2Prime;
    [ObservableProperty] private double _debyeTempZero;
    [ObservableProperty] private double _gammaZero;
    [ObservableProperty] private double _qZero;
    [ObservableProperty] private double _ehtaZero;
    [ObservableProperty] private double _refTemp = 300.0;

    [ObservableProperty] private double _testPressure;
    [ObservableProperty] private double _testTemperature = 300.0;
    [ObservableProperty] private string _testResult = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public MineralParams ToMineralParams()
    {
        return new MineralParams
        {
            MineralName = MineralName,
            PaperName = PaperName,
            NumAtoms = NumAtoms,
            MolarVolume = MolarVolume,
            MolarWeight = MolarWeight,
            KZero = KZero,
            K1Prime = K1Prime,
            K2Prime = K2Prime,
            GZero = GZero,
            G1Prime = G1Prime,
            G2Prime = G2Prime,
            DebyeTempZero = DebyeTempZero,
            GammaZero = GammaZero,
            QZero = QZero,
            EhtaZero = EhtaZero,
            RefTemp = RefTemp,
        };
    }

    public void FromMineralParams(MineralParams m)
    {
        MineralName = m.MineralName;
        PaperName = m.PaperName;
        NumAtoms = m.NumAtoms;
        MolarVolume = m.MolarVolume;
        MolarWeight = m.MolarWeight;
        KZero = m.KZero;
        K1Prime = m.K1Prime;
        K2Prime = m.K2Prime;
        GZero = m.GZero;
        G1Prime = m.G1Prime;
        G2Prime = m.G2Prime;
        DebyeTempZero = m.DebyeTempZero;
        GammaZero = m.GammaZero;
        QZero = m.QZero;
        EhtaZero = m.EhtaZero;
        RefTemp = m.RefTemp;
    }

    [RelayCommand]
    private void CalcTest()
    {
        try
        {
            var mineral = ToMineralParams();
            var optimizer = new MieGruneisenEOSOptimizer(mineral, TestPressure, TestTemperature);
            var result = optimizer.ExecOptimize().ExportResults();
            var sb = new StringBuilder();
            sb.AppendLine($"Vp = {result.Vp:F2} m/s");
            sb.AppendLine($"Vs = {result.Vs:F2} m/s");
            sb.AppendLine($"Vb = {result.Vb:F2} m/s");
            sb.AppendLine($"Density = {result.Density:F4} g/cm3");
            sb.AppendLine($"KS = {result.KS:F2} GPa");
            sb.AppendLine($"GS = {result.GS:F2} GPa");
            TestResult = sb.ToString();
            StatusMessage = "Calculation completed.";
        }
        catch (Exception ex)
        {
            TestResult = string.Empty;
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
