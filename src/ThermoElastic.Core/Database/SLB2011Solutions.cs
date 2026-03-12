namespace ThermoElastic.Core.Database;

/// <summary>
/// SLB2011 Table A2 interaction parameters for solid solutions.
/// W values in kJ/mol.
/// </summary>
public static class SLB2011Solutions
{
    public record InteractionEntry(string SolutionName, string EndmemberA, string EndmemberB, double W_kJ);

    public static List<InteractionEntry> GetAll()
    {
        return new List<InteractionEntry>
        {
            // Plagioclase
            new("Plagioclase", "an", "ab", 26.0),

            // Olivine
            new("Olivine", "fo", "fa", 7.6),

            // Wadsleyite
            new("Wadsleyite", "mw", "fw", 16.5),

            // Ringwoodite
            new("Ringwoodite", "mrw", "frw", 9.1),

            // Ferropericlase
            new("Ferropericlase", "pe", "wu", 13.0),

            // Spinel
            new("Spinel", "sp", "hc", 5.0),

            // Clinopyroxene
            new("Clinopyroxene", "di", "he", 24.0),
            new("Clinopyroxene", "di", "cats", 26.0),
            new("Clinopyroxene", "di", "jd", 24.0),
            new("Clinopyroxene", "he", "cats", 61.0),
            new("Clinopyroxene", "he", "jd", 24.0),
            new("Clinopyroxene", "cats", "jd", 10.0),

            // Orthopyroxene
            new("Orthopyroxene", "en", "fs", 0.0),
            new("Orthopyroxene", "en", "mgts", 48.0),
            new("Orthopyroxene", "fs", "mgts", 32.0),

            // Garnet
            new("Garnet", "py", "al", 0.0),
            new("Garnet", "py", "gr", 21.0),
            new("Garnet", "py", "maj", 0.0),
            new("Garnet", "al", "gr", 4.0),
            new("Garnet", "al", "maj", 0.0),
            new("Garnet", "gr", "maj", 58.0),

            // Akimotoite
            new("Akimotoite", "mak", "fak", 0.0),
            new("Akimotoite", "mak", "cor", 66.0),
            new("Akimotoite", "fak", "cor", 116.0),

            // Perovskite (Bridgmanite)
            new("Perovskite", "mpv", "fpv", 0.0),
            new("Perovskite", "mpv", "apv", 0.0),
            new("Perovskite", "fpv", "apv", 0.0),

            // Post-Perovskite
            new("PostPerovskite", "mppv", "fppv", 0.0),
            new("PostPerovskite", "mppv", "appv", 0.0),
            new("PostPerovskite", "fppv", "appv", 0.0),

            // CaFerrite-type
            new("CaFerrite", "mcf", "fcf", 0.0),
            new("CaFerrite", "mcf", "ncf", 0.0),
            new("CaFerrite", "fcf", "ncf", 0.0),
        };
    }
}
