using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace thermo_dynamics
{
    
    public class MineralParams
    {
        public MineralParams()
        {

        }
        /// <summary>
        /// MineralName
        /// </summary>
        public string MineralName { get; set; }
        /// <summary>
        /// PaperName
        /// </summary>
        public string PaperName { get; set; }
        /// <summary>
        /// Atoms per unit
        /// </summary>
        public int NumAtoms { get; set; }
        public double MolarVolume { get; set; }
        /// <summary>
        /// Molar Weight
        /// </summary>
        public double MolarWeight { get; set; }
        public double AveMolarWeight
        {
            get
            {
                return MolarWeight / (double)NumAtoms;
            }
        }
        /// <summary>
        /// Bulk Modulus[GPa]
        /// </summary>
        public double KZero { get; set; }
        /// <summary>
        /// Pressure Derivative of Bulk Modulus
        /// </summary>
        public double K1Prime { get; set; }
        /// <summary>
        /// PressureDerivative of K1Prime
        /// </summary>
        public double K2Prime { get; set; }
        /// <summary>
        /// Shear Modulus[GPa]
        /// </summary>
        public double GZero { get; set; }
        /// <summary>
        /// Pressure Derivative of Shear Modulus
        /// </summary>
        public double G1Prime { get; set; }
        /// <summary>
        /// Pressure Derivative of G1Prime
        /// </summary>
        public double G2Prime { get; set; }
        /// <summary>
        /// initial Debye Temperature[K]
        /// </summary>
        public double DebyeTempZero { get; set; }
        /// <summary>
        /// Gruneisen parameter
        /// </summary>
        public double GammaZero { get; set; }
        /// <summary>
        /// QZeroValue
        /// </summary>
        public double QZero { get; set; }
        /// <summary>
        /// ηs0 value
        /// </summary>
        public double EhtaZero { get; set; }
        /// <summary>
        /// Reference Temperature[K]
        /// </summary>
        public double RefTemp { get; set; } = 300.0d;

        public double Aii
        {
            get
            {
                return 6.0d * GammaZero;
            }
        }

        public double Aiikk
        {
            get
            {
                return -12.0d * GammaZero + 36.0 * GammaZero * GammaZero - 18.0d * GammaZero * QZero;
            }
        }

        public double As
        {
            get
            {
                return -2.0d * GammaZero - 2.0d * EhtaZero;
            }
        }

        public double GetPressure(double finite)
        {
            var term3 = Math.Pow((1.0d + 2.0d * finite), 5.0d / 2.0d);
            var term5 = (1.0d + 3.0d * (K1Prime - 4.0d) * finite / 2.0d);
            return 3.0d * KZero * term3 * finite * term5;
        }
        public double FiniteToVolume(double finite)
        {
            return MolarVolume / Math.Pow((2.0d * finite + 1.0d), 3.0d / 2.0d);
        }

        public double VolumeToFinite(double volume)
        {
            return (Math.Pow(MolarVolume / volume, -2.0d / 3.0d) - 1.0d) / 2.0d;
        }

        public double BM3Finite(double targetPressure, OptimizerType optimizerType = OptimizerType.ReglaFalsi)
        {
            Func<double, double> func = (double finite) => GetPressure(finite) - targetPressure;
            var opt = OptimizerFactory.CreateOptimizer(func, 0.0005, 0.02, optimizerType);
            return opt.DoOptimize();
        }

        public double BM3KT(double finite)
        {
            var term1 = Math.Pow(1.0d + 2.0d * finite, 5.0d / 2.0d);
            var term2 = (3.0d * K1Prime - 5.0d) * finite;
            var term3 = 27.0d / 2.0d * (K1Prime - 4.0d) * finite * finite;
            return term1 * KZero * (1.0d + term2 + term3);
        }

        public double BM3GT(double finite)
        {
            var term1 = Math.Pow(1.0d + 2.0d * finite, 5.0d / 2.0d);
            var term2 = (3.0d * KZero * G1Prime - 5.0d * GZero) * finite;
            var term3 = (6.0 * KZero * G1Prime - 24.0d * KZero - 14.0d * GZero + 9.0d / 2.0d * KZero * K1Prime) * finite * finite;
            return term1 * (GZero + term1 + term2 + term3);
        }

        public string ParamSymbol
        {
            get
            {
                return $"{MineralName} ({PaperName})";
            }
        }

        public string ExportJson()
        {
            return JsonSerializer.Serialize<MineralParams>(this, new JsonSerializerOptions { WriteIndented = true });
        }

        public static bool ImportJson(string jsonString, out MineralParams ret)
        {
            bool succeed = false;
            ret = null;
            try
            {
                ret = JsonSerializer.Deserialize<MineralParams>(jsonString);
                succeed = true;
            }
            catch
            {

            }

            return succeed;
        }
    }

    public class ThermoMineralParams
    {
        public ThermoMineralParams(double targetFinite, double targetTemperature, MineralParams mineral)
        {
            __targetTemperature = targetTemperature;
            __mineral = mineral;
            __targetFinite = targetFinite;
            __refP = __mineral.GetPressure(__targetFinite);
            __mu = (1.0d + __mineral.Aii * __targetFinite + __mineral.Aiikk * __targetFinite * __targetFinite / 2.0d);
            __gamma = 1.0d / __mu * (2.0d * __targetFinite + 1.0d) * (__mineral.Aii + __mineral.Aiikk * __targetFinite) / 6.0d;
            __ethaS = -Gamma - (2.0d * Finite + 1.0d) * (2.0d * Finite + 1.0d) * Mineral.As / __mu / 2.0d;
            __vibrationalDebyeTemp = Math.Sqrt(__mu) * __mineral.DebyeTempZero;
            __debyeCondition = new DebyeFunctionCalculator(__vibrationalDebyeTemp);
            __deltaP = (__gamma / Volume) * DeltaE / 1000.0d;
 //           __q = 
        }

        private double __mu;
        private double __deltaP;
        /// <summary>
        /// TemperatureEffect on Pressure[GPa]
        /// </summary>
        public double DeltaP
        {
            get
            {
                return __deltaP;
            }
        }

        private double __refP;
        /// <summary>
        /// Pressure without Temperature Effect[GPa]
        /// </summary>
        public double RefP
        {
            get
            {
                return __refP;

            }
        }

        /// <summary>
        /// Sum of Pressure with and without Temperature Effect[GPa]
        /// </summary>
        public double Pressure
        {
            get
            {
                return __refP + __deltaP;
            }
        }

        private MineralParams __mineral;
        public MineralParams Mineral
        {
            get
            {
                return __mineral;
            }
        }

        private DebyeFunctionCalculator __debyeCondition;

        /// <summary>
        /// targetFinite
        /// </summary>
        private double __targetFinite;
        /// <summary>
        /// Finite Strain
        /// </summary>
        public double Finite
        {
            get
            {
                return __targetFinite;
            }
        }
        /// <summary>
        /// MolarVolume under Condition[cm3/mol]
        /// </summary>
        public double Volume
        {
            get
            {
                return __mineral.FiniteToVolume(Finite);
            }
        }

        /// <summary>
        /// targetTemperature[K]
        /// </summary>
        private double __targetTemperature;
        /// <summary>
        /// Calculate Temperature[K]
        /// </summary>
        public double Temperature
        {
            get
            {
                return __targetTemperature;
            }
        }

        private double __vibrationalDebyeTemp;
        /// <summary>
        /// DebyeTemperature under Finite[K]
        /// </summary>
        public double DebyeTemperature 
        {
            get
            {
                return __vibrationalDebyeTemp; 
            }
        }

        public double DeltaE
        {
            get
            {
                return (__debyeCondition.GetInternalEnergy(Temperature) - __debyeCondition.GetInternalEnergy(__mineral.RefTemp)) * __mineral.NumAtoms;
            }
        }
        public double CvT
        {
            get
            {
                return __debyeCondition.GetCv(Temperature) * __mineral.NumAtoms * Temperature;
            }
        }

        public double DeltaCvT
        {
            get
            {
                return CvT - __debyeCondition.GetCv(__mineral.RefTemp) * __mineral.NumAtoms * __mineral.RefTemp;
            }
        }

        private double __gamma;
        /// <summary>
        /// Gruneisen Parameter under Condition
        /// </summary>
        public double Gamma
        {
            get
            {
                return __gamma;
            }
        }
        /// <summary>
        /// Density under Condition[g/cm3]
        /// </summary>
        public double Density
        {
            get
            {
                return __mineral.MolarWeight / Volume;
            }
        }
        /// <summary>
        /// Thermal Bulk Modulus under Condition[GPa]
        /// </summary>
        public double KT 
        { 
            get
            {
                var a = (Gamma + 1.0d - Mineral.QZero) * Gamma * DeltaE / Volume / 1000.0d;
                var b = Gamma * Gamma * DeltaCvT / Volume / 1000.0d;
                return Mineral.BM3KT(Finite) + a - b;
            }
        }
        /// <summary>
        /// Thermal Expansion under Condition
        /// </summary>
        public double Alpha
        {
            get
            {
                return Gamma * 3.0d * CvT / (double)Mineral.NumAtoms / KT / Volume / 1000.0d;
            }
        }
        /// <summary>
        /// Adiabatic Bulk Modulus under Condition[GPa]
        /// </summary>
        public double KS
        {
            get
            {
                var a = Gamma * Gamma / Volume * CvT / 1000.0d;
                return KT + a;
            }
        }

        private double __ethaS;

        public double EthaS
        {
            get
            {
                return __ethaS;
            }
        }

        private double __q;
        public double Q
        { 
            get
            {
                return __q;
            } 
        }
        /// <summary>
        /// Shear Modulus under Condition[GPa]
        /// </summary>
        public double GS
        {
            get
            {
                
                var b = EthaS / Volume * DeltaE / 1000.0d;
                return Mineral.BM3GT(Finite) - b;
            }
        }
        /// <summary>
        /// Primary wave velocity[m/s]
        /// </summary>
        public double Vp
        {
            get
            {
                return Math.Sqrt((KS + 4.0d / 3.0d * GS) / Density);
            }
        }
        /// <summary>
        /// Secondary wave velocity[m/s]
        /// </summary>
        public double Vs
        {
            get
            {
                return Math.Sqrt(GS / Density);
            }
        }
        /// <summary>
        /// Bulk Sound Velocityty[m/s]
        /// </summary>
        public double Vb
        {
            get
            {
                return Math.Sqrt(KS / Density);
            }
        }

        public ResultSummary ExportResults()
        {
            return new ResultSummary
            {
                Name = Mineral.MineralName,
                GivenP = Pressure,
                GivenT = Temperature,
                Alpha = Alpha,
                DebyeTemp = DebyeTemperature,
                Density = Density,
                EthaS = EthaS,
                Gamma = Gamma,
                GS = GS,
                KS = KS,
                KT = KT,
                Q = Q,
                Volume = Volume,
            };
        }
    }

    public class ResultSummary
    {
        public ResultSummary()
        {

        }
        public string Name { get; set; }
        public double GivenP { get; set; }
        public double GivenT { get; set; }
        public double KS { get; set; }
        public double KT { get; set; }
        public double GS { get; set; }

        public double Volume { get; set; }
        public double Density { get; set; }
        public double DebyeTemp { get; set; }
        public double Gamma { get; set; }
        public double Alpha { get; set; }
        public double EthaS { get; set; }
        public double Q { get; set; }

        public double Vb
        {
            get
            {
                return Math.Sqrt(KS / Density);
            } 
        }
        public double Vs
        {
            get
            {
                return Math.Sqrt(GS / Density);
            }
        }
        public double Vp
        {
            get
            {
                return Math.Sqrt((KS + 4.0d / 3.0d * GS) / Density);
            }
        }

        public static string ColumnsCSV
        {
            get
            {
                return $"P[GPa], T[K], Vp[m/s], Vs[m/s], Vb[m/s], ρ[g/cm3], V[cm3/mol], KS[GPa], KT[GPa], GS[GPa], α[K-1], θd[K], γ, ηs, q";
            }
        }

        public static List<string> ColumnStrs
        {
            get
            {
                return new List<string> { "P[GPa]", "T[K]", "Vp[m/s]", "Vs[m/s]", "Vb[m/s]", "ρ[g/cm3]", "V[cm3/mol]", "KS[GPa]", "KT[GPa]", "GS[GPa]", "α[K-1]",  "θd[K]", "γ", "ηs", "q" };
            }
        }

        public string ExportSummaryAsColumn()
        {
            return $"{GivenP}, {GivenT}, {Vp}, {Vs}, {Vb}, {Density}, {Volume}, {KS}, {KT}, {GS}, {Alpha}, {DebyeTemp}, {Gamma}, {EthaS}, {Q}";
        }

        public string ExportSummaryAsJson()
        {
            return JsonSerializer.Serialize<ResultSummary>(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class MieGruneisenEOSOptimizer
    {
        public MieGruneisenEOSOptimizer(MineralParams mineral, double givenPressure, double givenTemp)
        {
            __mineral = mineral;
            __givenPressure = givenPressure;
            __givenTemp = givenTemp;
        }

        public MieGruneisenEOSOptimizer(MineralParams mineral, PTData ptData)
        {
            __mineral = mineral;
            __givenPressure = ptData.Pressure;
            __givenTemp = ptData.Temperature;
        }

        private MineralParams __mineral;
        private double __givenTemp;
        private double __givenPressure;

        private double getPressureUnderGivenT(double finite)
        {
            var th = new ThermoMineralParams(__mineral.FiniteToVolume(finite), __givenTemp, __mineral);
            return th.Pressure;
        }

        public ThermoMineralParams ExecOptimize()
        {
            double finite;
            double refPressure = __givenPressure;
            while (true)
            {
                finite = __mineral.BM3Finite(refPressure);
                var th = new ThermoMineralParams(finite, __givenTemp, __mineral);
                if (Math.Abs(__givenPressure - th.Pressure) < 1e-5)
                {
                    break;
                }
                refPressure = refPressure + (__givenPressure - th.Pressure);
            }
            return new ThermoMineralParams(finite, __givenTemp, __mineral);
        }
    }

}
