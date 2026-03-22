namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Calculates the iron spin crossover (high-spin to low-spin) transition
/// in ferropericlase and its effects on elastic properties.
/// </summary>
public class SpinCrossoverCalculator
{
    /// <summary>
    /// Compute equilibrium low-spin fraction and effective properties.
    /// </summary>
    /// <param name="hsMineral">High-spin endmember (e.g., wustite "wu")</param>
    /// <param name="lsMineral">Low-spin endmember (approximated with modified parameters)</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <returns>(n_LS, V_eff, KS_eff, GS_eff, Vp_eff, Vs_eff)</returns>
    public (double nLS, double Volume, double KS, double GS, double Vp, double Vs)
        ComputeSpinState(MineralParams hsMineral, MineralParams lsMineral, double P, double T)
    {
        // Compute properties for both spin states
        var hsResult = new MieGruneisenEOSOptimizer(hsMineral, P, T).ExecOptimize();
        var lsResult = new MieGruneisenEOSOptimizer(lsMineral, P, T).ExecOptimize();

        // Electronic degeneracies: Fe2+ HS has S=2 -> g=2S+1=5, LS has S=0 -> g=1
        const double g_HS = 5.0;
        const double g_LS = 1.0;

        double G_HS = hsResult.GibbsG;
        double G_LS = lsResult.GibbsG;

        // Minimize F_mix(n_LS) = n_LS*G_LS + (1-n_LS)*G_HS - T*S_config(n_LS)
        // S_config = -R * [n_LS*ln(n_LS/g_LS) + (1-n_LS)*ln((1-n_LS)/g_HS)]
        // Use grid search over n_LS in [0, 1]
        double R = PhysicConstants.GasConst;
        double bestNLS = 0.0;
        double bestF = double.MaxValue;

        int nSteps = 1000;
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

        // Refine with finer grid around best
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

        // Effective properties: linear interpolation
        double V_eff = bestNLS * lsResult.Volume + (1.0 - bestNLS) * hsResult.Volume;
        double KS_eff = bestNLS * lsResult.KS + (1.0 - bestNLS) * hsResult.KS;
        double GS_eff = bestNLS * lsResult.GS + (1.0 - bestNLS) * hsResult.GS;
        double rho_eff = bestNLS * lsResult.Density + (1.0 - bestNLS) * hsResult.Density;

        double Vp_eff = 1000.0 * Math.Sqrt((KS_eff + 4.0 / 3.0 * GS_eff) / rho_eff);
        double Vs_eff = 1000.0 * Math.Sqrt(GS_eff / rho_eff);

        return (bestNLS, V_eff, KS_eff, GS_eff, Vp_eff, Vs_eff);
    }

    /// <summary>
    /// Create approximate low-spin endmember parameters from high-spin.
    /// LS has: smaller V0 (~-8%), higher K0 (~+15%), lower G0 (~-5%), lower Debye T (~-5%).
    /// Based on Lin et al. (2013) and Wentzcovitch et al. (2009).
    /// </summary>
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
            SpinQuantumNumber = 0.0, // LS has S=0
            MagneticAtomCount = hsMineral.MagneticAtomCount,
        };
    }

    private static double ComputeFMix(double n, double G_HS, double G_LS, double T, double R, double g_HS, double g_LS)
    {
        double fBase = n * G_LS + (1.0 - n) * G_HS;

        // Configurational entropy: S_config = -R * [n*ln(n/g_LS) + (1-n)*ln((1-n)/g_HS)]
        double sConfig = 0.0;
        if (n > 1e-12 && n < 1.0 - 1e-12)
        {
            sConfig = -R * (n * Math.Log(n / g_LS) + (1.0 - n) * Math.Log((1.0 - n) / g_HS));
        }
        else if (n <= 1e-12)
        {
            // n ~ 0: only (1-n)*ln((1-n)/g_HS) term matters
            sConfig = -R * Math.Log(1.0 / g_HS);
        }
        else
        {
            // n ~ 1: only n*ln(n/g_LS) term matters
            sConfig = -R * Math.Log(1.0 / g_LS);
        }

        return fBase - T * sConfig / 1000.0; // Convert S from J/mol/K to kJ/mol/K to match G in kJ/mol
    }
}
