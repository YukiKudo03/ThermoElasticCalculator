using ThermoElastic.Core.Models;

namespace ThermoElastic.Core;

public static class PhysicConstants
{
    public const double GasConst = 8.31477d;
    public const double NA = 6.02e23;
    public const double Boltzman = 1.38e-23;
    public const double Plank = 6.63e-34;
    public static readonly double PiPlank = Plank / (2.0d * Math.PI);
    public const double RefTemperature = 300.0d;
}

public static class CommonMethods
{
    public static bool DoubleEquals(double val1, double val2)
    {
        return Math.Abs(val2 - val1) < 1.0e-5;
    }

    public static double GetDensity(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
    {
        return (elem1Ratio * elem1.Volume * elem1.Density + (1.0d - elem1Ratio) * elem2.Volume * elem2.Density)
            / (elem1Ratio * elem1.Volume + (1.0d - elem1Ratio) * elem2.Volume);
    }
}
