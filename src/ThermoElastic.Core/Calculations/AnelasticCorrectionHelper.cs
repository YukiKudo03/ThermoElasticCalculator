using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Shared velocity correction logic: Anderson &amp; Given (1982).
/// V_anelastic = V_elastic * (1 - cot(π·α/2) / (2·Q))
/// </summary>
public static class AnelasticCorrectionHelper
{
    public static AnelasticResult Apply(double Vp, double Vs, double KS, double GS,
        double QS, double QK, double alpha)
    {
        double cot_alpha = 1.0 / Math.Tan(Math.PI * alpha / 2.0);

        double vs_factor = 1.0 - cot_alpha / (2.0 * QS);
        double vs_anelastic = Vs * Math.Max(vs_factor, 0.5); // safety clamp

        double M = KS + 4.0 / 3.0 * GS;
        double vp_correction = 1.0 - (4.0 / 3.0 * GS / M / (2.0 * QS)
                                     + KS / M / (2.0 * QK)) * cot_alpha;
        double vp_anelastic = Vp * Math.Max(vp_correction, 0.5);

        return new AnelasticResult
        {
            QS = QS,
            QK = QK,
            Vp_anelastic = vp_anelastic,
            Vs_anelastic = vs_anelastic,
            Vp_elastic = Vp,
            Vs_elastic = Vs,
        };
    }
}
