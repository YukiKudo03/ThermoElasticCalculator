namespace ThermoElastic.Core.Models;

/// <summary>
/// A collection of phases (endmembers and/or solid solutions) representing a mineral assemblage.
/// </summary>
public class PhaseAssemblage
{
    public List<PhaseEntry> Phases { get; set; } = new();

    /// <summary>Bulk composition in terms of oxide components (e.g., "MgO" → moles)</summary>
    public Dictionary<string, double> BulkComposition { get; set; } = new();

    /// <summary>Total Gibbs free energy of the assemblage [kJ]</summary>
    public double TotalGibbs => Phases.Sum(p => p.Amount * p.GibbsG);

    /// <summary>Pressure at which this assemblage was computed [GPa]</summary>
    public double Pressure { get; set; }

    /// <summary>Temperature at which this assemblage was computed [K]</summary>
    public double Temperature { get; set; }
}
