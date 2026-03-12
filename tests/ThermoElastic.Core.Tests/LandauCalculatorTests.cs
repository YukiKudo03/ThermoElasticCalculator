using Xunit;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Core.Tests;

public class LandauCalculatorTests
{
    [Fact]
    public void GetOrderParameter_AboveTc_ReturnsZero()
    {
        double Q = LandauCalculator.GetOrderParameter(900, 847);
        Assert.Equal(0.0, Q);
    }

    [Fact]
    public void GetOrderParameter_AtZeroK_ReturnsOne()
    {
        double Q = LandauCalculator.GetOrderParameter(0.001, 847);
        Assert.True(Q > 0.99 && Q <= 1.0);
    }

    [Fact]
    public void GetFreeEnergy_WhenTc0IsZero_ReturnsZero()
    {
        double F = LandauCalculator.GetFreeEnergy(500, 0, 5.164);
        Assert.Equal(0.0, F);
    }

    [Fact]
    public void GetOrderParameter_QuartzTransition_ShowsDiscontinuity()
    {
        // α-β quartz transition at Tc=847K
        double Tc = 847.0;
        double Q_below = LandauCalculator.GetOrderParameter(846, Tc);
        double Q_above = LandauCalculator.GetOrderParameter(848, Tc);

        Assert.True(Q_below > 0);
        Assert.Equal(0.0, Q_above);
    }

    [Fact]
    public void GetTc_PressureShift_IsCorrect()
    {
        // Tc(P) = Tc0 + VD*P/SD
        double Tc0 = 847.0;
        double VD = 1.222;
        double SD = 5.164;
        double P = 10.0; // GPa

        double Tc = LandauCalculator.GetTc(P, Tc0, VD, SD);
        double expected = Tc0 + VD * P / SD;
        Assert.Equal(expected, Tc, 6);
        Assert.True(Tc > Tc0); // pressure increases Tc
    }
}
