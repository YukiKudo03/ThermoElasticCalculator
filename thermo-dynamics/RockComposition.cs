using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;

namespace thermo_dynamics
{
    /// <summary>
    /// A single mineral entry in a rock composition (mineral params + volume ratio)
    /// </summary>
    public class RockMineralEntry
    {
        public MineralParams Mineral { get; set; }
        public double VolumeRatio { get; set; }
    }

    /// <summary>
    /// Rock composition: a named collection of minerals with volume ratios
    /// </summary>
    public class RockComposition
    {
        public RockComposition()
        {
            Minerals = new List<RockMineralEntry>();
        }

        public string Name { get; set; }
        public List<RockMineralEntry> Minerals { get; set; }

        public double TotalRatio
        {
            get { return Minerals.Sum(m => m.VolumeRatio); }
        }

        public string ExportJson()
        {
            return JsonSerializer.Serialize<RockComposition>(this, new JsonSerializerOptions { WriteIndented = true });
        }

        public static bool ImportJson(string jsonString, out RockComposition ret)
        {
            bool succeed = false;
            ret = null;
            try
            {
                ret = JsonSerializer.Deserialize<RockComposition>(jsonString);
                succeed = true;
            }
            catch
            {
            }
            return succeed;
        }
    }

    /// <summary>
    /// Calculates physical properties of a rock composition at given P-T
    /// </summary>
    public class RockCalculator
    {
        public RockCalculator(RockComposition rock, double pressure, double temperature, MixtureMethod method)
        {
            Rock = rock;
            Pressure = pressure;
            Temperature = temperature;
            Method = method;
        }

        public RockComposition Rock { get; set; }
        public double Pressure { get; set; }
        public double Temperature { get; set; }
        public MixtureMethod Method { get; set; }

        /// <summary>
        /// Calculate individual mineral properties at given P-T, then mix
        /// </summary>
        public (ResultSummary mixedResult, List<(string name, double ratio, ResultSummary result)> individualResults) Calculate()
        {
            var pt = new PTData { Pressure = Pressure, Temperature = Temperature };

            // Calculate each mineral at the given P-T
            var individualResults = Rock.Minerals.Select(entry =>
            {
                var optimizer = new MieGruneisenEOSOptimizer(entry.Mineral, pt);
                var result = optimizer.ExecOptimize().ExportResults();
                return (name: entry.Mineral.ParamSymbol, ratio: entry.VolumeRatio, result: result);
            }).ToList();

            // Normalize ratios
            double totalRatio = individualResults.Sum(r => r.ratio);
            var normalizedResults = individualResults.Select(r =>
                (r.ratio / totalRatio, r.result)).ToList();

            // Calculate mixture using MixtureCalculator
            var mixer = new MixtureCalculator(normalizedResults);
            ResultSummary mixedResult;
            switch (Method)
            {
                case MixtureMethod.Voigt:
                    mixedResult = mixer.VoigtAverage();
                    break;
                case MixtureMethod.Reuss:
                    mixedResult = mixer.ReussAverage();
                    break;
                case MixtureMethod.Hill:
                default:
                    mixedResult = mixer.HillAverage();
                    break;
            }

            if (mixedResult != null)
            {
                mixedResult.Name = Rock.Name;
                mixedResult.GivenP = Pressure;
                mixedResult.GivenT = Temperature;
            }

            return (mixedResult, individualResults);
        }
    }
}
