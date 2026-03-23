using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Decorator: applies partial melt effect on Q and elastic moduli.
/// Two effects: (1) elastic modulus reduction via contiguity, (2) enhanced dissipation.
/// Reference: Yamauchi &amp; Takei (2016), McCarthy &amp; Takei (2011).
/// </summary>
public class MeltQCorrector : IAnelasticityModel
{
    private readonly IAnelasticityModel _baseModel;

    public MeltQCorrector(IAnelasticityModel baseModel)
    {
        _baseModel = baseModel;
    }

    public double ComputeQS(double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();
        double qs = _baseModel.ComputeQS(T, P, frequency, p);

        if (p.MeltFraction <= 0) return qs;

        // Enhanced dissipation from melt squirt + grain boundary wetting
        // Q⁻¹_melt = Q⁻¹_dry + A_melt * φ^{0.5}
        double meltDissipation = 0.1 * Math.Pow(p.MeltFraction, 0.5);
        double qInv = 1.0 / qs + meltDissipation;
        return Math.Max(1.0 / qInv, 1.0);
    }

    public AnelasticResult ApplyCorrection(double Vp, double Vs, double KS, double GS,
        double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();

        // (1) Elastic effect: reduce moduli via contiguity
        double phi = p.MeltFraction;
        double contiguity = phi > 0 ? 1.0 - 1.8 * Math.Pow(phi, 0.5) : 1.0;
        contiguity = Math.Max(contiguity, 0.1);
        double GS_eff = GS * contiguity * contiguity;
        double KS_eff = KS * (1.0 - 0.5 * phi); // bulk modulus less affected

        // Recompute elastic velocities with reduced moduli
        double density = Vp > 0 && Vs > 0 ? (KS + 4.0 / 3.0 * GS) / (Vp * Vp / 1e6) : 4.0;
        double Vp_melt = 1000.0 * Math.Sqrt((KS_eff + 4.0 / 3.0 * GS_eff) / density);
        double Vs_melt = 1000.0 * Math.Sqrt(GS_eff / density);

        // (2) Anelastic correction on reduced velocities
        double qs = ComputeQS(T, P, frequency, p);
        return AnelasticCorrectionHelper.Apply(Vp_melt, Vs_melt, KS_eff, GS_eff, qs, p.QK, p.FrequencyExponent);
    }
}
