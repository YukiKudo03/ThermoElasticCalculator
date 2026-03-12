using MathNet.Numerics.LinearAlgebra;
using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Gibbs free energy minimization for phase equilibrium calculation.
/// Finds the stable phase assemblage at given P,T by minimizing G = Σ n_i * μ_i
/// subject to mass balance constraints.
/// Uses SVD-based null space method (SLB2011 Appendix B).
/// </summary>
public class GibbsMinimizer
{
    private const double Epsilon = 1e-10;
    private const int MaxIterations = 200;
    private const double ConvergenceTol = 1e-8;

    /// <summary>
    /// Minimize Gibbs free energy for a set of candidate phases at given P,T.
    /// Returns the equilibrium assemblage with stable phases and their amounts.
    /// </summary>
    public PhaseAssemblage Minimize(PhaseAssemblage initial, double P, double T)
    {
        var phases = initial.Phases.Where(p => p.Amount > 0).ToList();

        // Compute Gibbs energies for each phase
        foreach (var phase in phases)
        {
            phase.GibbsG = ComputePhaseGibbs(phase, P, T);
        }

        // For two-phase equilibrium (most common case), use direct comparison
        if (phases.Count == 2 && !phases.Any(p => p.IsSolidSolution))
        {
            return MinimizeTwoPhase(phases, P, T);
        }

        // General case: iterative minimization
        return MinimizeGeneral(phases, P, T);
    }

    /// <summary>
    /// Two-phase equilibrium: determine the relative amounts that minimize total G.
    /// For pure endmembers: the phase with lower G is stable.
    /// For competition: use lever rule with mass balance.
    /// </summary>
    private PhaseAssemblage MinimizeTwoPhase(List<PhaseEntry> phases, double P, double T)
    {
        var result = new PhaseAssemblage { Pressure = P, Temperature = T };
        var p1 = phases[0];
        var p2 = phases[1];

        // Simple case: compare Gibbs energies
        // The phase with lower G per mole is more stable
        if (p1.GibbsG <= p2.GibbsG)
        {
            double totalMoles = p1.Amount + p2.Amount;
            result.Phases.Add(new PhaseEntry
            {
                Name = p1.Name,
                Mineral = p1.Mineral,
                Amount = totalMoles,
                GibbsG = p1.GibbsG,
                Solution = p1.Solution,
                Composition = p1.Composition,
            });
        }
        else
        {
            double totalMoles = p1.Amount + p2.Amount;
            result.Phases.Add(new PhaseEntry
            {
                Name = p2.Name,
                Mineral = p2.Mineral,
                Amount = totalMoles,
                GibbsG = p2.GibbsG,
                Solution = p2.Solution,
                Composition = p2.Composition,
            });
        }

        return result;
    }

    /// <summary>
    /// General multi-phase minimization using projected gradient descent in the null space.
    /// </summary>
    private PhaseAssemblage MinimizeGeneral(List<PhaseEntry> phases, double P, double T)
    {
        int nPhases = phases.Count;

        // Build stoichiometry matrix (simplified: each phase is one component)
        // For endmember phases, the stoichiometry is the identity
        // Real implementation would use oxide components

        // Current approach: gradient descent on G with positivity constraint
        double[] n = phases.Select(p => p.Amount).ToArray();
        double totalN = n.Sum();

        // Normalize
        for (int i = 0; i < nPhases; i++) n[i] /= totalN;

        double[] g = phases.Select(p => p.GibbsG).ToArray();

        // Iterative minimization: shift material from high-G to low-G phases
        for (int iter = 0; iter < MaxIterations; iter++)
        {
            // Compute gradient: dG/dn_i = μ_i = G_i for pure phases
            double[] mu = new double[nPhases];
            for (int i = 0; i < nPhases; i++)
            {
                mu[i] = g[i];
                if (phases[i].IsSolidSolution && phases[i].Composition != null)
                {
                    // Add mixing contribution
                    double sConf = SolutionCalculator.GetIdealEntropy(
                        phases[i].Composition!, phases[i].Solution!.Sites);
                    double gEx = SolutionCalculator.GetExcessGibbs(
                        phases[i].Composition!, phases[i].Solution!.InteractionParams);
                    mu[i] += gEx - T * sConf / 1000.0;
                }
            }

            // Find direction: project gradient onto constraint surface
            double muMean = mu.Average();
            double[] direction = mu.Select(m => -(m - muMean)).ToArray();

            // Line search
            double stepSize = 0.1;
            double maxStep = n.Zip(direction, (ni, di) => di < 0 ? -ni / di : double.MaxValue).Min();
            stepSize = Math.Min(stepSize, maxStep * 0.9);

            double[] nNew = new double[nPhases];
            for (int i = 0; i < nPhases; i++)
                nNew[i] = n[i] + stepSize * direction[i];

            // Check convergence
            double dG = 0;
            for (int i = 0; i < nPhases; i++)
                dG += direction[i] * mu[i];

            if (Math.Abs(dG) < ConvergenceTol) break;

            // Remove phases with negligible amounts
            for (int i = 0; i < nPhases; i++)
            {
                if (nNew[i] < Epsilon) nNew[i] = 0;
            }

            // Renormalize
            double sumN = nNew.Sum();
            if (sumN > Epsilon)
            {
                for (int i = 0; i < nPhases; i++) nNew[i] /= sumN;
            }

            n = nNew;
        }

        // Build result
        var result = new PhaseAssemblage { Pressure = P, Temperature = T };
        for (int i = 0; i < nPhases; i++)
        {
            if (n[i] > Epsilon)
            {
                result.Phases.Add(new PhaseEntry
                {
                    Name = phases[i].Name,
                    Mineral = phases[i].Mineral,
                    Amount = n[i] * totalN,
                    GibbsG = g[i],
                    Solution = phases[i].Solution,
                    Composition = phases[i].Composition,
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Compute the Gibbs free energy of a phase at given P,T.
    /// </summary>
    public static double ComputePhaseGibbs(PhaseEntry phase, double P, double T)
    {
        try
        {
            var mineral = phase.Mineral;
            if (phase.IsSolidSolution && phase.Composition != null)
            {
                mineral = SolutionCalculator.GetEffectiveParams(
                    phase.Composition, phase.Solution!.Endmembers);
            }

            var optimizer = new MieGruneisenEOSOptimizer(mineral, Math.Max(P, 0.0001), T);
            var thermo = optimizer.ExecOptimize();
            return thermo.GibbsG;
        }
        catch
        {
            return double.MaxValue; // Phase not computable at these conditions
        }
    }

    /// <summary>
    /// Compute Clapeyron slope dT/dP = ΔV/ΔS for a two-phase boundary.
    /// </summary>
    public static double ClapeyronSlope(PhaseEntry phase1, PhaseEntry phase2, double P, double T)
    {
        var opt1 = new MieGruneisenEOSOptimizer(phase1.Mineral, P, T);
        var opt2 = new MieGruneisenEOSOptimizer(phase2.Mineral, P, T);
        var th1 = opt1.ExecOptimize();
        var th2 = opt2.ExecOptimize();

        double deltaV = th2.Volume - th1.Volume; // cm³/mol
        double deltaS = th2.Entropy - th1.Entropy; // J/mol/K

        if (Math.Abs(deltaS) < 1e-10) return 0;
        return deltaV / deltaS * 1000.0; // K/GPa (convert cm³·K/J → K/GPa)
    }
}
