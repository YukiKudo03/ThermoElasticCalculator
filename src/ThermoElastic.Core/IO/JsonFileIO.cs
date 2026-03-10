using System.Text;
using System.Text.Json;

namespace ThermoElastic.Core.IO;

public static class JsonFileIO
{
    public static T? Load<T>(string filePath) where T : class
    {
        var json = File.ReadAllText(filePath, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(json);
    }

    public static void Save<T>(string filePath, T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json, Encoding.UTF8);
    }
}
