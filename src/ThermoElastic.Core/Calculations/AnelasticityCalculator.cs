using System;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Applies frequency-dependent anelastic (Q) corrections to convert
/// elastic velocities to anelastic (seismic) velocities.
///
/// Q model follows Karato (1993) / Jackson &amp; Faul (2010):
///   QS = Q0 * exp(alpha * H* / (R*T)) * (freq / freq_ref)^alpha
///
/// Velocity correction follows Anderson &amp; Given (1982):
///   V_anelastic = V_elastic * (1 - 1/(2*Q) * cot(pi*alpha/2))
/// </summary>
public class AnelasticityCalculator
{
    // Model parameters (Karato 1993 style with pressure-dependent activation)
    private const double Q0 = 4.35e-4;                // pre-exponential factor
    private const double H_star = 500_000.0;           // activation enthalpy [J/mol]
    private const double V_act = 10.0e-6;              // activation volume [m³/mol]
    private const double Alpha = 0.27;                 // frequency exponent
    private const double R = 8.314;                     // gas constant [J/mol/K]
    private const double FreqRef = 1.0;                 // reference frequency [Hz]
    private const double QK_default = 1000.0;           // bulk quality factor (nearly elastic)

    // Premelting correction parameters
    private const double B_premelt = 10.0;              // premelting amplitude
    private const double C_premelt = 30.0;              // premelting exponent (sharp onset)
    private const double T_ratio_onset = 0.9;           // T/T_solidus threshold for premelting

    /// <summary>
    /// Compute shear quality factor QS at given conditions.
    /// </summary>
    /// <param name="T">Temperature [K]</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="frequency">Seismic frequency [Hz] (default 1 Hz)</param>
    /// <returns>Shear quality factor QS</returns>
    public double ComputeQS(double T, double P, double frequency = 1.0)
    {
        // Base Q from Karato (1993) model with pressure-dependent activation
        // H_eff = H* + P * V_act (P in GPa → Pa for consistent units)
        double H_eff = H_star + P * 1.0e9 * V_act;
        double qs = Q0 * Math.Exp(Alpha * H_eff / (R * T))
                       * Math.Pow(frequency / FreqRef, Alpha);

        // Premelting correction near solidus
        double T_solidus = GetSolidusTemperature(P);
        double t_ratio = T / T_solidus;

        if (t_ratio > T_ratio_onset)
        {
            double premelt_factor = Math.Exp(-B_premelt * Math.Pow(t_ratio, C_premelt));
            qs *= premelt_factor;
        }

        return qs;
    }

    /// <summary>
    /// Apply anelastic correction to elastic velocities.
    /// </summary>
    /// <param name="Vp_elastic">Elastic P-wave velocity [m/s]</param>
    /// <param name="Vs_elastic">Elastic S-wave velocity [m/s]</param>
    /// <param name="KS">Adiabatic bulk modulus [GPa]</param>
    /// <param name="GS">Shear modulus [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="frequency">Seismic frequency [Hz] (default 1 Hz)</param>
    /// <returns>AnelasticResult with corrected velocities and Q factors</returns>
    public AnelasticResult ApplyCorrection(double Vp_elastic, double Vs_elastic,
        double KS, double GS, double T, double P, double frequency = 1.0)
    {
        double qs = ComputeQS(T, P, frequency);
        double qk = QK_default;

        double cot_alpha = 1.0 / Math.Tan(Math.PI * Alpha / 2.0);

        // Vs correction: Anderson & Given (1982)
        double vs_factor = 1.0 - 1.0 / (2.0 * qs) * cot_alpha;
        double vs_anelastic = Vs_elastic * vs_factor;

        // Vp correction: account for both QS and QK contributions
        // M = KS + 4/3 * GS (P-wave modulus)
        double M = KS + 4.0 / 3.0 * GS;
        double vp_correction = 1.0 - (4.0 / 3.0 * GS / M * 1.0 / (2.0 * qs)
                                     + KS / M * 1.0 / (2.0 * qk)) * cot_alpha;
        double vp_anelastic = Vp_elastic * vp_correction;

        return new AnelasticResult
        {
            QS = qs,
            QK = qk,
            Vp_anelastic = vp_anelastic,
            Vs_anelastic = vs_anelastic,
            Vp_elastic = Vp_elastic,
            Vs_elastic = Vs_elastic
        };
    }

    /// <summary>
    /// Compute solidus temperature [K] at given pressure [GPa].
    /// Hirschmann (2000) parameterization for peridotite.
    /// </summary>
    /// <param name="P">Pressure [GPa]</param>
    /// <returns>Solidus temperature [K]</returns>
    public static double GetSolidusTemperature(double P)
    {
        double pClamped = Math.Min(P, 13.0); // Valid range for this parameterization
        return 1400.0 + 130.0 * pClamped - 5.0 * pClamped * pClamped;
    }
}
