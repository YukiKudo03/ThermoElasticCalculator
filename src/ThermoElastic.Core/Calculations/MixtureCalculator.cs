using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

public class MixtureCalculator
{
    public MixtureCalculator(List<(double elemRatio, ResultSummary elemResult)> results)
    {
        _results = results;
    }

    private readonly List<(double elemRatio, ResultSummary elemResult)> _results;

    private bool IsNotCalculatable =>
        _results.Any(res => res.elemRatio < 0 || res.elemRatio > 1) ||
        _results.Sum(res => res.elemRatio) < 0.0d || _results.Sum(res => res.elemRatio) > 1.01d ||
        _results.Any(res => Math.Abs(res.elemResult.GivenP - _results[0].elemResult.GivenP) > 0.01d ||
                            Math.Abs(res.elemResult.GivenT - _results[0].elemResult.GivenT) > 0.01d);

    public ResultSummary? VoigtAverage()
    {
        if (IsNotCalculatable) return null;
        return new ResultSummary
        {
            Name = "Voigt Average",
            GivenP = _results[0].elemResult.GivenP,
            GivenT = _results[0].elemResult.GivenT,
            Volume = _results.Sum(res => res.elemRatio * res.elemResult.Volume),
            Density = _results.Sum(res => res.elemRatio * res.elemResult.Volume * res.elemResult.Density) / _results.Sum(res => res.elemRatio * res.elemResult.Volume),
            KT = _results.Sum(res => res.elemRatio * res.elemResult.KT),
            KS = _results.Sum(res => res.elemRatio * res.elemResult.KS),
            GS = _results.Sum(res => res.elemRatio * res.elemResult.GS),
        };
    }

    public ResultSummary? ReussAverage()
    {
        if (IsNotCalculatable) return null;
        return new ResultSummary
        {
            Name = "Reuss Average",
            GivenP = _results[0].elemResult.GivenP,
            GivenT = _results[0].elemResult.GivenT,
            Volume = _results.Sum(res => res.elemRatio * res.elemResult.Volume),
            Density = _results.Sum(res => res.elemRatio * res.elemResult.Volume * res.elemResult.Density) / _results.Sum(res => res.elemRatio * res.elemResult.Volume),
            KT = 1.0d / _results.Sum(res => res.elemRatio / res.elemResult.KT),
            KS = 1.0d / _results.Sum(res => res.elemRatio / res.elemResult.KS),
            GS = 1.0d / _results.Sum(res => res.elemRatio / res.elemResult.GS),
        };
    }

    public ResultSummary? HillAverage()
    {
        if (IsNotCalculatable) return null;
        var reuss = ReussAverage()!;
        var voigt = VoigtAverage()!;
        return new ResultSummary
        {
            Name = "Hill Average",
            GivenP = reuss.GivenP,
            GivenT = reuss.GivenT,
            Volume = reuss.Volume,
            Density = reuss.Density,
            KT = (reuss.KT + voigt.KT) / 2.0d,
            KS = (reuss.KS + voigt.KS) / 2.0d,
            GS = (reuss.GS + voigt.GS) / 2.0d,
        };
    }
}
