using ThermoElastic.Core;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Debye model calculator using analytical numerical integration.
/// Replaces the previous lookup-table implementation with direct computation
/// of the Debye function D3(x) via composite Simpson's rule.
///
/// Formulas (per atom, using R = gas constant per mole of atoms):
///   Internal energy:  E(T) = 3R·T·D₃(θ/T)
///   Heat capacity:    Cv(T) = 3R·[4·D₃(x) - 3x/(eˣ-1)]  where x = θ/T
///   Free energy:      F(T) = 3kBT·[3·ln(1-e⁻ˣ) - D₃(x)]  per atom
///
/// Reference: Stixrude &amp; Lithgow-Bertelloni (2005), Appendix.
/// </summary>
public class DebyeFunctionCalculator
{
    public DebyeFunctionCalculator(double debyeTemp)
    {
        _debyeTemp = debyeTemp;
    }

    private readonly double _debyeTemp;

    // Use the same gas constant as PhysicConstants for internal consistency
    private static readonly double R = PhysicConstants.GasConst;

    /// <summary>
    /// Internal energy per atom [J/mol_atom]: E_atom(T) = 3R·T·D₃(θ/T).
    /// Usage: ΔE_thermal [J/mol] = (GetIE(T) - GetIE(T_ref)) * n
    /// </summary>
    public double GetInternalEnergy(double givenTemp)
    {
        if (givenTemp < 1e-10) return 0.0;
        double x = _debyeTemp / givenTemp;
        // Return 3R·D₃(x)·T, matching the original contract: table[idx] * T
        return 3.0 * R * DebyeFunction3(x) * givenTemp;
    }

    /// <summary>
    /// Heat capacity per atom at given temperature [J/(mol_atom·K)].
    /// Cv_atom(T) = 3R·[4·D₃(x) - 3x/(eˣ-1)]  where x = θ/T
    ///
    /// Original API: CvT = GetCv(T) * n * T, where CvT = Cv_molar * T
    /// So GetCv must return Cv_per_atom such that Cv_per_atom * n = Cv_molar
    /// </summary>
    public double GetCv(double givenTemp)
    {
        if (givenTemp < 1e-10) return 0.0;
        double x = _debyeTemp / givenTemp;
        double d3 = DebyeFunction3(x);

        double boseEinstein;
        if (x > 100)
            boseEinstein = 0.0; // x/(e^x - 1) → 0 for large x
        else if (x < 1e-8)
            boseEinstein = 1.0; // x/(e^x - 1) → 1 for small x
        else
            boseEinstein = x / (Math.Exp(x) - 1.0);

        return 3.0 * R * (4.0 * d3 - 3.0 * boseEinstein);
    }

    /// <summary>
    /// Debye function D₃(x) = (3/x³) · ∫₀ˣ t³/(eᵗ-1) dt
    /// Computed by composite Simpson's rule with 500 subintervals for high accuracy.
    /// </summary>
    public static double DebyeFunction3(double x)
    {
        if (x < 1e-10) return 1.0; // limit as x → 0

        // For very large x, use the asymptotic form: D₃(x) → (π⁴/5)/x³
        if (x > 150)
        {
            double pi4over5 = Math.PI * Math.PI * Math.PI * Math.PI / 5.0;
            return pi4over5 / (x * x * x);
        }

        // Composite Simpson's rule with n subintervals (n must be even)
        int n = 500;
        double h = x / n;
        double sum = 0.0;

        // Interior points
        for (int i = 1; i < n; i++)
        {
            double t = i * h;
            double integrand;
            if (t > 500)
            {
                integrand = 0.0; // exp overflow protection
            }
            else if (t < 1e-10)
            {
                integrand = t * t; // t³/(eᵗ-1) ≈ t² for t→0
            }
            else
            {
                // Use numerically stable form for large t: t³·e⁻ᵗ/(1-e⁻ᵗ)
                if (t > 40)
                    integrand = t * t * t * Math.Exp(-t);
                else
                    integrand = t * t * t / (Math.Exp(t) - 1.0);
            }

            sum += (i % 2 == 0) ? 2.0 * integrand : 4.0 * integrand;
        }

        // Endpoint at t=x
        double endVal;
        if (x > 500)
            endVal = 0.0;
        else if (x > 40)
            endVal = x * x * x * Math.Exp(-x);
        else
            endVal = x * x * x / (Math.Exp(x) - 1.0);

        sum += endVal; // t=0 endpoint is 0
        sum *= h / 3.0;

        return 3.0 / (x * x * x) * sum;
    }

    /// <summary>
    /// Thermal entropy per atom [J/(mol_atom·K)]:
    /// S_atom(T) = R · [4·D₃(x) - 3·ln(1 - e⁻ˣ)]  where x = θ/T
    /// Derived from S = -∂F/∂T analytically.
    /// </summary>
    public double GetEntropy(double givenTemp)
    {
        if (givenTemp < 1e-10) return 0.0;
        double x = _debyeTemp / givenTemp;

        double d3 = DebyeFunction3(x);
        double logTerm;
        if (x > 100)
            logTerm = -x; // ln(1 - e^(-x)) ≈ -x for large x
        else
            logTerm = Math.Log(1.0 - Math.Exp(-x));

        return R * (4.0 * d3 - 3.0 * logTerm);
    }

    /// <summary>
    /// Thermal free energy contribution per atom (in units of kB·T):
    /// f_th(x) = 3·ln(1 - e⁻ˣ) - D₃(x)
    /// where x = θ/T
    /// </summary>
    public double GetThermalFreeEnergyPerAtom(double givenTemp)
    {
        if (givenTemp < 1e-10) return 0.0;
        double x = _debyeTemp / givenTemp;
        double logTerm;
        if (x > 100)
            logTerm = -3.0 * x; // ln(1-e^(-x)) ≈ -x for large x
        else
            logTerm = 3.0 * Math.Log(1.0 - Math.Exp(-x));
        return logTerm - DebyeFunction3(x);
    }
}
