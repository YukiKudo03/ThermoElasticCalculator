using ThermoElastic.Core.Models;

namespace ThermoElastic.Core.Calculations;

/// <summary>
/// Calculates aggregate properties of a multi-phase assemblage with chemical equilibrium.
/// At each P-T point, re-equilibrates phase proportions and compositions using GibbsMinimizer,
/// then applies mechanical mixing (Voigt/Reuss/Hill) to compute aggregate elastic properties.
/// </summary>
public class EquilibriumAggregateCalculator
{
    private readonly GibbsMinimizer _minimizer = new();

    /// <summary>
    /// Calculate equilibrated aggregate properties along a P-T profile.
    /// </summary>
    /// <param name="initialAssemblage">Initial phase assemblage (phases with amounts)</param>
    /// <param name="ptProfile">P-T profile to evaluate along</param>
    /// <param name="method">Mixing method for elastic properties</param>
    /// <returns>List of (equilibrium assemblage, mixed result, individual results) at each P-T point</returns>
    public List<(PhaseAssemblage assemblage, ResultSummary? mixedResult, List<(string name, double ratio, ResultSummary result)> individualResults)>
        Calculate(PhaseAssemblage initialAssemblage, PTProfile ptProfile, MixtureMethod method)
    {
        var results = new List<(PhaseAssemblage, ResultSummary?, List<(string, double, ResultSummary)>)>();

        foreach (var pt in ptProfile.Profile)
        {
            // Re-equilibrate at this P,T
            var currentAssemblage = CloneAssemblage(initialAssemblage);
            var equilibrated = _minimizer.Minimize(currentAssemblage, pt.Pressure, pt.Temperature);

            // Compute individual mineral properties at this P,T
            var individualResults = new List<(string name, double ratio, ResultSummary result)>();
            double totalAmount = equilibrated.Phases.Sum(p => p.Amount);

            foreach (var phase in equilibrated.Phases)
            {
                double ratio = totalAmount > 0 ? phase.Amount / totalAmount : 0;
                try
                {
                    var optimizer = new MieGruneisenEOSOptimizer(phase.Mineral, pt.Pressure, pt.Temperature);
                    var thermo = optimizer.ExecOptimize();
                    var result = thermo.ExportResults();
                    individualResults.Add((phase.Name, ratio, result));
                }
                catch (Exception ex)
                {
                    // Log warning: phase skipped due to computation failure
                    System.Diagnostics.Debug.WriteLine(
                        $"Warning: Phase '{phase.Name}' skipped at P={pt.Pressure} GPa, T={pt.Temperature} K: {ex.Message}");
                }
            }

            // Apply mechanical mixing
            ResultSummary? mixedResult = null;
            if (individualResults.Count > 0)
            {
                var mixInput = individualResults.Select(r => (r.ratio, r.result)).ToList();
                var mixer = new MixtureCalculator(mixInput);
                mixedResult = method switch
                {
                    MixtureMethod.Voigt => mixer.VoigtAverage(),
                    MixtureMethod.Reuss => mixer.ReussAverage(),
                    MixtureMethod.HS => mixer.HashinShtrikmanAverage(),
                    _ => mixer.HillAverage(),
                };

                if (mixedResult != null)
                {
                    mixedResult.Name = "Equilibrium Aggregate";
                    mixedResult.GivenP = pt.Pressure;
                    mixedResult.GivenT = pt.Temperature;
                }
            }

            results.Add((equilibrated, mixedResult, individualResults));
        }

        return results;
    }

    /// <summary>
    /// Simple calculation without re-equilibration (mechanical mixing only).
    /// Equivalent to the existing RockCalculator behavior.
    /// </summary>
    public (ResultSummary? mixedResult, List<(string name, double ratio, ResultSummary result)> individualResults)
        CalculateMechanical(List<PhaseEntry> phases, double P, double T, MixtureMethod method)
    {
        var individualResults = new List<(string name, double ratio, ResultSummary result)>();
        double totalAmount = phases.Sum(p => p.Amount);

        foreach (var phase in phases)
        {
            double ratio = totalAmount > 0 ? phase.Amount / totalAmount : 0;
            try
            {
                var optimizer = new MieGruneisenEOSOptimizer(phase.Mineral, P, T);
                var thermo = optimizer.ExecOptimize();
                var result = thermo.ExportResults();
                individualResults.Add((phase.Name, ratio, result));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Warning: Phase '{phase.Name}' skipped at P={P} GPa, T={T} K: {ex.Message}");
            }
        }

        ResultSummary? mixedResult = null;
        if (individualResults.Count > 0)
        {
            var mixInput = individualResults.Select(r => (r.ratio, r.result)).ToList();
            var mixer = new MixtureCalculator(mixInput);
            mixedResult = method switch
            {
                MixtureMethod.Voigt => mixer.VoigtAverage(),
                MixtureMethod.Reuss => mixer.ReussAverage(),
                MixtureMethod.HS => mixer.HashinShtrikmanAverage(),
                _ => mixer.HillAverage(),
            };
        }

        return (mixedResult, individualResults);
    }

    private static PhaseAssemblage CloneAssemblage(PhaseAssemblage source)
    {
        return new PhaseAssemblage
        {
            Pressure = source.Pressure,
            Temperature = source.Temperature,
            BulkComposition = new Dictionary<string, double>(source.BulkComposition),
            Phases = source.Phases.Select(p => new PhaseEntry
            {
                Name = p.Name,
                Mineral = p.Mineral,
                Amount = p.Amount,
                GibbsG = p.GibbsG,
                Solution = p.Solution,
                Composition = p.Composition != null ? (double[])p.Composition.Clone() : null,
            }).ToList(),
        };
    }
}
