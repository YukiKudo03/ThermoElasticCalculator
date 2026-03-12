namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Landau theory for displacive phase transitions (e.g. α-β quartz).
/// SLB2011 Eq.28: G_t = S_D * [(T - Tc)*Q² + Tc*Q⁶/3] (tricritical)
/// </summary>
public static class LandauCalculator
{
    /// <summary>
    /// Equilibrium order parameter Q(T) for tricritical transition.
    /// Q = (1 - T/Tc)^(1/4) for T &lt; Tc, else 0.
    /// </summary>
    public static double GetOrderParameter(double T, double Tc)
    {
        if (Tc <= 0 || T >= Tc) return 0.0;
        return Math.Pow(1.0 - T / Tc, 0.25);
    }

    /// <summary>
    /// Pressure-dependent critical temperature: Tc(P) = Tc0 + VD*P/SD
    /// </summary>
    public static double GetTc(double P, double Tc0, double VD, double SD)
    {
        if (Tc0 == 0 || SD == 0) return 0.0;
        return Tc0 + VD * P / SD;
    }

    /// <summary>
    /// Landau free energy contribution.
    /// G_t = S_D * [(T - Tc)*Q² + Tc*Q⁶/3]
    /// Evaluated at equilibrium Q and at reference Q(T_ref=300K).
    /// Returns excess relative to a high-T disordered reference.
    /// </summary>
    public static double GetFreeEnergy(double T, double Tc, double SD)
    {
        if (Tc <= 0 || SD == 0) return 0.0;
        double Q = GetOrderParameter(T, Tc);
        double Q2 = Q * Q;
        double Q6 = Q2 * Q2 * Q2;
        return SD * ((T - Tc) * Q2 + Tc * Q6 / 3.0);
    }

    /// <summary>
    /// Landau entropy contribution: S_Landau = -dG_t/dT = -S_D * Q²
    /// (from analytic derivative at equilibrium)
    /// </summary>
    public static double GetEntropy(double T, double Tc, double SD)
    {
        if (Tc <= 0 || SD == 0) return 0.0;
        double Q = GetOrderParameter(T, Tc);
        return -SD * Q * Q;
    }

    /// <summary>
    /// Landau volume contribution: V_Landau = VD * Q²
    /// </summary>
    public static double GetVolume(double T, double Tc, double VD)
    {
        if (Tc <= 0 || VD == 0) return 0.0;
        double Q = GetOrderParameter(T, Tc);
        return VD * Q * Q;
    }
}
