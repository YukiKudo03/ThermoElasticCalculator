using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Checks thermodynamic self-consistency of the EOS framework
/// using Maxwell relations and other thermodynamic identities.
/// </summary>
public class ThermodynamicVerifier
{
    /// <summary>
    /// Runs all thermodynamic consistency checks at a single P-T point.
    /// </summary>
    public VerificationResult Verify(MineralParams mineral, double P, double T)
    {
        var result = new MieGruneisenEOSOptimizer(mineral, P, T).ExecOptimize();

        return new VerificationResult
        {
            MaxwellResidual = CheckMaxwellRelation(mineral, P, T, result),
            GibbsHelmholtzResidual = CheckGibbsHelmholtz(result),
            EntropyResidual = CheckEntropyConsistency(mineral, P, T, result),
            BulkModulusResidual = CheckBulkModulus(mineral, P, T, result),
            KsKtResidual = CheckKsKtIdentity(result),
        };
    }

    /// <summary>
    /// Runs all checks over a P-T grid.
    /// </summary>
    public List<VerificationResult> VerifyOverGrid(MineralParams mineral, double[] pressures, double[] temperatures)
    {
        var results = new List<VerificationResult>();
        foreach (var P in pressures)
        {
            foreach (var T in temperatures)
            {
                results.Add(Verify(mineral, P, T));
            }
        }
        return results;
    }

    /// <summary>
    /// Maxwell relation: (dS/dP)_T = -(dV/dT)_P.
    /// Returns |(dS/dP)_T + (dV/dT)_P| normalized by scale.
    /// </summary>
    private double CheckMaxwellRelation(MineralParams mineral, double P, double T, ThermoMineralParams center)
    {
        double dP = 0.1; // GPa
        double dT = 1.0; // K

        // (dS/dP)_T via central difference
        var rPlus = new MieGruneisenEOSOptimizer(mineral, P + dP, T).ExecOptimize();
        var rMinus = new MieGruneisenEOSOptimizer(mineral, P - dP, T).ExecOptimize();
        double dSdP = (rPlus.Entropy - rMinus.Entropy) / (2.0 * dP); // J/mol/K / GPa

        // (dV/dT)_P via central difference
        var rTPlus = new MieGruneisenEOSOptimizer(mineral, P, T + dT).ExecOptimize();
        var rTMinus = new MieGruneisenEOSOptimizer(mineral, P, T - dT).ExecOptimize();
        double dVdT = (rTPlus.Volume - rTMinus.Volume) / (2.0 * dT); // cm3/mol / K

        // Maxwell: dS/dP = -dV/dT, but units: dV/dT in cm3/mol/K = kJ/mol/K/GPa = 1000 J/mol/K/GPa
        // So convert dV/dT to same units as dS/dP: multiply by 1000
        double dVdT_converted = dVdT * 1000.0; // J/mol/K / GPa

        double residual = Math.Abs(dSdP + dVdT_converted);
        double scale = Math.Max(Math.Abs(dSdP), Math.Abs(dVdT_converted));
        if (scale < 1e-15) return 0.0;

        return residual / scale;
    }

    /// <summary>
    /// Gibbs-Helmholtz identity: G = F + PV. Returns |G - (F+PV)| in kJ/mol.
    /// </summary>
    private double CheckGibbsHelmholtz(ThermoMineralParams result)
    {
        double gExpected = result.HelmholtzF + result.Pressure * result.Volume;
        return Math.Abs(result.GibbsG - gExpected);
    }

    /// <summary>
    /// Entropy consistency: S = -dF/dT at fixed finite strain. Returns relative error.
    /// Uses same approach as ThermoMineralParams.Entropy (fixed finite strain, vary T).
    /// </summary>
    private double CheckEntropyConsistency(MineralParams mineral, double P, double T, ThermoMineralParams center)
    {
        double dT = 1.0;
        double f = center.Finite;

        // Compute F at T±dT with fixed finite strain (matching ThermoMineralParams.Entropy)
        var thPlus = new ThermoMineralParams(f, T + dT, mineral);
        var thMinus = new ThermoMineralParams(f, T - dT, mineral);

        // S_numerical = -dF/dT * 1000 (kJ -> J)
        double sNumerical = -(thPlus.HelmholtzF - thMinus.HelmholtzF) / (2.0 * dT) * 1000.0;
        double sComputed = center.Entropy;

        if (Math.Abs(sComputed) < 1e-10) return 0.0;
        return Math.Abs(sNumerical - sComputed) / Math.Abs(sComputed);
    }

    /// <summary>
    /// Bulk modulus consistency: KT = -V * (dP/dV)_T. Returns relative error.
    /// Uses finite strain perturbation to vary volume at constant T.
    /// </summary>
    private double CheckBulkModulus(MineralParams mineral, double P, double T, ThermoMineralParams center)
    {
        double f = center.Finite;
        double df = 1e-6;

        // Perturb finite strain to get V±dV and corresponding P
        // Use four-point stencil for better accuracy: f'(x) ≈ [-f(x+2h)+8f(x+h)-8f(x-h)+f(x-2h)]/(12h)
        var th1 = new ThermoMineralParams(f + df, T, mineral);
        var th2 = new ThermoMineralParams(f - df, T, mineral);
        var th3 = new ThermoMineralParams(f + 2 * df, T, mineral);
        var th4 = new ThermoMineralParams(f - 2 * df, T, mineral);

        double dPdf = (-th3.Pressure + 8 * th1.Pressure - 8 * th2.Pressure + th4.Pressure) / (12 * df);
        double dVdf = (-th3.Volume + 8 * th1.Volume - 8 * th2.Volume + th4.Volume) / (12 * df);

        double ktNumerical = -center.Volume * (dPdf / dVdf);
        double ktComputed = center.KT;

        if (Math.Abs(ktComputed) < 1e-10) return 0.0;
        return Math.Abs(ktNumerical - ktComputed) / Math.Abs(ktComputed);
    }

    /// <summary>
    /// KS-KT identity: KS = KT * (1 + alpha * gamma * T). Returns relative error.
    /// </summary>
    private double CheckKsKtIdentity(ThermoMineralParams result)
    {
        double ksExpected = result.KT * (1.0 + result.Alpha * result.Gamma * result.Temperature);
        double ksComputed = result.KS;

        if (Math.Abs(ksComputed) < 1e-10) return 0.0;
        return Math.Abs(ksExpected - ksComputed) / Math.Abs(ksComputed);
    }
}
