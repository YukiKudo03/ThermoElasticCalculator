namespace ThermoElastic.Core.Models;

/// <summary>
/// Interaction parameter for a pair of endmembers in a solid solution (van Laar model).
/// </summary>
public class InteractionParam
{
    public int EndmemberA { get; set; }
    public int EndmemberB { get; set; }
    /// <summary>Interaction energy W [kJ/mol]</summary>
    public double W { get; set; }
    /// <summary>Size parameter for endmember A (default 1.0)</summary>
    public double SizeA { get; set; } = 1.0;
    /// <summary>Size parameter for endmember B (default 1.0)</summary>
    public double SizeB { get; set; } = 1.0;
}
