using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thermo_dynamics
{

    public class PhysicConstants
    {
        public static double GasConst = 8.31477d;
        public static double NA = 6.02e23;
        public static double Boltzman = 1.38e-23;
        public static double Plank = 6.63e-34;
        public double PiPlank = Plank / (2.0d * Math.PI);

        public static double RefTemperature = 300.0d;
    }
    public class CommonMethods
    {
        public static bool DoubleEquals(double val1, double val2)
        {
            return (Math.Abs(val2 - val1) < 1.0e-5);
        }
        public static double GetDensity(double elem1Ratio, ResultSummary elem1, ResultSummary elem2)
        {
            return (elem1Ratio * elem1.Volume * elem1.Density + (1.0d - elem1Ratio) * elem2.Volume * elem2.Density) / (elem1Ratio * elem1.Volume + (1.0d - elem1Ratio) * elem2.Volume);
        }
    }
}
