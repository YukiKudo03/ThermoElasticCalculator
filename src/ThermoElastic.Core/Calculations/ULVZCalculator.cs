namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Calculates elastic properties of ULVZ candidate materials.
/// Supports partial melt + solid mixtures using Hashin-Shtrikman bounds.
/// </summary>
public class ULVZCalculator
{
    /// <summary>
    /// Compute properties of a solid-melt mixture at given P-T.
    /// Uses Reuss (lower) bound for mixing since melt has G=0.
    /// </summary>
    /// <param name="solidMineral">Solid mineral parameters</param>
    /// <param name="meltFraction">Volume fraction of melt (0 to 1)</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="melt">Melt parameters (optional, defaults to silicate melt)</param>
    /// <returns>(Vp, Vs, density, dVp_percent, dVs_percent vs pure solid)</returns>
    public (double Vp, double Vs, double Density, double dVp_pct, double dVs_pct)
        ComputeMeltMixture(MineralParams solidMineral, double meltFraction, double P, double T, MeltParams? melt = null)
    {
        melt ??= new MeltParams();

        // Compute solid properties at P, T
        var solidResult = new MieGruneisenEOSOptimizer(solidMineral, P, T).ExecOptimize();
        double solidVp = solidResult.Vp;
        double solidVs = solidResult.Vs;
        double solidDensity = solidResult.Density;
        double solidKS = solidResult.KS;
        double solidGS = solidResult.GS;

        if (meltFraction <= 0.0)
        {
            return (solidVp, solidVs, solidDensity, 0.0, 0.0);
        }

        // Compute melt density at P using BM3 EOS (bisection)
        double meltDensity = SolveMeltDensity(melt, P);

        // Melt bulk modulus at P: K = K0 * (1 + 2f)^(5/2) * (1 + (3K1-5)*f)
        // where f = ((rho/rho0)^(2/3) - 1) / 2
        double fMelt = (Math.Pow(meltDensity / melt.Rho0, 2.0 / 3.0) - 1.0) / 2.0;
        double meltK = melt.K0 * Math.Pow(1.0 + 2.0 * fMelt, 5.0 / 2.0)
            * (1.0 + (3.0 * melt.K1 - 5.0) * fMelt);

        double phi = meltFraction;

        // Reuss lower bound for bulk modulus
        double K_mix = 1.0 / (phi / meltK + (1.0 - phi) / solidKS);

        // Shear modulus: G_melt = 0, so simple mixing
        double G_mix = (1.0 - phi) * solidGS;

        // Density mixing
        double rho_mix = phi * meltDensity + (1.0 - phi) * solidDensity;

        // Velocities
        double Vp_mix = 1000.0 * Math.Sqrt((K_mix + 4.0 / 3.0 * G_mix) / rho_mix);
        double Vs_mix = 1000.0 * Math.Sqrt(G_mix / rho_mix);

        double dVp_pct = (Vp_mix - solidVp) / solidVp * 100.0;
        double dVs_pct = (Vs_mix - solidVs) / solidVs * 100.0;

        return (Vp_mix, Vs_mix, rho_mix, dVp_pct, dVs_pct);
    }

    /// <summary>
    /// Solve for melt density at given pressure using BM3 EOS and bisection.
    /// </summary>
    private static double SolveMeltDensity(MeltParams melt, double P)
    {
        // Bisection: find rho such that P_BM3(rho) = P
        double rhoLo = melt.Rho0;
        double rhoHi = melt.Rho0 * 3.0; // generous upper bound

        for (int i = 0; i < 200; i++)
        {
            double rhoMid = (rhoLo + rhoHi) / 2.0;
            double pMid = MeltPressure(melt, rhoMid);
            if (Math.Abs(pMid - P) < 1e-6)
                return rhoMid;
            if (pMid < P)
                rhoLo = rhoMid;
            else
                rhoHi = rhoMid;
        }
        return (rhoLo + rhoHi) / 2.0;
    }

    /// <summary>
    /// BM3 pressure for melt given density.
    /// f = ((rho/rho0)^(2/3) - 1) / 2
    /// P = 3*K0*f*(1+2f)^(5/2) * (1 + 3/2*(K1-4)*f)
    /// </summary>
    private static double MeltPressure(MeltParams melt, double rho)
    {
        double f = (Math.Pow(rho / melt.Rho0, 2.0 / 3.0) - 1.0) / 2.0;
        return 3.0 * melt.K0 * f * Math.Pow(1.0 + 2.0 * f, 5.0 / 2.0)
            * (1.0 + 1.5 * (melt.K1 - 4.0) * f);
    }
}
