namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Calculates the iron spin crossover (high-spin to low-spin) transition.
/// Supports Fe2+ (ferropericlase: wu/wuls) and Fe3+ (bridgmanite: hebg/hlbg).
/// SLB2024: uses explicit LS endmembers instead of approximate scaling.
/// </summary>
public class SpinCrossoverCalculator
{
    /// <summary>
    /// Compute equilibrium low-spin fraction and effective properties
    /// for a general HS/LS pair with specified degeneracies.
    /// </summary>
    public (double nLS, double Volume, double KS, double GS, double Vp, double Vs, double Density)
        ComputeSpinState(MineralParams hsMineral, MineralParams lsMineral, double P, double T,
                         double g_HS, double g_LS)
    {
        var hsResult = new MieGruneisenEOSOptimizer(hsMineral, P, T).ExecOptimize();
        var lsResult = new MieGruneisenEOSOptimizer(lsMineral, P, T).ExecOptimize();

        double R = PhysicConstants.GasConst;
        double G_HS = hsResult.GibbsG;
        double G_LS = lsResult.GibbsG;

        // Two-stage grid search for optimal n_LS
        double bestNLS = MinimizeLSFraction(G_HS, G_LS, T, R, g_HS, g_LS);

        // Effective properties via linear interpolation
        double V_eff = bestNLS * lsResult.Volume + (1.0 - bestNLS) * hsResult.Volume;
        double KS_eff = bestNLS * lsResult.KS + (1.0 - bestNLS) * hsResult.KS;
        double GS_eff = bestNLS * lsResult.GS + (1.0 - bestNLS) * hsResult.GS;
        double rho_eff = bestNLS * lsResult.Density + (1.0 - bestNLS) * hsResult.Density;

        double Vp_eff = 1000.0 * Math.Sqrt((KS_eff + 4.0 / 3.0 * GS_eff) / rho_eff);
        double Vs_eff = 1000.0 * Math.Sqrt(GS_eff / rho_eff);

        return (bestNLS, V_eff, KS_eff, GS_eff, Vp_eff, Vs_eff, rho_eff);
    }

    /// <summary>
    /// Fe2+ spin crossover in ferropericlase using explicit wuls endmember (SLB2024).
    /// g_HS = 5 (S=2, 2S+1=5), g_LS = 1 (S=0, 2S+1=1).
    /// </summary>
    public (double nLS, double Volume, double KS, double GS, double Vp, double Vs, double Density)
        ComputeFe2SpinState(MineralParams wu, MineralParams wuls, double P, double T)
    {
        return ComputeSpinState(wu, wuls, P, T, g_HS: 5.0, g_LS: 1.0);
    }

    /// <summary>
    /// Fe3+ spin crossover in bridgmanite B-site (hebg → hlbg).
    /// g_HS = 6 (S=5/2, 2S+1=6), g_LS = 2 (S=1/2, 2S+1=2).
    /// </summary>
    public (double nLS, double Volume, double KS, double GS, double Vp, double Vs, double Density)
        ComputeFe3SpinState(MineralParams hebg, MineralParams hlbg, double P, double T)
    {
        return ComputeSpinState(hebg, hlbg, P, T, g_HS: 6.0, g_LS: 2.0);
    }

    /// <summary>
    /// Legacy method: compute spin state with old (HS, LS) signature.
    /// Kept for backward compatibility with existing callers.
    /// Uses Fe2+ degeneracies (g_HS=5, g_LS=1) by default.
    /// </summary>
    public (double nLS, double Volume, double KS, double GS, double Vp, double Vs)
        ComputeSpinState(MineralParams hsMineral, MineralParams lsMineral, double P, double T)
    {
        var result = ComputeSpinState(hsMineral, lsMineral, P, T, g_HS: 5.0, g_LS: 1.0);
        return (result.nLS, result.Volume, result.KS, result.GS, result.Vp, result.Vs);
    }

    /// <summary>
    /// Create approximate low-spin endmember parameters from high-spin.
    /// DEPRECATED: Use explicit wuls/hlbg endmembers from SLB2024 database instead.
    /// Kept for backward compatibility only.
    /// </summary>
    [Obsolete("Use explicit LS endmembers from SLB2024 database (wuls, hlbg) instead.")]
    public static MineralParams CreateLSEndmember(MineralParams hsMineral)
    {
        return new MineralParams
        {
            MineralName = hsMineral.MineralName + "_LS",
            PaperName = hsMineral.PaperName + "_ls",
            NumAtoms = hsMineral.NumAtoms,
            MolarVolume = hsMineral.MolarVolume * 0.92,
            MolarWeight = hsMineral.MolarWeight,
            KZero = hsMineral.KZero * 1.15,
            K1Prime = hsMineral.K1Prime,
            K2Prime = hsMineral.K2Prime,
            GZero = hsMineral.GZero * 0.95,
            G1Prime = hsMineral.G1Prime,
            G2Prime = hsMineral.G2Prime,
            DebyeTempZero = hsMineral.DebyeTempZero * 0.95,
            GammaZero = hsMineral.GammaZero,
            QZero = hsMineral.QZero,
            EhtaZero = hsMineral.EhtaZero,
            RefTemp = hsMineral.RefTemp,
            F0 = hsMineral.F0,
            Tc0 = hsMineral.Tc0,
            VD = hsMineral.VD,
            SD = hsMineral.SD,
            SpinQuantumNumber = 0.0,
            MagneticAtomCount = hsMineral.MagneticAtomCount,
        };
    }

    private static double MinimizeLSFraction(double G_HS, double G_LS, double T, double R,
                                              double g_HS, double g_LS)
    {
        double bestNLS = 0.0;
        double bestF = double.MaxValue;

        // Coarse grid search
        const int nSteps = 1000;
        for (int i = 0; i <= nSteps; i++)
        {
            double n = i / (double)nSteps;
            double fMix = ComputeFMix(n, G_HS, G_LS, T, R, g_HS, g_LS);
            if (fMix < bestF)
            {
                bestF = fMix;
                bestNLS = n;
            }
        }

        // Refine around best
        double lo = Math.Max(0.0, bestNLS - 0.002);
        double hi = Math.Min(1.0, bestNLS + 0.002);
        for (int i = 0; i <= 1000; i++)
        {
            double n = lo + (hi - lo) * i / 1000.0;
            double fMix = ComputeFMix(n, G_HS, G_LS, T, R, g_HS, g_LS);
            if (fMix < bestF)
            {
                bestF = fMix;
                bestNLS = n;
            }
        }

        return bestNLS;
    }

    private static double ComputeFMix(double n, double G_HS, double G_LS, double T, double R,
                                       double g_HS, double g_LS)
    {
        double fBase = n * G_LS + (1.0 - n) * G_HS;

        double sConfig = 0.0;
        if (n > 1e-12 && n < 1.0 - 1e-12)
        {
            sConfig = -R * (n * Math.Log(n / g_LS) + (1.0 - n) * Math.Log((1.0 - n) / g_HS));
        }
        else if (n <= 1e-12)
        {
            sConfig = -R * Math.Log(1.0 / g_HS);
        }
        else
        {
            sConfig = -R * Math.Log(1.0 / g_LS);
        }

        return fBase - T * sConfig / 1000.0; // J → kJ
    }
}
