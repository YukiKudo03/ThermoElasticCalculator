using System.Text.Json;

namespace ThermoElastic.Core.Models;

public class ResultSummary
{
    public ResultSummary() { }

    public string Name { get; set; } = string.Empty;
    public double GivenP { get; set; }
    public double GivenT { get; set; }
    public double KS { get; set; }
    public double KT { get; set; }
    public double GS { get; set; }
    public double Volume { get; set; }
    public double Density { get; set; }
    public double DebyeTemp { get; set; }
    public double Gamma { get; set; }
    public double Alpha { get; set; }
    public double EthaS { get; set; }
    public double Q { get; set; }
    public double HelmholtzF { get; set; }
    public double GibbsG { get; set; }
    public double Entropy { get; set; }

    public double Vb => 1000.0d * Math.Sqrt(KS / Density);
    public double Vs => 1000.0d * Math.Sqrt(GS / Density);
    public double Vp => 1000.0d * Math.Sqrt((KS + 4.0d / 3.0d * GS) / Density);

    public static string ColumnsCSV =>
        "P[GPa], T[K], Vp[m/s], Vs[m/s], Vb[m/s], ρ[g/cm3], V[cm3/mol], KS[GPa], KT[GPa], GS[GPa], α[K-1], θd[K], γ, ηs, q";

    public static List<string> ColumnStrs =>
        new() { "P[GPa]", "T[K]", "Vp[m/s]", "Vs[m/s]", "Vb[m/s]", "ρ[g/cm3]", "V[cm3/mol]", "KS[GPa]", "KT[GPa]", "GS[GPa]", "α[K-1]", "θd[K]", "γ", "ηs", "q" };

    public string ExportSummaryAsColumn()
    {
        return $"{GivenP}, {GivenT}, {Vp}, {Vs}, {Vb}, {Density}, {Volume}, {KS}, {KT}, {GS}, {Alpha}, {DebyeTemp}, {Gamma}, {EthaS}, {Q}";
    }

    public string ExportSummaryAsJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}
