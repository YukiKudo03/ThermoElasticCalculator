using System.Globalization;

namespace ThermoElastic.Core.Models;

public class ExperimentalDataPoint
{
    public double Pressure { get; set; }      // GPa
    public double Temperature { get; set; }   // K
    public double? Vp { get; set; }           // m/s
    public double? Vs { get; set; }           // m/s
    public double? Density { get; set; }      // g/cm3
    public double? SigmaVp { get; set; }      // m/s
    public double? SigmaVs { get; set; }      // m/s
    public double? SigmaDensity { get; set; } // g/cm3
}

public class ExperimentalDataset
{
    public string Name { get; set; } = "";
    public string MineralName { get; set; } = "";
    public List<ExperimentalDataPoint> Data { get; set; } = new();

    /// <summary>
    /// Parse CSV text with columns: P(GPa), T(K), Vp(m/s), Vs(m/s), Density(g/cm3), SigmaVp, SigmaVs, SigmaDensity.
    /// Missing values are represented as empty fields or "NaN".
    /// Lines starting with '#' are treated as comments and skipped.
    /// First non-comment line is treated as header if it contains non-numeric text.
    /// Supports comma, tab, and semicolon delimiters.
    /// </summary>
    public static ExperimentalDataset ParseCsv(string csvText, string name = "Imported")
    {
        var dataset = new ExperimentalDataset { Name = name };
        var lines = csvText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
            throw new FormatException("CSV is empty.");

        int firstDataIndex = 0;
        while (firstDataIndex < lines.Length &&
               (string.IsNullOrWhiteSpace(lines[firstDataIndex]) || lines[firstDataIndex].TrimStart().StartsWith("#")))
        {
            firstDataIndex++;
        }

        if (firstDataIndex >= lines.Length)
            throw new FormatException("CSV contains no data lines (only comments or blank lines).");

        // Detect delimiter from first non-comment line
        char delimiter = DetectDelimiter(lines[firstDataIndex]);

        int startLine = firstDataIndex;
        // Skip header if first field is not a number
        var firstFields = lines[firstDataIndex].Split(delimiter);
        if (firstFields.Length > 0 && !double.TryParse(firstFields[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            startLine = firstDataIndex + 1;

        for (int i = startLine; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("#")) continue;

            var fields = line.Split(delimiter);
            if (fields.Length < 2)
                throw new FormatException($"Line {i + 1}: expected at least 2 columns (P, T), got {fields.Length}.");

            var point = new ExperimentalDataPoint
            {
                Pressure = ParseRequired(fields, 0, i + 1, "Pressure"),
                Temperature = ParseRequired(fields, 1, i + 1, "Temperature"),
                Vp = ParseOptional(fields, 2),
                Vs = ParseOptional(fields, 3),
                Density = ParseOptional(fields, 4),
                SigmaVp = ParseOptional(fields, 5),
                SigmaVs = ParseOptional(fields, 6),
                SigmaDensity = ParseOptional(fields, 7),
            };

            if (point.Pressure < 0)
                throw new FormatException($"Line {i + 1}: Pressure must be non-negative, got {point.Pressure}.");
            if (point.Temperature <= 0)
                throw new FormatException($"Line {i + 1}: Temperature must be positive, got {point.Temperature}.");

            dataset.Data.Add(point);
        }

        if (dataset.Data.Count == 0)
            throw new FormatException("No data points found in CSV.");

        return dataset;
    }

    /// <summary>
    /// Count how many observable types (Vp, Vs, Density) have data in this dataset.
    /// </summary>
    public int ObservableCount()
    {
        bool hasVp = Data.Any(d => d.Vp.HasValue);
        bool hasVs = Data.Any(d => d.Vs.HasValue);
        bool hasDensity = Data.Any(d => d.Density.HasValue);
        return (hasVp ? 1 : 0) + (hasVs ? 1 : 0) + (hasDensity ? 1 : 0);
    }

    private static char DetectDelimiter(string line)
    {
        if (line.Contains('\t')) return '\t';
        if (line.Contains(';')) return ';';
        return ',';
    }

    private static double ParseRequired(string[] fields, int index, int lineNum, string fieldName)
    {
        if (index >= fields.Length)
            throw new FormatException($"Line {lineNum}: missing {fieldName} column.");
        var text = fields[index].Trim();
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            throw new FormatException($"Line {lineNum}: invalid {fieldName} value '{text}'.");
        return val;
    }

    private static double? ParseOptional(string[] fields, int index)
    {
        if (index >= fields.Length) return null;
        var text = fields[index].Trim();
        if (string.IsNullOrEmpty(text) || text.Equals("NaN", StringComparison.OrdinalIgnoreCase))
            return null;
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
            return val;
        return null;
    }
}
