using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thermo_dynamics
{
    public abstract class Optimizer
    {
        public Optimizer(Func<double, double> func, double initMin, double initMax, double thresh = 1e-8)
        {
            OptimizeFunction = func;
            InitMin = initMin;
            InitMax = initMax;
            Thresh = thresh;
        }
        public Func<double, double> OptimizeFunction { get; set;  }

        protected double Thresh = 1e-8;
        public double InitMin { get; set; }
        public double InitMax { get; set; }
        public virtual double DoOptimize()
        {
            return 0.0d;
        }
    }

    public class ReglaFalsiOptimizer : Optimizer
    {
        public ReglaFalsiOptimizer(Func<double, double>func, double initMin, double initMax, double thresh=1e-8):base(func,initMin, initMax, thresh)
        {

        }        

        public override double DoOptimize()
        {
            var low = InitMin;
            var high = InitMax;
            var fa = OptimizeFunction(InitMin);
            var fb = OptimizeFunction(InitMax);
            double x;
            while (true)
            {
                x = (low * OptimizeFunction(high) - high * OptimizeFunction(low)) / (OptimizeFunction(high) - OptimizeFunction(low));
                double fx = OptimizeFunction(x);
                if(Math.Abs(fx) < Thresh)
                {
                    break;
                }

                if((fx > 0) == (fa > 0))
                {
                    low = x;
                    fa = fx;
                }
                else
                {
                    high = x;
                    fb = fx;
                }
            }
            return x;
        }
    }

    public class SecantOptimizer : Optimizer
    {
        public SecantOptimizer(Func<double,double>func, double initMin, double initMax, double thresh = 1e-8) : base(func, initMin, initMax, thresh)
        {

        }

        private double df(double a, double b)
        {
            return OptimizeFunction(b) * (b - a) / (OptimizeFunction(b) - OptimizeFunction(a));
        }

        public override double DoOptimize()
        {
            double x0 = InitMin;
            double x1 = InitMax;
            while (true)
            {
                double x2 = x1 - df(x1, x0);
                if(Math.Abs(x2-x1) < Thresh)
                {
                    break;
                }
                x0 = x1;
                x1 = x2;
            }
            return x1;
        }
    }

    public class BisectionOptimizer : Optimizer
    {
        public BisectionOptimizer(Func<double,double>func, double initMin, double initMax, double thresh = 1e-8) : base(func,initMin, initMax, thresh)
        {

        }

        public override double DoOptimize()
        {
            double low = InitMin;
            double high = InitMax;
            double fa = OptimizeFunction(low);
            double fb = OptimizeFunction(high);

            double x;
            while (true)
            {
                x = (low + high) / 2.0d;
                double fx = OptimizeFunction(x);
                if(Math.Abs(fx) < Thresh)
                {
                    break;
                }
                if((fx > 0) == (fa > 0))
                {
                    low = x;
                    fa = fx;
                }
                else
                {
                    high = x;
                    fb = fx;
                }
            }
            return x;
        }
    }
    public enum OptimizerType
    {
        ReglaFalsi = 0,
        Secant = 1,
        Bisection = 2,
    }

    public class OptimizerFactory
    {
        public static Optimizer CreateOptimizer(Func<double, double> func, double initMin, double initMax, OptimizerType type)
        {
            var dic = new Dictionary<OptimizerType, Optimizer>
            {
                {OptimizerType.ReglaFalsi, new ReglaFalsiOptimizer(func, initMin, initMax) },
                {OptimizerType.Secant, new SecantOptimizer(func, initMin, initMax) },
                {OptimizerType.Bisection, new BisectionOptimizer(func, initMin, initMax) },
            };
            return dic[type];
        }
    }


}
