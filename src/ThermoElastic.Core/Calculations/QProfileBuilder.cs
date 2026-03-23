using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Builds a depth-dependent Q profile and anelastic velocity corrections.
/// Combines: geotherm T(z) + EOS → elastic V + Q(T,P,f,d) → anelastic V.
/// Compares with PREM Q model.
/// </summary>
public class QProfileBuilder
{
    /// <summary>
    /// Compute Q profile from surface to given depth.
    /// </summary>
    /// <param name="model">Anelasticity model to use</param>
    /// <param name="potentialTemp_K">Mantle potential temperature [K]</param>
    /// <param name="frequency">Seismic frequency [Hz]</param>
    /// <param name="grainSize_m">Grain size [m]</param>
    /// <param name="maxDepth_km">Maximum depth [km]</param>
    /// <param name="depthStep_km">Depth step [km]</param>
    public List<QProfilePoint> Build(IAnelasticityModel model, double potentialTemp_K,
        double frequency = 1.0, double grainSize_m = 0.01,
        double maxDepth_km = 800.0, double depthStep_km = 25.0)
    {
        var profile = new List<QProfilePoint>();

        for (double depth = 25.0; depth <= maxDepth_km; depth += depthStep_km)
        {
            var prem = PREMModel.GetPropertiesAtDepth(depth);
            double P = prem.Pressure;
            double T = ComputeAdiabat(potentialTemp_K, depth);

            // Determine dominant mineral phase and get parameters
            string phase = GetDominantPhase(depth);
            string paperName = GetRepresentativeMineral(phase);
            var mineral = SLB2011Endmembers.GetAll().FirstOrDefault(m => m.PaperName == paperName);
            if (mineral == null) continue;

            // Forward model: elastic properties
            var eos = new MieGruneisenEOSOptimizer(mineral, P, T);
            var th = eos.ExecOptimize();
            if (!th.IsConverged) continue;

            // Anelastic correction
            var anelParams = AnelasticityDatabase.GetParamsForMineral(paperName) with
            {
                GrainSize_m = grainSize_m,
            };

            var result = model.ApplyCorrection(th.Vp, th.Vs, th.KS, th.GS, T, P, frequency, anelParams);

            profile.Add(new QProfilePoint
            {
                Depth_km = depth,
                Pressure_GPa = P,
                Temperature_K = T,
                QS = result.QS,
                QS_PREM = PREMModel.GetQSAtDepth(depth),
                Vp_elastic = th.Vp,
                Vs_elastic = th.Vs,
                Vp_anelastic = result.Vp_anelastic,
                Vs_anelastic = result.Vs_anelastic,
                DominantPhase = phase,
            });
        }

        return profile;
    }

    /// <summary>Simple adiabatic geotherm: T(z) = Tp + dT/dz * z.</summary>
    private static double ComputeAdiabat(double Tp, double depth_km)
    {
        // Gradient ~0.3-0.5 K/km in upper mantle, increasing slightly with depth
        double gradient = 0.4; // K/km average
        return Tp + gradient * depth_km;
    }

    private static string GetDominantPhase(double depth_km)
    {
        if (depth_km < 410) return "olivine";
        if (depth_km < 520) return "wadsleyite";
        if (depth_km < 660) return "ringwoodite";
        return "bridgmanite";
    }

    private static string GetRepresentativeMineral(string phase) => phase switch
    {
        "olivine" => "fo",
        "wadsleyite" => "mw",
        "ringwoodite" => "mrw",
        "bridgmanite" => "mpv",
        "ferropericlase" => "pe",
        _ => "fo",
    };
}
