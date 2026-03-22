using System;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Calculates electrical conductivity of mantle minerals using Arrhenius models.
/// σ = σ0 * exp(-H/(R*T)) + σ_H * C_H2O^r * exp(-H_H/(R*T))
/// References: Karato (1990), Yoshino et al. (2009).
/// </summary>
public class ElectricalConductivityCalculator
{
    // Dry olivine parameters (Karato 1990)
    private const double Sigma0_dry = 1.0e2;   // S/m pre-exponential
    private const double H_dry = 150000.0;      // J/mol activation enthalpy

    // Hydrous contribution
    private const double Sigma0_wet = 1.0e3;    // S/m
    private const double H_wet = 90000.0;       // J/mol
    private const double R_exponent = 0.62;     // water content exponent

    // Activation volume for pressure correction
    private const double V_act = 1.0e-6;        // m³/mol ≈ 1 cm³/mol

    // Gas constant [J/(mol·K)]
    private const double R = 8.31477;

    /// <summary>
    /// Compute electrical conductivity [S/m] for olivine-like mineral.
    /// </summary>
    /// <param name="T">Temperature [K]</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="waterContent_ppm">Water content [ppm wt]</param>
    /// <returns>Electrical conductivity in S/m</returns>
    public double ComputeConductivity(double T, double P, double waterContent_ppm = 0.0)
    {
        // Dry conductivity: σ_dry = σ0_dry * exp(-H_dry / (R * T))
        double sigma_dry = Sigma0_dry * Math.Exp(-H_dry / (R * T));

        // Wet (hydrous) contribution
        double sigma_wet = 0.0;
        if (waterContent_ppm > 0.0)
        {
            double C_wt_fraction = waterContent_ppm / 1.0e6;
            sigma_wet = Sigma0_wet * Math.Pow(C_wt_fraction, R_exponent)
                        * Math.Exp(-H_wet / (R * T));
        }

        double sigma_total = sigma_dry + sigma_wet;

        // Pressure correction: multiply by exp(P * V_act / (R * T))
        // P in GPa → Pa: P * 1e9
        double pressureCorrection = Math.Exp(P * 1.0e9 * V_act / (R * T));
        sigma_total *= pressureCorrection;

        return sigma_total;
    }

    /// <summary>
    /// Compute aggregate conductivity for multi-mineral mixture using Hashin-Shtrikman bounds.
    /// Returns geometric mean of individual conductivities weighted by volume fractions
    /// as a practical approximation of HS bounds.
    /// </summary>
    /// <param name="conductivities">Individual mineral conductivities [S/m]</param>
    /// <param name="volumeFractions">Volume fractions (sum to 1)</param>
    /// <returns>Aggregate electrical conductivity in S/m</returns>
    public double ComputeAggregateConductivity(double[] conductivities, double[] volumeFractions)
    {
        if (conductivities.Length != volumeFractions.Length)
            throw new ArgumentException("conductivities and volumeFractions must have the same length.");

        if (conductivities.Length == 0)
            throw new ArgumentException("At least one phase is required.");

        double volSum = 0.0;
        for (int i = 0; i < volumeFractions.Length; i++)
            volSum += volumeFractions[i];
        if (Math.Abs(volSum - 1.0) > 0.01)
            throw new ArgumentException($"Volume fractions must sum to 1 (got {volSum}).");

        // Geometric mean: σ_agg = exp(Σ f_i * ln(σ_i))
        double sumLogSigma = 0.0;
        for (int i = 0; i < conductivities.Length; i++)
        {
            if (conductivities[i] <= 0)
                throw new ArgumentException($"Conductivity[{i}] must be positive, got {conductivities[i]}.");
            sumLogSigma += volumeFractions[i] * Math.Log(conductivities[i]);
        }

        return Math.Exp(sumLogSigma);
    }
}
