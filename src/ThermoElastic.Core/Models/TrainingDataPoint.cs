namespace ThermoElastic.Core.Models;

public record TrainingDataPoint
{
    public double Pressure { get; init; }
    public double Temperature { get; init; }
    public double Vp { get; init; }
    public double Vs { get; init; }
    public double Density { get; init; }
    public double KS { get; init; }
    public double GS { get; init; }
    public double Alpha { get; init; }
}
