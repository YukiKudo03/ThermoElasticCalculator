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

    /// <summary>
    /// MCMC-based Bayesian inversion for Mg# with posterior distribution.
    /// Uses Gaussian likelihood on Vp, Vs, density misfit vs PREM.
    /// </summary>
    /// <param name="mgEndmember">Mg-rich endmember</param>
    /// <param name="feEndmember">Fe-rich endmember</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="depth_km">Depth for PREM comparison</param>
    /// <param name="nSamples">Number of MCMC samples</param>
    /// <param name="burnIn">Burn-in samples to discard</param>
    /// <returns>Inversion result with Mg# posterior statistics</returns>
    public MCMCInversionResult MCMCInversion(MineralParams mgEndmember, MineralParams feEndmember,
        double P, double T, double depth_km,
        int nSamples = 5000, int burnIn = 1000)
    {
        // Log-posterior: Gaussian likelihood + uniform prior on [0, 1]
        Func<double[], double> logPosterior = (double[] param) =>
        {
            double mg = param[0];
            if (mg < 0.0 || mg > 1.0) return double.NegativeInfinity;

            double fe = 1.0 - mg;
            var interpolated = InterpolateMineral(mgEndmember, feEndmember, mg, fe);

            try
            {
                double misfit = ComputeMisfit(interpolated, P, T, depth_km);
                // Log-likelihood: -0.5 * misfit / sigma^2, with sigma = 0.01 (1% relative)
                return -0.5 * misfit / (0.01 * 0.01);
            }
            catch { return double.NegativeInfinity; }
        };

        // Run grid search first for initial guess
        var gridResult = GridSearch(mgEndmember, feEndmember, P, T, depth_km);
        double initialMg = gridResult.BestMgNumber;

        var sampler = new MCMCSampler();
        var chain = sampler.Sample(
            logPosterior,
            initialParams: new[] { initialMg },
            stepSizes: new[] { 0.02 },
            nSamples: nSamples,
            paramNames: new[] { "Mg#" }
        );

        // Extract posterior statistics (after burn-in)
        var posteriorSamples = new List<double>();
        for (int i = burnIn; i < nSamples; i++)
            posteriorSamples.Add(chain.Samples[i, 0]);

        double mean = posteriorSamples.Average();
        double std = Math.Sqrt(posteriorSamples.Average(x => (x - mean) * (x - mean)));

        // 95% credible interval
        posteriorSamples.Sort();
        int n = posteriorSamples.Count;
        double ci_lo = posteriorSamples[(int)(n * 0.025)];
        double ci_hi = posteriorSamples[(int)(n * 0.975)];

        return new MCMCInversionResult
        {
            MeanMgNumber = mean,
            StdMgNumber = std,
            CI95_Low = ci_lo,
            CI95_High = ci_hi,
            AcceptanceRate = (double)chain.Accepted / chain.Total,
            Chain = chain,
            GridSearchResult = gridResult,
        };
    }

    private static MineralParams InterpolateMineral(MineralParams mg, MineralParams fe, double xMg, double xFe)
    {
        return new MineralParams
        {
            MineralName = $"Interp_Mg{xMg:F2}",
            PaperName = "interp",
            NumAtoms = mg.NumAtoms,
            MolarVolume = xMg * mg.MolarVolume + xFe * fe.MolarVolume,
            MolarWeight = xMg * mg.MolarWeight + xFe * fe.MolarWeight,
            KZero = xMg * mg.KZero + xFe * fe.KZero,
            K1Prime = xMg * mg.K1Prime + xFe * fe.K1Prime,
            K2Prime = xMg * mg.K2Prime + xFe * fe.K2Prime,
            GZero = xMg * mg.GZero + xFe * fe.GZero,
            G1Prime = xMg * mg.G1Prime + xFe * fe.G1Prime,
            G2Prime = xMg * mg.G2Prime + xFe * fe.G2Prime,
            DebyeTempZero = xMg * mg.DebyeTempZero + xFe * fe.DebyeTempZero,
            GammaZero = xMg * mg.GammaZero + xFe * fe.GammaZero,
            QZero = xMg * mg.QZero + xFe * fe.QZero,
            EhtaZero = xMg * mg.EhtaZero + xFe * fe.EhtaZero,
            RefTemp = mg.RefTemp,
            F0 = xMg * mg.F0 + xFe * fe.F0,
        };
    }
}

/// <summary>
/// Result of MCMC-based composition inversion.
/// </summary>
public class MCMCInversionResult
{
    public double MeanMgNumber { get; set; }
    public double StdMgNumber { get; set; }
    public double CI95_Low { get; set; }
    public double CI95_High { get; set; }
    public double AcceptanceRate { get; set; }
    public MCMCChain Chain { get; set; } = new();
    public InversionResult GridSearchResult { get; set; } = new();
}
