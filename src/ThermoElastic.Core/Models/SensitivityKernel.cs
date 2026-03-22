namespace ThermoElastic.Core.Models;

/// <summary>
/// Sensitivity of seismic velocities to temperature and composition changes.
/// </summary>
public record SensitivityKernel
{
    /// <summary>∂ln(Vp)/∂T [1/K]</summary>
    public double DlnVp_dT { get; init; }
    /// <summary>∂ln(Vs)/∂T [1/K]</summary>
    public double DlnVs_dT { get; init; }
    /// <summary>∂ln(ρ)/∂T [1/K]</summary>
    public double DlnRho_dT { get; init; }
    /// <summary>R ratio = ∂ln(Vs)/∂ln(Vp) for thermal anomalies</summary>
    public double R_thermal { get; init; }
    /// <summary>Pressure [GPa]</summary>
    public double Pressure { get; init; }
    /// <summary>Temperature [K]</summary>
    public double Temperature { get; init; }
}
