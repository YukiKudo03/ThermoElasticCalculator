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
    public class AveMethods
    {
        public static double ReussAverage(List<(double VolumeRatio, double param)> inputParams)
        {
            double sumVolume = inputParams.Sum(param => param.VolumeRatio);
            return inputParams.Sum(param => param.VolumeRatio * param.param / sumVolume);
        }

        public static double VaugtAverage(List<(double VolumeRatio, double param)> inputParams)
        {
            double sumVolume = inputParams.Sum(param => param.VolumeRatio);
            return 1.0d / inputParams.Sum(param => param.VolumeRatio / param.param / sumVolume);
        }

        public static double HillAverage(List<(double VolumeRatio, double param)> inputParams)
        {
            return (ReussAverage(inputParams) + VaugtAverage(inputParams)) / 2.0d;
        }

    }
}
