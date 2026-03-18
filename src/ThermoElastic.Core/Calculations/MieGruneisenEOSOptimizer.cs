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
        int maxIter = 500;
        for (int i = 0; i < maxIter; i++)
        {
            finite = _mineral.BM3Finite(refPressure);
            var th = new ThermoMineralParams(finite, _givenTemp, _mineral);
            if (Math.Abs(_givenPressure - th.Pressure) < 1e-5)
            {
                th.IsConverged = true;
                th.Iterations = i + 1;
                th.PressureResidual = Math.Abs(_givenPressure - th.Pressure);
                return th;
            }
            refPressure = refPressure + (_givenPressure - th.Pressure);
        }
        // Return best effort if not converged
        finite = _mineral.BM3Finite(refPressure);
        var result = new ThermoMineralParams(finite, _givenTemp, _mineral);
        result.IsConverged = false;
        result.Iterations = maxIter;
        result.PressureResidual = Math.Abs(_givenPressure - result.Pressure);
        return result;
    }
}
