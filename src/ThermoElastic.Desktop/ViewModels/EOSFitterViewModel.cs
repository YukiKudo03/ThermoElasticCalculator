using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Desktop.ViewModels;

public partial class EOSFitterViewModel : ObservableObject
{
    [ObservableProperty] private double _v0 = 11.248;
    [ObservableProperty] private string _pvDataText = string.Empty;
    [ObservableProperty] private double _fittedK0;
    [ObservableProperty] private double _fittedK1Prime;
    [ObservableProperty] private bool _converged;
    [ObservableProperty] private double _chiSquared;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public EOSFitterViewModel()
    {
        PvDataText = GenerateDefaultPVData();
    }

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var data = ParsePVData(PvDataText);
            if (data.Count == 0)
            {
                StatusMessage = "Error: No valid data points found.";
                return;
            }

            var fitter = new EOSFitter();
            var result = fitter.FitPV(data, V0);

            FittedK0 = result.Parameters[0];
            FittedK1Prime = result.Parameters[1];
            Converged = result.Converged;
            ChiSquared = result.ChiSquared;

            StatusMessage = $"Fit {(Converged ? "converged" : "did not converge")}: K0={FittedK0:F1} GPa, K'={FittedK1Prime:F2}, χ²={ChiSquared:F3}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }

    private static List<(double P, double V, double sigmaV)> ParsePVData(string text)
    {
        var data = new List<(double P, double V, double sigmaV)>();
        if (string.IsNullOrWhiteSpace(text)) return data;

        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('#') || string.IsNullOrWhiteSpace(trimmed)) continue;

            var parts = trimmed.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2
                && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double p)
                && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
            {
                double sigma = v * 0.001;
                if (parts.Length >= 3 && double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double s))
                    sigma = s;
                data.Add((p, v, sigma));
            }
        }

        return data;
    }

    private static string GenerateDefaultPVData()
    {
        var sb = new StringBuilder();
        double V0 = 11.248, K0 = 160.2, Kp = 4.03;
        for (int i = 1; i <= 10; i++)
        {
            double P = i * 10.0;
            // Solve V from P using bisection
            double vMin = V0 * 0.5, vMax = V0;
            for (int j = 0; j < 50; j++)
            {
                double vMid = (vMin + vMax) / 2;
                double f = 0.5 * (Math.Pow(V0 / vMid, 2.0 / 3.0) - 1);
                double pCalc = 3 * K0 * f * Math.Pow(1 + 2 * f, 2.5) * (1 + 1.5 * (Kp - 4) * f);
                if (pCalc < P) vMax = vMid; else vMin = vMid;
            }
            double V = (vMin + vMax) / 2;
            sb.AppendLine($"{P:F1}, {V:F4}, {V * 0.001:F6}");
        }
        return sb.ToString();
    }
}
