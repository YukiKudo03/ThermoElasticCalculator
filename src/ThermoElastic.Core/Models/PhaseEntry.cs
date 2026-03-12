namespace ThermoElastic.Core.Models;

/// <summary>
/// A phase (endmember or solid solution) with its amount and composition in an assemblage.
/// </summary>
public class PhaseEntry
{
    /// <summary>Name of the phase</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Endmember parameters (for pure phase or effective params of solution)</summary>
    public MineralParams Mineral { get; set; } = new();

    /// <summary>Solid solution definition (null for pure endmember phases)</summary>
    public SolidSolution? Solution { get; set; }

    /// <summary>Composition within the solid solution (mole fractions of endmembers)</summary>
    public double[]? Composition { get; set; }

    /// <summary>Molar amount of this phase in the assemblage</summary>
    public double Amount { get; set; }

    /// <summary>Gibbs free energy of this phase at current P,T [kJ/mol]</summary>
    public double GibbsG { get; set; }

    public bool IsSolidSolution => Solution != null;
}
