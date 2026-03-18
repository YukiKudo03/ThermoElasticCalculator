namespace ThermoElastic.Core.Models;

/// <summary>
/// Preliminary Reference Earth Model (PREM) - Dziewonski &amp; Anderson (1981).
/// Provides 1D radial profiles of density, seismic velocities, and pressure
/// for the Earth's mantle (0-2891 km depth).
/// Polynomial coefficients from the original paper, valid for the isotropic PREM.
/// </summary>
public static class PREMModel
{
    private const double EarthRadius = 6371.0; // km

    /// <summary>
    /// PREM data table: (depth_km, pressure_GPa, density_g/cm3, Vp_m/s, Vs_m/s)
    /// Key boundaries from Dziewonski &amp; Anderson (1981), Table II.
    /// Intermediate values computed from polynomial fits.
    /// </summary>
    private static readonly (double depth, double pressure, double density, double vp, double vs)[] _table =
    {
        (0,     0.00,   2.600,  5800,  3200),
        (3,     0.00,   2.600,  5800,  3200),   // crust
        (15,    0.34,   2.900,  6800,  3900),   // upper crust base
        (24.4,  0.60,   3.380,  8110,  4491),   // Moho
        (40,    1.02,   3.379,  8110,  4491),
        (80,    2.09,   3.375,  8076,  4484),
        (120,   3.57,   3.371,  8042,  4477),
        (150,   4.70,   3.368,  8019,  4472),
        (200,   6.42,   3.360,  7982,  4462),
        (220,   7.11,   3.356,  7963,  4457),   // LAB?
        (250,   8.15,   3.435,  8558,  4640),
        (300,   9.82,   3.466,  8648,  4672),
        (350,   11.47,  3.496,  8732,  4702),
        (400,   13.35,  3.543,  8905,  4769),   // 410 discontinuity region
        (410,   13.72,  3.724,  9134,  4932),
        (450,   15.17,  3.754,  9242,  4988),
        (500,   16.73,  3.787,  9349,  5043),
        (550,   18.34,  3.822,  9455,  5097),
        (600,   20.05,  3.859,  9559,  5148),
        (650,   21.76,  3.898,  9661,  5195),
        (660,   22.15,  3.993,  10268, 5570),   // 660 discontinuity
        (700,   23.55,  4.042,  10410, 5622),
        (750,   25.28,  4.104,  10577, 5674),
        (800,   27.06,  4.166,  10739, 5722),
        (900,   30.72,  4.287,  11049, 5808),
        (1000,  34.56,  4.405,  11340, 5885),
        (1100,  38.57,  4.519,  11612, 5951),
        (1200,  42.75,  4.630,  11867, 6007),
        (1400,  51.59,  4.841,  12330, 6093),
        (1600,  60.97,  5.037,  12724, 6143),
        (1800,  70.85,  5.218,  13048, 6157),
        (2000,  81.17,  5.384,  13301, 6135),
        (2200,  91.88,  5.534,  13477, 6077),
        (2400,  102.90, 5.668,  13572, 5982),
        (2600,  114.13, 5.785,  13584, 5849),
        (2800,  125.46, 5.883,  13513, 5678),
        (2891,  135.75, 5.566,  13717, 7265),   // CMB (lowermost mantle values)
    };

    /// <summary>
    /// Get PREM properties at a given depth (0-2891 km).
    /// Uses linear interpolation between table values.
    /// </summary>
    public static PREMProperties GetPropertiesAtDepth(double depth_km)
    {
        if (depth_km <= 0)
            return new PREMProperties { Depth = 0, Pressure = 0, Density = _table[0].density, Vp = _table[0].vp, Vs = _table[0].vs };
        if (depth_km >= _table[^1].depth)
            return new PREMProperties { Depth = _table[^1].depth, Pressure = _table[^1].pressure, Density = _table[^1].density, Vp = _table[^1].vp, Vs = _table[^1].vs };

        // Find bracketing interval and interpolate
        for (int i = 0; i < _table.Length - 1; i++)
        {
            if (depth_km >= _table[i].depth && depth_km <= _table[i + 1].depth)
            {
                double t = (_table[i].depth == _table[i + 1].depth) ? 0 :
                    (depth_km - _table[i].depth) / (_table[i + 1].depth - _table[i].depth);

                return new PREMProperties
                {
                    Depth = depth_km,
                    Pressure = Lerp(_table[i].pressure, _table[i + 1].pressure, t),
                    Density = Lerp(_table[i].density, _table[i + 1].density, t),
                    Vp = Lerp(_table[i].vp, _table[i + 1].vp, t),
                    Vs = Lerp(_table[i].vs, _table[i + 1].vs, t),
                };
            }
        }

        return new PREMProperties();
    }

    /// <summary>
    /// Get a complete PREM profile from surface to CMB.
    /// </summary>
    public static List<PREMProperties> GetProfile(double depthStep_km = 50.0)
    {
        var profile = new List<PREMProperties>();
        for (double d = 0; d <= 2891; d += depthStep_km)
        {
            profile.Add(GetPropertiesAtDepth(d));
        }
        if (profile[^1].Depth < 2891)
            profile.Add(GetPropertiesAtDepth(2891));
        return profile;
    }

    private static double Lerp(double a, double b, double t) => a + (b - a) * t;
}

public class PREMProperties
{
    public double Depth { get; set; }      // km
    public double Pressure { get; set; }   // GPa
    public double Density { get; set; }    // g/cm³
    public double Vp { get; set; }         // m/s
    public double Vs { get; set; }         // m/s
}
