namespace ThermoElastic.Core.Models;

/// <summary>
/// Configuration for thermoelastic parameter fitting.
/// Specifies which parameters to fit and provides initial/fixed values.
/// </summary>
public class FittingConfig
{
    // Parameter index constants
    public const int IndexV0 = 0;
    public const int IndexK0 = 1;
    public const int IndexK1Prime = 2;
    public const int IndexG0 = 3;
    public const int IndexG1Prime = 4;
    public const int IndexDebyeTemp = 5;
    public const int IndexGamma = 6;
    public const int IndexQ = 7;
    public const int IndexEtaS = 8;

    public const int TotalParams = 9;

    public static readonly string[] ParameterNames =
    {
        "V0", "K0", "K'", "G0", "G'", "θ0", "γ0", "q0", "ηS0"
    };

    /// <summary>
    /// Boolean flags indicating which parameters to fit.
    /// Index mapping matches the IndexXxx constants.
    /// </summary>
    public bool[] FitFlags { get; set; } = new bool[TotalParams];

    /// <summary>
    /// Base mineral parameters providing initial guesses for free parameters
    /// and fixed values for constrained parameters.
    /// </summary>
    public MineralParams BaseMineralParams { get; set; } = new();

    /// <summary>Number of parameters that will be fit (flags set to true).</summary>
    public int FreeParameterCount => FitFlags.Count(f => f);

    /// <summary>
    /// Extract all 9 parameter values from BaseMineralParams in index order.
    /// </summary>
    public double[] ExtractAllParams()
    {
        var m = BaseMineralParams;
        return new double[]
        {
            m.MolarVolume,     // 0: V0
            m.KZero,           // 1: K0
            m.K1Prime,         // 2: K'
            m.GZero,           // 3: G0
            m.G1Prime,         // 4: G'
            m.DebyeTempZero,   // 5: θ0
            m.GammaZero,       // 6: γ0
            m.QZero,           // 7: q0
            m.EhtaZero,        // 8: ηS0
        };
    }

    /// <summary>
    /// Pack only the free (flagged) parameters into a compact array.
    /// </summary>
    public double[] PackFreeParams()
    {
        var all = ExtractAllParams();
        var packed = new List<double>();
        for (int i = 0; i < TotalParams; i++)
        {
            if (FitFlags[i]) packed.Add(all[i]);
        }
        return packed.ToArray();
    }

    /// <summary>
    /// Create a new MineralParams by overwriting the free parameters
    /// from a compact array, keeping fixed parameters from BaseMineralParams.
    /// </summary>
    public MineralParams UnpackToMineralParams(double[] freeParams)
    {
        if (freeParams.Length != FreeParameterCount)
            throw new ArgumentException(
                $"Expected {FreeParameterCount} free parameters, got {freeParams.Length}.");

        var m = BaseMineralParams.Clone();
        int idx = 0;
        for (int i = 0; i < TotalParams; i++)
        {
            if (FitFlags[i])
            {
                SetParam(m, i, freeParams[idx]);
                idx++;
            }
        }
        return m;
    }

    private static void SetParam(MineralParams m, int index, double value)
    {
        switch (index)
        {
            case IndexV0: m.MolarVolume = value; break;
            case IndexK0: m.KZero = value; break;
            case IndexK1Prime: m.K1Prime = value; break;
            case IndexG0: m.GZero = value; break;
            case IndexG1Prime: m.G1Prime = value; break;
            case IndexDebyeTemp: m.DebyeTempZero = value; break;
            case IndexGamma: m.GammaZero = value; break;
            case IndexQ: m.QZero = value; break;
            case IndexEtaS: m.EhtaZero = value; break;
        }
    }
}
