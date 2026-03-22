namespace ThermoElastic.Core.Calculations;
using MathNet.Numerics.LinearAlgebra;
using ThermoElastic.Core.Models;

/// <summary>
/// Calculates directional seismic velocities from elastic tensors using the Christoffel equation.
/// </summary>
public class ElasticTensorCalculator
{
    /// <summary>
    /// Solve the Christoffel equation for seismic velocities along a given direction.
    /// Gamma_ij = C_ijkl * n_k * n_l, eigenvalues give rho*V^2.
    /// Uses simplified form valid for orthorhombic and higher symmetry.
    /// </summary>
    /// <param name="tensor">Elastic tensor</param>
    /// <param name="direction">Propagation direction [nx, ny, nz] (will be normalized)</param>
    /// <returns>(Vp [m/s], Vs1 [m/s], Vs2 [m/s]) sorted descending</returns>
    public (double Vp, double Vs1, double Vs2) SolveChristoffel(ElasticTensor tensor, double[] direction)
    {
        if (direction.Length != 3)
            throw new ArgumentException("Direction must have 3 components.");

        // Normalize direction
        double mag = Math.Sqrt(direction[0] * direction[0] + direction[1] * direction[1] + direction[2] * direction[2]);
        if (mag < 1e-15)
            throw new ArgumentException("Direction vector must be non-zero.");

        double n1 = direction[0] / mag;
        double n2 = direction[1] / mag;
        double n3 = direction[2] / mag;

        var c = tensor.C;

        // Build 3x3 Christoffel matrix (simplified for orthorhombic and higher symmetry)
        // Gamma[i,j] uses Voigt notation indices
        double g00 = c[0, 0] * n1 * n1 + c[5, 5] * n2 * n2 + c[4, 4] * n3 * n3;
        double g11 = c[5, 5] * n1 * n1 + c[1, 1] * n2 * n2 + c[3, 3] * n3 * n3;
        double g22 = c[4, 4] * n1 * n1 + c[3, 3] * n2 * n2 + c[2, 2] * n3 * n3;
        double g01 = (c[0, 1] + c[5, 5]) * n1 * n2;
        double g02 = (c[0, 2] + c[4, 4]) * n1 * n3;
        double g12 = (c[1, 2] + c[3, 3]) * n2 * n3;

        // Build symmetric matrix
        var gamma = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { g00, g01, g02 },
            { g01, g11, g12 },
            { g02, g12, g22 }
        });

        // Eigenvalues = rho * V^2 in GPa
        var evd = gamma.Evd();
        var eigenvalues = evd.EigenValues;

        // Convert eigenvalues to velocities
        // eigenvalue [GPa] / density [g/cm^3] = [GPa / (g/cm^3)]
        // 1 GPa = 1e9 Pa, 1 g/cm^3 = 1e3 kg/m^3
        // So eigenvalue/density [GPa/(g/cm^3)] = 1e9/1e3 [Pa/(kg/m^3)] = 1e6 [m^2/s^2]
        // V = sqrt(eigenvalue/density) * 1e3 [m/s] ... but we want km/s typically
        // Actually: V = sqrt(eigenvalue * 1e9 / (density * 1e3)) = sqrt(eigenvalue/density * 1e6)
        // V [m/s] = sqrt(eigenvalue/density) * 1000

        double rho = tensor.Density;
        var velocities = new double[3];
        for (int i = 0; i < 3; i++)
        {
            double ev = eigenvalues[i].Real;
            if (ev < 0) ev = 0; // numerical safety
            velocities[i] = Math.Sqrt(ev / rho) * 1000.0; // m/s
        }

        // Sort descending: Vp > Vs1 > Vs2
        Array.Sort(velocities);
        Array.Reverse(velocities);

        return (velocities[0], velocities[1], velocities[2]);
    }

    /// <summary>
    /// Compute maximum P-wave anisotropy by sampling many directions on a sphere.
    /// Uses Fibonacci sphere sampling for uniform coverage.
    /// </summary>
    /// <param name="tensor">Elastic tensor</param>
    /// <param name="nDirections">Number of sampling directions</param>
    /// <returns>Anisotropy percentage = (Vp_max - Vp_min) / Vp_mean * 100</returns>
    public double ComputeMaxAnisotropy(ElasticTensor tensor, int nDirections = 100)
    {
        double vpMax = double.MinValue;
        double vpMin = double.MaxValue;
        double vpSum = 0;

        double goldenRatio = (1.0 + Math.Sqrt(5.0)) / 2.0;

        for (int i = 0; i < nDirections; i++)
        {
            // Fibonacci sphere sampling
            double theta = Math.Acos(1.0 - 2.0 * (i + 0.5) / nDirections);
            double phi = 2.0 * Math.PI * i / goldenRatio;

            double n1 = Math.Sin(theta) * Math.Cos(phi);
            double n2 = Math.Sin(theta) * Math.Sin(phi);
            double n3 = Math.Cos(theta);

            var (vp, _, _) = SolveChristoffel(tensor, new[] { n1, n2, n3 });

            if (vp > vpMax) vpMax = vp;
            if (vp < vpMin) vpMin = vp;
            vpSum += vp;
        }

        double vpMean = vpSum / nDirections;
        return (vpMax - vpMin) / vpMean * 100.0;
    }

    /// <summary>
    /// Compute VRH (Voigt-Reuss-Hill) average isotropic moduli from elastic tensor.
    /// </summary>
    /// <param name="tensor">Elastic tensor</param>
    /// <returns>(K_VRH [GPa], G_VRH [GPa])</returns>
    public (double K_VRH, double G_VRH) ComputeVRH(ElasticTensor tensor)
    {
        var c = tensor.C;

        // Voigt averages
        double kV = (c[0, 0] + c[1, 1] + c[2, 2] + 2.0 * (c[0, 1] + c[0, 2] + c[1, 2])) / 9.0;
        double gV = ((c[0, 0] + c[1, 1] + c[2, 2]) - (c[0, 1] + c[0, 2] + c[1, 2])
                      + 3.0 * (c[3, 3] + c[4, 4] + c[5, 5])) / 15.0;

        // Reuss averages: need compliance tensor S = C^(-1)
        var cMatrix = Matrix<double>.Build.DenseOfArray(c);
        var sMatrix = cMatrix.Inverse();
        var s = sMatrix.ToArray();

        double kR = 1.0 / ((s[0, 0] + s[1, 1] + s[2, 2]) + 2.0 * (s[0, 1] + s[0, 2] + s[1, 2]));
        double gR = 15.0 / (4.0 * (s[0, 0] + s[1, 1] + s[2, 2]) - 4.0 * (s[0, 1] + s[0, 2] + s[1, 2])
                             + 3.0 * (s[3, 3] + s[4, 4] + s[5, 5]));

        // Hill averages
        double kH = (kV + kR) / 2.0;
        double gH = (gV + gR) / 2.0;

        return (kH, gH);
    }
}
