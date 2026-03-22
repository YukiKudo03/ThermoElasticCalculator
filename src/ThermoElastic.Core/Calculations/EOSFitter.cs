namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Fits equation-of-state parameters to experimental P-V or P-V-T data.
/// </summary>
public class EOSFitter
{
    private readonly LevenbergMarquardtOptimizer _optimizer = new();

    /// <summary>
    /// Fit Birch-Murnaghan 3rd order EOS to P-V data.
    /// Fits K0 and K' (V0 fixed).
    /// </summary>
    /// <param name="data">List of (P [GPa], V [cm3/mol], sigmaV [cm3/mol]) data points</param>
    /// <param name="V0">Reference volume [cm3/mol]</param>
    /// <returns>OptimizationResult where Parameters = [K0, K']</returns>
    public OptimizationResult FitPV(List<(double P, double V, double sigmaV)> data, double V0)
    {
        double[] observed = data.Select(d => d.P).ToArray();
        double[] volumes = data.Select(d => d.V).ToArray();

        // Convert sigmaV to sigmaP via |dP/dV| * sigmaV
        // For simplicity use a fractional uncertainty on P: max(|P|*0.01, 0.01)
        // unless the data uncertainties are meaningful
        double[] sigmaP = data.Select(d =>
        {
            double dPdV = Math.Abs(ComputeDPDV(V0, d.V, 160.0, 4.0));
            double sp = dPdV * d.sigmaV;
            return Math.Max(sp, 0.01);
        }).ToArray();

        Func<double[], double[]> model = (parameters) =>
        {
            double K0 = parameters[0];
            double K1 = parameters[1];
            double[] predictions = new double[volumes.Length];
            for (int i = 0; i < volumes.Length; i++)
            {
                predictions[i] = BM3Pressure(V0, volumes[i], K0, K1);
            }
            return predictions;
        };

        // Initial guesses
        double[] initialParams = { 160.0, 4.0 };

        return _optimizer.Optimize(model, initialParams, observed, sigmaP);
    }

    /// <summary>
    /// Generate F-f (Eulerian finite strain) diagnostic plot data.
    /// F = P / (3f(1+2f)^{5/2}), f = ((V0/V)^{2/3} - 1) / 2
    /// For BM3: F = K0 + 3/2 * K0 * (K'-4) * f
    /// </summary>
    public List<(double f, double F_data, double F_fit)> GenerateFfPlot(
        List<(double P, double V)> data, double V0, double K0_fit, double K1_fit)
    {
        var result = new List<(double f, double F_data, double F_fit)>();

        foreach (var (P, V) in data)
        {
            double f = EulerianStrain(V0, V);
            double factor = 3.0 * f * Math.Pow(1.0 + 2.0 * f, 2.5);
            double F_data = (Math.Abs(factor) > 1e-15) ? P / factor : 0.0;
            double F_fit = K0_fit + 1.5 * K0_fit * (K1_fit - 4.0) * f;
            result.Add((f, F_data, F_fit));
        }

        return result.OrderBy(x => x.f).ToList();
    }

    /// <summary>
    /// Birch-Murnaghan 3rd order pressure.
    /// P = 3*K0*f*(1+2f)^{5/2} * (1 + 3/2*(K'-4)*f)
    /// where f = ((V0/V)^{2/3} - 1) / 2
    /// </summary>
    public static double BM3Pressure(double V0, double V, double K0, double K1Prime)
    {
        double f = EulerianStrain(V0, V);
        return 3.0 * K0 * f * Math.Pow(1.0 + 2.0 * f, 2.5) * (1.0 + 1.5 * (K1Prime - 4.0) * f);
    }

    /// <summary>
    /// Eulerian finite strain: f = ((V0/V)^{2/3} - 1) / 2
    /// </summary>
    public static double EulerianStrain(double V0, double V)
    {
        return (Math.Pow(V0 / V, 2.0 / 3.0) - 1.0) / 2.0;
    }

    private static double ComputeDPDV(double V0, double V, double K0, double K1)
    {
        double h = V * 1e-6;
        double pPlus = BM3Pressure(V0, V + h, K0, K1);
        double pMinus = BM3Pressure(V0, V - h, K0, K1);
        return (pPlus - pMinus) / (2.0 * h);
    }
}
