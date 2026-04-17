namespace ThermoElastic.Core.Calculations;

using ThermoElastic.Core.Models;

public enum FitTarget { Joint, VpOnly, VsOnly, DensityOnly }

public class FittingOptions
{
    public bool FitV0 { get; set; } = true;
    public bool FitK0 { get; set; } = true;
    public bool FitK0Prime { get; set; } = true;
    public bool FitG0 { get; set; } = true;
    public bool FitG0Prime { get; set; } = true;
    public bool FitDebyeTemp { get; set; } = false;
    public bool FitGamma0 { get; set; } = false;
    public bool FitQ0 { get; set; } = false;
    public bool FitEtaS0 { get; set; } = false;
    public FitTarget Target { get; set; } = FitTarget.Joint;
}

public class FittingResult
{
    public MineralParams FittedMineral { get; set; } = new();
    public OptimizationResult Optimization { get; set; } = new();
    public string[] FittedParameterNames { get; set; } = Array.Empty<string>();
    public List<(double P, double T, double? ObsVp, double? ObsVs, double? ObsRho,
                  double PredVp, double PredVs, double PredRho)> Predictions { get; set; } = new();
}

/// <summary>
/// Fits SLB thermoelastic parameters to experimental Vp/Vs/density data.
/// Wraps LevenbergMarquardtOptimizer with the MieGruneisenEOS forward model.
/// </summary>
public class SLBParameterFitter
{
    private readonly LevenbergMarquardtOptimizer _optimizer = new();

    public FittingResult Fit(ExperimentalDataset data, MineralParams initialGuess, FittingOptions options)
    {
        if (data.Data.Count == 0)
            throw new ArgumentException("Dataset is empty.");

        // Build parameter mapping: which MineralParams fields are free vs. fixed
        var mapping = BuildParameterMapping(initialGuess, options);
        double[] initialParams = mapping.Select(m => m.getter(initialGuess)).ToArray();

        // Build observed and sigma arrays from data
        var (observed, sigma) = BuildObservedArrays(data, options.Target);

        if (observed.Length == 0)
            throw new ArgumentException("No observable data for the selected fit target.");

        // Forward model: parameter vector -> predicted observables
        Func<double[], double[]> model = paramVector =>
        {
            var mineral = CloneWithParams(initialGuess, paramVector, mapping);
            return ComputePredictions(mineral, data, options.Target);
        };

        // Run LM optimizer
        var result = _optimizer.Optimize(model, initialParams, observed, sigma);

        // Build output
        var fittedMineral = CloneWithParams(initialGuess, result.Parameters, mapping);
        var predictions = ComputePredictionDetails(fittedMineral, data);

        return new FittingResult
        {
            FittedMineral = fittedMineral,
            Optimization = result,
            FittedParameterNames = mapping.Select(m => m.name).ToArray(),
            Predictions = predictions,
        };
    }

    private static double[] ComputePredictions(MineralParams mineral, ExperimentalDataset data, FitTarget target)
    {
        var results = new List<double>();

        // Parallel computation of EOS for each data point
        var eosResults = new ThermoMineralParams?[data.Data.Count];
        Parallel.For(0, data.Data.Count, i =>
        {
            try
            {
                var eos = new MieGruneisenEOSOptimizer(mineral, data.Data[i].Pressure, data.Data[i].Temperature);
                eosResults[i] = eos.ExecOptimize();
            }
            catch
            {
                eosResults[i] = null;
            }
        });

        for (int i = 0; i < data.Data.Count; i++)
        {
            var pt = data.Data[i];
            var eos = eosResults[i];
            double vp = eos?.Vp ?? double.NaN;
            double vs = eos?.Vs ?? double.NaN;
            double rho = eos?.Density ?? double.NaN;

            if (target == FitTarget.Joint || target == FitTarget.VpOnly)
                if (pt.Vp.HasValue) results.Add(vp);
            if (target == FitTarget.Joint || target == FitTarget.VsOnly)
                if (pt.Vs.HasValue) results.Add(vs);
            if (target == FitTarget.Joint || target == FitTarget.DensityOnly)
                if (pt.Density.HasValue) results.Add(rho);
        }

        return results.ToArray();
    }

    private static (double[] observed, double[] sigma) BuildObservedArrays(ExperimentalDataset data, FitTarget target)
    {
        var obs = new List<double>();
        var sig = new List<double>();

        foreach (var pt in data.Data)
        {
            if (target == FitTarget.Joint || target == FitTarget.VpOnly)
            {
                if (pt.Vp.HasValue)
                {
                    obs.Add(pt.Vp.Value);
                    sig.Add(pt.SigmaVp ?? pt.Vp.Value * 0.01); // default 1% uncertainty
                }
            }
            if (target == FitTarget.Joint || target == FitTarget.VsOnly)
            {
                if (pt.Vs.HasValue)
                {
                    obs.Add(pt.Vs.Value);
                    sig.Add(pt.SigmaVs ?? pt.Vs.Value * 0.01);
                }
            }
            if (target == FitTarget.Joint || target == FitTarget.DensityOnly)
            {
                if (pt.Density.HasValue)
                {
                    obs.Add(pt.Density.Value);
                    sig.Add(pt.SigmaDensity ?? pt.Density.Value * 0.005); // default 0.5%
                }
            }
        }

        return (obs.ToArray(), sig.ToArray());
    }

    private static List<(double P, double T, double? ObsVp, double? ObsVs, double? ObsRho,
        double PredVp, double PredVs, double PredRho)> ComputePredictionDetails(
        MineralParams mineral, ExperimentalDataset data)
    {
        var predictions = new List<(double, double, double?, double?, double?, double, double, double)>();

        var eosResults = new ThermoMineralParams?[data.Data.Count];
        Parallel.For(0, data.Data.Count, i =>
        {
            try
            {
                var eos = new MieGruneisenEOSOptimizer(mineral, data.Data[i].Pressure, data.Data[i].Temperature);
                eosResults[i] = eos.ExecOptimize();
            }
            catch
            {
                eosResults[i] = null;
            }
        });

        for (int i = 0; i < data.Data.Count; i++)
        {
            var pt = data.Data[i];
            var eos = eosResults[i];
            predictions.Add((pt.Pressure, pt.Temperature, pt.Vp, pt.Vs, pt.Density,
                eos?.Vp ?? double.NaN, eos?.Vs ?? double.NaN, eos?.Density ?? double.NaN));
        }

        return predictions;
    }

    private record struct ParamMapping(string name, Func<MineralParams, double> getter, Action<MineralParams, double> setter);

    private static List<ParamMapping> BuildParameterMapping(MineralParams mineral, FittingOptions options)
    {
        var mapping = new List<ParamMapping>();

        if (options.FitV0) mapping.Add(new("V0", m => m.MolarVolume, (m, v) => m.MolarVolume = v));
        if (options.FitK0) mapping.Add(new("K0", m => m.KZero, (m, v) => m.KZero = v));
        if (options.FitK0Prime) mapping.Add(new("K0'", m => m.K1Prime, (m, v) => m.K1Prime = v));
        if (options.FitG0) mapping.Add(new("G0", m => m.GZero, (m, v) => m.GZero = v));
        if (options.FitG0Prime) mapping.Add(new("G0'", m => m.G1Prime, (m, v) => m.G1Prime = v));
        if (options.FitDebyeTemp) mapping.Add(new("θ0", m => m.DebyeTempZero, (m, v) => m.DebyeTempZero = v));
        if (options.FitGamma0) mapping.Add(new("γ0", m => m.GammaZero, (m, v) => m.GammaZero = v));
        if (options.FitQ0) mapping.Add(new("q0", m => m.QZero, (m, v) => m.QZero = v));
        if (options.FitEtaS0) mapping.Add(new("ηS0", m => m.EhtaZero, (m, v) => m.EhtaZero = v));

        if (mapping.Count == 0)
            throw new ArgumentException("No parameters selected for fitting.");

        return mapping;
    }

    private static MineralParams CloneWithParams(MineralParams template, double[] paramVector, List<ParamMapping> mapping)
    {
        // Deep clone via JSON roundtrip
        var json = template.ExportJson();
        MineralParams.ImportJson(json, out var clone);
        var mineral = clone ?? throw new InvalidOperationException("Failed to clone mineral parameters.");
        for (int i = 0; i < mapping.Count; i++)
            mapping[i].setter(mineral, paramVector[i]);
        return mineral;
    }
}
