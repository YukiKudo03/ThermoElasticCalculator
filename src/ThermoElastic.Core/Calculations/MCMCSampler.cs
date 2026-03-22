namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Metropolis-Hastings MCMC sampler for Bayesian inversion.
/// </summary>
public class MCMCSampler
{
    private readonly Random _rng;

    public MCMCSampler(int seed = 42) { _rng = new Random(seed); }

    /// <summary>
    /// Run Metropolis-Hastings MCMC sampling.
    /// </summary>
    /// <param name="logPosterior">Function computing log-posterior: params -> log(posterior)</param>
    /// <param name="initialParams">Starting parameter values</param>
    /// <param name="stepSizes">Proposal step sizes for each parameter</param>
    /// <param name="nSamples">Total number of samples to collect</param>
    /// <param name="paramNames">Optional parameter names</param>
    /// <returns>MCMC chain with samples</returns>
    public MCMCChain Sample(
        Func<double[], double> logPosterior,
        double[] initialParams,
        double[] stepSizes,
        int nSamples = 10000,
        string[]? paramNames = null)
    {
        int nParams = initialParams.Length;
        var samples = new double[nSamples, nParams];
        var logPost = new double[nSamples];
        int accepted = 0;

        var current = (double[])initialParams.Clone();
        double logP_current = logPosterior(current);

        for (int i = 0; i < nSamples; i++)
        {
            // Propose candidate
            var candidate = new double[nParams];
            for (int j = 0; j < nParams; j++)
            {
                candidate[j] = current[j] + stepSizes[j] * NextGaussian();
            }

            // Compute log-posterior of candidate
            double logP_candidate = logPosterior(candidate);

            // Accept/reject
            double logAlpha = logP_candidate - logP_current;
            if (logAlpha >= 0 || Math.Log(_rng.NextDouble()) < logAlpha)
            {
                current = candidate;
                logP_current = logP_candidate;
                accepted++;
            }

            // Store current state
            for (int j = 0; j < nParams; j++)
                samples[i, j] = current[j];
            logPost[i] = logP_current;
        }

        return new MCMCChain
        {
            Samples = samples,
            LogPosterior = logPost,
            Accepted = accepted,
            Total = nSamples,
            NParams = nParams,
            ParamNames = paramNames ?? Enumerable.Range(0, nParams).Select(i => $"p{i}").ToArray()
        };
    }

    private double NextGaussian()
    {
        double u1 = 1.0 - _rng.NextDouble();
        double u2 = _rng.NextDouble();
        double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return z;
    }
}
