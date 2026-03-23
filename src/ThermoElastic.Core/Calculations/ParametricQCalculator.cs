using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Tier 2: Parametric Q(T,P,f,d) model.
/// Extends the existing Karato (1993) model with grain-size dependence.
/// At reference grain size (d_ref), this reproduces the existing AnelasticityCalculator results.
/// Scaling: Q_S = Q_base(T,P,f) · (d/d_mantle_ref)^{m·α}
/// where Q_base uses the same formula as AnelasticityCalculator.
/// </summary>
public class ParametricQCalculator : IAnelasticityModel
{
    private const double R = 8.314;
    // Default mantle grain size reference: 1 cm (same as AnelasticityParams default)
    private const double MantleGrainRef = 0.01; // 1 cm

    public double ComputeQS(double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();
        double alpha = p.FrequencyExponent;
        double m = p.GrainSizeExponent;

        // Base Q from Karato (1993) formula (same as existing AnelasticityCalculator)
        double H_eff = p.ActivationEnergy + P * 1e9 * p.ActivationVolume;
        double qs_base = p.PreFactor * Math.Pow(frequency, alpha) * Math.Exp(alpha * H_eff / (R * T));

        // Grain-size scaling relative to mantle reference (1 cm)
        // Larger grains → higher Q, smaller grains → lower Q
        double grainFactor = Math.Pow(p.GrainSize_m / MantleGrainRef, m * alpha);

        double qs = qs_base * grainFactor;

        // Premelting correction near solidus
        double T_solidus = AnelasticityCalculator.GetSolidusTemperature(P);
        double t_ratio = T / T_solidus;
        if (t_ratio > 0.9)
        {
            qs *= Math.Exp(-10.0 * Math.Pow(t_ratio, 30.0));
        }

        return Math.Max(qs, 1.0);
    }

    public AnelasticResult ApplyCorrection(double Vp, double Vs, double KS, double GS,
        double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();
        double qs = ComputeQS(T, P, frequency, p);
        double qk = p.QK;
        return AnelasticCorrectionHelper.Apply(Vp, Vs, KS, GS, qs, qk, p.FrequencyExponent);
    }
}
