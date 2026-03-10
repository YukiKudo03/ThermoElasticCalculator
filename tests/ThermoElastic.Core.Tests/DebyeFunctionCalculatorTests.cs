using Xunit;
using ThermoElastic.Core.Calculations;

namespace ThermoElastic.Core.Tests;

public class DebyeFunctionCalculatorTests
{
    [Fact]
    public void GetInternalEnergy_AtRoomTemp_ReturnsPositive()
    {
        var calc = new DebyeFunctionCalculator(809.0);
        var energy = calc.GetInternalEnergy(300.0);

        Assert.True(energy > 0);
    }

    [Fact]
    public void GetInternalEnergy_SameTemp_ReturnsSameValue()
    {
        var calc = new DebyeFunctionCalculator(809.0);
        var e1 = calc.GetInternalEnergy(300.0);
        var e2 = calc.GetInternalEnergy(300.0);

        Assert.Equal(e1, e2, 10);
    }

    [Fact]
    public void GetCv_AtRoomTemp_ReturnsPositive()
    {
        var calc = new DebyeFunctionCalculator(809.0);
        var cv = calc.GetCv(300.0);

        Assert.True(cv > 0);
    }

    [Fact]
    public void GetCv_HighTemp_ApproachesDulongPetit()
    {
        var calc = new DebyeFunctionCalculator(809.0);
        var cvHigh = calc.GetCv(5000.0);

        // Dulong-Petit limit: Cv -> 3R per atom ≈ 24.94 J/(mol·K)
        // At very high T, Cv should be close to 3R = 24.94
        Assert.True(cvHigh > 20.0);
        Assert.True(cvHigh < 30.0);
    }

    [Fact]
    public void GetCv_DifferentDebyeTemps_VaryResult()
    {
        var calc1 = new DebyeFunctionCalculator(500.0);
        var calc2 = new DebyeFunctionCalculator(1000.0);
        var cv1 = calc1.GetCv(300.0);
        var cv2 = calc2.GetCv(300.0);

        // Lower Debye temp -> closer to classical limit at same T
        Assert.True(cv1 > cv2);
    }
}
