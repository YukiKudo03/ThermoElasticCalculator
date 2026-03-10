using System.Text.Json;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

public class VProfileCalculator
{
    public VProfileCalculator(List<double> elem1RatioList, ResultSummary elem1, ResultSummary elem2, string name)
    {
        Elem1RatioList = elem1RatioList;
        Elem1 = elem1;
        Elem2 = elem2;
        Name = name;
    }

    public List<double> Elem1RatioList { get; set; }
    public ResultSummary Elem1 { get; set; }
    public ResultSummary Elem2 { get; set; }
    public string Name { get; set; }

    public string ExportJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    public static bool ImportJson(string jsonString, out VProfileCalculator? ret)
    {
        ret = null;
        try
        {
            ret = JsonSerializer.Deserialize<VProfileCalculator>(jsonString);
            return ret != null;
        }
        catch
        {
            return false;
        }
    }

    public List<(double elem1Ratio, ResultSummary ret)> ReussResults() =>
        Elem1RatioList.Select(rto => (rto, ReussAverage(rto, Elem1, Elem2)!)).ToList();

    public List<(double elem1Ratio, ResultSummary ret)> VoigtResults() =>
        Elem1RatioList.Select(rto => (rto, VoigtAverage(rto, Elem1, Elem2)!)).ToList();

    public List<(double elem1Ratio, ResultSummary ret)> HillResults() =>
        Elem1RatioList.Select(rto => (rto, HillAverage(rto, Elem1, Elem2)!)).ToList();

    public List<(double elem1Ratio, ResultSummary ret)> HSResults() =>
        Elem1RatioList.Select(rto => (rto, HashinShtrikmanBond(rto, Elem1, Elem2)!)).ToList();

    public static ResultSummary? VoigtAverage(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
    {
        if (elem1Ratio < 0 || elem1Ratio > 1) return null;
        if (!CommonMethods.DoubleEquals(elem1.GivenP, elem2.GivenP) || !CommonMethods.DoubleEquals(elem1.GivenT, elem2.GivenT))
            return null;

        return new ResultSummary
        {
            Name = $"{elem1.Name} : {elem2.Name} = {elem1Ratio} : {1.0d - elem1Ratio} Voigt",
            GivenP = elem1.GivenP,
            GivenT = elem1.GivenT,
            KT = elem1Ratio * elem1.KT + (1.0d - elem1Ratio) * elem2.KT,
            KS = elem1Ratio * elem1.KS + (1.0d - elem1Ratio) * elem2.KS,
            GS = elem1Ratio * elem1.GS + (1.0d - elem1Ratio) * elem2.GS,
            Volume = elem1Ratio * elem1.Volume + (1.0d - elem1Ratio) * elem2.Volume,
            Density = CommonMethods.GetDensity(elem1Ratio, elem1, elem2),
        };
    }

    public static ResultSummary? ReussAverage(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
    {
        if (elem1Ratio < 0 || elem1Ratio > 1) return null;
        if (!CommonMethods.DoubleEquals(elem1.GivenP, elem2.GivenP) || !CommonMethods.DoubleEquals(elem1.GivenT, elem2.GivenT))
            return null;

        return new ResultSummary
        {
            Name = $"{elem1.Name} : {elem2.Name} = {elem1Ratio} : {1.0d - elem1Ratio} Reuss",
            GivenP = elem1.GivenP,
            GivenT = elem1.GivenT,
            KT = 1.0d / ((elem1Ratio / elem1.KT) + (1.0d - elem1Ratio) / elem2.KT),
            KS = 1.0d / ((elem1Ratio / elem1.KS) + (1.0d - elem1Ratio) / elem2.KS),
            GS = 1.0d / ((elem1Ratio / elem1.GS) + (1.0d - elem1Ratio) / elem2.GS),
            Volume = elem1Ratio * elem1.Volume + (1.0d - elem1Ratio) * elem2.Volume,
            Density = CommonMethods.GetDensity(elem1Ratio, elem1, elem2),
        };
    }

    public static ResultSummary? HillAverage(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
    {
        if (elem1Ratio < 0 || elem1Ratio > 1) return null;
        if (!CommonMethods.DoubleEquals(elem1.GivenP, elem2.GivenP) || !CommonMethods.DoubleEquals(elem1.GivenT, elem2.GivenT))
            return null;

        var voigtResult = VoigtAverage(elem1Ratio, elem1, elem2)!;
        var reussResult = ReussAverage(elem1Ratio, elem1, elem2)!;

        return new ResultSummary
        {
            Name = $"{elem1.Name} : {elem2.Name} = {elem1Ratio} : {1.0d - elem1Ratio} Hill",
            GivenP = elem1.GivenP,
            GivenT = elem1.GivenT,
            KT = (voigtResult.KT + reussResult.KT) / 2.0d,
            KS = (voigtResult.KS + reussResult.KS) / 2.0d,
            GS = (voigtResult.GS + reussResult.GS) / 2.0d,
            Volume = elem1Ratio * elem1.Volume + (1.0d - elem1Ratio) * elem2.Volume,
            Density = CommonMethods.GetDensity(elem1Ratio, elem1, elem2),
        };
    }

    public static ResultSummary? HashinShtrikmanBond(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
    {
        if (elem1Ratio < 0 || elem1Ratio > 1) return null;
        if (!CommonMethods.DoubleEquals(elem1.GivenP, elem2.GivenP) || !CommonMethods.DoubleEquals(elem1.GivenT, elem2.GivenT))
            return null;

        return new ResultSummary
        {
            Name = $"{elem1.Name} : {elem2.Name} = {elem1Ratio} : {1.0d - elem1Ratio} H-S",
            GivenP = elem1.GivenP,
            GivenT = elem1.GivenT,
            KS = elem1.KS + (1.0d - elem1Ratio) / (1.0d / (elem2.KS - elem1.KS) + elem1Ratio / (elem1.KS + 4.0d / 3.0d * elem1.GS)),
            KT = elem1.KT + (1.0d - elem1Ratio) / (1.0d / (elem2.KT - elem1.KT) + elem1Ratio / (elem1.KT + 4.0d / 3.0d * elem1.GS)),
            GS = elem1.GS + (1.0d - elem1Ratio) / (1.0d / (elem2.GS - elem1.GS) + 2.0d * elem1Ratio * (elem1.KS + 2.0d * elem1.GS) / 5.0d * elem1.GS * (elem1.KS + 4.0d / 3.0d * elem1.GS)),
            Volume = elem1Ratio * elem1.Volume + (1.0d - elem1Ratio) * elem2.Volume,
            Density = CommonMethods.GetDensity(elem1Ratio, elem1, elem2),
        };
    }
}
