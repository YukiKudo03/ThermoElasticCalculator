namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Solves Fe-Mg partitioning between bridgmanite and ferropericlase
/// by minimizing total Gibbs free energy subject to mass balance.
/// </summary>
public class IronPartitioningSolver
{
    private const double R = 8.314; // J/mol/K

    /// <summary>
    /// Solve Fe-Mg partitioning between perovskite and ferropericlase.
    /// </summary>
    /// <param name="mgPv">Mg-perovskite endmember</param>
    /// <param name="fePv">Fe-perovskite endmember</param>
    /// <param name="mgFp">Periclase endmember</param>
    /// <param name="feFp">Wustite endmember</param>
    /// <param name="bulkXFe">Bulk iron mole fraction</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="molFractionPv">Molar fraction of perovskite in the assemblage (default 0.5)</param>
    /// <returns>(X_Fe in perovskite, X_Fe in ferropericlase, K_D)</returns>
    /// <remarks>
    /// Assumes 1:1 molar ratio of perovskite to ferropericlase,
    /// appropriate for the Mg2SiO4 → MgSiO3 + MgO decomposition.
    /// Mass balance: bulkXFe = 0.5 * XFe_pv + 0.5 * XFe_fp,
    /// hence XFe_fp = 2 * bulkXFe - XFe_pv.
    /// </remarks>
    public (double XFe_pv, double XFe_fp, double KD) SolvePartitioning(
        MineralParams mgPv, MineralParams fePv, MineralParams mgFp, MineralParams feFp,
        double bulkXFe, double P, double T, double molFractionPv = 0.5)
    {
        // Compute endmember Gibbs energies via EOS
        double gMgPv = ComputeGibbs(mgPv, P, T);
        double gFePv = ComputeGibbs(fePv, P, T);
        double gMgFp = ComputeGibbs(mgFp, P, T);
        double gFeFp = ComputeGibbs(feFp, P, T);

        // 1D grid search over X_Fe_pv to minimize total Gibbs
        int nSteps = 1000;
        double bestG = double.MaxValue;
        double bestXFePv = 0.001;

        double xMin = 0.001;
        double xMax = (bulkXFe - 0.001 * (1.0 - molFractionPv)) / molFractionPv; // from mass balance constraint
        xMax = Math.Min(xMax, 0.999);
        if (xMax <= xMin) xMax = xMin + 0.001;

        for (int i = 0; i <= nSteps; i++)
        {
            double xFePv = xMin + (xMax - xMin) * i / nSteps;
            double xFeFp = (bulkXFe - molFractionPv * xFePv) / (1.0 - molFractionPv);

            if (xFeFp <= 0.001 || xFeFp >= 0.999) continue;

            double totalG = ComputeTotalG(xFePv, xFeFp, gMgPv, gFePv, gMgFp, gFeFp, T);

            if (totalG < bestG)
            {
                bestG = totalG;
                bestXFePv = xFePv;
            }
        }

        // Refine with a second pass around the best value
        double refineLow = Math.Max(xMin, bestXFePv - (xMax - xMin) / nSteps * 2);
        double refineHigh = Math.Min(xMax, bestXFePv + (xMax - xMin) / nSteps * 2);

        for (int i = 0; i <= nSteps; i++)
        {
            double xFePv = refineLow + (refineHigh - refineLow) * i / nSteps;
            double xFeFp = (bulkXFe - molFractionPv * xFePv) / (1.0 - molFractionPv);

            if (xFeFp <= 0.001 || xFeFp >= 0.999) continue;

            double totalG = ComputeTotalG(xFePv, xFeFp, gMgPv, gFePv, gMgFp, gFeFp, T);

            if (totalG < bestG)
            {
                bestG = totalG;
                bestXFePv = xFePv;
            }
        }

        double bestXFeFp = (bulkXFe - molFractionPv * bestXFePv) / (1.0 - molFractionPv);
        double kd = (bestXFePv * (1.0 - bestXFeFp)) / ((1.0 - bestXFePv) * bestXFeFp);

        return (bestXFePv, bestXFeFp, kd);
    }

    private static double ComputeTotalG(double xFePv, double xFeFp,
        double gMgPv, double gFePv, double gMgFp, double gFeFp, double T)
    {
        double xMgPv = 1.0 - xFePv;
        double xMgFp = 1.0 - xFeFp;

        // Mechanical mixing + ideal mixing entropy for perovskite
        double gPv = xMgPv * gMgPv + xFePv * gFePv
            + R * T / 1000.0 * (xMgPv * Math.Log(xMgPv) + xFePv * Math.Log(xFePv));

        // Mechanical mixing + ideal mixing entropy for ferropericlase
        double gFp = xMgFp * gMgFp + xFeFp * gFeFp
            + R * T / 1000.0 * (xMgFp * Math.Log(xMgFp) + xFeFp * Math.Log(xFeFp));

        return gPv + gFp;
    }

    private static double ComputeGibbs(MineralParams mineral, double P, double T)
    {
        var optimizer = new MieGruneisenEOSOptimizer(mineral, Math.Max(P, 0.0001), T);
        var thermo = optimizer.ExecOptimize();
        return thermo.GibbsG;
    }
}
