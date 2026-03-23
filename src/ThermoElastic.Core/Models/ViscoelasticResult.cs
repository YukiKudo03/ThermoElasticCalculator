namespace ThermoElastic.Core.Models;

/// <summary>
/// Extended result from viscoelastic models (Andrade/ExtendedBurgers).
/// Includes complex compliance J*(ω) in addition to standard AnelasticResult fields.
/// </summary>
public record ViscoelasticResult : AnelasticResult
{
    /// <summary>Real part of complex compliance Re[J*(ω)] [Pa⁻¹]</summary>
    public double J_real { get; init; }
    /// <summary>Imaginary part of complex compliance Im[J*(ω)] [Pa⁻¹]</summary>
    public double J_imag { get; init; }
    /// <summary>Unrelaxed shear modulus [GPa]</summary>
    public double G_unrelaxed { get; init; }
    /// <summary>Relaxed (effective) shear modulus [GPa]</summary>
    public double G_relaxed { get; init; }
}
