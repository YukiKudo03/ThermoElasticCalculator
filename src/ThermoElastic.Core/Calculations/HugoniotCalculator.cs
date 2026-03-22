using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Computes Hugoniot curves from the Mie-Gruneisen EOS using the Rankine-Hugoniot relation:
///   E_H - E_0 = 0.5 * (P_H + P_0) * (V_0 - V_H)
/// where E_H is internal energy on the Hugoniot and E_0, P_0, V_0 are initial conditions.
///
/// For each compressed volume V_H, iterates on temperature T until the Rankine-Hugoniot
/// energy balance is satisfied.
///
/// References: Duffy &amp; Ahrens (1995), Mosenfelder et al. (2009), McQueen et al. (1967).
/// </summary>
public class HugoniotCalculator
{
    private readonly MineralParams _mineral;
    private readonly double _v0;
    private readonly double _t0;
    private readonly double _p0;
    private readonly double _e0;

    /// <summary>
    /// Initialize Hugoniot calculator for a mineral starting from ambient conditions.
    /// </summary>
    /// <param name="mineral">Mineral parameters (SLB2011)</param>
    /// <param name="t0">Initial temperature [K] (default 300 K)</param>
    /// <param name="p0">Initial pressure [GPa] (default ~0 GPa)</param>
    public HugoniotCalculator(MineralParams mineral, double t0 = 300.0, double p0 = 0.0001)
    {
        _mineral = mineral;
        _v0 = mineral.MolarVolume;
        _t0 = t0;
        _p0 = p0;

        // Compute reference internal energy at (V0, T0)
        double f0 = VolumeToFinite(_v0);
        var thermo0 = new ThermoMineralParams(f0, _t0, _mineral);
        _e0 = ComputeInternalEnergy(thermo0);
    }

    /// <summary>
    /// Compute the Hugoniot curve from V/V0 = 1.0 down to maxCompression.
    /// </summary>
    /// <param name="nPoints">Number of points on the curve</param>
    /// <param name="maxCompression">Minimum V/V0 ratio (e.g. 0.6 = 40% compression)</param>
    /// <returns>List of HugoniotPoint from least to most compressed</returns>
    public List<HugoniotPoint> ComputeHugoniot(int nPoints = 20, double maxCompression = 0.6)
    {
        if (nPoints < 2)
            throw new ArgumentOutOfRangeException(nameof(nPoints), "Must be >= 2.");

        var points = new List<HugoniotPoint>();

        // Volume steps from V0 down to V0 * maxCompression
        double vMin = _v0 * maxCompression;
        double dv = (_v0 - vMin) / (nPoints - 1);

        for (int i = 0; i < nPoints; i++)
        {
            double vH = _v0 - i * dv;
            if (vH <= 0) break;

            var point = ComputeHugoniotAtVolume(vH);
            if (point != null)
                points.Add(point);
        }

        return points;
    }

    /// <summary>
    /// Compute a single Hugoniot point at a given volume by iterating on temperature.
    /// Uses bisection to find T where the Rankine-Hugoniot energy balance is satisfied.
    /// </summary>
    private HugoniotPoint? ComputeHugoniotAtVolume(double vH)
    {
        double finite = VolumeToFinite(vH);

        // For V = V0 (no compression), return initial conditions
        if (Math.Abs(vH - _v0) < 1e-6)
        {
            var thermo0 = new ThermoMineralParams(finite, _t0, _mineral);
            return new HugoniotPoint(_v0)
            {
                Volume = vH,
                Pressure = thermo0.Pressure,
                Temperature = _t0,
                Density = _mineral.MolarWeight / vH,
                Us = 0,
                Up = 0,
                InternalEnergy = _e0,
            };
        }

        // Bisection: find T where E_actual(V_H, T) = E_0 + 0.5*(P_H(V_H,T) + P_0)*(V_0 - V_H)
        double tLow = 300.0;
        double tHigh = 20000.0;
        double tMid = 300.0;
        double bestT = 300.0;
        double bestResidual = double.MaxValue;

        // Check if Hugoniot energy balance can be satisfied
        // At T=300 K (cold), E_actual is just cold compression energy
        // Hugoniot requires more energy (thermal contribution from shock heating)
        for (int iter = 0; iter < 100; iter++)
        {
            tMid = (tLow + tHigh) / 2.0;

            double residual = HugoniotResidual(vH, finite, tMid);

            if (Math.Abs(residual) < Math.Abs(bestResidual))
            {
                bestResidual = residual;
                bestT = tMid;
            }

            if (Math.Abs(residual) < 0.01) // 0.01 kJ/mol tolerance
                break;

            // residual = E_actual - E_hugoniot
            // If E_actual > E_hugoniot, temperature is too high
            if (residual > 0)
                tHigh = tMid;
            else
                tLow = tMid;
        }

        // Compute final state at bestT
        var thermo = new ThermoMineralParams(finite, bestT, _mineral);
        double pH = thermo.Pressure;
        double eH = ComputeInternalEnergy(thermo);

        // Compute Us and Up from Rankine-Hugoniot jump conditions
        var (us, up) = ComputeShockVelocities(pH, vH);

        return new HugoniotPoint(_v0)
        {
            Volume = vH,
            Pressure = pH,
            Temperature = bestT,
            Density = _mineral.MolarWeight / vH,
            Us = us,
            Up = up,
            InternalEnergy = eH,
        };
    }

    /// <summary>
    /// Compute the Rankine-Hugoniot energy residual:
    ///   residual = E_actual(V_H, T) - [E_0 + 0.5*(P_H + P_0)*(V_0 - V_H)]
    /// When residual = 0, the Hugoniot condition is satisfied.
    /// </summary>
    private double HugoniotResidual(double vH, double finite, double T)
    {
        var thermo = new ThermoMineralParams(finite, T, _mineral);
        double pH = thermo.Pressure;

        // E_actual: total internal energy at (V_H, T) relative to reference
        double eActual = ComputeInternalEnergy(thermo);

        // E_hugoniot: energy required by Rankine-Hugoniot relation
        // E_H = E_0 + 0.5 * (P_H + P_0) * (V_0 - V_H)
        // P in GPa, V in cm³/mol → PV in GPa·cm³/mol = kJ/mol
        double eHugoniot = _e0 + 0.5 * (pH + _p0) * (_v0 - vH);

        return eActual - eHugoniot;
    }

    /// <summary>
    /// Compute internal energy from ThermoMineralParams.
    /// E = F_cold + DeltaE (thermal energy change from Debye model).
    /// FCold is in kJ/mol, DeltaE is in J/mol (needs /1000).
    /// </summary>
    private static double ComputeInternalEnergy(ThermoMineralParams thermo)
    {
        // FCold: cold compression energy [kJ/mol]
        // DeltaE: thermal energy relative to reference T [J/mol], convert to kJ/mol
        return thermo.FCold + thermo.DeltaE / 1000.0;
    }

    /// <summary>
    /// Compute shock velocity Us and particle velocity Up from Rankine-Hugoniot jump conditions.
    /// Uses specific volume (per unit mass) for proper unit conversion.
    /// </summary>
    /// <param name="pH">Hugoniot pressure [GPa]</param>
    /// <param name="vH">Molar volume at Hugoniot [cm³/mol]</param>
    /// <returns>(Us [km/s], Up [km/s])</returns>
    private (double us, double up) ComputeShockVelocities(double pH, double vH)
    {
        double dP = pH - _p0; // GPa
        double dV = _v0 - vH; // cm³/mol

        if (dP <= 0 || dV <= 0)
            return (0, 0);

        double M = _mineral.MolarWeight; // g/mol

        // Specific volumes: v = V/M [cm³/g]
        // Convert to m³/kg: 1 cm³/g = 1e-3 m³/kg
        double v0_specific = _v0 / M * 1e-3; // m³/kg
        double vH_specific = vH / M * 1e-3;  // m³/kg
        double dP_Pa = dP * 1e9;             // Pa

        // Rankine-Hugoniot: Up² = (P_H - P_0) * (v_0 - v_H) [m²/s²]
        double up_ms = Math.Sqrt(dP_Pa * (v0_specific - vH_specific));

        // Us = v_0 * (P_H - P_0) / Up [m/s]
        double us_ms = v0_specific * dP_Pa / up_ms;

        // Convert to km/s
        return (us_ms / 1000.0, up_ms / 1000.0);
    }

    /// <summary>
    /// Convert volume to Eulerian finite strain: f = ((V0/V)^(2/3) - 1) / 2
    /// </summary>
    private double VolumeToFinite(double volume)
    {
        return (Math.Pow(_v0 / volume, 2.0 / 3.0) - 1.0) / 2.0;
    }

    /// <summary>
    /// Linear least-squares fit of Us vs Up: Us = c0 + s * Up.
    /// Excludes points with very small Up (near zero compression).
    /// </summary>
    /// <param name="points">Hugoniot curve points</param>
    /// <returns>(c0 [km/s], s [dimensionless]) intercept and slope</returns>
    public static (double c0, double s) FitUsUp(List<HugoniotPoint> points)
    {
        // Filter to points with meaningful particle velocity
        var filtered = points.Where(p => p.Up > 0.1).ToList();
        if (filtered.Count < 2)
            return (0, 0);

        // Linear regression: Us = c0 + s * Up
        int n = filtered.Count;
        double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
        foreach (var pt in filtered)
        {
            sumX += pt.Up;
            sumY += pt.Us;
            sumXX += pt.Up * pt.Up;
            sumXY += pt.Up * pt.Us;
        }

        double denom = n * sumXX - sumX * sumX;
        if (Math.Abs(denom) < 1e-20)
            return (0, 0);

        double slope = (n * sumXY - sumX * sumY) / denom;
        double intercept = (sumY - slope * sumX) / n;

        return (intercept, slope);
    }
}
