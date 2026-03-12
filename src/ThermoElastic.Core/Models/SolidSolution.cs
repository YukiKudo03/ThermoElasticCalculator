using System.Text.Json;

namespace ThermoElastic.Core.Models;

/// <summary>
/// Represents a solid solution with multiple endmembers, crystallographic sites, and interaction parameters.
/// </summary>
public class SolidSolution
{
    public string Name { get; set; } = string.Empty;
    public List<MineralParams> Endmembers { get; set; } = new();
    public List<SolutionSite> Sites { get; set; } = new();
    public List<InteractionParam> InteractionParams { get; set; } = new();

    public string ExportJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    public static bool ImportJson(string jsonString, out SolidSolution? ret)
    {
        ret = null;
        try
        {
            ret = JsonSerializer.Deserialize<SolidSolution>(jsonString);
            return ret != null;
        }
        catch
        {
            return false;
        }
    }
}
