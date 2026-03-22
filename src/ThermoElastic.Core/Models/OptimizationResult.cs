namespace ThermoElastic.Core.Models;

/// <summary>
/// Result of a nonlinear least-squares optimization.
/// </summary>
public class OptimizationResult
{
    /// <summary>Best-fit parameters</summary>
    public double[] Parameters { get; set; } = Array.Empty<double>();
    /// <summary>Parameter uncertainties (sqrt of covariance diagonal)</summary>
    public double[] Uncertainties { get; set; } = Array.Empty<double>();
    /// <summary>Covariance matrix</summary>
    public double[,]? CovarianceMatrix { get; set; }
    /// <summary>Final chi-squared value</summary>
    public double ChiSquared { get; set; }
    /// <summary>Reduced chi-squared (chi2 / degrees of freedom)</summary>
    public double ReducedChiSquared { get; set; }
    /// <summary>Number of iterations</summary>
    public int Iterations { get; set; }
    /// <summary>Whether optimization converged</summary>
    public bool Converged { get; set; }
    /// <summary>Residuals at best-fit</summary>
    public double[] Residuals { get; set; } = Array.Empty<double>();
}
