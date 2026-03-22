namespace ThermoElastic.Core.Models;

/// <summary>
/// Result of thermodynamic identity verification at a single P-T point.
/// </summary>
public record VerificationResult
{
    /// <summary>|(dS/dP)_T + (dV/dT)_P| — should be near zero (Maxwell relation)</summary>
    public double MaxwellResidual { get; init; }
    /// <summary>|G - (F + PV)| — should be exactly zero</summary>
    public double GibbsHelmholtzResidual { get; init; }
    /// <summary>|S_numerical + dF/dT| relative error</summary>
    public double EntropyResidual { get; init; }
    /// <summary>|KT_computed - (-V * dP/dV)| relative error</summary>
    public double BulkModulusResidual { get; init; }
    /// <summary>|KS - KT*(1+α*γ*T)| relative error</summary>
    public double KsKtResidual { get; init; }
    /// <summary>Whether all residuals are below threshold</summary>
    public bool IsValid => MaxwellResidual < 0.02 && GibbsHelmholtzResidual < 1e-6
        && EntropyResidual < 0.05 && BulkModulusResidual < 0.03 && KsKtResidual < 0.01;
}
