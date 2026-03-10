using System.Text.Json;

namespace ThermoElastic.Core.Models;

public class PTProfile
{
    public PTProfile() { }

    public string Name { get; set; } = string.Empty;
    public List<PTData> Profile { get; set; } = new();

    public string ExportJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    public static bool ImportJson(string jsonString, out PTProfile? ret)
    {
        ret = null;
        try
        {
            ret = JsonSerializer.Deserialize<PTProfile>(jsonString);
            return ret != null;
        }
        catch
        {
            return false;
        }
    }
}
