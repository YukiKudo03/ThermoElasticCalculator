using System.Text.Json;

namespace ThermoElastic.Core.Models;

public class RockMineralEntry
{
    public MineralParams Mineral { get; set; } = new();
    public double VolumeRatio { get; set; }
}

public class RockComposition
{
    public RockComposition()
    {
        Minerals = new List<RockMineralEntry>();
    }

    public string Name { get; set; } = string.Empty;
    public List<RockMineralEntry> Minerals { get; set; }

    public double TotalRatio => Minerals.Sum(m => m.VolumeRatio);

    public string ExportJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    public static bool ImportJson(string jsonString, out RockComposition? ret)
    {
        ret = null;
        try
        {
            ret = JsonSerializer.Deserialize<RockComposition>(jsonString);
            return ret != null;
        }
        catch
        {
            return false;
        }
    }
}

public enum MixtureMethod
{
    Hill = 0,
    Voigt = 1,
    Reuss = 2,
    HS = 3,
}
