using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Computes isentropic (adiabatic) temperature profiles T(P) where S(P,T) = constant.
/// At each pressure step, finds the temperature that preserves the initial entropy
/// using Brent's method root-finding.
/// </summary>
public class IsentropeCalculator
{
    private readonly MineralParams _mineral;

    public IsentropeCalculator(MineralParams mineral)
    {
        _mineral = mineral;
    }

    /// <summary>
    /// Compute an isentropic T(P) profile.
    /// </summary>
    /// <param name="pressureStart">Starting pressure [GPa]</param>
    /// <param name="temperatureStart">Starting temperature [K]</param>
    /// <param name="pressureMax">Maximum pressure [GPa]</param>
    /// <param name="pressureStep">Pressure increment [GPa]</param>
    /// <returns>List of (P, T) points along the isentrope</returns>
    public List<PTData> ComputeIsentrope(double pressureStart, double temperatureStart,
        double pressureMax = 135.0, double pressureStep = 1.0)
    {
        var profile = new List<PTData>();

        // Compute reference entropy at starting conditions
        var opt0 = new MieGruneisenEOSOptimizer(_mineral, Math.Max(pressureStart, 0.0001), temperatureStart);
        var thermo0 = opt0.ExecOptimize();
        double s0 = thermo0.Entropy;

        profile.Add(new PTData { Pressure = pressureStart, Temperature = temperatureStart });

        double currentT = temperatureStart;

        for (double p = pressureStart + pressureStep; p <= pressureMax + 1e-10; p += pressureStep)
        {
            // Find T at this pressure where S(P,T) = S0
            // Use bisection: T is bounded roughly by [currentT - 200, currentT + 500]
            double tLow = Math.Max(100, currentT - 200);
            double tHigh = currentT + 500;

            // Refine bounds
            double sLow = ComputeEntropy(p, tLow);
            double sHigh = ComputeEntropy(p, tHigh);

            // Ensure bracket contains the root
            int attempts = 0;
            while (sLow > s0 && attempts < 5)
            {
                tLow = Math.Max(100, tLow - 200);
                sLow = ComputeEntropy(p, tLow);
                attempts++;
            }
            while (sHigh < s0 && attempts < 10)
            {
                tHigh += 500;
                sHigh = ComputeEntropy(p, tHigh);
                attempts++;
            }

            if ((sLow - s0) * (sHigh - s0) > 0)
            {
                // Can't bracket - use linear extrapolation from adiabatic gradient
                currentT += pressureStep * ComputeAdiabaticGradient(p - pressureStep, currentT);
                profile.Add(new PTData { Pressure = p, Temperature = currentT });
                continue;
            }

            // Bisection to find T where S(P,T) = S0
            for (int i = 0; i < 40; i++)
            {
                double tMid = (tLow + tHigh) / 2.0;
                double sMid = ComputeEntropy(p, tMid);

                if (Math.Abs(sMid - s0) < 0.1) // 0.1 J/mol/K tolerance
                {
                    currentT = tMid;
                    break;
                }

                if ((sLow - s0) * (sMid - s0) < 0)
                    tHigh = tMid;
                else
                {
                    tLow = tMid;
                    sLow = sMid;
                }

                currentT = (tLow + tHigh) / 2.0;
            }

            profile.Add(new PTData { Pressure = p, Temperature = currentT });
        }

        return profile;
    }

    private double ComputeEntropy(double P, double T)
    {
        var opt = new MieGruneisenEOSOptimizer(_mineral, Math.Max(P, 0.0001), T);
        var thermo = opt.ExecOptimize();
        return thermo.Entropy;
    }

    /// <summary>
    /// Adiabatic gradient dT/dP = α*T*V / Cp [K/GPa]
    /// Approximated as γ*T/KS
    /// </summary>
    private double ComputeAdiabaticGradient(double P, double T)
    {
        var opt = new MieGruneisenEOSOptimizer(_mineral, Math.Max(P, 0.0001), T);
        var thermo = opt.ExecOptimize();
        if (thermo.KS < 1e-10) return 10.0; // fallback
        return thermo.Gamma * T / thermo.KS;
    }
}
