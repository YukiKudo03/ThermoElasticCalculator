using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class ThermoElasticFitterViewModel : ObservableObject
{
    // === Initial Parameters ===
    [ObservableProperty] private string _selectedMineralName = string.Empty;
    [ObservableProperty] private double _v0 = 43.603;
    [ObservableProperty] private double _k0 = 127.96;
    [ObservableProperty] private double _k1Prime = 4.218;
    [ObservableProperty] private double _g0 = 81.60;
    [ObservableProperty] private double _g1Prime = 1.4626;
    [ObservableProperty] private double _debyeTemp0 = 809.17;
    [ObservableProperty] private double _gamma0 = 0.99282;
    [ObservableProperty] private double _q0 = 2.10672;
    [ObservableProperty] private double _etaS0 = 2.2997;
    [ObservableProperty] private int _numAtoms = 7;
    [ObservableProperty] private double _molarWeight = 140.69;

    // === Fit Selection Checkboxes ===
    [ObservableProperty] private bool _fitV0;
    [ObservableProperty] private bool _fitK0 = true;
    [ObservableProperty] private bool _fitK1Prime;
    [ObservableProperty] private bool _fitG0 = true;
    [ObservableProperty] private bool _fitG1Prime;
    [ObservableProperty] private bool _fitDebyeTemp;
    [ObservableProperty] private bool _fitGamma;
    [ObservableProperty] private bool _fitQ;
    [ObservableProperty] private bool _fitEtaS;

    // === Data Input ===
    [ObservableProperty] private string _dataText = string.Empty;

    // === Results ===
    [ObservableProperty] private string _resultText = string.Empty;
    [ObservableProperty] private bool _converged;
    [ObservableProperty] private double _chiSquared;
    [ObservableProperty] private double _reducedChiSquared;
    [ObservableProperty] private string _statusMessage = string.Empty;

    // === Mineral List ===
    public ObservableCollection<string> MineralNames { get; } = new();

    private readonly List<MineralParams> _minerals;

    public ThermoElasticFitterViewModel()
    {
        _minerals = SLB2011Endmembers.GetAll();
        foreach (var m in _minerals)
            MineralNames.Add(m.ParamSymbol);

        DataText = GenerateDefaultData();
    }

    partial void OnSelectedMineralNameChanged(string value)
    {
        var mineral = _minerals.Find(m => m.ParamSymbol == value);
        if (mineral == null) return;

        V0 = mineral.MolarVolume;
        K0 = mineral.KZero;
        K1Prime = mineral.K1Prime;
        G0 = mineral.GZero;
        G1Prime = mineral.G1Prime;
        DebyeTemp0 = mineral.DebyeTempZero;
        Gamma0 = mineral.GammaZero;
        Q0 = mineral.QZero;
        EtaS0 = mineral.EhtaZero;
        NumAtoms = mineral.NumAtoms;
        MolarWeight = mineral.MolarWeight;
    }

    [RelayCommand]
    private async Task FitAsync()
    {
        try
        {
            var data = ParseData(DataText);
            if (data.Count == 0)
            {
                StatusMessage = "Error: No valid data points found.";
                return;
            }

            var mineral = BuildMineralParams();
            var config = new FittingConfig { BaseMineralParams = mineral };
            config.FitFlags[FittingConfig.IndexV0] = FitV0;
            config.FitFlags[FittingConfig.IndexK0] = FitK0;
            config.FitFlags[FittingConfig.IndexK1Prime] = FitK1Prime;
            config.FitFlags[FittingConfig.IndexG0] = FitG0;
            config.FitFlags[FittingConfig.IndexG1Prime] = FitG1Prime;
            config.FitFlags[FittingConfig.IndexDebyeTemp] = FitDebyeTemp;
            config.FitFlags[FittingConfig.IndexGamma] = FitGamma;
            config.FitFlags[FittingConfig.IndexQ] = FitQ;
            config.FitFlags[FittingConfig.IndexEtaS] = FitEtaS;

            if (config.FreeParameterCount == 0)
            {
                StatusMessage = "Error: Select at least one parameter to fit.";
                return;
            }

            StatusMessage = "Fitting...";
            var fitter = new ThermoElasticFitter();
            var result = await Task.Run(() => fitter.Fit(config, data));

            Converged = result.Converged;
            ChiSquared = result.ChiSquared;
            ReducedChiSquared = result.ReducedChiSquared;

            ResultText = FormatResults(config, result);
            StatusMessage = $"Fit {(Converged ? "converged" : "did not converge")} in {result.Iterations} iterations, χ²r={ReducedChiSquared:F4}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private MineralParams BuildMineralParams()
    {
        return new MineralParams
        {
            MineralName = SelectedMineralName,
            NumAtoms = NumAtoms,
            MolarWeight = MolarWeight,
            MolarVolume = V0,
            KZero = K0,
            K1Prime = K1Prime,
            GZero = G0,
            G1Prime = G1Prime,
            DebyeTempZero = DebyeTemp0,
            GammaZero = Gamma0,
            QZero = Q0,
            EhtaZero = EtaS0,
            RefTemp = 300.0,
        };
    }

    private static string FormatResults(FittingConfig config, OptimizationResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Fitted Parameters ===");
        int idx = 0;
        for (int i = 0; i < FittingConfig.TotalParams; i++)
        {
            if (config.FitFlags[i])
            {
                string name = FittingConfig.ParameterNames[i];
                double val = result.Parameters[idx];
                double unc = result.Uncertainties[idx];
                sb.AppendLine($"  {name} = {val:G6} ± {unc:G4}");
                idx++;
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Converged: {result.Converged}");
        sb.AppendLine($"Iterations: {result.Iterations}");
        sb.AppendLine($"χ² = {result.ChiSquared:F4}");
        sb.AppendLine($"χ²r = {result.ReducedChiSquared:F4}");

        // Correlation matrix
        if (result.CovarianceMatrix != null && result.Parameters.Length > 1
            && result.Uncertainties.All(u => !double.IsNaN(u) && u > 0))
        {
            sb.AppendLine();
            sb.AppendLine("=== Correlation Matrix ===");
            int n = result.Parameters.Length;
            for (int i = 0; i < n; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < n; j++)
                {
                    double corr = result.CovarianceMatrix[i, j]
                        / (result.Uncertainties[i] * result.Uncertainties[j]);
                    row.Add($"{corr:F3}");
                }
                sb.AppendLine("  " + string.Join("  ", row));
            }
        }

        return sb.ToString();
    }

    internal static List<FittingDataPoint> ParseData(string text)
    {
        var data = new List<FittingDataPoint>();
        if (string.IsNullOrWhiteSpace(text)) return data;

        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('#') || string.IsNullOrWhiteSpace(trimmed)) continue;

            var parts = trimmed.Split(new[] { ',', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3) continue;

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double T)) continue;
            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double P)) continue;

            // Skip unphysical T,P values
            if (T <= 0 || P < 0) continue;

            var dp = new FittingDataPoint { Temperature = T, Pressure = P };

            // Format A: T, P, Vp, Vs [, σVp, σVs] — velocity mode
            // Format B: T, P, V [, σV] — volume mode
            // Format C: T, P, Vp, Vs, V [, σVp, σVs, σV] — combined mode
            // Heuristic: if 3rd AND 4th values > 100, they are Vp/Vs (m/s); else 3rd is V (cm³/mol)
            if (double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double val3))
            {
                if (val3 > 100.0 && parts.Length >= 4
                    && double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double val4)
                    && val4 > 100.0)
                {
                    // Velocity mode (or combined)
                    dp.Vp = val3;
                    dp.Vs = val4;
                    dp.SigmaVp = dp.Vp * 0.005;
                    dp.SigmaVs = dp.Vs * 0.005;

                    int nextIdx = 4;
                    // Check for volume (combined mode): next value < 100
                    if (parts.Length > nextIdx
                        && double.TryParse(parts[nextIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out double val5)
                        && val5 < 100.0)
                    {
                        dp.Volume = val5;
                        dp.SigmaVolume = dp.Volume * 0.001;
                        nextIdx++;
                    }

                    // Parse optional sigmas
                    if (parts.Length > nextIdx
                        && double.TryParse(parts[nextIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out double s1))
                    {
                        dp.SigmaVp = s1;
                        nextIdx++;
                    }
                    if (parts.Length > nextIdx
                        && double.TryParse(parts[nextIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out double s2))
                    {
                        dp.SigmaVs = s2;
                        nextIdx++;
                    }
                    if (dp.HasVolumeData && parts.Length > nextIdx
                        && double.TryParse(parts[nextIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out double s3))
                    {
                        dp.SigmaVolume = s3;
                    }
                }
                else
                {
                    // Volume mode
                    dp.Volume = val3;
                    dp.SigmaVolume = dp.Volume * 0.001;
                    if (parts.Length > 3
                        && double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double sv))
                    {
                        dp.SigmaVolume = sv;
                    }
                }
            }

            if (dp.HasVelocityData || dp.HasVolumeData)
                data.Add(dp);
        }

        return data;
    }

    private static string GenerateDefaultData()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Forsterite synthetic Vp/Vs data");
        sb.AppendLine("# T[K], P[GPa], Vp[m/s], Vs[m/s]");

        var mineral = SLB2011Endmembers.GetAll().Find(m => m.PaperName == "fo");
        if (mineral == null) return sb.ToString();

        var conditions = new (double T, double P)[]
        {
            (300, 5), (300, 10), (300, 20), (300, 30),
            (1000, 10), (1000, 20), (1000, 30),
            (1500, 10), (1500, 20), (1500, 30),
        };

        foreach (var (T, P) in conditions)
        {
            var eos = new MieGruneisenEOSOptimizer(mineral, P, T);
            var th = eos.ExecOptimize();
            if (th.IsConverged)
            {
                sb.AppendLine($"{T:F0}, {P:F0}, {th.Vp:F1}, {th.Vs:F1}");
            }
        }

        return sb.ToString();
    }
}
