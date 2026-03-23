namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Fits SLB2011 thermoelastic mineral parameters from observed Vp/Vs and/or V(T,P) data.
/// Uses the existing Mie-Gruneisen forward model and Levenberg-Marquardt optimizer.
/// </summary>
public class ThermoElasticFitter
{
    private readonly LevenbergMarquardtOptimizer _optimizer = new();

    /// <summary>
    /// Fit thermoelastic parameters to observed data.
    /// </summary>
    /// <param name="config">Configuration specifying which parameters to fit and initial values.</param>
    /// <param name="data">Observed data points (Vp/Vs and/or V at T,P conditions).</param>
    /// <returns>OptimizationResult where Parameters contains only the free (fitted) parameter values
    /// in the order they appear in the FitFlags array.</returns>
    public OptimizationResult Fit(FittingConfig config, List<FittingDataPoint> data)
    {
        if (data.Count == 0)
            throw new ArgumentException("No data points provided.");
        if (config.FreeParameterCount == 0)
            throw new ArgumentException("No parameters selected for fitting.");

        // Build observed and sigma arrays from data
        var (observed, sigma) = BuildObservedArrays(data);

        if (observed.Length == 0)
            throw new ArgumentException("No valid observed data (need Vp/Vs or Volume).");

        // Initial free parameter values
        double[] initialFree = config.PackFreeParams();

        // Scale factors for normalization (improves Jacobian conditioning)
        double[] scale = (double[])initialFree.Clone();
        for (int i = 0; i < scale.Length; i++)
            scale[i] = Math.Max(Math.Abs(scale[i]), 1e-10);

        // Normalized initial parameters
        double[] normalizedInit = new double[initialFree.Length];
        for (int i = 0; i < initialFree.Length; i++)
            normalizedInit[i] = initialFree[i] / scale[i];

        // Build model function: normalized params -> predicted values
        Func<double[], double[]> model = (double[] normalizedParams) =>
        {
            // Denormalize
            double[] freeParams = new double[normalizedParams.Length];
            for (int i = 0; i < normalizedParams.Length; i++)
                freeParams[i] = normalizedParams[i] * scale[i];

            MineralParams mineral = config.UnpackToMineralParams(freeParams);
            return ComputePredicted(mineral, data);
        };

        var result = _optimizer.Optimize(model, normalizedInit, observed, sigma);

        // Denormalize the result parameters and uncertainties
        double[] denormParams = new double[result.Parameters.Length];
        double[] denormUncertainties = new double[result.Uncertainties.Length];
        for (int i = 0; i < result.Parameters.Length; i++)
        {
            denormParams[i] = result.Parameters[i] * scale[i];
            denormUncertainties[i] = result.Uncertainties[i] * scale[i];
        }

        // Denormalize covariance matrix
        double[,]? denormCov = null;
        if (result.CovarianceMatrix != null)
        {
            int n = result.Parameters.Length;
            denormCov = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    denormCov[i, j] = result.CovarianceMatrix[i, j] * scale[i] * scale[j];
        }

        return new OptimizationResult
        {
            Parameters = denormParams,
            Uncertainties = denormUncertainties,
            CovarianceMatrix = denormCov,
            ChiSquared = result.ChiSquared,
            ReducedChiSquared = result.ReducedChiSquared,
            Iterations = result.Iterations,
            Converged = result.Converged,
            Residuals = result.Residuals,
        };
    }

    /// <summary>
    /// Build flat observed and sigma arrays from data points.
    /// Order: for each data point, append Vp then Vs (if velocity data), then V (if volume data).
    /// </summary>
    private static (double[] observed, double[] sigma) BuildObservedArrays(List<FittingDataPoint> data)
    {
        var obs = new List<double>();
        var sig = new List<double>();

        foreach (var dp in data)
        {
            if (dp.HasVelocityData)
            {
                obs.Add(dp.Vp);
                sig.Add(double.IsNaN(dp.SigmaVp) || dp.SigmaVp <= 0 ? dp.Vp * 0.01 : dp.SigmaVp);
                obs.Add(dp.Vs);
                sig.Add(double.IsNaN(dp.SigmaVs) || dp.SigmaVs <= 0 ? dp.Vs * 0.01 : dp.SigmaVs);
            }
            if (dp.HasVolumeData)
            {
                obs.Add(dp.Volume);
                sig.Add(double.IsNaN(dp.SigmaVolume) || dp.SigmaVolume <= 0 ? dp.Volume * 0.001 : dp.SigmaVolume);
            }
        }

        return (obs.ToArray(), sig.ToArray());
    }

    /// <summary>
    /// Compute predicted values (Vp, Vs, V) for all data points using the forward model.
    /// Must return values in the same order as BuildObservedArrays.
    /// </summary>
    private static double[] ComputePredicted(MineralParams mineral, List<FittingDataPoint> data)
    {
        var predictions = new List<double>();
        int failCount = 0;

        foreach (var dp in data)
        {
            ThermoMineralParams? th = null;
            bool failed = false;
            try
            {
                var eos = new MieGruneisenEOSOptimizer(mineral, dp.Pressure, dp.Temperature);
                th = eos.ExecOptimize();
                if (!th.IsConverged) failed = true;
            }
            catch
            {
                failed = true;
            }

            if (failed || th == null)
            {
                failCount++;
                if (dp.HasVelocityData)
                {
                    predictions.Add(1e6);
                    predictions.Add(1e6);
                }
                if (dp.HasVolumeData)
                    predictions.Add(1e6);
                continue;
            }

            if (dp.HasVelocityData)
            {
                predictions.Add(th.Vp);
                predictions.Add(th.Vs);
            }
            if (dp.HasVolumeData)
            {
                predictions.Add(th.Volume);
            }
        }

        if (failCount == data.Count)
            throw new InvalidOperationException(
                "Forward model failed for all data points. Check initial parameter values.");

        return predictions.ToArray();
    }
}
