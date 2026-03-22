using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Tests for Bayesian MCMC inversion using Metropolis-Hastings sampling.
/// </summary>
public class BayesianInversionTests
{
    private readonly ITestOutputHelper _output;

    public BayesianInversionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Test Metropolis-Hastings on 1D Gaussian: sample from N(3.0, 1.0).
    /// After burn-in, mean should be ~3.0 +/- 0.3, std ~1.0 +/- 0.3.
    /// </summary>
    [Fact]
    public void MetropolisHastings_1DGaussian_RecoversMeanAndStd()
    {
        double trueMean = 3.0;
        double trueStd = 1.0;

        // Log-posterior for N(3, 1): -0.5 * ((x - 3) / 1)^2
        Func<double[], double> logPosterior = p =>
        {
            double diff = (p[0] - trueMean) / trueStd;
            return -0.5 * diff * diff;
        };

        var sampler = new MCMCSampler(seed: 42);
        var chain = sampler.Sample(
            logPosterior,
            initialParams: new[] { 0.0 },
            stepSizes: new[] { 1.0 },
            nSamples: 20000,
            paramNames: new[] { "mu" });

        int burnIn = 2000;
        var mean = chain.GetMean(burnIn);
        var std = chain.GetStdDev(burnIn);

        _output.WriteLine($"Posterior mean: {mean[0]:F4} (true: {trueMean})");
        _output.WriteLine($"Posterior std:  {std[0]:F4} (true: {trueStd})");
        _output.WriteLine($"Acceptance rate: {chain.AcceptanceRate:F4}");

        Assert.InRange(mean[0], trueMean - 0.3, trueMean + 0.3);
        Assert.InRange(std[0], trueStd - 0.3, trueStd + 0.3);
    }

    /// <summary>
    /// Test 2D Gaussian: sample from bivariate Gaussian with known mean [2, 5]
    /// and std [0.5, 1.0]. Verify posterior means near true values.
    /// </summary>
    [Fact]
    public void MetropolisHastings_2DGaussian_RecoversBothParameters()
    {
        double[] trueMeans = { 2.0, 5.0 };
        double[] trueStds = { 0.5, 1.0 };

        Func<double[], double> logPosterior = p =>
        {
            double ll = 0;
            for (int i = 0; i < 2; i++)
            {
                double diff = (p[i] - trueMeans[i]) / trueStds[i];
                ll -= 0.5 * diff * diff;
            }
            return ll;
        };

        var sampler = new MCMCSampler(seed: 123);
        var chain = sampler.Sample(
            logPosterior,
            initialParams: new[] { 0.0, 0.0 },
            stepSizes: new[] { 0.5, 1.0 },
            nSamples: 30000,
            paramNames: new[] { "x", "y" });

        int burnIn = 5000;
        var mean = chain.GetMean(burnIn);
        var std = chain.GetStdDev(burnIn);

        _output.WriteLine($"Posterior mean x: {mean[0]:F4} (true: {trueMeans[0]})");
        _output.WriteLine($"Posterior mean y: {mean[1]:F4} (true: {trueMeans[1]})");
        _output.WriteLine($"Posterior std x:  {std[0]:F4} (true: {trueStds[0]})");
        _output.WriteLine($"Posterior std y:  {std[1]:F4} (true: {trueStds[1]})");
        _output.WriteLine($"Acceptance rate: {chain.AcceptanceRate:F4}");

        Assert.InRange(mean[0], trueMeans[0] - 0.3, trueMeans[0] + 0.3);
        Assert.InRange(mean[1], trueMeans[1] - 0.3, trueMeans[1] + 0.3);
        Assert.InRange(std[0], trueStds[0] - 0.3, trueStds[0] + 0.3);
        Assert.InRange(std[1], trueStds[1] - 0.3, trueStds[1] + 0.3);
    }

    /// <summary>
    /// Test acceptance rate: for well-tuned step size, acceptance rate should be 20-60%.
    /// </summary>
    [Fact]
    public void MetropolisHastings_WellTunedStepSize_AcceptanceRateInRange()
    {
        Func<double[], double> logPosterior = p =>
        {
            double diff = p[0] - 1.0;
            return -0.5 * diff * diff;
        };

        var sampler = new MCMCSampler(seed: 99);
        var chain = sampler.Sample(
            logPosterior,
            initialParams: new[] { 0.0 },
            stepSizes: new[] { 1.5 },
            nSamples: 10000);

        _output.WriteLine($"Acceptance rate: {chain.AcceptanceRate:F4}");
        _output.WriteLine($"Accepted: {chain.Accepted}, Total: {chain.Total}");

        Assert.InRange(chain.AcceptanceRate, 0.20, 0.60);
    }

    /// <summary>
    /// Test burn-in removal: mean with burn-in=100 should be closer to truth
    /// than mean with burn-in=0 when starting far from the mode.
    /// </summary>
    [Fact]
    public void BurnInRemoval_ImprovesPosteriorEstimate()
    {
        double trueMean = 5.0;

        Func<double[], double> logPosterior = p =>
        {
            double diff = (p[0] - trueMean) / 0.5;
            return -0.5 * diff * diff;
        };

        var sampler = new MCMCSampler(seed: 77);
        var chain = sampler.Sample(
            logPosterior,
            initialParams: new[] { -10.0 },  // Start far from truth
            stepSizes: new[] { 0.5 },
            nSamples: 10000);

        var meanNoBurnIn = chain.GetMean(0);
        var meanWithBurnIn = chain.GetMean(100);

        double errorNoBurnIn = Math.Abs(meanNoBurnIn[0] - trueMean);
        double errorWithBurnIn = Math.Abs(meanWithBurnIn[0] - trueMean);

        _output.WriteLine($"Mean (no burn-in):   {meanNoBurnIn[0]:F4}, error: {errorNoBurnIn:F4}");
        _output.WriteLine($"Mean (burn-in=100):  {meanWithBurnIn[0]:F4}, error: {errorWithBurnIn:F4}");

        Assert.True(errorWithBurnIn <= errorNoBurnIn,
            $"Burn-in should improve estimate: error with burn-in ({errorWithBurnIn:F4}) should be <= error without ({errorNoBurnIn:F4})");
    }

    /// <summary>
    /// Test chain length: output chain should have the requested number of samples.
    /// </summary>
    [Fact]
    public void ChainLength_MatchesRequestedSamples()
    {
        Func<double[], double> logPosterior = p => -0.5 * p[0] * p[0];

        var sampler = new MCMCSampler(seed: 42);
        int requestedSamples = 5000;

        var chain = sampler.Sample(
            logPosterior,
            initialParams: new[] { 0.0 },
            stepSizes: new[] { 1.0 },
            nSamples: requestedSamples);

        _output.WriteLine($"Requested samples: {requestedSamples}");
        _output.WriteLine($"Chain length: {chain.Samples.GetLength(0)}");
        _output.WriteLine($"LogPosterior length: {chain.LogPosterior.Length}");
        _output.WriteLine($"NParams: {chain.NParams}");
        _output.WriteLine($"Total: {chain.Total}");

        Assert.Equal(requestedSamples, chain.Samples.GetLength(0));
        Assert.Equal(requestedSamples, chain.LogPosterior.Length);
        Assert.Equal(requestedSamples, chain.Total);
        Assert.Equal(1, chain.NParams);
        Assert.Single(chain.ParamNames);
    }
}
