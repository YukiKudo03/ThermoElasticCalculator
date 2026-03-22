using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Desktop.ViewModels;

public partial class BayesianInversionViewModel : ObservableObject
{
    [ObservableProperty] private double _trueMean = 3.0;
    [ObservableProperty] private double _trueSigma = 1.0;
    [ObservableProperty] private double _initialGuess = 0.0;
    [ObservableProperty] private double _stepSize = 1.0;
    [ObservableProperty] private int _nSamples = 5000;
    [ObservableProperty] private int _burnIn = 500;
    [ObservableProperty] private double _recoveredMean;
    [ObservableProperty] private double _recoveredStdDev;
    [ObservableProperty] private double _acceptanceRate;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public ObservableCollection<ChainPoint> ChainResults { get; } = new();

    [RelayCommand]
    private void Calculate()
    {
        try
        {
            var sampler = new MCMCSampler(seed: 42);
            double mean = TrueMean, sigma = TrueSigma;
            double logPosterior(double[] p) => -0.5 * (p[0] - mean) * (p[0] - mean) / (sigma * sigma);

            var chain = sampler.Sample(logPosterior, new[] { InitialGuess }, new[] { StepSize }, NSamples);
            var posteriorMean = chain.GetMean(BurnIn);
            var posteriorStd = chain.GetStdDev(BurnIn);

            RecoveredMean = posteriorMean[0];
            RecoveredStdDev = posteriorStd[0];
            AcceptanceRate = chain.AcceptanceRate;

            ChainResults.Clear();
            int step = Math.Max(1, NSamples / 200);
            for (int i = 0; i < chain.Samples.GetLength(0); i += step)
                ChainResults.Add(new ChainPoint { Index = i, Value = chain.Samples[i, 0] });

            StatusMessage = $"MCMC: mean={RecoveredMean:F3}, std={RecoveredStdDev:F3}, acceptance={AcceptanceRate:P0}";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
    }

    public class ChainPoint
    {
        public int Index { get; set; }
        public double Value { get; set; }
    }
}
