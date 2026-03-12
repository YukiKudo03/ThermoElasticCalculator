using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Database;

public static class MineralDatabase
{
    private static readonly Lazy<List<MineralParams>> _minerals =
        new(() => SLB2011Endmembers.GetAll());

    public static List<MineralParams> GetAll() => _minerals.Value;

    public static MineralParams? GetByName(string name)
    {
        return _minerals.Value.FirstOrDefault(m =>
            m.MineralName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            m.PaperName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static List<MineralParams> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return GetAll();
        return _minerals.Value.Where(m =>
            m.MineralName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            m.PaperName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
