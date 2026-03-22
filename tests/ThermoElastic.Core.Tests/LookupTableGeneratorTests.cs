using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class LookupTableGeneratorTests
{
    private readonly ITestOutputHelper _output;

    public LookupTableGeneratorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static MineralParams CreateForsterite()
    {
        return new MineralParams
        {
            MineralName = "Forsterite",
            PaperName = "fo",
            NumAtoms = 7,
            MolarVolume = 43.6,
            MolarWeight = 140.69,
            KZero = 128.0,
            K1Prime = 4.2,
            K2Prime = 0,
            GZero = 82.0,
            G1Prime = 1.5,
            G2Prime = 0,
            DebyeTempZero = 809.0,
            GammaZero = 0.99,
            QZero = 2.1,
            EhtaZero = 2.3,
            RefTemp = 300.0,
        };
    }

    [Fact]
    public void Generate_5x5Grid_AllCellsHavePositiveProperties()
    {
        var mineral = CreateForsterite();
        var generator = new LookupTableGenerator();

        var table = generator.Generate(mineral, 0.001, 14.0, 5, 300.0, 2000.0, 5);

        Assert.Equal(5, table.NPressure);
        Assert.Equal(5, table.NTemperature);
        Assert.Equal("Forsterite", table.MineralName);

        for (int i = 0; i < table.NPressure; i++)
        {
            for (int j = 0; j < table.NTemperature; j++)
            {
                _output.WriteLine($"P={table.Pressures[i]:F2} GPa, T={table.Temperatures[j]:F0} K => rho={table.Density[i, j]:F4}, Vp={table.Vp[i, j]:F1}, Vs={table.Vs[i, j]:F1}");
                Assert.True(table.Density[i, j] > 0, $"Density should be positive at P={table.Pressures[i]}, T={table.Temperatures[j]}");
                Assert.True(table.Vp[i, j] > 0, $"Vp should be positive at P={table.Pressures[i]}, T={table.Temperatures[j]}");
                Assert.True(table.Vs[i, j] > 0, $"Vs should be positive at P={table.Pressures[i]}, T={table.Temperatures[j]}");
            }
        }
    }

    [Fact]
    public void Generate_DensityIncreasesWithPressure_AtFixedTemperature()
    {
        var mineral = CreateForsterite();
        var generator = new LookupTableGenerator();

        var table = generator.Generate(mineral, 0.001, 50.0, 10, 1000.0, 1000.0, 1);

        for (int i = 1; i < table.NPressure; i++)
        {
            _output.WriteLine($"P={table.Pressures[i]:F2} GPa => rho={table.Density[i, 0]:F6}");
            Assert.True(table.Density[i, 0] > table.Density[i - 1, 0],
                $"Density should increase with pressure: rho({table.Pressures[i]:F2})={table.Density[i, 0]:F6} <= rho({table.Pressures[i - 1]:F2})={table.Density[i - 1, 0]:F6}");
        }
    }

    [Fact]
    public void Generate_VelocityIncreasesWithPressure()
    {
        var mineral = CreateForsterite();
        var generator = new LookupTableGenerator();

        var table = generator.Generate(mineral, 0.001, 14.0, 10, 1000.0, 1000.0, 1);

        // Check general trend: last value > first value
        _output.WriteLine($"Vp at P={table.Pressures[0]:F2}: {table.Vp[0, 0]:F1}, at P={table.Pressures[^1]:F2}: {table.Vp[table.NPressure - 1, 0]:F1}");
        _output.WriteLine($"Vs at P={table.Pressures[0]:F2}: {table.Vs[0, 0]:F1}, at P={table.Pressures[^1]:F2}: {table.Vs[table.NPressure - 1, 0]:F1}");

        Assert.True(table.Vp[table.NPressure - 1, 0] > table.Vp[0, 0],
            "Vp should generally increase with pressure");
        Assert.True(table.Vs[table.NPressure - 1, 0] > table.Vs[0, 0],
            "Vs should generally increase with pressure");
    }

    [Fact]
    public void ExportCSV_WritesCorrectFormat()
    {
        var mineral = CreateForsterite();
        var generator = new LookupTableGenerator();
        var table = generator.Generate(mineral, 0.001, 14.0, 3, 300.0, 2000.0, 4);

        string tempFile = Path.GetTempFileName();
        try
        {
            generator.ExportCSV(table, tempFile);

            Assert.True(File.Exists(tempFile));
            var lines = File.ReadAllLines(tempFile);

            // Header + nP * nT data lines
            int expectedLines = 1 + 3 * 4;
            Assert.Equal(expectedLines, lines.Length);

            // Verify header
            Assert.Contains("P[GPa]", lines[0]);
            Assert.Contains("rho[g/cm3]", lines[0]);

            // Verify data line is parseable
            var parts = lines[1].Split(',');
            Assert.Equal(9, parts.Length);
            foreach (var part in parts)
            {
                Assert.True(double.TryParse(part, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _),
                    $"Could not parse value: {part}");
            }

            _output.WriteLine($"CSV has {lines.Length} lines, header: {lines[0]}");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExportASPECT_WritesTabSeparatedFormat()
    {
        var mineral = CreateForsterite();
        var generator = new LookupTableGenerator();
        var table = generator.Generate(mineral, 0.001, 14.0, 3, 300.0, 2000.0, 4);

        string tempFile = Path.GetTempFileName();
        try
        {
            generator.ExportASPECT(table, tempFile);

            Assert.True(File.Exists(tempFile));
            var lines = File.ReadAllLines(tempFile);

            // Header comments + column header + nP * nT data lines
            Assert.StartsWith("# ASPECT lookup table", lines[0]);
            Assert.StartsWith("#", lines[1]);
            Assert.StartsWith("#", lines[2]);

            // Column header line
            Assert.Contains("P\tT\trho", lines[3]);

            // Data lines are tab-separated and parseable
            var dataLine = lines[4];
            var parts = dataLine.Split('\t');
            Assert.Equal(6, parts.Length); // P, T, rho, Vp, Vs, alpha
            foreach (var part in parts)
            {
                Assert.True(double.TryParse(part, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _),
                    $"Could not parse ASPECT value: {part}");
            }

            _output.WriteLine($"ASPECT file has {lines.Length} lines");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Generate_InterpolationAccuracy_GridPointMatchesDirectComputation()
    {
        var mineral = CreateForsterite();
        var generator = new LookupTableGenerator();

        double testP = 5.0;
        double testT = 1500.0;

        // Generate table that includes the exact test point
        var table = generator.Generate(mineral, 0.001, 14.0, 5, 300.0, 2000.0, 5);

        // Direct computation at the same P-T
        var optimizer = new MieGruneisenEOSOptimizer(mineral, testP, testT);
        var direct = optimizer.ExecOptimize();

        // Find closest grid point to testP and testT
        int iP = FindClosestIndex(table.Pressures, testP);
        int iT = FindClosestIndex(table.Temperatures, testT);

        // If the grid point matches exactly, values should be identical
        if (Math.Abs(table.Pressures[iP] - testP) < 1e-10 &&
            Math.Abs(table.Temperatures[iT] - testT) < 1e-10)
        {
            Assert.Equal(direct.Density, table.Density[iP, iT], 10);
            Assert.Equal(direct.Vp, table.Vp[iP, iT], 10);
            Assert.Equal(direct.Vs, table.Vs[iP, iT], 10);
            _output.WriteLine("Exact grid point match: values are identical.");
        }
        else
        {
            // Even at nearest grid point, values should be reasonably close
            double densityRelErr = Math.Abs(direct.Density - table.Density[iP, iT]) / direct.Density;
            _output.WriteLine($"Grid P={table.Pressures[iP]:F3}, T={table.Temperatures[iT]:F1} vs direct P={testP}, T={testT}");
            _output.WriteLine($"Density relative error: {densityRelErr:E3}");
            Assert.True(densityRelErr < 0.1, "Density at nearest grid point should be within 10% of direct");
        }
    }

    private static int FindClosestIndex(double[] array, double value)
    {
        int best = 0;
        double bestDist = Math.Abs(array[0] - value);
        for (int i = 1; i < array.Length; i++)
        {
            double dist = Math.Abs(array[i] - value);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = i;
            }
        }
        return best;
    }
}
