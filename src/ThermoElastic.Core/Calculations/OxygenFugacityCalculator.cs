namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Calculates oxygen fugacity for common mineral buffer assemblages.
/// Implements IW (Iron-Wustite), QFM (Quartz-Fayalite-Magnetite), and NNO (Nickel-Nickel Oxide) buffers.
/// Reference: O'Neill (1987), Frost (1991).
/// </summary>
public class OxygenFugacityCalculator
{
    private static readonly double R = PhysicConstants.GasConst;

    // Buffer parameterizations: log10(fO2) = A/T + B  (at 1 bar, in log10(bar))
    // IW: O'Neill (1987)
    private const double IW_A = -27489.0;
    private const double IW_B = 6.702;
    // QFM: O'Neill (1987), simplified
    private const double QFM_A = -25096.3;
    private const double QFM_B = 8.735;
    // NNO: O'Neill & Pownceby (1993)
    private const double NNO_A = -24930.0;
    private const double NNO_B = 9.36;

    // Pressure correction volume terms ΔV [m³/mol]
    private const double IW_DeltaV = 0.5e-6;
    private const double QFM_DeltaV = 0.8e-6;
    private const double NNO_DeltaV = 0.6e-6;

    /// <summary>
    /// Compute log10(fO2) for a specified buffer at given P-T.
    /// </summary>
    /// <param name="buffer">Buffer name: "IW", "QFM", "NNO"</param>
    /// <param name="T">Temperature [K]</param>
    /// <param name="P_GPa">Pressure [GPa]</param>
    /// <returns>log10(fO2) in log10(bar)</returns>
    public double ComputeLogFO2(string buffer, double T, double P_GPa = 0.0001)
    {
        var (a, b, deltaV) = GetBufferParams(buffer);

        // 1-bar value
        double logfO2_1bar = a / T + b;

        // Pressure correction: ΔV * (P - 0.0001 GPa) * 1e9 / (2.303 * R * T)
        double P_Pa_diff = (P_GPa - 0.0001) * 1.0e9; // Convert GPa difference to Pa
        double pressureCorrection = deltaV * P_Pa_diff / (2.303 * R * T);

        return logfO2_1bar + pressureCorrection;
    }

    /// <summary>
    /// Compute fO2 relative to a reference buffer.
    /// Returns delta_logfO2 = log10(fO2) - log10(fO2_buffer).
    /// </summary>
    public double ComputeDeltaBuffer(string buffer, double T, double P_GPa, double logfO2)
    {
        double logfO2_buffer = ComputeLogFO2(buffer, T, P_GPa);
        return logfO2 - logfO2_buffer;
    }

    /// <summary>
    /// List available buffer names.
    /// </summary>
    public static string[] AvailableBuffers => new[] { "IW", "QFM", "NNO" };

    private static (double A, double B, double DeltaV) GetBufferParams(string buffer)
    {
        return buffer.ToUpperInvariant() switch
        {
            "IW" => (IW_A, IW_B, IW_DeltaV),
            "QFM" => (QFM_A, QFM_B, QFM_DeltaV),
            "NNO" => (NNO_A, NNO_B, NNO_DeltaV),
            _ => throw new ArgumentException($"Unknown buffer: {buffer}. Available: {string.Join(", ", AvailableBuffers)}")
        };
    }
}
