namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Computes joint log-likelihood for combining multiple observational datasets.
/// </summary>
public class JointLikelihood
{
    /// <summary>
    /// Compute Gaussian log-likelihood: -0.5 * Sigma((observed - predicted)^2 / sigma^2)
    /// </summary>
    public static double GaussianLogLikelihood(double[] observed, double[] predicted, double[] sigma)
    {
        double ll = 0;
        for (int i = 0; i < observed.Length; i++)
        {
            double diff = (observed[i] - predicted[i]) / sigma[i];
            ll -= 0.5 * diff * diff;
        }
        return ll;
    }

    /// <summary>
    /// Compute uniform prior log-probability.
    /// Returns 0 if all params within bounds, -infinity otherwise.
    /// </summary>
    public static double UniformPrior(double[] parameters, double[] lowerBounds, double[] upperBounds)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i] < lowerBounds[i] || parameters[i] > upperBounds[i])
                return double.NegativeInfinity;
        }
        return 0.0;
    }
}
