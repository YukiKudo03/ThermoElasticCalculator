using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Tier 3b: Andrade viscoelastic model with analytical J*(ω).
/// J*(ω) = J_U + β_s·Γ(1+n)·(iω)^{-n} - i/(ω·η_s)
/// where β_s and η_s are scaled with T, P, and grain size.
/// No numerical integration needed (analytical formula).
/// References: Sundberg &amp; Cooper (2010), Jackson &amp; Faul (2010).
/// </summary>
public class AndradeCalculator : IAnelasticityModel
{
    private const double R = 8.314;
    private const double Gamma_4_3 = 0.89297951156924921; // Γ(4/3) for n=1/3

    public double ComputeQS(double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();
        double G_U = 80e9; // nominal unrelaxed shear modulus [Pa]
        var (jReal, jImag) = ComputeCompliance(T, P, frequency, p, G_U);
        return Math.Max(Math.Abs(jReal / jImag), 1.0);
    }

    public AnelasticResult ApplyCorrection(double Vp, double Vs, double KS, double GS,
        double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();
        double G_U = GS * 1e9; // actual unrelaxed modulus [Pa]
        var (jReal, jImag) = ComputeCompliance(T, P, frequency, p, G_U);

        double qs = Math.Max(Math.Abs(jReal / jImag), 1.0);
        double G_eff_GPa = 1.0 / jReal / 1e9;

        var result = AnelasticCorrectionHelper.Apply(Vp, Vs, KS, GS, qs, p.QK, p.FrequencyExponent);

        return new ViscoelasticResult
        {
            QS = result.QS, QK = result.QK,
            Vp_anelastic = result.Vp_anelastic, Vs_anelastic = result.Vs_anelastic,
            Vp_elastic = result.Vp_elastic, Vs_elastic = result.Vs_elastic,
            J_real = jReal, J_imag = jImag,
            G_unrelaxed = GS, G_relaxed = G_eff_GPa,
        };
    }

    /// <summary>
    /// Compute J*(ω) = J_U + β_s·Γ(1+n)·(iω)^{-n} - i/(ω·η_s)
    /// </summary>
    public (double jReal, double jImag) ComputeCompliance(double T, double P, double frequency,
        AnelasticityParams p, double G_U_Pa)
    {
        double omega = 2.0 * Math.PI * frequency;
        double n = p.AndradeExponent;
        double J_U = 1.0 / G_U_Pa;

        // Scale relaxation time with T, P, grain size
        double tau_scale = ComputeRelaxationTimeScale(T, P, p);

        // Scaled beta: β_s = β_ref · tau_scale^n
        double beta_s = p.AndradeBeta * Math.Pow(tau_scale, n);
        // Scaled viscosity: η_s = η_ref · tau_scale
        double eta_s = p.RefViscosity_PaS * tau_scale;

        double gammaN = (Math.Abs(n - 1.0 / 3.0) < 0.01) ? Gamma_4_3 : TGamma(1.0 + n);

        // (iω)^{-n} decomposition
        double omegaN = Math.Pow(omega, -n);
        double cosN = Math.Cos(n * Math.PI / 2.0);
        double sinN = Math.Sin(n * Math.PI / 2.0);

        double jReal = J_U + beta_s * gammaN * omegaN * cosN;
        double jImag = -(beta_s * gammaN * omegaN * sinN + 1.0 / (omega * eta_s));

        return (jReal, jImag);
    }

    /// <summary>
    /// Relaxation time scale factor: tau/tau_ref.
    /// tau ∝ d^m · exp((E*+PV*)/(RT)) relative to reference conditions.
    /// </summary>
    private static double ComputeRelaxationTimeScale(double T, double P, AnelasticityParams p)
    {
        double m = p.GrainSizeExponent;
        double grainScale = Math.Pow(p.GrainSize_m / p.RefGrainSize_m, m);

        double H_eff = p.ActivationEnergy + P * 1e9 * p.ActivationVolume;
        double H_ref = p.ActivationEnergy + p.RefPressure_GPa * 1e9 * p.ActivationVolume;
        double exponent = H_eff / (R * T) - H_ref / (R * p.RefTemperature_K);
        exponent = Math.Clamp(exponent, -50, 50);

        return grainScale * Math.Exp(exponent);
    }

    private static double TGamma(double x)
    {
        if (x <= 0) return double.NaN;
        double[] c = { 76.18009172947146, -86.50532032941677, 24.01409824083091,
                       -1.231739572450155, 0.1208650973866179e-2, -0.5395239384953e-5 };
        double y = x;
        double tmp = x + 5.5;
        tmp -= (x + 0.5) * Math.Log(tmp);
        double ser = 1.000000000190015;
        for (int j = 0; j < 6; j++) ser += c[j] / ++y;
        return Math.Exp(-tmp + Math.Log(2.5066282746310005 * ser / x));
    }
}
