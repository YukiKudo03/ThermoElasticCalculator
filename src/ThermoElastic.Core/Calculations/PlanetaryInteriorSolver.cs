namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

/// <summary>
/// Solves for planetary interior structure by integrating hydrostatic equilibrium.
/// dP/dr = -rho*g, dM/dr = 4*pi*r^2*rho, with adiabatic temperature profile.
/// </summary>
public class PlanetaryInteriorSolver
{
    private const double G_const = 6.674e-11; // Gravitational constant [m^3/(kg*s^2)]

    /// <summary>
    /// Compute radial profile from surface inward using step-wise integration.
    /// Two-pass approach: first estimate density, then integrate pressure.
    /// </summary>
    public RadialProfile Solve(PlanetaryConfig config, int nPoints = 100)
    {
        double R = config.Radius_km * 1e3; // total radius in meters
        double Rcore = config.CoreRadius_km * 1e3;
        double dr = R / nPoints;

        // Arrays indexed from center (i=0) to surface (i=nPoints)
        int n = nPoints + 1;
        double[] r = new double[n];
        double[] rho = new double[n];     // kg/m^3
        double[] mass = new double[n];
        double[] g = new double[n];
        double[] P = new double[n];       // Pa
        double[] T = new double[n];
        double[] vp = new double[n];
        double[] vs = new double[n];

        // Build radial grid from center to surface
        for (int i = 0; i < n; i++)
            r[i] = i * dr;

        // --- Pass 1: estimate density profile and compute mass/gravity ---
        double rhoCore = config.CoreDensity * 1000.0; // g/cm^3 -> kg/m^3
        double rhoMantleGuess = 4.0 * 1000.0; // initial mantle guess kg/m^3

        for (int i = 0; i < n; i++)
        {
            if (r[i] <= Rcore)
                rho[i] = rhoCore;
            else
                rho[i] = rhoMantleGuess;
        }

        // Compute mass enclosed from center outward
        ComputeMassAndGravity(r, rho, mass, g, dr, n);

        // --- Pass 2: integrate pressure from surface inward, compute EOS densities ---
        // Surface boundary: P = 0
        P[n - 1] = 0.0;
        T[n - 1] = config.SurfaceTemperature;

        // Set surface seismic velocities
        if (r[n - 1] > Rcore && config.MantleMineral != null)
        {
            try
            {
                var props = new MieGruneisenEOSOptimizer(config.MantleMineral, 0.0001, T[n - 1]).ExecOptimize();
                rho[n - 1] = props.Density * 1000.0;
                vp[n - 1] = props.Vp;
                vs[n - 1] = props.Vs;
            }
            catch (Exception) // EOS may fail at extreme P-T; use fallback values
            {
                vp[n - 1] = 0; vs[n - 1] = 0;
            }
        }

        // Integrate from surface (i=n-1) inward to center (i=0)
        for (int i = n - 2; i >= 0; i--)
        {
            double depth_km = (R - r[i]) / 1e3;

            if (r[i] <= Rcore)
            {
                // Core region
                rho[i] = rhoCore;
                vp[i] = 0;
                vs[i] = 0;
                T[i] = config.PotentialTemperature * (1.0 + 0.3e-3 * depth_km);
            }
            else if (config.MantleMineral != null)
            {
                // Mantle region: adiabatic T approximation
                T[i] = config.PotentialTemperature * (1.0 + 0.3e-3 * depth_km);

                double P_GPa = P[i + 1] + rho[i + 1] * g[i + 1] * dr / 1e9;
                if (P_GPa < 0.0001) P_GPa = 0.0001;
                P[i] = P_GPa;

                try
                {
                    var props = new MieGruneisenEOSOptimizer(config.MantleMineral, P_GPa, T[i]).ExecOptimize();
                    rho[i] = props.Density * 1000.0; // g/cm^3 -> kg/m^3
                    vp[i] = props.Vp;
                    vs[i] = props.Vs;
                }
                catch (Exception) // EOS may fail at extreme P-T; use fallback values
                {
                    rho[i] = rho[i + 1];
                    vp[i] = 0; vs[i] = 0;
                }
            }
            else
            {
                T[i] = config.PotentialTemperature * (1.0 + 0.3e-3 * depth_km);
                rho[i] = rhoMantleGuess;
            }
        }

        // --- Recompute mass and gravity with updated densities ---
        ComputeMassAndGravity(r, rho, mass, g, dr, n);

        // --- Final pressure integration from surface inward ---
        P[n - 1] = 0.0;
        for (int i = n - 2; i >= 0; i--)
        {
            double dP = rho[i] * g[i] * dr; // Pa
            P[i] = P[i + 1] + dP / 1e9;     // GPa
        }

        // Compute MoI factor
        double totalMass = mass[n - 1];
        double moiFactor = ComputeMoIFactor(r, rho, totalMass, R);

        // Build output profile (from center to surface)
        var profile = new RadialProfile
        {
            PlanetName = config.Name,
            Radius_km = new double[n],
            Depth_km = new double[n],
            Pressure_GPa = new double[n],
            Temperature_K = new double[n],
            Density = new double[n],
            Gravity = new double[n],
            Vp = new double[n],
            Vs = new double[n],
            TotalMass = totalMass,
            MomentOfInertiaFactor = moiFactor,
        };

        for (int i = 0; i < n; i++)
        {
            profile.Radius_km[i] = r[i] / 1e3;
            profile.Depth_km[i] = (R - r[i]) / 1e3;
            profile.Pressure_GPa[i] = P[i];
            profile.Temperature_K[i] = T[i];
            profile.Density[i] = rho[i] / 1000.0; // back to g/cm^3
            profile.Gravity[i] = g[i];
            profile.Vp[i] = vp[i];
            profile.Vs[i] = vs[i];
        }

        return profile;
    }

    private static void ComputeMassAndGravity(double[] r, double[] rho, double[] mass, double[] g, double dr, int n)
    {
        mass[0] = 0;
        g[0] = 0;
        for (int i = 1; i < n; i++)
        {
            double shellVolume = 4.0 * Math.PI * r[i] * r[i] * dr;
            mass[i] = mass[i - 1] + rho[i] * shellVolume;
            g[i] = G_const * mass[i] / (r[i] * r[i]);
        }
    }

    /// <summary>
    /// Compute moment of inertia factor I/(MR^2).
    /// I = 8*pi/3 * integral(rho(r) * r^4 dr)
    /// </summary>
    public static double ComputeMoIFactor(double[] radius_m, double[] density_kgm3, double totalMass, double totalRadius)
    {
        if (radius_m.Length < 2 || totalMass <= 0 || totalRadius <= 0)
            return 0;

        double dr = radius_m[1] - radius_m[0];
        double I = 0;
        for (int i = 1; i < radius_m.Length; i++)
        {
            double r4 = radius_m[i] * radius_m[i] * radius_m[i] * radius_m[i];
            I += density_kgm3[i] * r4 * dr;
        }
        I *= 8.0 * Math.PI / 3.0;

        return I / (totalMass * totalRadius * totalRadius);
    }
}
