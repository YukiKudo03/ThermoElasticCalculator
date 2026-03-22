namespace ThermoElastic.Core.Models;

/// <summary>
/// Result of a mantle composition inversion.
/// </summary>
public record InversionResult
{
    /// <summary>Best-fit Mg# (Mg/(Mg+Fe) molar ratio)</summary>
    public double BestMgNumber { get; init; }
    /// <summary>Minimum misfit value</summary>
    public double MinMisfit { get; init; }
    /// <summary>All evaluated (Mg#, misfit) pairs</summary>
    public List<(double MgNumber, double Misfit)> MisfitProfile { get; init; } = new();
}
