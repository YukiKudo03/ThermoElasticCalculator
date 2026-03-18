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

    /// <summary>
    /// Hashin-Shtrikman lower bound for N-component mixtures.
    /// Uses the softest phase as reference (minimum K and G).
    /// K_HS = K_ref + Σ f_i / (1/(K_i - K_ref) + 3*f_ref / (3*K_ref + 4*G_ref))
    /// G_HS = G_ref + Σ f_i / (1/(G_i - G_ref) + 6*f_ref*(K_ref + 2*G_ref) / (5*G_ref*(3*K_ref + 4*G_ref)))
    /// Reference: Watt, Davies & O'Connell (1976)
    /// </summary>
    public ResultSummary? HashinShtrikmanAverage()
    {
        if (IsNotCalculatable) return null;
        if (_results.Count == 1)
        {
            return new ResultSummary
            {
                Name = "Hashin-Shtrikman",
                GivenP = _results[0].elemResult.GivenP,
                GivenT = _results[0].elemResult.GivenT,
                Volume = _results[0].elemResult.Volume,
                Density = _results[0].elemResult.Density,
                KT = _results[0].elemResult.KT,
                KS = _results[0].elemResult.KS,
                GS = _results[0].elemResult.GS,
            };
        }

        // Find the softest phase (minimum K and G) as reference for lower bound
        double Kmin = _results.Min(r => r.elemResult.KS);
        double Gmin = _results.Min(r => r.elemResult.GS);

        double zK = 4.0 * Gmin / 3.0;  // ζ_K = 4G_min/3
        double zG = Gmin * (9.0 * Kmin + 8.0 * Gmin) / (6.0 * (Kmin + 2.0 * Gmin)); // ζ_G

        // K_HS- = <1/(K_i + ζ_K)>^{-1} - ζ_K
        double sumInvKpZ = 0, sumInvGpZ = 0;
        for (int i = 0; i < _results.Count; i++)
        {
            double fi = _results[i].elemRatio;
            double Ki = _results[i].elemResult.KS;
            double Gi = _results[i].elemResult.GS;

            sumInvKpZ += fi / (Ki + zK);
            sumInvGpZ += fi / (Gi + zG);
        }

        double KHS = 1.0 / sumInvKpZ - zK;
        double GHS = 1.0 / sumInvGpZ - zG;

        // Repeat for KT
        double KTmin = _results.Min(r => r.elemResult.KT);
        double sumInvKTpZ = 0;
        for (int i = 0; i < _results.Count; i++)
        {
            double fi = _results[i].elemRatio;
            double KTi = _results[i].elemResult.KT;
            sumInvKTpZ += fi / (KTi + zK);
        }
        double KTHS = 1.0 / sumInvKTpZ - zK;

        return new ResultSummary
        {
            Name = "Hashin-Shtrikman",
            GivenP = _results[0].elemResult.GivenP,
            GivenT = _results[0].elemResult.GivenT,
            Volume = _results.Sum(r => r.elemRatio * r.elemResult.Volume),
            Density = _results.Sum(r => r.elemRatio * r.elemResult.Volume * r.elemResult.Density)
                / _results.Sum(r => r.elemRatio * r.elemResult.Volume),
            KT = KTHS,
            KS = KHS,
            GS = GHS,
        };
    }
}
