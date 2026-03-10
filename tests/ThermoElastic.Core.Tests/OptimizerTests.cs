using Xunit;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Core.Tests;

public class OptimizerTests
{
    // f(x) = x^2 - 4 => roots at x = ±2
    private static double QuadraticFunc(double x) => x * x - 4.0;

    [Fact]
    public void ReglaFalsi_FindsRoot()
    {
        var opt = new ReglaFalsiOptimizer(QuadraticFunc, 0.5, 3.0);
        var root = opt.DoOptimize();

        Assert.Equal(2.0, root, 6);
    }

    [Fact]
    public void Bisection_FindsRoot()
    {
        var opt = new BisectionOptimizer(QuadraticFunc, 0.5, 3.0);
        var root = opt.DoOptimize();

        Assert.Equal(2.0, root, 6);
    }

    [Fact]
    public void OptimizerFactory_CreatesCorrectType()
    {
        var regla = OptimizerFactory.CreateOptimizer(QuadraticFunc, 0.5, 3.0, OptimizerType.ReglaFalsi);
        var secant = OptimizerFactory.CreateOptimizer(QuadraticFunc, 0.5, 3.0, OptimizerType.Secant);
        var bisect = OptimizerFactory.CreateOptimizer(QuadraticFunc, 0.5, 3.0, OptimizerType.Bisection);

        Assert.IsType<ReglaFalsiOptimizer>(regla);
        Assert.IsType<SecantOptimizer>(secant);
        Assert.IsType<BisectionOptimizer>(bisect);
    }

    [Fact]
    public void AllBracketingOptimizers_ConvergeToSameRoot()
    {
        var regla = new ReglaFalsiOptimizer(QuadraticFunc, 0.5, 3.0).DoOptimize();
        var bisect = new BisectionOptimizer(QuadraticFunc, 0.5, 3.0).DoOptimize();

        Assert.Equal(regla, bisect, 5);
    }
}
