using System.Text;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.IO;

public static class MineralCsvIO
{
    public static List<MineralParams> Import(string filePath)
    {
        return MineralParams.ImportCsvFile(filePath);
    }

    public static void Export(string filePath, List<MineralParams> minerals)
    {
        MineralParams.ExportCsvFile(filePath, minerals);
    }

    public static void ExportResults(string filePath, List<ResultSummary> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine(ResultSummary.ColumnsCSV);
        foreach (var r in results)
        {
            sb.AppendLine(r.ExportSummaryAsColumn());
        }
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }
}
