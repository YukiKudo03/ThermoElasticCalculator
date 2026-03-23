using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Database;

/// <summary>
/// Mineral-specific anelastic parameters for Q models.
/// Sources: Jackson &amp; Faul (2010), Faul &amp; Jackson (2015), research survey.
/// </summary>
public static class AnelasticityDatabase
{
    /// <summary>
    /// Get anelastic parameters for a mineral identified by SLB2011 PaperName.
    /// Returns olivine defaults for unrecognized minerals.
    /// </summary>
    public static AnelasticityParams GetParamsForMineral(string paperName)
    {
        return ClassifyPhase(paperName) switch
        {
            "olivine" => Olivine(),
            "wadsleyite" => Wadsleyite(),
            "ringwoodite" => Ringwoodite(),
            "bridgmanite" => Bridgmanite(),
            "ferropericlase" => Ferropericlase(),
            "garnet" => Garnet(),
            "pyroxene" => Pyroxene(),
            _ => Olivine(),
        };
    }

    /// <summary>Classify SLB2011 PaperName to mineral phase group.</summary>
    public static string ClassifyPhase(string paperName)
    {
        return paperName switch
        {
            "fo" or "fa" => "olivine",
            "mw" or "fw" => "wadsleyite",
            "mrw" or "frw" => "ringwoodite",
            "mpv" or "fpv" or "apv" or "capv" => "bridgmanite",
            "mppv" or "fppv" or "appv" => "bridgmanite",
            "pe" or "wu" => "ferropericlase",
            "py" or "al" or "gr" or "maj" or "namaj" => "garnet",
            "en" or "fs" or "di" or "he" or "jd" or "cats" or "mgts" => "pyroxene",
            "hpcen" or "hpcfs" => "pyroxene",
            _ => "olivine",
        };
    }

    // Jackson & Faul (2010), Table 3
    public static AnelasticityParams Olivine() => new()
    {
        MineralPhase = "olivine",
        ActivationEnergy = 424_000.0,    // J/mol
        ActivationVolume = 10.0e-6,      // m³/mol
        FrequencyExponent = 0.27,
        GrainSizeExponent = 3.0,
        PreFactor = 4.35e-4,
        RefGrainSize_m = 13.4e-6,        // 13.4 μm
        RefTemperature_K = 1473.0,       // 1200°C
        RefPressure_GPa = 0.2,
        RefViscosity_PaS = 6.2e21,
        AndradeExponent = 1.0 / 3.0,
        AndradeBeta = 3.2e-15,
        RelaxationStrength = 1.04,
        DistributionWidth = 4.0,
    };

    // Estimated: similar to olivine but lower E* (less rigid)
    public static AnelasticityParams Wadsleyite() => new()
    {
        MineralPhase = "wadsleyite",
        ActivationEnergy = 370_000.0,
        ActivationVolume = 8.0e-6,
        FrequencyExponent = 0.27,
        GrainSizeExponent = 3.0,
        PreFactor = 4.35e-4,
        RefGrainSize_m = 13.4e-6,
        RefTemperature_K = 1473.0,
        RefPressure_GPa = 0.2,
        RefViscosity_PaS = 6.2e21,
        AndradeExponent = 1.0 / 3.0,
        AndradeBeta = 3.2e-15,
        RelaxationStrength = 1.04,
        DistributionWidth = 4.0,
    };

    public static AnelasticityParams Ringwoodite() => Wadsleyite() with { MineralPhase = "ringwoodite" };

    // High-pressure phase: higher E*, lower V*, lower α
    public static AnelasticityParams Bridgmanite() => new()
    {
        MineralPhase = "bridgmanite",
        ActivationEnergy = 500_000.0,
        ActivationVolume = 3.0e-6,
        FrequencyExponent = 0.20,
        GrainSizeExponent = 2.5,
        PreFactor = 4.35e-4,
        RefGrainSize_m = 13.4e-6,
        RefTemperature_K = 1473.0,
        RefPressure_GPa = 0.2,
        RefViscosity_PaS = 1.0e22,
        AndradeExponent = 1.0 / 3.0,
        AndradeBeta = 1.0e-15,
        RelaxationStrength = 0.8,
        DistributionWidth = 4.0,
    };

    public static AnelasticityParams Ferropericlase() => new()
    {
        MineralPhase = "ferropericlase",
        ActivationEnergy = 300_000.0,
        ActivationVolume = 5.0e-6,
        FrequencyExponent = 0.25,
        GrainSizeExponent = 3.0,
        PreFactor = 4.35e-4,
        RefGrainSize_m = 13.4e-6,
        RefTemperature_K = 1473.0,
        RefPressure_GPa = 0.2,
        RefViscosity_PaS = 3.0e21,
        AndradeExponent = 1.0 / 3.0,
        AndradeBeta = 5.0e-15,
        RelaxationStrength = 1.0,
        DistributionWidth = 4.0,
    };

    // Estimated from olivine-like behavior
    public static AnelasticityParams Garnet() => Olivine() with
    {
        MineralPhase = "garnet",
        ActivationEnergy = 450_000.0,
        ActivationVolume = 8.0e-6,
    };

    public static AnelasticityParams Pyroxene() => Olivine() with
    {
        MineralPhase = "pyroxene",
        ActivationEnergy = 400_000.0,
        ActivationVolume = 9.0e-6,
    };
}
