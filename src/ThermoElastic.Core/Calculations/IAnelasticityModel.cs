using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Strategy interface for anelastic Q models.
/// Implementations: AnelasticityCalculator (Simple/Tier1),
/// ParametricQCalculator (Tier2), AndradeCalculator (Tier3b),
/// ExtendedBurgersCalculator (Tier3a).
/// </summary>
public interface IAnelasticityModel
{
    /// <summary>Compute shear quality factor QS.</summary>
    double ComputeQS(double T, double P, double frequency, AnelasticityParams? prms = null);

    /// <summary>Apply anelastic correction to elastic velocities.</summary>
    AnelasticResult ApplyCorrection(double Vp, double Vs, double KS, double GS,
        double T, double P, double frequency, AnelasticityParams? prms = null);
}
