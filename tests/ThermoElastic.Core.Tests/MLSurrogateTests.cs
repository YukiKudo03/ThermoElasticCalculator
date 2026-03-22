using Xunit;
using Xunit.Abstractions;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Tests;

public class MLSurrogateTests
{
    private readonly ITestOutputHelper _output;

    public MLSurrogateTests(ITestOutputHelper output)
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
            MolarVolume = 43.603,
            MolarWeight = 140.69,
            KZero = 127.96,
            K1Prime = 4.2180,
            GZero = 81.60,
            G1Prime = 1.4626,
            DebyeTempZero = 809.17,
            GammaZero = 0.99282,
            QZero = 2.10672,
            EhtaZero = 2.2997,
            F0 = -2055.403,
            RefTemp = 300.0,
        };
    }

    [Fact]
    public void MLSurrogate_GenerateTrainingData_Returns100Rows()
    {
        var mineral = CreateForsterite();
        var generator = new TrainingDataGenerator();

        var data = generator.Generate(mineral, 0.001, 25.0, 300.0, 2000.0, 100);

        _output.WriteLine($"Generated {data.Count} training data points");
        Assert.Equal(100, data.Count);

        foreach (var point in data.Take(5))
        {
            _output.WriteLine($"P={point.Pressure:F2} GPa, T={point.Temperature:F0} K => Vp={point.Vp:F1}, Vs={point.Vs:F1}, rho={point.Density:F4}");
        }
    }

    [Fact]
    public void MLSurrogate_LatinHypercubeSampling_SpansFullRange()
    {
        var mineral = CreateForsterite();
        var generator = new TrainingDataGenerator();

        var data = generator.Generate(mineral, 0.001, 25.0, 300.0, 2000.0, 100);

        double minP = data.Min(d => d.Pressure);
        double maxP = data.Max(d => d.Pressure);
        double minT = data.Min(d => d.Temperature);
        double maxT = data.Max(d => d.Temperature);

        _output.WriteLine($"P range: {minP:F3} - {maxP:F3} GPa");
        _output.WriteLine($"T range: {minT:F1} - {maxT:F1} K");

        Assert.True(minP < 1.0, $"Min P should be near 0, got {minP}");
        Assert.True(maxP > 24.0, $"Max P should be near 25, got {maxP}");
        Assert.True(minT < 320.0, $"Min T should be near 300, got {minT}");
        Assert.True(maxT > 1980.0, $"Max T should be near 2000, got {maxT}");
    }

    [Fact]
    public void MLSurrogate_ExportCSV_CreatesParseableFile()
    {
        var mineral = CreateForsterite();
        var generator = new TrainingDataGenerator();

        var data = generator.Generate(mineral, 0.001, 25.0, 300.0, 2000.0, 10);

        var tempPath = Path.Combine(Path.GetTempPath(), $"ml_training_{Guid.NewGuid()}.csv");
        try
        {
            generator.ExportCSV(data, tempPath);

            Assert.True(File.Exists(tempPath), "CSV file should exist");

            var lines = File.ReadAllLines(tempPath);
            _output.WriteLine($"CSV has {lines.Length} lines (1 header + {lines.Length - 1} data)");

            // Header + data rows
            Assert.Equal(data.Count + 1, lines.Length);

            // Verify header
            Assert.Equal("Pressure,Temperature,Vp,Vs,Density,KS,GS,Alpha", lines[0]);

            // Verify first data line is parseable
            var parts = lines[1].Split(',');
            Assert.Equal(8, parts.Length);
            foreach (var part in parts)
            {
                Assert.True(double.TryParse(part, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out _),
                    $"Could not parse '{part}' as double");
            }
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void MLSurrogate_DataValidity_AllPositiveValues()
    {
        var mineral = CreateForsterite();
        var generator = new TrainingDataGenerator();

        var data = generator.Generate(mineral, 0.001, 25.0, 300.0, 2000.0, 100);

        foreach (var point in data)
        {
            Assert.True(point.Vp > 0, $"Vp should be > 0 at P={point.Pressure}, T={point.Temperature}");
            Assert.True(point.Vs > 0, $"Vs should be > 0 at P={point.Pressure}, T={point.Temperature}");
            Assert.True(point.Density > 0, $"Density should be > 0 at P={point.Pressure}, T={point.Temperature}");
        }
    }

    [Fact]
    public void MLSurrogate_Predict_ThrowsWhenNoModelLoaded()
    {
        var model = new MLSurrogateModel();

        Assert.False(model.IsLoaded);

        var ex = Assert.Throws<InvalidOperationException>(() => model.Predict(10.0, 1500.0));
        Assert.Contains("No model loaded", ex.Message);
    }
}
