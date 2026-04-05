using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class SLBFitterViewModel : ObservableObject
{
    // === Import Tab ===
    [ObservableProperty] private string _csvText = string.Empty;
    [ObservableProperty] private string _importStatus = "Load CSV data or paste from clipboard.";
    [ObservableProperty] private int _dataPointCount;
    [ObservableProperty] private string _dataPreview = string.Empty;

    // === Fit Config Tab ===
    [ObservableProperty] private int _selectedMineralIndex;
    [ObservableProperty] private bool _fitV0 = true;
    [ObservableProperty] private bool _fitK0 = true;
    [ObservableProperty] private bool _fitK0Prime = true;
    [ObservableProperty] private bool _fitG0 = true;
    [ObservableProperty] private bool _fitG0Prime = true;
    [ObservableProperty] private bool _fitDebyeTemp;
    [ObservableProperty] private bool _fitGamma0;
    [ObservableProperty] private bool _fitQ0;
    [ObservableProperty] private bool _fitEtaS0;
    [ObservableProperty] private int _selectedFitTarget;
    [ObservableProperty] private bool _isFitting;
    [ObservableProperty] private string _fitStatus = string.Empty;

    // === Results Tab ===
    [ObservableProperty] private string _resultsText = string.Empty;
    [ObservableProperty] private string _residualsText = string.Empty;
    [ObservableProperty] private bool _hasResults;
    [ObservableProperty] private bool _fitConverged;
    [ObservableProperty] private double _chiSquared;
    [ObservableProperty] private double _reducedChiSquared;
    [ObservableProperty] private int _fitIterations;

    // === PREM Tab ===
    [ObservableProperty] private string _premComparisonText = string.Empty;
    [ObservableProperty] private double _adiabaticTemp = 1600.0;
    [ObservableProperty] private double _depthMin = 100.0;
    [ObservableProperty] private double _depthMax = 2891.0;
    [ObservableProperty] private double _depthStep = 50.0;

    // Internal state
    private ExperimentalDataset? _dataset;
    private MineralParams? _fittedMineral;
    private FittingResult? _lastResult;

    public ObservableCollection<string> MineralNames { get; } = new();
    public List<MineralParams> AllMinerals { get; } = new();
    public string[] FitTargetOptions { get; } = { "Joint (Vp+Vs+ρ)", "Vp only", "Vs only", "Density only" };

    public SLBFitterViewModel()
    {
        var minerals = SLB2011Endmembers.GetAll();
        AllMinerals.AddRange(minerals);
        foreach (var m in minerals)
            MineralNames.Add($"{m.MineralName} ({m.PaperName})");
        SelectedMineralIndex = minerals.FindIndex(m => m.PaperName == "mpv"); // default: Mg-Perovskite
        if (SelectedMineralIndex < 0) SelectedMineralIndex = 0;

        CsvText = GenerateSampleData();
    }

    [RelayCommand]
    private void LoadData()
    {
        try
        {
            _dataset = ExperimentalDataset.ParseCsv(CsvText, "User Data");
            DataPointCount = _dataset.Data.Count;
            ImportStatus = $"Loaded {DataPointCount} data points. Observables: " +
                           $"Vp={_dataset.Data.Count(d => d.Vp.HasValue)}, " +
                           $"Vs={_dataset.Data.Count(d => d.Vs.HasValue)}, " +
                           $"ρ={_dataset.Data.Count(d => d.Density.HasValue)}";

            var sb = new StringBuilder();
            sb.AppendLine("P(GPa)\tT(K)\tVp(m/s)\tVs(m/s)\tρ(g/cm³)");
            foreach (var pt in _dataset.Data.Take(20))
            {
                sb.AppendLine($"{pt.Pressure:F1}\t{pt.Temperature:F0}\t" +
                              $"{pt.Vp?.ToString("F0") ?? "-"}\t" +
                              $"{pt.Vs?.ToString("F0") ?? "-"}\t" +
                              $"{pt.Density?.ToString("F4") ?? "-"}");
            }
            if (_dataset.Data.Count > 20)
                sb.AppendLine($"... and {_dataset.Data.Count - 20} more rows");
            DataPreview = sb.ToString();
        }
        catch (Exception ex)
        {
            ImportStatus = $"Error: {ex.Message}";
            _dataset = null;
            DataPointCount = 0;
            DataPreview = string.Empty;
        }
    }

    [RelayCommand]
    private async Task RunFitAsync()
    {
        if (_dataset == null || _dataset.Data.Count == 0)
        {
            FitStatus = "Load data first.";
            return;
        }

        IsFitting = true;
        FitStatus = "Fitting...";

        try
        {
            var guess = AllMinerals[SelectedMineralIndex];
            var options = new FittingOptions
            {
                FitV0 = FitV0, FitK0 = FitK0, FitK0Prime = FitK0Prime,
                FitG0 = FitG0, FitG0Prime = FitG0Prime,
                FitDebyeTemp = FitDebyeTemp, FitGamma0 = FitGamma0,
                FitQ0 = FitQ0, FitEtaS0 = FitEtaS0,
                Target = (FitTarget)SelectedFitTarget,
            };

            var fitter = new SLBParameterFitter();
            _lastResult = await Task.Run(() => fitter.Fit(_dataset, guess, options));

            _fittedMineral = _lastResult.FittedMineral;
            FitConverged = _lastResult.Optimization.Converged;
            ChiSquared = _lastResult.Optimization.ChiSquared;
            ReducedChiSquared = _lastResult.Optimization.ReducedChiSquared;
            FitIterations = _lastResult.Optimization.Iterations;

            // Build results text
            var sb = new StringBuilder();
            sb.AppendLine($"Converged: {FitConverged}");
            sb.AppendLine($"Iterations: {FitIterations}");
            sb.AppendLine($"χ²: {ChiSquared:E4}");
            sb.AppendLine($"Reduced χ²: {ReducedChiSquared:F4}");
            sb.AppendLine();
            sb.AppendLine("Parameter        Value          Uncertainty    Unit");
            sb.AppendLine("─────────────────────────────────────────────────────");
            sb.AppendLine($"V0               {_fittedMineral.MolarVolume,12:F4}  {(FitV0 ? $"±{FindUncertainty("V0"):F4}" : "fixed"),14}  cm³/mol");
            sb.AppendLine($"K0               {_fittedMineral.KZero,12:F2}  {(FitK0 ? $"±{FindUncertainty("K0"):F3}" : "fixed"),14}  GPa");
            sb.AppendLine($"K0'              {_fittedMineral.K1Prime,12:F3}  {(FitK0Prime ? $"±{FindUncertainty("K0'"):F4}" : "fixed"),14}  ");
            sb.AppendLine($"G0               {_fittedMineral.GZero,12:F2}  {(FitG0 ? $"±{FindUncertainty("G0"):F3}" : "fixed"),14}  GPa");
            sb.AppendLine($"G0'              {_fittedMineral.G1Prime,12:F3}  {(FitG0Prime ? $"±{FindUncertainty("G0'"):F4}" : "fixed"),14}  ");
            sb.AppendLine($"θ0               {_fittedMineral.DebyeTempZero,12:F2}  {(FitDebyeTemp ? $"±{FindUncertainty("θ0"):F2}" : "fixed"),14}  K");
            sb.AppendLine($"γ0               {_fittedMineral.GammaZero,12:F4}  {(FitGamma0 ? $"±{FindUncertainty("γ0"):F4}" : "fixed"),14}  ");
            sb.AppendLine($"q0               {_fittedMineral.QZero,12:F4}  {(FitQ0 ? $"±{FindUncertainty("q0"):F4}" : "fixed"),14}  ");
            sb.AppendLine($"ηS0              {_fittedMineral.EhtaZero,12:F4}  {(FitEtaS0 ? $"±{FindUncertainty("ηS0"):F4}" : "fixed"),14}  ");
            ResultsText = sb.ToString();

            // Build residuals text
            var rsb = new StringBuilder();
            rsb.AppendLine("P(GPa)  T(K)   Obs_Vp  Pred_Vp  Res_Vp   Obs_Vs  Pred_Vs  Res_Vs   Obs_ρ   Pred_ρ  Res_ρ");
            rsb.AppendLine("──────────────────────────────────────────────────────────────────────────────────────────────");
            foreach (var pred in _lastResult.Predictions)
            {
                var vpRes = pred.ObsVp.HasValue ? $"{pred.ObsVp.Value - pred.PredVp,8:F1}" : "       -";
                var vsRes = pred.ObsVs.HasValue ? $"{pred.ObsVs.Value - pred.PredVs,8:F1}" : "       -";
                var rhoRes = pred.ObsRho.HasValue ? $"{pred.ObsRho.Value - pred.PredRho,7:F4}" : "      -";
                rsb.AppendLine($"{pred.P,6:F1}  {pred.T,5:F0}  " +
                               $"{pred.ObsVp?.ToString("F0") ?? "     -",7}  {pred.PredVp,7:F0}  {vpRes}  " +
                               $"{pred.ObsVs?.ToString("F0") ?? "     -",7}  {pred.PredVs,7:F0}  {vsRes}  " +
                               $"{pred.ObsRho?.ToString("F4") ?? "    -",7}  {pred.PredRho,6:F4}  {rhoRes}");
            }
            ResidualsText = rsb.ToString();

            HasResults = true;
            FitStatus = FitConverged ? "Fit converged ✓" : "Fit did not converge — check initial guess or data quality.";
        }
        catch (Exception ex)
        {
            FitStatus = $"Error: {ex.Message}";
            HasResults = false;
        }
        finally
        {
            IsFitting = false;
        }
    }

    [RelayCommand]
    private void ComputePREMComparison()
    {
        if (_fittedMineral == null)
        {
            PremComparisonText = "Fit parameters first.";
            return;
        }

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Mantle profile using fitted parameters at T={AdiabaticTemp:F0} K adiabat");
            sb.AppendLine();
            sb.AppendLine("Depth(km)  P(GPa)   Vp_calc  Vp_PREM  ΔVp     Vs_calc  Vs_PREM  ΔVs     ρ_calc   ρ_PREM   Δρ");
            sb.AppendLine("────────────────────────────────────────────────────────────────────────────────────────────────────");

            for (double depth = DepthMin; depth <= DepthMax; depth += DepthStep)
            {
                var premProps = PREMModel.GetPropertiesAtDepth(depth);
                double P = premProps.Pressure;
                double T = AdiabaticTemp + depth * 0.3; // simple adiabatic gradient

                try
                {
                    var eos = new MieGruneisenEOSOptimizer(_fittedMineral, P, T);
                    var result = eos.ExecOptimize();
                    sb.AppendLine($"{depth,8:F0}  {P,7:F2}  {result.Vp,7:F0}  {premProps.Vp * 1000,7:F0}  {result.Vp - premProps.Vp * 1000,6:F0}  " +
                                 $"{result.Vs,7:F0}  {premProps.Vs * 1000,7:F0}  {result.Vs - premProps.Vs * 1000,6:F0}  " +
                                 $"{result.Density,7:F4}  {premProps.Density,7:F4}  {result.Density - premProps.Density,7:F4}");
                }
                catch
                {
                    sb.AppendLine($"{depth,8:F0}  {P,7:F2}  (computation failed at this P-T)");
                }
            }

            PremComparisonText = sb.ToString();
        }
        catch (Exception ex)
        {
            PremComparisonText = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ExportFittedParams()
    {
        if (_fittedMineral == null)
        {
            FitStatus = "No fitted parameters to export.";
            return;
        }
        var json = _fittedMineral.ExportJson();
        // Copy to clipboard as simple text output for now
        ResultsText += "\n\n=== Exported JSON ===\n" + json;
        FitStatus = "Fitted parameters appended as JSON below.";
    }

    private double FindUncertainty(string paramName)
    {
        if (_lastResult == null) return 0;
        var idx = Array.IndexOf(_lastResult.FittedParameterNames, paramName);
        if (idx < 0 || idx >= _lastResult.Optimization.Uncertainties.Length) return 0;
        return _lastResult.Optimization.Uncertainties[idx];
    }

    private static string GenerateSampleData()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Sample: MgSiO3 Bridgmanite (synthetic from SLB2011 parameters)");
        sb.AppendLine("# P(GPa), T(K), Vp(m/s), Vs(m/s), Density(g/cm3)");
        var mpv = SLB2011Endmembers.GetAll().First(m => m.PaperName == "mpv");
        foreach (var p in new[] { 25.0, 30.0, 40.0, 50.0, 60.0, 70.0, 80.0, 90.0, 100.0, 110.0, 120.0 })
        {
            try
            {
                var eos = new MieGruneisenEOSOptimizer(mpv, p, 300.0);
                var r = eos.ExecOptimize();
                sb.AppendLine($"{p:F1}, 300, {r.Vp:F0}, {r.Vs:F0}, {r.Density:F4}");
            }
            catch { /* skip if EOS fails */ }
        }
        return sb.ToString();
    }
}
