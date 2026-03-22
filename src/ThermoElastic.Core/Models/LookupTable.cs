namespace ThermoElastic.Core.Models;

/// <summary>
/// Pre-computed P-T lookup table of mineral/rock properties.
/// </summary>
public class LookupTable
{
    public string MineralName { get; set; } = string.Empty;
    public double[] Pressures { get; set; } = Array.Empty<double>();
    public double[] Temperatures { get; set; } = Array.Empty<double>();

    /// <summary>2D arrays [pressureIndex, temperatureIndex]</summary>
    public double[,] Density { get; set; } = new double[0, 0];
    public double[,] Vp { get; set; } = new double[0, 0];
    public double[,] Vs { get; set; } = new double[0, 0];
    public double[,] KS { get; set; } = new double[0, 0];
    public double[,] GS { get; set; } = new double[0, 0];
    public double[,] Alpha { get; set; } = new double[0, 0];
    public double[,] Gamma { get; set; } = new double[0, 0];

    public int NPressure => Pressures.Length;
    public int NTemperature => Temperatures.Length;
}
