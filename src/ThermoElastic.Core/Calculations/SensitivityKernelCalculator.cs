using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

public class SensitivityKernelCalculator
{
    /// <summary>
    /// Compute thermal sensitivity of seismic velocities for a single mineral.
    /// Uses central finite differences with dT step.
    /// </summary>
    public SensitivityKernel ComputeThermalSensitivity(MineralParams mineral, double P, double T, double dT = 5.0)
    {
        var resultPlus = new MieGruneisenEOSOptimizer(mineral, P, T + dT).ExecOptimize();
        var resultMinus = new MieGruneisenEOSOptimizer(mineral, P, T - dT).ExecOptimize();

        double dlnVp_dT = (Math.Log(resultPlus.Vp) - Math.Log(resultMinus.Vp)) / (2.0 * dT);
        double dlnVs_dT = (Math.Log(resultPlus.Vs) - Math.Log(resultMinus.Vs)) / (2.0 * dT);
        double dlnRho_dT = (Math.Log(resultPlus.Density) - Math.Log(resultMinus.Density)) / (2.0 * dT);
        double R_thermal = Math.Abs(dlnVp_dT) > 1e-20 ? dlnVs_dT / dlnVp_dT : double.NaN;

        return new SensitivityKernel
        {
            DlnVp_dT = dlnVp_dT,
            DlnVs_dT = dlnVs_dT,
            DlnRho_dT = dlnRho_dT,
            R_thermal = R_thermal,
            Pressure = P,
            Temperature = T,
        };
    }

    /// <summary>
    /// Compute sensitivities over a pressure array at fixed temperature.
    /// </summary>
    public List<SensitivityKernel> ComputeOnPressureProfile(MineralParams mineral, double[] pressures, double T, double dT = 5.0)
    {
        var results = new List<SensitivityKernel>(pressures.Length);
        foreach (var P in pressures)
        {
            results.Add(ComputeThermalSensitivity(mineral, P, T, dT));
        }
        return results;
    }
}
