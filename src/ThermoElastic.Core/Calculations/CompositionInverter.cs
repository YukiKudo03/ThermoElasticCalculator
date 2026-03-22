namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Inverts seismic observations for mantle composition using grid search.
/// </summary>
public class CompositionInverter
{
    /// <summary>
    /// Compute velocity misfit between a mineral and PREM at a given depth.
    /// Misfit = ((Vp_calc - Vp_PREM)/Vp_PREM)^2 + ((Vs_calc - Vs_PREM)/Vs_PREM)^2 + ((rho_calc - rho_PREM)/rho_PREM)^2
    /// </summary>
    public double ComputeMisfit(MineralParams mineral, double P, double T, double depth_km)
    {
        var result = new MieGruneisenEOSOptimizer(mineral, P, T).ExecOptimize();
        var prem = PREMModel.GetPropertiesAtDepth(depth_km);

        double vpRel = (result.Vp - prem.Vp) / prem.Vp;
        double vsRel = (result.Vs - prem.Vs) / prem.Vs;
        // PREM density is in g/cm3, result.Density is also in g/cm3
        double rhoRel = (result.Density - prem.Density) / prem.Density;

        return vpRel * vpRel + vsRel * vsRel + rhoRel * rhoRel;
    }

    /// <summary>
    /// Grid search over Mg# for best-fit composition.
    /// Interpolates between Mg-endmember and Fe-endmember properties.
    /// </summary>
    /// <param name="mgEndmember">Mg-rich endmember (e.g., forsterite or Mg-perovskite)</param>
    /// <param name="feEndmember">Fe-rich endmember (e.g., fayalite or Fe-perovskite)</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="depth_km">Depth for PREM comparison [km]</param>
    /// <param name="mgMin">Minimum Mg# to search</param>
    /// <param name="mgMax">Maximum Mg# to search</param>
    /// <param name="nSteps">Number of grid points</param>
    /// <returns>Inversion result with best Mg# and misfit profile</returns>
    public InversionResult GridSearch(MineralParams mgEndmember, MineralParams feEndmember,
        double P, double T, double depth_km,
        double mgMin = 0.80, double mgMax = 1.00, int nSteps = 20)
    {
        var misfitProfile = new List<(double MgNumber, double Misfit)>();
        double bestMg = mgMin;
        double bestMisfit = double.MaxValue;

        for (int i = 0; i <= nSteps; i++)
        {
            double mg = mgMin + (mgMax - mgMin) * i / nSteps;
            double fe = 1.0 - mg;

            var interpolated = new MineralParams
            {
                MineralName = $"Interp_Mg{mg:F2}",
                PaperName = "interp",
                NumAtoms = mgEndmember.NumAtoms,
                MolarVolume = mg * mgEndmember.MolarVolume + fe * feEndmember.MolarVolume,
                MolarWeight = mg * mgEndmember.MolarWeight + fe * feEndmember.MolarWeight,
                KZero = mg * mgEndmember.KZero + fe * feEndmember.KZero,
                K1Prime = mg * mgEndmember.K1Prime + fe * feEndmember.K1Prime,
                K2Prime = mg * mgEndmember.K2Prime + fe * feEndmember.K2Prime,
                GZero = mg * mgEndmember.GZero + fe * feEndmember.GZero,
                G1Prime = mg * mgEndmember.G1Prime + fe * feEndmember.G1Prime,
                G2Prime = mg * mgEndmember.G2Prime + fe * feEndmember.G2Prime,
                DebyeTempZero = mg * mgEndmember.DebyeTempZero + fe * feEndmember.DebyeTempZero,
                GammaZero = mg * mgEndmember.GammaZero + fe * feEndmember.GammaZero,
                QZero = mg * mgEndmember.QZero + fe * feEndmember.QZero,
                EhtaZero = mg * mgEndmember.EhtaZero + fe * feEndmember.EhtaZero,
                RefTemp = mgEndmember.RefTemp,
                F0 = mg * mgEndmember.F0 + fe * feEndmember.F0,
            };

            double misfit = ComputeMisfit(interpolated, P, T, depth_km);
            misfitProfile.Add((mg, misfit));

            if (misfit < bestMisfit)
            {
                bestMisfit = misfit;
                bestMg = mg;
            }
        }

        return new InversionResult
        {
            BestMgNumber = bestMg,
            MinMisfit = bestMisfit,
            MisfitProfile = misfitProfile,
        };
    }
}
