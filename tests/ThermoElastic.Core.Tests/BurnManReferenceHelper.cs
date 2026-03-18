using System.Globalization;

namespace ThermoElastic.Core.Tests;

/// <summary>
/// Helper to load BurnMan reference CSV data for verification tests.
/// </summary>
public static class BurnManReferenceHelper
{
    public record EndmemberRecord(
        string PaperName, double P_GPa, double T_K,
        double Rho_kg_m3, double V_m3_mol,
        double KS_GPa, double KT_GPa, double G_GPa,
        double Vp_m_s, double Vs_m_s, double Vb_m_s,
        double Alpha_1_K, double Cv_J_mol_K, double Cp_J_mol_K,
        double Gamma, double F_J_mol, double Gibbs_J_mol, double Entropy_J_mol_K);

    public record SolutionRecord(
        string SolutionName, string Composition, double P_GPa, double T_K,
        double Rho_kg_m3, double KS_GPa, double G_GPa,
        double Vp_m_s, double Vs_m_s,
        double Gibbs_J_mol, double Entropy_J_mol_K, double ExcessGibbs_J_mol);

    public record DebyeRecord(double X, double D3);

    private static string GetTestDataPath(string filename)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", filename);
    }

    public static List<EndmemberRecord> LoadEndmemberReference()
    {
        var records = new List<EndmemberRecord>();
        var lines = File.ReadAllLines(GetTestDataPath("burnman_endmember_reference.csv"));
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var f = lines[i].Split(',');
            records.Add(new EndmemberRecord(
                f[0].Trim(),
                double.Parse(f[1].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[2].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[3].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[4].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[5].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[6].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[7].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[8].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[9].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[10].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[11].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[12].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[13].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[14].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[15].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[16].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[17].Trim(), CultureInfo.InvariantCulture)
            ));
        }
        return records;
    }

    public static List<SolutionRecord> LoadSolutionReference()
    {
        var records = new List<SolutionRecord>();
        var lines = File.ReadAllLines(GetTestDataPath("burnman_solution_reference.csv"));
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var f = lines[i].Split(',');
            records.Add(new SolutionRecord(
                f[0].Trim(), f[1].Trim(),
                double.Parse(f[2].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[3].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[4].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[5].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[6].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[7].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[8].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[9].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[10].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[11].Trim(), CultureInfo.InvariantCulture)
            ));
        }
        return records;
    }

    public static List<DebyeRecord> LoadDebyeReference()
    {
        var records = new List<DebyeRecord>();
        var lines = File.ReadAllLines(GetTestDataPath("burnman_debye_reference.csv"));
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var f = lines[i].Split(',');
            records.Add(new DebyeRecord(
                double.Parse(f[0].Trim(), CultureInfo.InvariantCulture),
                double.Parse(f[1].Trim(), CultureInfo.InvariantCulture)
            ));
        }
        return records;
    }

    /// <summary>
    /// Asserts that the actual value is within the given relative tolerance of the expected value.
    /// </summary>
    public static void AssertRelativeEqual(double expected, double actual, double tolerancePercent, string label)
    {
        if (Math.Abs(expected) < 1e-15)
        {
            Xunit.Assert.True(Math.Abs(actual) < 1e-10, $"{label}: expected ~0, got {actual}");
            return;
        }
        double relError = Math.Abs((actual - expected) / expected) * 100.0;
        Xunit.Assert.True(relError <= tolerancePercent,
            $"{label}: expected {expected:G8}, got {actual:G8}, relative error {relError:F4}% > tolerance {tolerancePercent}%");
    }
}
