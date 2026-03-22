namespace ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

/// <summary>
/// Single-crystal elastic constants at ambient conditions.
/// References: Abramson et al. (1997), Zha et al. (2000), Sinogeikin &amp; Bass (2000).
/// </summary>
public static class SingleCrystalElasticConstants
{
    /// <summary>Get elastic tensor for a mineral by paper name. Returns null if not available.</summary>
    public static ElasticTensor? GetTensor(string paperName)
    {
        return paperName switch
        {
            "fo" => Forsterite(),
            "pe" => Periclase(),
            _ => null,
        };
    }

    /// <summary>Forsterite (Mg2SiO4) at ambient. Abramson et al. (1997).</summary>
    public static ElasticTensor Forsterite()
    {
        var t = new ElasticTensor { MineralName = "Forsterite", Density = 3.222 };
        // Orthorhombic: 9 independent constants
        t.C[0,0] = 328.1; t.C[1,1] = 199.5; t.C[2,2] = 235.4;
        t.C[0,1] = 66.2;  t.C[0,2] = 68.2;  t.C[1,2] = 73.2;
        t.C[3,3] = 66.7;  t.C[4,4] = 81.3;  t.C[5,5] = 79.4;
        // Symmetric
        t.C[1,0] = t.C[0,1]; t.C[2,0] = t.C[0,2]; t.C[2,1] = t.C[1,2];
        return t;
    }

    /// <summary>Periclase (MgO) at ambient. Zha et al. (2000).</summary>
    public static ElasticTensor Periclase()
    {
        var t = new ElasticTensor { MineralName = "Periclase", Density = 3.584 };
        // Cubic: 3 independent constants
        t.C[0,0] = 297.0; t.C[1,1] = 297.0; t.C[2,2] = 297.0;
        t.C[0,1] = 95.2;  t.C[0,2] = 95.2;  t.C[1,2] = 95.2;
        t.C[3,3] = 155.7; t.C[4,4] = 155.7; t.C[5,5] = 155.7;
        t.C[1,0] = t.C[0,1]; t.C[2,0] = t.C[0,2]; t.C[2,1] = t.C[1,2];
        return t;
    }
}
