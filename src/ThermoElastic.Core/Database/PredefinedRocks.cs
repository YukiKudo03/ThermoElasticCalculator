using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Database;

/// <summary>
/// Predefined rock compositions for common mantle lithologies.
/// Volume fractions are based on typical upper mantle estimates
/// from Stixrude &amp; Lithgow-Bertelloni (2011) and Ringwood (1991).
/// </summary>
public static class PredefinedRocks
{
    /// <summary>
    /// Pyrolite: model upper mantle composition (Ringwood, 1962).
    /// ~60% olivine, ~15% orthopyroxene, ~15% clinopyroxene, ~10% garnet by volume.
    /// </summary>
    public static RockComposition Pyrolite()
    {
        return new RockComposition
        {
            Name = "Pyrolite",
            Minerals = new List<RockMineralEntry>
            {
                new() { Mineral = MineralDatabase.GetByName("Forsterite")!, VolumeRatio = 0.60 },
                new() { Mineral = MineralDatabase.GetByName("Enstatite")!, VolumeRatio = 0.15 },
                new() { Mineral = MineralDatabase.GetByName("Diopside")!, VolumeRatio = 0.15 },
                new() { Mineral = MineralDatabase.GetByName("Pyrope")!, VolumeRatio = 0.10 },
            },
        };
    }

    /// <summary>
    /// Harzburgite: depleted mantle residue after basalt extraction.
    /// ~80% olivine, ~20% orthopyroxene.
    /// </summary>
    public static RockComposition Harzburgite()
    {
        return new RockComposition
        {
            Name = "Harzburgite",
            Minerals = new List<RockMineralEntry>
            {
                new() { Mineral = MineralDatabase.GetByName("Forsterite")!, VolumeRatio = 0.80 },
                new() { Mineral = MineralDatabase.GetByName("Enstatite")!, VolumeRatio = 0.20 },
            },
        };
    }

    /// <summary>
    /// MORB (Mid-Ocean Ridge Basalt) composition in the mantle transition zone.
    /// Dominated by garnet and clinopyroxene (eclogite facies).
    /// </summary>
    public static RockComposition MORB()
    {
        return new RockComposition
        {
            Name = "MORB",
            Minerals = new List<RockMineralEntry>
            {
                new() { Mineral = MineralDatabase.GetByName("Pyrope")!, VolumeRatio = 0.45 },
                new() { Mineral = MineralDatabase.GetByName("Diopside")!, VolumeRatio = 0.30 },
                new() { Mineral = MineralDatabase.GetByName("Stishovite")!, VolumeRatio = 0.15 },
                new() { Mineral = MineralDatabase.GetByName("Jadeite")!, VolumeRatio = 0.10 },
            },
        };
    }

    /// <summary>
    /// Lower mantle peridotite: bridgmanite-dominated assemblage.
    /// ~75% bridgmanite, ~17% ferropericlase, ~8% Ca-perovskite.
    /// </summary>
    public static RockComposition LowerMantlePeridotite()
    {
        return new RockComposition
        {
            Name = "Lower Mantle Peridotite",
            Minerals = new List<RockMineralEntry>
            {
                new() { Mineral = MineralDatabase.GetByName("Mg-Perovskite")!, VolumeRatio = 0.75 },
                new() { Mineral = MineralDatabase.GetByName("Periclase")!, VolumeRatio = 0.17 },
                new() { Mineral = MineralDatabase.GetByName("Ca-Perovskite")!, VolumeRatio = 0.08 },
            },
        };
    }

    /// <summary>
    /// Get all predefined rock compositions.
    /// </summary>
    public static List<RockComposition> GetAll()
    {
        return new List<RockComposition>
        {
            Pyrolite(),
            Harzburgite(),
            MORB(),
            LowerMantlePeridotite(),
        };
    }
}
