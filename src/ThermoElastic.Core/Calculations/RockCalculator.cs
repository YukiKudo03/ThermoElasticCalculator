using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

public class RockCalculator
{
    public RockCalculator(RockComposition rock, double pressure, double temperature, MixtureMethod method)
    {
        Rock = rock;
        Pressure = pressure;
        Temperature = temperature;
        Method = method;
    }

    public RockComposition Rock { get; }
    public double Pressure { get; }
    public double Temperature { get; }
    public MixtureMethod Method { get; }

    public (ResultSummary? mixedResult, List<(string name, double ratio, ResultSummary result)> individualResults) Calculate()
    {
        var pt = new PTData { Pressure = Pressure, Temperature = Temperature };

        var individualResults = Rock.Minerals.Select(entry =>
        {
            var optimizer = new MieGruneisenEOSOptimizer(entry.Mineral, pt);
            var result = optimizer.ExecOptimize().ExportResults();
            return (name: entry.Mineral.ParamSymbol, ratio: entry.VolumeRatio, result: result);
        }).ToList();

        double totalRatio = individualResults.Sum(r => r.ratio);
        var normalizedResults = individualResults.Select(r =>
            (r.ratio / totalRatio, r.result)).ToList();

        var mixer = new MixtureCalculator(normalizedResults);
        ResultSummary? mixedResult = Method switch
        {
            MixtureMethod.Voigt => mixer.VoigtAverage(),
            MixtureMethod.Reuss => mixer.ReussAverage(),
            _ => mixer.HillAverage(),
        };

        if (mixedResult != null)
        {
            mixedResult.Name = Rock.Name;
            mixedResult.GivenP = Pressure;
            mixedResult.GivenT = Temperature;
        }

        return (mixedResult, individualResults);
    }
}
