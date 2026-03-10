using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

public class PTProfileCalculator
{
    public PTProfileCalculator(MineralParams mineral, PTProfile profile)
    {
        Mineral = mineral;
        Profile = profile;
    }

    public MineralParams Mineral { get; }
    public PTProfile Profile { get; }

    public List<ThermoMineralParams> DoProfileCalculation()
    {
        return Profile.Profile.Select(pt => new MieGruneisenEOSOptimizer(Mineral, pt).ExecOptimize()).ToList();
    }

    public List<ResultSummary> DoProfileCalculationAsSummary()
    {
        return DoProfileCalculation().Select(tmp => tmp.ExportResults()).ToList();
    }

    public List<string> DoProfileCalculationAsCSV()
    {
        var ret = new List<string> { ResultSummary.ColumnsCSV };
        ret.AddRange(DoProfileCalculationAsSummary().Select(res => res.ExportSummaryAsColumn()));
        return ret;
    }
}
