namespace ThermoElastic.Core.Models;

/// <summary>Result of an MCMC sampling run.</summary>
public class MCMCChain
{
    /// <summary>Sampled parameter values [nSamples, nParams]</summary>
    public double[,] Samples { get; set; } = new double[0, 0];
    /// <summary>Log-posterior values for each sample</summary>
    public double[] LogPosterior { get; set; } = Array.Empty<double>();
    /// <summary>Number of accepted proposals</summary>
    public int Accepted { get; set; }
    /// <summary>Total proposals</summary>
    public int Total { get; set; }
    /// <summary>Acceptance rate</summary>
    public double AcceptanceRate => Total > 0 ? (double)Accepted / Total : 0;
    /// <summary>Number of parameters</summary>
    public int NParams { get; set; }
    /// <summary>Parameter names</summary>
    public string[] ParamNames { get; set; } = Array.Empty<string>();

    /// <summary>Get posterior mean for each parameter.</summary>
    public double[] GetMean(int burnIn = 0)
    {
        int nSamples = Samples.GetLength(0);
        var means = new double[NParams];
        int count = 0;
        for (int i = burnIn; i < nSamples; i++)
        {
            for (int j = 0; j < NParams; j++)
                means[j] += Samples[i, j];
            count++;
        }
        if (count == 0)
            throw new ArgumentOutOfRangeException(nameof(burnIn), $"burnIn ({burnIn}) must be less than number of samples ({nSamples}).");
        for (int j = 0; j < NParams; j++)
            means[j] /= count;
        return means;
    }

    /// <summary>Get posterior standard deviation for each parameter.</summary>
    public double[] GetStdDev(int burnIn = 0)
    {
        var means = GetMean(burnIn);
        int nSamples = Samples.GetLength(0);
        var vars = new double[NParams];
        int count = 0;
        for (int i = burnIn; i < nSamples; i++)
        {
            for (int j = 0; j < NParams; j++)
                vars[j] += (Samples[i, j] - means[j]) * (Samples[i, j] - means[j]);
            count++;
        }
        if (count == 0)
            throw new ArgumentOutOfRangeException(nameof(burnIn), $"burnIn ({burnIn}) must be less than number of samples ({nSamples}).");
        for (int j = 0; j < NParams; j++)
            vars[j] = Math.Sqrt(vars[j] / count);
        return vars;
    }
}
