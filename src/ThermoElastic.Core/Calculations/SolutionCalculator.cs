using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Calculates thermodynamic properties of solid solutions:
/// ideal configurational entropy, excess Gibbs energy (van Laar model),
/// activity coefficients, and chemical potentials.
/// </summary>
public static class SolutionCalculator
{
    /// <summary>
    /// Ideal configurational entropy: S_conf = -R * Σ_sites [m_s * Σ_j (x_j * ln(x_j))]
    /// For a simple binary A-B solution on one site with multiplicity m:
    /// S_conf = -m * R * [x*ln(x) + (1-x)*ln(1-x)]
    /// </summary>
    public static double GetIdealEntropy(double[] x, List<SolutionSite> sites)
    {
        double R = PhysicConstants.GasConst;
        double sConf = 0.0;

        foreach (var site in sites)
        {
            double siteEntropy = 0.0;
            // Compute site occupancies from endmember compositions
            // For simple solutions, occupancy on the site equals the endmember fraction
            if (site.Occupancies.Count > 0)
            {
                // Complex site model: compute site fractions from endmember fractions
                int nSpecies = site.Occupancies.Values.First().Length;
                double[] siteFractions = new double[nSpecies];
                for (int i = 0; i < x.Length; i++)
                {
                    if (site.Occupancies.TryGetValue(i, out var occ))
                    {
                        for (int s = 0; s < nSpecies; s++)
                            siteFractions[s] += x[i] * occ[s];
                    }
                }
                foreach (var frac in siteFractions)
                {
                    if (frac > 1e-15)
                        siteEntropy -= frac * Math.Log(frac);
                }
            }
            else
            {
                // Simple: each endmember maps directly to a site species
                foreach (var xi in x)
                {
                    if (xi > 1e-15)
                        siteEntropy -= xi * Math.Log(xi);
                }
            }
            sConf += site.Multiplicity * siteEntropy;
        }

        return R * sConf; // J/mol/K
    }

    /// <summary>
    /// Excess Gibbs energy using asymmetric van Laar model:
    /// G_ex = Σ_{a&lt;b} φ_a * φ_b * B_ab
    /// where φ_a = x_a*d_a / Σ(x_i*d_i), B_ab = d_a*d_b/(d_a+d_b) * W_ab
    /// For symmetric case (d_a = d_b), reduces to G_ex = x_a*x_b*W_ab
    /// Result in kJ/mol.
    /// </summary>
    public static double GetExcessGibbs(double[] x, List<InteractionParam> interactions)
    {
        if (interactions.Count == 0) return 0.0;

        // Collect all size parameters
        int n = x.Length;
        double[] d = new double[n];
        for (int i = 0; i < n; i++) d[i] = 1.0; // default size

        foreach (var ip in interactions)
        {
            if (ip.EndmemberA < n) d[ip.EndmemberA] = ip.SizeA;
            if (ip.EndmemberB < n) d[ip.EndmemberB] = ip.SizeB;
        }

        // Compute volume fractions φ
        double sumXD = 0.0;
        for (int i = 0; i < n; i++) sumXD += x[i] * d[i];
        if (sumXD < 1e-15) return 0.0;

        double[] phi = new double[n];
        for (int i = 0; i < n; i++) phi[i] = x[i] * d[i] / sumXD;

        // Sum interactions
        double gEx = 0.0;
        foreach (var ip in interactions)
        {
            int a = ip.EndmemberA;
            int b = ip.EndmemberB;
            if (a >= n || b >= n) continue;
            double dA = d[a];
            double dB = d[b];
            double B = dA * dB / (dA + dB) * ip.W;
            gEx += phi[a] * phi[b] * B;
        }

        return gEx; // kJ/mol
    }

    /// <summary>
    /// Activity coefficients γ_i from excess Gibbs energy.
    /// ln(γ_i) = (∂(n*G_ex)/∂n_i - G_ex) / RT
    /// Computed by numerical differentiation.
    /// </summary>
    public static double[] GetActivityCoefficients(double[] x, List<InteractionParam> interactions, double T)
    {
        double R = PhysicConstants.GasConst;
        int n = x.Length;
        double[] gamma = new double[n];
        double gEx0 = GetExcessGibbs(x, interactions);

        double dx = 1e-6;
        for (int i = 0; i < n; i++)
        {
            // Partial derivative by finite difference
            double[] xPlus = (double[])x.Clone();
            xPlus[i] += dx;
            // Renormalize
            double sum = xPlus.Sum();
            for (int j = 0; j < n; j++) xPlus[j] /= sum;

            double gExPlus = GetExcessGibbs(xPlus, interactions);
            double dlnGamma = (gExPlus - gEx0) / dx * 1000.0 / (R * T); // kJ→J
            gamma[i] = Math.Exp(dlnGamma);
        }

        return gamma;
    }

    /// <summary>
    /// Chemical potential of endmember i:
    /// μ_i = G_i(P,T) + RT*ln(x_i) + RT*ln(γ_i)
    /// Result in kJ/mol.
    /// </summary>
    public static double GetChemicalPotential(int i, double[] x, List<InteractionParam> interactions,
        double gibbsI_kJ, double T)
    {
        double R = PhysicConstants.GasConst;
        double[] gamma = GetActivityCoefficients(x, interactions, T);

        double xi = x[i];
        if (xi < 1e-15) return double.PositiveInfinity;

        return gibbsI_kJ + R * T * Math.Log(xi * gamma[i]) / 1000.0; // J→kJ
    }

    /// <summary>
    /// Linear interpolation of EOS parameters for a solid solution at given composition.
    /// V = Σ x_i * V_i, K = Σ x_i * K_i, etc.
    /// </summary>
    public static MineralParams GetEffectiveParams(double[] x, List<MineralParams> endmembers)
    {
        if (x.Length != endmembers.Count)
            throw new ArgumentException("Composition array length must match endmember count");

        var eff = new MineralParams();
        eff.MineralName = "SolidSolution";
        eff.NumAtoms = endmembers[0].NumAtoms; // same structure

        for (int i = 0; i < x.Length; i++)
        {
            var m = endmembers[i];
            eff.MolarVolume += x[i] * m.MolarVolume;
            eff.MolarWeight += x[i] * m.MolarWeight;
            eff.KZero += x[i] * m.KZero;
            eff.K1Prime += x[i] * m.K1Prime;
            eff.K2Prime += x[i] * m.K2Prime;
            eff.GZero += x[i] * m.GZero;
            eff.G1Prime += x[i] * m.G1Prime;
            eff.G2Prime += x[i] * m.G2Prime;
            eff.DebyeTempZero += x[i] * m.DebyeTempZero;
            eff.GammaZero += x[i] * m.GammaZero;
            eff.QZero += x[i] * m.QZero;
            eff.EhtaZero += x[i] * m.EhtaZero;
            eff.F0 += x[i] * m.F0;
        }

        return eff;
    }

    /// <summary>
    /// Rigorous solid solution property calculation: compute each endmember at P,T
    /// individually, then mix the results (volume-weighted).
    /// This avoids the approximation of linear EOS parameter interpolation.
    /// </summary>
    public static ResultSummary? ComputeRigorousSolution(
        double[] x, List<MineralParams> endmembers, double P, double T)
    {
        if (x.Length != endmembers.Count) return null;

        var results = new List<(double ratio, ResultSummary result)>();
        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] < 1e-15) continue;
            try
            {
                var opt = new MieGruneisenEOSOptimizer(endmembers[i], Math.Max(P, 0.0001), T);
                var thermo = opt.ExecOptimize();
                results.Add((x[i], thermo.ExportResults()));
            }
            catch
            {
                return null;
            }
        }

        if (results.Count == 0) return null;

        // Volume-weighted mixing (Voigt-Reuss-Hill average)
        var mixer = new MixtureCalculator(results);
        var mixed = mixer.HillAverage();
        if (mixed != null)
        {
            mixed.Name = "SolidSolution";
            mixed.GivenP = P;
            mixed.GivenT = T;
        }
        return mixed;
    }

    /// <summary>
    /// Validates that composition fractions sum to 1.0 (within tolerance).
    /// </summary>
    public static bool ValidateComposition(double[] x, double tolerance = 1e-6)
    {
        if (x.Length == 0) return false;
        double sum = x.Sum();
        return Math.Abs(sum - 1.0) < tolerance && x.All(xi => xi >= 0);
    }
}
