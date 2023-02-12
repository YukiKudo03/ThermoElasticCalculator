using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace thermo_dynamics
{
    public class PTData
    {
        public double Temperature { get; set; }
        public double Pressure { get; set; }
    }

    public class PTProfile
    {
        public PTProfile()
        {

        }
        public string Name { get; set; }

        public List<PTData> Profile { get; set; }

        public string ExportJson()
        {
            return JsonSerializer.Serialize<PTProfile>(this, new JsonSerializerOptions { WriteIndented = true });
        }

        public static bool ImportJson(string jsonString, out PTProfile ret)
        {
            bool succeed = false;
            ret = null;
            try
            {
                ret = JsonSerializer.Deserialize<PTProfile>(jsonString);
                succeed = true;
            }
            catch
            {

            }

            return succeed;
        }
    }

    public class PTProfileCalculator
    {
        public PTProfileCalculator(MineralParams mineral, PTProfile profile)
        {
            Mineral = mineral;
            Profile = profile;
        }
        public MineralParams Mineral;
        public PTProfile Profile;

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
            return JsonSerializer.Serialize<VProfileCalculator>(this, new JsonSerializerOptions { WriteIndented = true });
        }

        public static bool ImportJson(string jsonString, out VProfileCalculator ret)
        {
            bool succeed = false;
            ret = null;
            try
            {
                ret = JsonSerializer.Deserialize<VProfileCalculator>(jsonString);
                succeed = true;
            }
            catch
            {

            }

            return succeed;
        }

        public List<(double elem1Ratio, ResultSummary ret)> ReussResults()
        {
            return Elem1RatioList.Select(rto => (rto, ReussAverage(rto, Elem1, Elem2))).ToList();
        }
        public List<(double elem1Ratio, ResultSummary ret)> VoigtResults()
        {
            return Elem1RatioList.Select(rto => (rto, VoigtAverage(rto, Elem1, Elem2))).ToList();
        }
        public List<(double elem1Ratio, ResultSummary ret)> HillResults()
        {
            return Elem1RatioList.Select(rto => (rto, HillAverage(rto, Elem1, Elem2))).ToList();
        }
        public List<(double elem1Ratio, ResultSummary ret)> HSResults()
        {
            return Elem1RatioList.Select(rto => (rto, HashinShtrikmanBond(rto, Elem1, Elem2))).ToList();
        }

        public static ResultSummary VoigtAverage(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
        {
            if (elem1Ratio < 0 || elem1Ratio > 1)
            {
                return null;
            }
            if(!CommonMethods.DoubleEquals(elem1.GivenP, elem2.GivenP) || !CommonMethods.DoubleEquals(elem1.GivenT, elem2.GivenT))
            {
                return null;
            }

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

        public static ResultSummary ReussAverage(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
        {
            if (elem1Ratio < 0 || elem1Ratio > 1)
            {
                return null;
            }
            if (!CommonMethods.DoubleEquals(elem1.GivenP, elem2.GivenP) || !CommonMethods.DoubleEquals(elem1.GivenT, elem2.GivenT))
            {
                return null;
            }

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

        public static ResultSummary HillAverage(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
        {
            if (elem1Ratio < 0 || elem1Ratio > 1)
            {
                return null;
            }
            if (!CommonMethods.DoubleEquals(elem1.GivenP, elem2.GivenP) || !CommonMethods.DoubleEquals(elem1.GivenT, elem2.GivenT))
            {
                return null;
            }
            var VoigtResult = VoigtAverage(elem1Ratio, elem1, elem2);
            var ReussResult = ReussAverage(elem1Ratio, elem1, elem2);

            return new ResultSummary
            {
                Name = $"{elem1.Name} : {elem2.Name} = {elem1Ratio} : {1.0d - elem1Ratio} Hill",
                GivenP = elem1.GivenP,
                GivenT = elem1.GivenT,
                KT = (VoigtResult.KT + ReussResult.KT) / 2.0d,
                KS = (VoigtResult.KS + ReussResult.KS) / 2.0d,
                GS = (VoigtResult.GS + ReussResult.GS) / 2.0d,
                Volume = elem1Ratio * elem1.Volume + (1.0d - elem1Ratio) * elem2.Volume,
                Density = CommonMethods.GetDensity(elem1Ratio, elem1, elem2),
            };
        }

        public static ResultSummary HashinShtrikmanBond(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
        {
            if (elem1Ratio < 0 || elem1Ratio > 1)
            {
                return null;
            }
            if (!CommonMethods.DoubleEquals(elem1.GivenP, elem2.GivenP) || !CommonMethods.DoubleEquals(elem1.GivenT, elem2.GivenT))
            {
                return null;
            }
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

    public class MixtureCalculator
    {
        public MixtureCalculator(List<(double elemRatio, ResultSummary elemResult)> results)
        {
            __results = results;
        }

        private List<(double elemRatio, ResultSummary elemResult)> __results;

        private bool __isNotCalculatable
        {
            get
            {
                return (__results.Any(res => res.elemRatio < 0 || res.elemRatio > 1) ||
                    (__results.Sum(res => res.elemRatio) < 0.0d || __results.Sum(res => res.elemRatio) > 1.01d) ||
                     (__results.Any(res => res.elemResult.GivenP != __results[0].elemResult.GivenP || res.elemResult.GivenT != __results[0].elemResult.GivenT)));
            }
        }

        public ResultSummary VoigtAverage()
        {
            if (__isNotCalculatable)
            {
                return null;
            }
            return new ResultSummary
            {
                GivenP = __results[0].elemResult.GivenP,
                GivenT = __results[0].elemResult.GivenT,
                Volume = __results.Sum(res => res.elemRatio * res.elemResult.Volume),
                Density = __results.Sum(res => res.elemRatio * res.elemResult.Volume * res.elemResult.Density) / __results.Sum(res => res.elemRatio * res.elemResult.Volume),
                KT = __results.Sum(res => res.elemRatio * res.elemResult.KT),
                KS = __results.Sum(res => res.elemRatio * res.elemResult.KS),
                GS = __results.Sum(res => res.elemRatio * res.elemResult.GS),
            };
        }

        public ResultSummary ReussAverage()
        {
            if (__isNotCalculatable)
            {
                return null;
            }
            return new ResultSummary
            {
                GivenP = __results[0].elemResult.GivenP,
                GivenT = __results[0].elemResult.GivenT,
                Volume = __results.Sum(res => res.elemRatio * res.elemResult.Volume),
                Density = __results.Sum(res => res.elemRatio * res.elemResult.Volume * res.elemResult.Density) / __results.Sum(res => res.elemRatio * res.elemResult.Volume),
                KT = 1.0d / __results.Sum(res => res.elemRatio / res.elemResult.KT),
                KS = 1.0d / __results.Sum(res => res.elemRatio / res.elemResult.KS),
                GS = 1.0d / __results.Sum(res => res.elemRatio / res.elemResult.GS),
            };
        }

        public ResultSummary HillAverage()
        {
            if (__isNotCalculatable)
            {
                return null;
            }
            var reuss = ReussAverage();
            var voigt = VoigtAverage();
            return new ResultSummary
            {
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
}
