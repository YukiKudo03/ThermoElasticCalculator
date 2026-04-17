using System.Globalization;
using System.Text;
using ThermoElastic.Core.Calculations;
using ThermoElastic.Core.Database;
using ThermoElastic.Core.Models;

// ============================================================================
// SLB Fitter Sample Data Generator
// ----------------------------------------------------------------------------
// Generates synthetic Brillouin-style Vp/Vs/density data from SLB2011 forward
// model + Gaussian noise, for demonstrating the SLB Parameter Fitter.
//
// Scientific design choices (edit these if needed):
// ----------------------------------------------------------------------------
const string MineralPaperName = "mpv";        // Mg-Perovskite (bridgmanite)
const int RandomSeed = 42;                     // Reproducibility
const double VpRelativeSigma = 0.005;          // 0.5% — typical Brillouin precision
const double VsRelativeSigma = 0.005;          // 0.5% — typical Brillouin precision
const double RhoRelativeSigma = 0.010;         // 1.0% — less precise than Vp/Vs

// Lower mantle P-T grid. 5 pressures × 3 temperatures = 15 points.
// P covers 670 km (~25 GPa) to near core-mantle boundary (~125 GPa).
// T spans cool subducted slab to hot thermal boundary layer.
double[] pressures = { 25.0, 50.0, 75.0, 100.0, 125.0 };  // GPa
double[] temperatures = { 2000.0, 2500.0, 3000.0 };       // K
// ============================================================================

var mineral = SLB2011Endmembers.GetAll().FirstOrDefault(m => m.PaperName == MineralPaperName)
              ?? throw new InvalidOperationException($"Mineral '{MineralPaperName}' not found in SLB2011 database.");

Console.WriteLine($"Generating synthetic data for: {mineral.MineralName} ({mineral.PaperName})");
Console.WriteLine($"Grid: {pressures.Length} pressures × {temperatures.Length} temperatures = {pressures.Length * temperatures.Length} points");
Console.WriteLine();

var rng = new Random(RandomSeed);
var csv = new StringBuilder();
csv.AppendLine("# Synthetic Brillouin-style dataset — bridgmanite (MgSiO3 perovskite)");
csv.AppendLine("# Generated from SLB2011 forward model + Gaussian noise (seed=42)");
csv.AppendLine("# Mineral: " + mineral.MineralName + " (" + mineral.PaperName + ")");
csv.AppendLine("# Reference params: K0=" + mineral.KZero.ToString("F2", CultureInfo.InvariantCulture) +
               " GPa, G0=" + mineral.GZero.ToString("F2", CultureInfo.InvariantCulture) +
               " GPa, V0=" + mineral.MolarVolume.ToString("F3", CultureInfo.InvariantCulture) + " cm3/mol");
csv.AppendLine("# Noise: Vp/Vs ±0.5%, density ±1.0%");
csv.AppendLine("P(GPa),T(K),Vp(m/s),Vs(m/s),Density(g/cm3),SigmaVp(m/s),SigmaVs(m/s),SigmaDensity(g/cm3)");

foreach (var p in pressures)
{
    foreach (var t in temperatures)
    {
        var eos = new MieGruneisenEOSOptimizer(mineral, p, t);
        var result = eos.ExecOptimize();

        double sigmaVp = result.Vp * VpRelativeSigma;
        double sigmaVs = result.Vs * VsRelativeSigma;
        double sigmaRho = result.Density * RhoRelativeSigma;

        double noisyVp = result.Vp + SampleGaussian(rng) * sigmaVp;
        double noisyVs = result.Vs + SampleGaussian(rng) * sigmaVs;
        double noisyRho = result.Density + SampleGaussian(rng) * sigmaRho;

        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "{0:F2},{1:F0},{2:F1},{3:F1},{4:F4},{5:F1},{6:F1},{7:F4}",
            p, t, noisyVp, noisyVs, noisyRho, sigmaVp, sigmaVs, sigmaRho));

        Console.WriteLine($"  P={p,6:F1} GPa  T={t,5:F0} K  Vp={noisyVp,6:F0}  Vs={noisyVs,6:F0}  rho={noisyRho:F3}");
    }
}

string outputPath = Path.Combine("samples", "bridgmanite_synthetic.csv");
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(outputPath, csv.ToString());

Console.WriteLine();
Console.WriteLine($"Wrote {pressures.Length * temperatures.Length} data points to: {outputPath}");

// ============================================================================
// Self-verification: round-trip the generated CSV through the SLB Fitter and
// confirm we recover the known K0 and G0 within uncertainty.
// ============================================================================
Console.WriteLine();
Console.WriteLine("=== Round-trip verification ===");

var rawCsv = File.ReadAllText(outputPath);
var loaded = ExperimentalDataset.ParseCsv(rawCsv, "BridgmaniteSynthetic");
Console.WriteLine($"Loaded {loaded.Data.Count} points from CSV.");

// Perturb the initial guess so the fitter has work to do
var guess = SLB2011Endmembers.GetAll().First(m => m.PaperName == MineralPaperName);
double trueK0 = guess.KZero;
double trueG0 = guess.GZero;
guess.KZero *= 1.05;  // 5% high on K0
guess.GZero *= 0.95;  // 5% low on G0
Console.WriteLine($"Initial guess: K0={guess.KZero:F2} (true {trueK0:F2}), G0={guess.GZero:F2} (true {trueG0:F2})");

var fitter = new SLBParameterFitter();
var options = new FittingOptions
{
    FitV0 = false,       // fix V0 (density-constrained)
    FitK0 = true,
    FitK0Prime = false,
    FitG0 = true,
    FitG0Prime = false,
    Target = FitTarget.Joint,
};

var fit = fitter.Fit(loaded, guess, options);

Console.WriteLine($"Converged: {fit.Optimization.Converged} ({fit.Optimization.Iterations} iterations)");
Console.WriteLine($"Chi² = {fit.Optimization.ChiSquared:E3}");
Console.WriteLine($"K0: fitted = {fit.FittedMineral.KZero,7:F2} GPa  (true {trueK0:F2}, err ±{fit.Optimization.Uncertainties[0]:F2})");
Console.WriteLine($"G0: fitted = {fit.FittedMineral.GZero,7:F2} GPa  (true {trueG0:F2}, err ±{fit.Optimization.Uncertainties[1]:F2})");

double k0Deviation = Math.Abs(fit.FittedMineral.KZero - trueK0);
double g0Deviation = Math.Abs(fit.FittedMineral.GZero - trueG0);
bool k0Ok = k0Deviation < 3.0 * fit.Optimization.Uncertainties[0];  // within 3σ
bool g0Ok = g0Deviation < 3.0 * fit.Optimization.Uncertainties[1];

Console.WriteLine();
Console.WriteLine(k0Ok && g0Ok
    ? "✓ Round-trip PASSED: parameters recovered within 3σ."
    : "✗ Round-trip FAILED: fitted parameters outside 3σ of true values.");
Environment.Exit(k0Ok && g0Ok ? 0 : 1);

// Box-Muller transform for Gaussian random samples
static double SampleGaussian(Random rng)
{
    double u1 = 1.0 - rng.NextDouble();
    double u2 = 1.0 - rng.NextDouble();
    return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
}
