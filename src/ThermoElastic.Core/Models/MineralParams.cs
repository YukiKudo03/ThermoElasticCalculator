using System.Text;
using System.Text.Json;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Core.Models;

public class MineralParams
{
    public MineralParams() { }

    public string MineralName { get; set; } = string.Empty;
    public string PaperName { get; set; } = string.Empty;
    public int NumAtoms { get; set; }
    public double MolarVolume { get; set; }
    public double MolarWeight { get; set; }

    public double AveMolarWeight => MolarWeight / (double)NumAtoms;

    /// <summary>Bulk Modulus [GPa]</summary>
    public double KZero { get; set; }
    /// <summary>Pressure Derivative of Bulk Modulus</summary>
    public double K1Prime { get; set; }
    /// <summary>Pressure Derivative of K1Prime</summary>
    public double K2Prime { get; set; }
    /// <summary>Shear Modulus [GPa]</summary>
    public double GZero { get; set; }
    /// <summary>Pressure Derivative of Shear Modulus</summary>
    public double G1Prime { get; set; }
    /// <summary>Pressure Derivative of G1Prime</summary>
    public double G2Prime { get; set; }
    /// <summary>Initial Debye Temperature [K]</summary>
    public double DebyeTempZero { get; set; }
    /// <summary>Gruneisen parameter</summary>
    public double GammaZero { get; set; }
    /// <summary>q0 value</summary>
    public double QZero { get; set; }
    /// <summary>etaS0 value</summary>
    public double EhtaZero { get; set; }
    /// <summary>Reference Temperature [K]</summary>
    public double RefTemp { get; set; } = 300.0d;

    public double Aii => 6.0d * GammaZero;

    public double Aiikk => -12.0d * GammaZero + 36.0 * GammaZero * GammaZero - 18.0d * GammaZero * QZero;

    public double As => -2.0d * GammaZero - 2.0d * EhtaZero;

    public double GetPressure(double finite)
    {
        var term3 = Math.Pow(1.0d + 2.0d * finite, 5.0d / 2.0d);
        var term5 = 1.0d + 3.0d * (K1Prime - 4.0d) * finite / 2.0d;
        return 3.0d * KZero * term3 * finite * term5;
    }

    public double FiniteToVolume(double finite)
    {
        return MolarVolume / Math.Pow(2.0d * finite + 1.0d, 3.0d / 2.0d);
    }

    public double VolumeToFinite(double volume)
    {
        return (Math.Pow(MolarVolume / volume, -2.0d / 3.0d) - 1.0d) / 2.0d;
    }

    public double BM3Finite(double targetPressure, OptimizerType optimizerType = OptimizerType.ReglaFalsi)
    {
        Func<double, double> func = (double finite) => GetPressure(finite) - targetPressure;
        var opt = OptimizerFactory.CreateOptimizer(func, 0.0005, 0.02, optimizerType);
        return opt.DoOptimize();
    }

    public double BM3KT(double finite)
    {
        var term1 = Math.Pow(1.0d + 2.0d * finite, 5.0d / 2.0d);
        var term2 = (3.0d * K1Prime - 5.0d) * finite;
        var term3 = 27.0d / 2.0d * (K1Prime - 4.0d) * finite * finite;
        return term1 * KZero * (1.0d + term2 + term3);
    }

    public double BM3GT(double finite)
    {
        var term1 = Math.Pow(1.0d + 2.0d * finite, 5.0d / 2.0d);
        var term2 = (3.0d * KZero * G1Prime - 5.0d * GZero) * finite;
        var term3 = (6.0 * KZero * G1Prime - 24.0d * KZero - 14.0d * GZero + 9.0d / 2.0d * KZero * K1Prime) * finite * finite;
        return term1 * (GZero + term1 + term2 + term3);
    }

    public string ParamSymbol => $"{MineralName} ({PaperName})";

    public string ExportJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    public static bool ImportJson(string jsonString, out MineralParams? ret)
    {
        ret = null;
        try
        {
            ret = JsonSerializer.Deserialize<MineralParams>(jsonString);
            return ret != null;
        }
        catch
        {
            return false;
        }
    }

    public static string CsvHeader =>
        "MineralName,PaperName,NumAtoms,MolarVolume,MolarWeight,KZero,K1Prime,K2Prime,GZero,G1Prime,G2Prime,DebyeTempZero,GammaZero,QZero,EhtaZero,RefTemp";

    public string ExportCsvRow()
    {
        return $"{MineralName},{PaperName},{NumAtoms},{MolarVolume},{MolarWeight},{KZero},{K1Prime},{K2Prime},{GZero},{G1Prime},{G2Prime},{DebyeTempZero},{GammaZero},{QZero},{EhtaZero},{RefTemp}";
    }

    public static bool ImportCsvRow(string csvRow, out MineralParams? ret)
    {
        ret = null;
        try
        {
            var fields = csvRow.Split(',');
            if (fields.Length < 16) return false;

            ret = new MineralParams
            {
                MineralName = fields[0].Trim(),
                PaperName = fields[1].Trim(),
                NumAtoms = int.Parse(fields[2].Trim()),
                MolarVolume = double.Parse(fields[3].Trim()),
                MolarWeight = double.Parse(fields[4].Trim()),
                KZero = double.Parse(fields[5].Trim()),
                K1Prime = double.Parse(fields[6].Trim()),
                K2Prime = double.Parse(fields[7].Trim()),
                GZero = double.Parse(fields[8].Trim()),
                G1Prime = double.Parse(fields[9].Trim()),
                G2Prime = double.Parse(fields[10].Trim()),
                DebyeTempZero = double.Parse(fields[11].Trim()),
                GammaZero = double.Parse(fields[12].Trim()),
                QZero = double.Parse(fields[13].Trim()),
                EhtaZero = double.Parse(fields[14].Trim()),
                RefTemp = double.Parse(fields[15].Trim()),
            };
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static List<MineralParams> ImportCsvFile(string filePath)
    {
        var results = new List<MineralParams>();
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            if (ImportCsvRow(lines[i], out MineralParams? mineral) && mineral != null)
            {
                results.Add(mineral);
            }
        }
        return results;
    }

    public static void ExportCsvFile(string filePath, List<MineralParams> minerals)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CsvHeader);
        foreach (var m in minerals)
        {
            sb.AppendLine(m.ExportCsvRow());
        }
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }
}
