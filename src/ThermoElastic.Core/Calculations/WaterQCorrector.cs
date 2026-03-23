using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Decorator: applies water (hydrogen) content effect on Q.
/// Water enhances grain-boundary diffusion → shorter relaxation time → lower Q.
/// Q⁻¹_wet = Q⁻¹_dry · (C_OH / C_ref)^{r·α}
/// Note: when WaterContent_ppm = 0, returns base model results unchanged.
/// The base model parameters (E*=424kJ) were calibrated at ~50 ppm H/Si reference;
/// 0 ppm returns the 50-ppm calibrated Q, not a truly anhydrous value.
/// References: Aizawa et al. (2008), Cline et al. (2018).
/// </summary>
public class WaterQCorrector : IAnelasticityModel
{
    private readonly IAnelasticityModel _baseModel;

    public WaterQCorrector(IAnelasticityModel baseModel)
    {
        _baseModel = baseModel;
    }

    public double ComputeQS(double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();
        double qs_dry = _baseModel.ComputeQS(T, P, frequency, p);

        if (p.WaterContent_ppm <= 0) return qs_dry;

        double waterFactor = Math.Pow(p.WaterContent_ppm / p.RefWaterContent_ppm,
            p.WaterExponent * p.FrequencyExponent);
        return qs_dry / waterFactor;
    }

    public AnelasticResult ApplyCorrection(double Vp, double Vs, double KS, double GS,
        double T, double P, double frequency, AnelasticityParams? prms = null)
    {
        var p = prms ?? new AnelasticityParams();

        // Delegate to base model first to preserve Tier-3 G_relaxed physics
        var baseResult = _baseModel.ApplyCorrection(Vp, Vs, KS, GS, T, P, frequency, p);

        if (p.WaterContent_ppm <= 0) return baseResult;

        // Apply water correction to Q, then recompute velocities
        double qs_wet = ComputeQS(T, P, frequency, p);
        return AnelasticCorrectionHelper.Apply(Vp, Vs, KS, GS, qs_wet, p.QK, p.FrequencyExponent);
    }
}
