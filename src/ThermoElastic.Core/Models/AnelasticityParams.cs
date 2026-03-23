namespace ThermoElastic.Core.Models;

/// <summary>
/// Parameters for anelastic Q models. Immutable record with sensible defaults for olivine.
/// All models (Parametric, Andrade, Extended Burgers) use this parameter set.
/// </summary>
public record AnelasticityParams
{
    /// <summary>Grain size [m]. Default 0.01 m = 1 cm (typical upper mantle).</summary>
    public double GrainSize_m { get; init; } = 0.01;
    /// <summary>Activation energy E* [J/mol]. Jackson &amp; Faul (2010) for olivine.</summary>
    public double ActivationEnergy { get; init; } = 424_000.0;
    /// <summary>Activation volume V* [m³/mol]. Estimated for olivine.</summary>
    public double ActivationVolume { get; init; } = 10.0e-6;
    /// <summary>Frequency exponent α. Jackson &amp; Faul (2010).</summary>
    public double FrequencyExponent { get; init; } = 0.27;
    /// <summary>Grain-size exponent m for diffusion-assisted GBS.</summary>
    public double GrainSizeExponent { get; init; } = 3.0;
    /// <summary>Pre-exponential factor A for Q⁻¹.</summary>
    public double PreFactor { get; init; } = 4.35e-4;
    /// <summary>Water content [ppm H/Si]. 0 = dry.</summary>
    public double WaterContent_ppm { get; init; } = 0.0;
    /// <summary>Melt volume fraction. 0 = no melt.</summary>
    public double MeltFraction { get; init; } = 0.0;
    /// <summary>Bulk quality factor Q_K (typically ~1000, nearly elastic).</summary>
    public double QK { get; init; } = 1000.0;
    /// <summary>Reference grain size from lab experiments [m]. Jackson &amp; Faul (2010): 13.4 μm.</summary>
    public double RefGrainSize_m { get; init; } = 13.4e-6;
    /// <summary>Reference temperature for parameter scaling [K].</summary>
    public double RefTemperature_K { get; init; } = 1473.0;
    /// <summary>Reference pressure for parameter scaling [GPa].</summary>
    public double RefPressure_GPa { get; init; } = 0.2;
    /// <summary>Mineral phase identifier (e.g., "olivine", "bridgmanite").</summary>
    public string MineralPhase { get; init; } = "olivine";

    // === Andrade-specific ===
    /// <summary>Andrade exponent n (typically ~1/3).</summary>
    public double AndradeExponent { get; init; } = 1.0 / 3.0;
    /// <summary>Andrade beta pre-factor [Pa^{-1} s^{-n}].</summary>
    public double AndradeBeta { get; init; } = 3.2e-15;
    /// <summary>Steady-state viscosity at reference conditions [Pa·s].</summary>
    public double RefViscosity_PaS { get; init; } = 6.2e21;

    // === Extended Burgers-specific ===
    /// <summary>Relaxation strength Delta (total anelastic relaxation).</summary>
    public double RelaxationStrength { get; init; } = 1.04;
    /// <summary>Log-normal distribution width sigma_tau.</summary>
    public double DistributionWidth { get; init; } = 4.0;
    /// <summary>Viscosity grain-size exponent m_v.</summary>
    public double ViscosityGrainExponent { get; init; } = 3.0;

    // === Water correction ===
    /// <summary>Water exponent r for relaxation time scaling. Range 0.5-2.0.</summary>
    public double WaterExponent { get; init; } = 1.0;
    /// <summary>Reference water content [ppm H/Si].</summary>
    public double RefWaterContent_ppm { get; init; } = 50.0;
}
