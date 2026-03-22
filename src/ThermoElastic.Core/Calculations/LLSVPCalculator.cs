using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Calculator for LLSVP composition and structure interpretation.
/// Evaluates thermal vs compositional origins of velocity anomalies in the deep mantle.
/// </summary>
public class LLSVPCalculator
{
    /// <summary>
    /// Compute the temperature anomaly required to produce a given dVs reduction
    /// for a mineral at specified P-T conditions (pure thermal origin).
    /// </summary>
    /// <param name="mineral">Mineral parameters</param>
    /// <param name="P">Pressure [GPa]</param>
    /// <param name="T_reference">Reference temperature [K]</param>
    /// <param name="targetDlnVs">Target dln(Vs) (negative, e.g., -0.02 for -2%)</param>
    /// <returns>Required temperature anomaly DeltaT [K]</returns>
    public double ComputeRequiredDeltaT(MineralParams mineral, double P, double T_reference, double targetDlnVs)
    {
        // Compute Vs at reference temperature
        var refResult = new MieGruneisenEOSOptimizer(mineral, P, T_reference).ExecOptimize();
        double vsRef = refResult.Vs;
        double lnVsRef = Math.Log(vsRef);

        // Bisection: find DeltaT such that ln(Vs(T_ref + DeltaT)) - ln(Vs(T_ref)) = targetDlnVs
        double dtLow = 0.0;
        double dtHigh = 2000.0;

        // Evaluate at boundaries to confirm sign change
        double dlnVsHigh = ComputeDlnVs(mineral, P, T_reference, dtHigh, lnVsRef);

        // If the target is not reachable within [0, 2000], extend or return boundary
        if (targetDlnVs < dlnVsHigh)
            return dtHigh;

        for (int i = 0; i < 100; i++)
        {
            double dtMid = (dtLow + dtHigh) / 2.0;
            double dlnVsMid = ComputeDlnVs(mineral, P, T_reference, dtMid, lnVsRef);

            if (Math.Abs(dlnVsMid - targetDlnVs) < 1e-6)
                return dtMid;

            // dlnVs becomes more negative as DeltaT increases
            if (dlnVsMid > targetDlnVs)
                dtLow = dtMid;
            else
                dtHigh = dtMid;
        }

        return (dtLow + dtHigh) / 2.0;
    }

    /// <summary>
    /// Compare properties of reference vs anomalous assemblage at given P-T.
    /// </summary>
    public (ThermoMineralParams reference, ThermoMineralParams anomalous, double dlnVp, double dlnVs, double dlnRho)
        CompareAssemblages(MineralParams referenceMineral, MineralParams anomalousMineral, double P, double T)
    {
        var refResult = new MieGruneisenEOSOptimizer(referenceMineral, P, T).ExecOptimize();
        var anomResult = new MieGruneisenEOSOptimizer(anomalousMineral, P, T).ExecOptimize();

        double dlnVp = Math.Log(anomResult.Vp / refResult.Vp);
        double dlnVs = Math.Log(anomResult.Vs / refResult.Vs);
        double dlnRho = Math.Log(anomResult.Density / refResult.Density);

        return (refResult, anomResult, dlnVp, dlnVs, dlnRho);
    }

    /// <summary>
    /// Compute mineral properties at CMB-like conditions.
    /// </summary>
    public ThermoMineralParams ComputeAtCMB(MineralParams mineral, double P = 135.0, double T = 3800.0)
    {
        return new MieGruneisenEOSOptimizer(mineral, P, T).ExecOptimize();
    }

    private static double ComputeDlnVs(MineralParams mineral, double P, double T_reference, double deltaT, double lnVsRef)
    {
        var result = new MieGruneisenEOSOptimizer(mineral, P, T_reference + deltaT).ExecOptimize();
        return Math.Log(result.Vs) - lnVsRef;
    }
}
