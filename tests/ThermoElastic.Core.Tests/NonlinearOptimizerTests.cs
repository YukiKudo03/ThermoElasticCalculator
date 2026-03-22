using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Core.Tests;

public class NonlinearOptimizerTests
{
    private readonly ITestOutputHelper _output;

    public NonlinearOptimizerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void FitQuadratic_RecoversCoefficients()
    {
        // f(x) = a*x^2 + b*x + c with known a=2.5, b=-1.3, c=0.7
        double aTrue = 2.5, bTrue = -1.3, cTrue = 0.7;
        var rng = new Random(42);

        double[] xValues = Enumerable.Range(0, 50).Select(i => i * 0.1).ToArray();
        double noiseSigma = 0.05;
        double[] observed = xValues.Select(x =>
            aTrue * x * x + bTrue * x + cTrue + noiseSigma * (rng.NextDouble() * 2 - 1)).ToArray();
        double[] sigma = Enumerable.Repeat(noiseSigma, observed.Length).ToArray();

        Func<double[], double[]> model = (p) =>
            xValues.Select(x => p[0] * x * x + p[1] * x + p[2]).ToArray();

        var optimizer = new LevenbergMarquardtOptimizer();
        var result = optimizer.Optimize(model, new[] { 1.0, 0.0, 0.0 }, observed, sigma);

        _output.WriteLine($"Fitted: a={result.Parameters[0]:F4}, b={result.Parameters[1]:F4}, c={result.Parameters[2]:F4}");
        _output.WriteLine($"Uncertainties: {result.Uncertainties[0]:F4}, {result.Uncertainties[1]:F4}, {result.Uncertainties[2]:F4}");
        _output.WriteLine($"Converged: {result.Converged}, Iterations: {result.Iterations}");

        // Parameters should be recovered within their uncertainties (use 3-sigma for safety)
        Assert.True(Math.Abs(result.Parameters[0] - aTrue) < 3 * result.Uncertainties[0] + 0.1,
            $"a not recovered: {result.Parameters[0]} vs {aTrue}");
        Assert.True(Math.Abs(result.Parameters[1] - bTrue) < 3 * result.Uncertainties[1] + 0.1,
            $"b not recovered: {result.Parameters[1]} vs {bTrue}");
        Assert.True(Math.Abs(result.Parameters[2] - cTrue) < 3 * result.Uncertainties[2] + 0.1,
            $"c not recovered: {result.Parameters[2]} vs {cTrue}");
    }

    [Fact]
    public void FitExponential_RecoversParameters()
    {
        // f(x) = A * exp(-k * x), A=3.0, k=0.5
        double ATrue = 3.0, kTrue = 0.5;
        var rng = new Random(123);

        double[] xValues = Enumerable.Range(0, 40).Select(i => i * 0.2).ToArray();
        double noiseSigma = 0.02;
        double[] observed = xValues.Select(x =>
            ATrue * Math.Exp(-kTrue * x) + noiseSigma * (rng.NextDouble() * 2 - 1)).ToArray();
        double[] sigma = Enumerable.Repeat(noiseSigma, observed.Length).ToArray();

        Func<double[], double[]> model = (p) =>
            xValues.Select(x => p[0] * Math.Exp(-p[1] * x)).ToArray();

        var optimizer = new LevenbergMarquardtOptimizer();
        var result = optimizer.Optimize(model, new[] { 1.0, 1.0 }, observed, sigma);

        _output.WriteLine($"Fitted: A={result.Parameters[0]:F4}, k={result.Parameters[1]:F4}");
        _output.WriteLine($"True:   A={ATrue}, k={kTrue}");

        Assert.True(Math.Abs(result.Parameters[0] - ATrue) < 0.1, $"A not recovered: {result.Parameters[0]}");
        Assert.True(Math.Abs(result.Parameters[1] - kTrue) < 0.05, $"k not recovered: {result.Parameters[1]}");
    }

    [Fact]
    public void WellPosedProblem_ConvergesTrue()
    {
        // Simple linear fit should converge easily
        double[] xValues = Enumerable.Range(0, 20).Select(i => (double)i).ToArray();
        double[] observed = xValues.Select(x => 2.0 * x + 1.0).ToArray();

        Func<double[], double[]> model = (p) =>
            xValues.Select(x => p[0] * x + p[1]).ToArray();

        var optimizer = new LevenbergMarquardtOptimizer();
        var result = optimizer.Optimize(model, new[] { 0.0, 0.0 }, observed);

        _output.WriteLine($"Converged: {result.Converged}, Iterations: {result.Iterations}");
        Assert.True(result.Converged, "Well-posed problem should converge");
    }

    [Fact]
    public void GoodFit_ReducedChiSquaredNearOne()
    {
        // Generate data with known sigma, fit should give reduced chi2 ~ 1
        double aTrue = 1.5, bTrue = 0.3;
        var rng = new Random(999);
        double noiseSigma = 0.1;

        double[] xValues = Enumerable.Range(0, 100).Select(i => i * 0.05).ToArray();
        double[] observed = xValues.Select(x =>
            aTrue * x + bTrue + noiseSigma * NextGaussian(rng)).ToArray();
        double[] sigma = Enumerable.Repeat(noiseSigma, observed.Length).ToArray();

        Func<double[], double[]> model = (p) =>
            xValues.Select(x => p[0] * x + p[1]).ToArray();

        var optimizer = new LevenbergMarquardtOptimizer();
        var result = optimizer.Optimize(model, new[] { 0.0, 0.0 }, observed, sigma);

        _output.WriteLine($"Reduced chi2: {result.ReducedChiSquared:F4}");
        _output.WriteLine($"Chi2: {result.ChiSquared:F4}, DOF: {observed.Length - 2}");

        // Reduced chi2 should be approximately 1 (within reason for 98 DOF)
        Assert.True(result.ReducedChiSquared > 0.5 && result.ReducedChiSquared < 2.0,
            $"Reduced chi2 should be ~1, got {result.ReducedChiSquared}");
    }

    private static double NextGaussian(Random rng)
    {
        // Box-Muller transform
        double u1 = 1.0 - rng.NextDouble();
        double u2 = rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }
}
