using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Tier 3a: Extended Burgers viscoelastic model (Jackson &amp; Faul 2010).
/// J*(ω) = J_U · (1 + Δ·∫D(τ)/(1+iωτ)dlnτ) - i/(ω·η_M)
/// Most accurate but computationally more expensive.
/// </summary>
public class ExtendedBurgersCalculator : IAnelasticityModel
{
    private const double R = 8.314;
    private const int IntegrationPoints = 200;

    public double ComputeQS(double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();
        double G_U = 80e9;
        var (jReal, jImag) = ComputeCompliance(T, P, frequency, p, G_U);
        return Math.Max(Math.Abs(jReal / jImag), 1.0);
    }

    public AnelasticResult ApplyCorrection(double Vp, double Vs, double KS, double GS,
        double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();
        double G_U = GS * 1e9;
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

    public (double jReal, double jImag) ComputeCompliance(double T, double P, double frequency,
        AnelasticityParams p, double G_U_Pa)
    {
        double omega = 2.0 * Math.PI * frequency;
        double J_U = 1.0 / G_U_Pa;

        // Relaxation time scaling
        double tau_scale = ComputeRelaxationTimeScale(T, P, p);

        // Maxwell time and viscosity at current conditions
        double tau_M_ref = p.RefViscosity_PaS * J_U; // reference Maxwell time [s]
        double tau_M = tau_M_ref * tau_scale;
        double eta_M = G_U_Pa * tau_M;

        // Integration bounds in ln(tau) space
        double lnTau_M = Math.Log(tau_M);
        double sigma = p.DistributionWidth;
        double lnTau_L = lnTau_M - 5.0 * sigma;
        double lnTau_H = lnTau_M + 5.0 * sigma;

        double alpha = p.FrequencyExponent;
        double Delta = p.RelaxationStrength;

        // Trapezoidal integration over ln(tau)
        double dlnTau = (lnTau_H - lnTau_L) / IntegrationPoints;
        double integReal = 0, integImag = 0, norm = 0;

        for (int i = 0; i <= IntegrationPoints; i++)
        {
            double lnTau = lnTau_L + i * dlnTau;
            double tau = Math.Exp(lnTau);

            double z = (lnTau - lnTau_M) / sigma;
            double D = Math.Pow(tau / tau_M, -alpha) * Math.Exp(-0.5 * z * z);

            double w = (i == 0 || i == IntegrationPoints) ? 0.5 : 1.0;

            double denom = 1.0 + omega * omega * tau * tau;
            integReal += w * D / denom * dlnTau;
            integImag += w * D * (-omega * tau) / denom * dlnTau;
            norm += w * D * dlnTau;
        }

        if (norm > 0)
        {
            integReal /= norm;
            integImag /= norm;
        }

        double jReal = J_U * (1.0 + Delta * integReal);
        double jImag = J_U * Delta * integImag - 1.0 / (omega * eta_M);

        return (jReal, jImag);
    }

    private static double ComputeRelaxationTimeScale(double T, double P, AnelasticityParams p)
    {
        double m = p.GrainSizeExponent;
        double grainScale = Math.Pow(p.GrainSize_m / p.RefGrainSize_m, m);

        double H_eff = p.ActivationEnergy + P * 1e9 * p.ActivationVolume;
        double H_ref = p.ActivationEnergy + p.RefPressure_GPa * 1e9 * p.ActivationVolume;

        // Clamp exponent to avoid overflow
        double exponent = H_eff / (R * T) - H_ref / (R * p.RefTemperature_K);
        exponent = Math.Clamp(exponent, -50, 50);

        return grainScale * Math.Exp(exponent);
    }
}
