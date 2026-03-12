namespace ThermoElastic.Core.Models;

/// <summary>
/// Represents a crystallographic site in a solid solution.
/// </summary>
public class SolutionSite
{
    public string SiteName { get; set; } = string.Empty;
    public double Multiplicity { get; set; } = 1.0;

    /// <summary>
    /// Occupancy of each endmember on this site.
    /// Key = endmember index, Value = fraction of this site occupied by that endmember's species.
    /// For simple A-B solutions on one site, endmember 0 has occupancy x, endmember 1 has occupancy (1-x).
    /// </summary>
    public Dictionary<int, double[]> Occupancies { get; set; } = new();
}
