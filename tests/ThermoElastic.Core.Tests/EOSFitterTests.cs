using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Core.Tests;

public class EOSFitterTests
{
    private readonly ITestOutputHelper _output;

    // MgO reference parameters
    private const double K0_MgO = 160.2;   // GPa
    private const double K1_MgO = 4.03;
    private const double V0_MgO = 11.248;  // cm3/mol

    public EOSFitterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void FitBM3_MgO_RecoversParameters()
    {
        // Generate synthetic P-V data from known MgO parameters with 0.1% noise
        var rng = new Random(314);
        var data = new List<(double P, double V, double sigmaV)>();

        // Generate volumes from V0 down to ~80% V0 (pressures up to ~80 GPa)
        for (int i = 1; i <= 30; i++)
        {
            double V = V0_MgO * (1.0 - i * 0.006);  // compress by 0.6% each step
            double P = EOSFitter.BM3Pressure(V0_MgO, V, K0_MgO, K1_MgO);
            // Add 0.1% noise to V
            double noiseV = V * 0.001 * (rng.NextDouble() * 2 - 1);
            double sigmaV = V * 0.001;
            data.Add((P, V + noiseV, sigmaV));
        }

        var fitter = new EOSFitter();
        var result = fitter.FitPV(data, V0_MgO);

        double K0_fit = result.Parameters[0];
        double K1_fit = result.Parameters[1];

        _output.WriteLine($"True:    K0={K0_MgO:F2} GPa, K'={K1_MgO:F3}");
        _output.WriteLine($"Fitted:  K0={K0_fit:F2} GPa, K'={K1_fit:F3}");
        _output.WriteLine($"Errors:  K0={result.Uncertainties[0]:F2}, K'={result.Uncertainties[1]:F3}");
        _output.WriteLine($"Converged: {result.Converged}, Iterations: {result.Iterations}");

        // K0 within 2%
        Assert.True(Math.Abs(K0_fit - K0_MgO) / K0_MgO < 0.02,
            $"K0 deviation too large: {K0_fit:F2} vs {K0_MgO:F2} ({Math.Abs(K0_fit - K0_MgO) / K0_MgO * 100:F1}%)");

        // K' within 5%
        Assert.True(Math.Abs(K1_fit - K1_MgO) / K1_MgO < 0.05,
            $"K' deviation too large: {K1_fit:F3} vs {K1_MgO:F3} ({Math.Abs(K1_fit - K1_MgO) / K1_MgO * 100:F1}%)");

        Assert.True(result.Converged, "Fit should converge");
    }

    [Fact]
    public void FfPlot_LinearForBM3()
    {
        // Generate exact P-V data from BM3
        var data = new List<(double P, double V)>();
        for (int i = 1; i <= 20; i++)
        {
            double V = V0_MgO * (1.0 - i * 0.008);
            double P = EOSFitter.BM3Pressure(V0_MgO, V, K0_MgO, K1_MgO);
            data.Add((P, V));
        }

        var fitter = new EOSFitter();
        var ffData = fitter.GenerateFfPlot(data, V0_MgO, K0_MgO, K1_MgO);

        _output.WriteLine("f\tF_data\tF_fit");
        foreach (var (f, F_data, F_fit) in ffData)
        {
            _output.WriteLine($"{f:F6}\t{F_data:F4}\t{F_fit:F4}");
        }

        // For exact BM3 data, F_data should be close to F_fit (linear in f)
        // Check that F_data matches F_fit within 1% for each point
        foreach (var (f, F_data, F_fit) in ffData)
        {
            if (Math.Abs(F_fit) > 1e-6)
            {
                double relError = Math.Abs(F_data - F_fit) / Math.Abs(F_fit);
                Assert.True(relError < 0.01,
                    $"F-f linearity check failed at f={f:F6}: F_data={F_data:F4}, F_fit={F_fit:F4}, rel error={relError:E2}");
            }
        }

        // Check that the points span a reasonable range of f
        Assert.True(ffData.Count == 20, "Should have 20 data points");
        Assert.True(ffData.First().f > 0, "Eulerian strain should be positive for compression");
    }

    [Fact]
    public void K0KPrime_NegativelyCorrelated()
    {
        // Fit BM3 data and check covariance matrix
        var rng = new Random(271);
        var data = new List<(double P, double V, double sigmaV)>();

        for (int i = 1; i <= 30; i++)
        {
            double V = V0_MgO * (1.0 - i * 0.006);
            double P = EOSFitter.BM3Pressure(V0_MgO, V, K0_MgO, K1_MgO);
            double noiseV = V * 0.001 * (rng.NextDouble() * 2 - 1);
            double sigmaV = V * 0.001;
            data.Add((P, V + noiseV, sigmaV));
        }

        var fitter = new EOSFitter();
        var result = fitter.FitPV(data, V0_MgO);

        Assert.NotNull(result.CovarianceMatrix);

        double cov01 = result.CovarianceMatrix![0, 1];
        double sigma0 = result.Uncertainties[0];
        double sigma1 = result.Uncertainties[1];
        double correlation = cov01 / (sigma0 * sigma1);

        _output.WriteLine($"Covariance K0-K': {cov01:F6}");
        _output.WriteLine($"Correlation coefficient: {correlation:F4}");

        // K0 and K' should be negatively correlated
        Assert.True(correlation < 0,
            $"K0-K' correlation should be negative, got {correlation:F4}");
    }
}
