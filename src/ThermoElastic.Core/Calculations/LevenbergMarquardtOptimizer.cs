namespace ThermoElastic.Core.Calculations;

using MathNet.Numerics.LinearAlgebra;
using ThermoElastic.Core.Models;

/// <summary>
/// General-purpose Levenberg-Marquardt nonlinear least-squares optimizer.
/// </summary>
public class LevenbergMarquardtOptimizer
{
    private const int MaxIterations = 200;
    private const double InitialLambda = 0.001;
    private const double LambdaFactor = 10.0;
    private const double ConvergenceTol = 1e-8;
    private const double GradientTol = 1e-10;

    /// <summary>
    /// Fit a model to observed data.
    /// </summary>
    /// <param name="model">Model function: params -> predicted values (same length as observed)</param>
    /// <param name="initialParams">Initial parameter guesses</param>
    /// <param name="observed">Observed data values</param>
    /// <param name="uncertainties">Measurement uncertainties (1-sigma). If null, assume uniform.</param>
    /// <returns>Optimization result with best-fit parameters and statistics</returns>
    public OptimizationResult Optimize(
        Func<double[], double[]> model,
        double[] initialParams,
        double[] observed,
        double[]? uncertainties = null)
    {
        int nData = observed.Length;
        int nParams = initialParams.Length;
        int dof = nData - nParams;

        // Default uncertainties to 1.0
        double[] sigma = uncertainties ?? Enumerable.Repeat(1.0, nData).ToArray();

        double[] currentParams = (double[])initialParams.Clone();
        double lambda = InitialLambda;

        double[] predicted = model(currentParams);
        double[] residuals = ComputeResiduals(observed, predicted, sigma);
        double chi2 = residuals.Sum(r => r * r);

        bool converged = false;
        int iteration = 0;

        for (iteration = 0; iteration < MaxIterations; iteration++)
        {
            // Compute Jacobian numerically using central differences
            var J = ComputeJacobian(model, currentParams, predicted, nData, nParams, sigma);

            // J^T * J and J^T * r
            var JtJ = J.TransposeThisAndMultiply(J);
            var Jtr = J.TransposeThisAndMultiply(
                Vector<double>.Build.DenseOfArray(residuals));

            // Check gradient convergence
            if (Jtr.L2Norm() < GradientTol)
            {
                converged = true;
                break;
            }

            // Try step with current lambda
            bool stepAccepted = false;
            for (int lambdaTries = 0; lambdaTries < 20; lambdaTries++)
            {
                // Damped normal equations: (J^T*J + lambda*diag(J^T*J)) * delta = J^T * r
                var damped = JtJ.Clone();
                for (int i = 0; i < nParams; i++)
                {
                    damped[i, i] += lambda * Math.Max(JtJ[i, i], 1e-12);
                }

                Vector<double> delta;
                try
                {
                    delta = damped.Solve(Jtr);
                }
                catch (Exception)
                {
                    lambda *= LambdaFactor;
                    continue;
                }

                // Trial parameters
                double[] trialParams = new double[nParams];
                for (int i = 0; i < nParams; i++)
                    trialParams[i] = currentParams[i] + delta[i];

                double[] trialPredicted = model(trialParams);
                double[] trialResiduals = ComputeResiduals(observed, trialPredicted, sigma);
                double trialChi2 = trialResiduals.Sum(r => r * r);

                if (trialChi2 < chi2)
                {
                    // Accept step
                    double relChange = Math.Abs(chi2 - trialChi2) / (chi2 + 1e-30);
                    currentParams = trialParams;
                    predicted = trialPredicted;
                    residuals = trialResiduals;
                    chi2 = trialChi2;
                    lambda /= LambdaFactor;
                    stepAccepted = true;

                    if (relChange < ConvergenceTol)
                    {
                        converged = true;
                    }
                    break;
                }
                else
                {
                    lambda *= LambdaFactor;
                }
            }

            if (converged)
                break;

            if (!stepAccepted)
            {
                // Could not find a descent direction
                converged = false;
                break;
            }
        }

        // Compute final covariance matrix: (J^T * J)^{-1} * reduced_chi2
        double reducedChi2 = dof > 0 ? chi2 / dof : chi2;
        var finalJ = ComputeJacobian(model, currentParams, predicted, nData, nParams, sigma);
        var finalJtJ = finalJ.TransposeThisAndMultiply(finalJ);

        double[,]? covarianceMatrix = null;
        double[] paramUncertainties = new double[nParams];

        try
        {
            var cov = finalJtJ.Inverse() * reducedChi2;
            covarianceMatrix = new double[nParams, nParams];
            for (int i = 0; i < nParams; i++)
            {
                for (int j = 0; j < nParams; j++)
                    covarianceMatrix[i, j] = cov[i, j];
                paramUncertainties[i] = Math.Sqrt(Math.Max(0, cov[i, i]));
            }
        }
        catch (Exception)
        {
            // Singular matrix; signal unknown uncertainties with NaN
            for (int i = 0; i < nParams; i++)
                paramUncertainties[i] = double.NaN;
        }

        return new OptimizationResult
        {
            Parameters = currentParams,
            Uncertainties = paramUncertainties,
            CovarianceMatrix = covarianceMatrix,
            ChiSquared = chi2,
            ReducedChiSquared = reducedChi2,
            Iterations = iteration + 1,
            Converged = converged,
            Residuals = residuals
        };
    }

    private static double[] ComputeResiduals(double[] observed, double[] predicted, double[] sigma)
    {
        double[] residuals = new double[observed.Length];
        for (int i = 0; i < observed.Length; i++)
            residuals[i] = (observed[i] - predicted[i]) / sigma[i];
        return residuals;
    }

    private static Matrix<double> ComputeJacobian(
        Func<double[], double[]> model,
        double[] parameters,
        double[] predicted,
        int nData,
        int nParams,
        double[] sigma)
    {
        var J = Matrix<double>.Build.Dense(nData, nParams);
        double[] paramsCopy = (double[])parameters.Clone();

        for (int j = 0; j < nParams; j++)
        {
            double h = Math.Max(Math.Abs(parameters[j]) * 1e-7, 1e-10);

            paramsCopy[j] = parameters[j] + h;
            double[] fPlus = model(paramsCopy);

            paramsCopy[j] = parameters[j] - h;
            double[] fMinus = model(paramsCopy);

            paramsCopy[j] = parameters[j]; // restore

            for (int i = 0; i < nData; i++)
            {
                J[i, j] = (fPlus[i] - fMinus[i]) / (2.0 * h * sigma[i]);
            }
        }

        return J;
    }
}
