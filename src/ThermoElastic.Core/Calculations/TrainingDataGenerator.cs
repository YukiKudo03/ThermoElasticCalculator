using System.Globalization;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Generates training data from the thermodynamic engine for ML surrogate models.
/// </summary>
public class TrainingDataGenerator
{
    /// <summary>
    /// Generate training data for a mineral over a P-T range using Latin Hypercube sampling.
    /// </summary>
    /// <param name="mineral">Mineral to evaluate</param>
    /// <param name="pMin">Min pressure [GPa]</param>
    /// <param name="pMax">Max pressure [GPa]</param>
    /// <param name="tMin">Min temperature [K]</param>
    /// <param name="tMax">Max temperature [K]</param>
    /// <param name="nSamples">Number of P-T points</param>
    /// <returns>List of training data points with computed properties</returns>
    public List<TrainingDataPoint> Generate(MineralParams mineral,
        double pMin, double pMax, double tMin, double tMax, int nSamples)
    {
        var results = new List<TrainingDataPoint>(nSamples);

        // Latin Hypercube Sampling
        var random = new Random(42); // fixed seed for reproducibility
        var pBins = Enumerable.Range(0, nSamples).OrderBy(_ => random.Next()).ToArray();
        var tBins = Enumerable.Range(0, nSamples).OrderBy(_ => random.Next()).ToArray();

        for (int i = 0; i < nSamples; i++)
        {
            double p = pMin + (pBins[i] + random.NextDouble()) / nSamples * (pMax - pMin);
            double t = tMin + (tBins[i] + random.NextDouble()) / nSamples * (tMax - tMin);

            try
            {
                var thermo = new MieGruneisenEOSOptimizer(mineral, p, t).ExecOptimize();

                results.Add(new TrainingDataPoint
                {
                    Pressure = p,
                    Temperature = t,
                    Vp = thermo.Vp,
                    Vs = thermo.Vs,
                    Density = thermo.Density,
                    KS = thermo.KS,
                    GS = thermo.GS,
                    Alpha = thermo.Alpha,
                });
            }
            catch
            {
                // Skip points where EOS fails to converge
            }
        }

        return results;
    }

    /// <summary>
    /// Export training data to CSV file.
    /// </summary>
    public void ExportCSV(List<TrainingDataPoint> data, string filePath)
    {
        using var writer = new StreamWriter(filePath);
        writer.WriteLine("Pressure,Temperature,Vp,Vs,Density,KS,GS,Alpha");

        foreach (var point in data)
        {
            writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4},{5},{6},{7}",
                point.Pressure, point.Temperature,
                point.Vp, point.Vs, point.Density,
                point.KS, point.GS, point.Alpha));
        }
    }
}
