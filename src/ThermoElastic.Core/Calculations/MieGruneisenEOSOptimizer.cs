using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

public class MieGruneisenEOSOptimizer
{
    public MieGruneisenEOSOptimizer(MineralParams mineral, double givenPressure, double givenTemp)
    {
        _mineral = mineral;
        _givenPressure = givenPressure;
        _givenTemp = givenTemp;
    }

    public MieGruneisenEOSOptimizer(MineralParams mineral, PTData ptData)
    {
        _mineral = mineral;
        _givenPressure = ptData.Pressure;
        _givenTemp = ptData.Temperature;
    }

    private readonly MineralParams _mineral;
    private readonly double _givenTemp;
    private readonly double _givenPressure;

    public ThermoMineralParams ExecOptimize()
    {
        double finite;
        double refPressure = _givenPressure;
        while (true)
        {
            finite = _mineral.BM3Finite(refPressure);
            var th = new ThermoMineralParams(finite, _givenTemp, _mineral);
            if (Math.Abs(_givenPressure - th.Pressure) < 1e-5)
            {
                break;
            }
            refPressure = refPressure + (_givenPressure - th.Pressure);
        }
        return new ThermoMineralParams(finite, _givenTemp, _mineral);
    }
}
