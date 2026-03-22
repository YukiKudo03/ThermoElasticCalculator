using System.Globalization;
using System.Text;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Generates pre-computed P-T lookup tables for mantle convection simulations.
/// </summary>
public class LookupTableGenerator
{
    /// <summary>
    /// Generate a lookup table for a single mineral over a P-T grid.
    /// </summary>
    public LookupTable Generate(MineralParams mineral,
        double pMin, double pMax, int nP,
        double tMin, double tMax, int nT)
    {
        var table = new LookupTable
        {
            MineralName = mineral.MineralName,
            Pressures = LinSpace(pMin, pMax, nP),
            Temperatures = LinSpace(tMin, tMax, nT),
            Density = new double[nP, nT],
            Vp = new double[nP, nT],
            Vs = new double[nP, nT],
            KS = new double[nP, nT],
            GS = new double[nP, nT],
            Alpha = new double[nP, nT],
            Gamma = new double[nP, nT],
        };

        for (int i = 0; i < nP; i++)
        {
            for (int j = 0; j < nT; j++)
            {
                var optimizer = new MieGruneisenEOSOptimizer(mineral, table.Pressures[i], table.Temperatures[j]);
                var result = optimizer.ExecOptimize();

                table.Density[i, j] = result.Density;
                table.Vp[i, j] = result.Vp;
                table.Vs[i, j] = result.Vs;
                table.KS[i, j] = result.KS;
                table.GS[i, j] = result.GS;
                table.Alpha[i, j] = result.Alpha;
                table.Gamma[i, j] = result.Gamma;
            }
        }

        return table;
    }

    /// <summary>
    /// Export table in CSV format.
    /// Columns: P[GPa], T[K], rho[g/cm3], Vp[m/s], Vs[m/s], KS[GPa], GS[GPa], alpha[1/K], gamma
    /// </summary>
    public void ExportCSV(LookupTable table, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("P[GPa],T[K],rho[g/cm3],Vp[m/s],Vs[m/s],KS[GPa],GS[GPa],alpha[1/K],gamma");

        for (int i = 0; i < table.NPressure; i++)
        {
            for (int j = 0; j < table.NTemperature; j++)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0:G},{1:G},{2:G6},{3:G6},{4:G6},{5:G6},{6:G6},{7:E4},{8:G6}",
                    table.Pressures[i], table.Temperatures[j],
                    table.Density[i, j], table.Vp[i, j], table.Vs[i, j],
                    table.KS[i, j], table.GS[i, j],
                    table.Alpha[i, j], table.Gamma[i, j]));
            }
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    /// Export in ASPECT-compatible format (tab-separated).
    /// </summary>
    public void ExportASPECT(LookupTable table, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# ASPECT lookup table");
        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "# {0} {1}", table.NPressure, table.NTemperature));
        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "# {0} {1} {2} {3}",
            table.Pressures[0], table.Pressures[^1],
            table.Temperatures[0], table.Temperatures[^1]));
        sb.AppendLine("P\tT\trho\tVp\tVs\talpha");

        for (int i = 0; i < table.NPressure; i++)
        {
            for (int j = 0; j < table.NTemperature; j++)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0:G}\t{1:G}\t{2:G6}\t{3:G6}\t{4:G6}\t{5:E4}",
                    table.Pressures[i], table.Temperatures[j],
                    table.Density[i, j], table.Vp[i, j], table.Vs[i, j],
                    table.Alpha[i, j]));
            }
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    private static double[] LinSpace(double start, double end, int count)
    {
        if (count < 2) return new[] { start };
        var result = new double[count];
        double step = (end - start) / (count - 1);
        for (int i = 0; i < count; i++)
        {
            result[i] = start + i * step;
        }
        return result;
    }
}
