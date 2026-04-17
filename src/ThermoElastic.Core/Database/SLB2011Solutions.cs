namespace ThermoElastic.Core.Database;

/// <summary>
/// Interaction parameters for solid solutions.
/// SLB2011 Table A2 + SLB2024 Table C2 updates.
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

            // Ferropericlase — SLB2024 updated W values (Table C2)
            new("Ferropericlase", "pe", "wu", 44.0),        // Updated from 13.0 (SLB2011)
            new("Ferropericlase", "pe", "wuls", -87.1),      // SLB2024: favorable HS-LS interaction
            new("Ferropericlase", "pe", "mag", 303.0),       // SLB2024
            new("Ferropericlase", "wu", "wuls", -60.0),      // SLB2024
            new("Ferropericlase", "wu", "mag", 120.0),       // SLB2024
            new("Ferropericlase", "wuls", "mag", 120.0),     // SLB2024

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

            // Perovskite (Bridgmanite) — SLB2024 extended with Fe3+ (Table C2)
            new("Perovskite", "mpv", "fpv", 0.0),
            new("Perovskite", "mpv", "apv", 0.0),
            new("Perovskite", "fpv", "apv", 0.0),
            new("Perovskite", "mpv", "hebg", 93.9),          // SLB2024
            new("Perovskite", "mpv", "hlbg", 49.9),          // SLB2024
            new("Perovskite", "mpv", "fabg", 31.7),          // SLB2024 (albg-like)
            new("Perovskite", "mpv", "crpv", 93.0),          // SLB2024
            new("Perovskite", "fpv", "hebg", 93.9),          // SLB2024
            new("Perovskite", "fpv", "hlbg", 49.9),          // SLB2024
            new("Perovskite", "fpv", "fabg", 65.0),          // SLB2024
            new("Perovskite", "fpv", "crpv", 93.0),          // SLB2024
            new("Perovskite", "hebg", "hlbg", -5.9),         // SLB2024: negative = favorable
            new("Perovskite", "hebg", "fabg", 65.0),         // SLB2024
            new("Perovskite", "hebg", "crpv", 65.0),         // SLB2024
            new("Perovskite", "hlbg", "fabg", 40.0),         // SLB2024
            new("Perovskite", "hlbg", "crpv", 40.0),         // SLB2024
            new("Perovskite", "apv", "hebg", 65.0),          // SLB2024
            new("Perovskite", "apv", "hlbg", 65.0),          // SLB2024
            new("Perovskite", "apv", "fabg", 65.0),          // SLB2024
            new("Perovskite", "apv", "crpv", 40.0),          // SLB2024
            new("Perovskite", "fabg", "crpv", 40.0),         // SLB2024

            // Post-Perovskite — SLB2024 extended
            new("PostPerovskite", "mppv", "fppv", 0.0),
            new("PostPerovskite", "mppv", "appv", 0.0),
            new("PostPerovskite", "fppv", "appv", 0.0),
            new("PostPerovskite", "mppv", "hppv", 93.9),     // SLB2024
            new("PostPerovskite", "mppv", "cppv", 93.0),     // SLB2024
            new("PostPerovskite", "fppv", "hppv", 93.9),     // SLB2024
            new("PostPerovskite", "fppv", "cppv", 93.0),     // SLB2024
            new("PostPerovskite", "appv", "hppv", 65.0),     // SLB2024
            new("PostPerovskite", "appv", "cppv", 40.0),     // SLB2024
            new("PostPerovskite", "hppv", "cppv", 65.0),     // SLB2024

            // CaFerrite-type
            new("CaFerrite", "mcf", "fcf", 0.0),
            new("CaFerrite", "mcf", "ncf", 0.0),
            new("CaFerrite", "fcf", "ncf", 0.0),
        };
    }
}
