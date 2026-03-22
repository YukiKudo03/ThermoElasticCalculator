<!-- Generated: 2026-03-23 | v1.0.0 | 25 model classes -->

# Core Models

## Overview

25 immutable/sealed data structures for thermodynamic properties, phase assemblages, results, and specialized containers.

## Input / Configuration Models

| Class | File | Key Properties | Purpose |
|-------|------|-----------------|---------|
| **MineralParams** | `MineralParams.cs` | K0, KPrime, Debye, Volume, Alpha, Gamma, V0, EtaS | BM3 EOS parameters + static methods: BM3Finite(), BM3KT(), BM3GT() |
| **RockComposition** | `RockComposition.cs` | Minerals, VolumeFractions | Multi-mineral assemblage for bulk calculations |
| **SolidSolution** | `SolidSolution.cs` | Endmembers, Sites, InteractionParams | End-member distribution + mixing properties |
| **SolutionSite** | `SolutionSite.cs` | Occupants, SiteMultiplicity | Crystallographic site definition |
| **InteractionParam** | `InteractionParam.cs` | EndmemberPair, W_parameter, Temperature_dependence | Non-ideal solution interaction coefficients |
| **PlanetaryConfig** | `PlanetaryConfig.cs` | PlanetMass, Radius, CoreFraction, Composition, Geotherm | Planetary interior boundary conditions |

## Output / Result Models

| Class | File | Key Properties | Purpose |
|-------|------|-----------------|---------|
| **ThermoMineralParams** | `ThermoMineralParams.cs` | Temperature, Pressure, Density, KT, KS, GS, VP, VS, AlphaT, F_Helmholtz, G_Gibbs, S_entropy, ConvergenceData | Complete P,T-dependent mineral state; convergence status |
| **ResultSummary** | `ResultSummary.cs` | Temperature, Pressure, Mineral, Properties (18-column), ExportToCSV(), ExportToJSON() | Output DTO with 18-col CSV export format |
| **PhaseAssemblage** | `PhaseAssemblage.cs` | Phases (PhaseEntry[]), StabilityFlags | Collection of stable phases at given (P,T) from Gibbs min |
| **PhaseEntry** | `PhaseEntry.cs` | Phase, Abundance, ChemicalPotential, Activity | Individual phase in assemblage |
| **OptimizationResult** | `OptimizationResult.cs` | IsConverged, Iterations, ResidualPressure, ResidualTemp | Solver convergence diagnostics |

## Specialized Thermodynamic Models

| Class | File | Key Properties | Purpose |
|-------|------|-----------------|---------|
| **HugoniotPoint** | `HugoniotPoint.cs` | Pressure, Temperature, Volume, InternalEnergy, Entropy | Single Hugoniot curve point (shock EOS) |
| **ElasticTensor** | `ElasticTensor.cs` | C_ijkl (4th-rank), Voigt_ij, AverageVp, AverageVs | Elastic stiffness tensor + Voigt notation |
| **AnelasticResult** | `AnelasticResult.cs` | QInverse, Attenuation, LogDecrement | Seismic attenuation frequency/temperature dependence |
| **SensitivityKernel** | `SensitivityKernel.cs` | Depth, dVp_dT, dVp_dP, dVs_dP, dVs_dRho | Travel time partial derivatives |
| **MeltParams** | `MeltParams.cs` | MeltFraction, Density_liquid, Viscosity, CompletionTemp | Partial melt properties |

## Profile & Path Models

| Class | File | Key Properties | Purpose |
|-------|------|-----------------|---------|
| **PTProfile** | `PTProfile.cs` | Points (PTData[]), MinPressure, MaxPressure, IntegrationPath | Collection of P-T points (e.g., isentrope, geotherm) |
| **PTData** | `PTData.cs` | Pressure, Temperature, Mineral, Properties | Single P-T point result |
| **RadialProfile** | `RadialProfile.cs` | Radius_km, Pressure, Temperature, Density, Mineral | Interior vs. depth profile (planetary models) |
| **PREMModel** | `PREMModel.cs` | Query(depth) → (density, Vp, Vs, Qmu) | Reference Earth Model (1-D layered Earth); 0-2891 km |

## Lookup & Interpolation Models

| Class | File | Key Properties | Purpose |
|-------|------|-----------------|---------|
| **LookupTable** | `LookupTable.cs` | MinP, MaxP, MinT, MaxT, Nodes_P, Nodes_T, Data_cube | Interpolation grid for property vs. (P,T) |
| **TrainingDataPoint** | `TrainingDataPoint.cs` | Input (P, T, composition), Output (properties) | Single instance for ML training |

## Statistical & Uncertainty Models

| Class | File | Key Properties | Purpose |
|-------|------|-----------------|---------|
| **MCMCChain** | `MCMCChain.cs` | Samples (parameter chains), Likelihood, Acceptance_rate | Posterior samples from Markov chain Monte Carlo |
| **InversionResult** | `InversionResult.cs` | BestFit, Uncertainty, Covariance, GoodnessOfFit | Inverse problem solution summary |
| **VerificationResult** | `VerificationResult.cs` | Passed, Residuals, MaxError, Reference | Cross-validation result (BurnMan, literature) |

## File Locations

All 25 model classes are located in:
```
src/ThermoElastic.Core/Models/*.cs
```

## Design Patterns

| Pattern | Usage |
|---------|-------|
| **Immutable/Readonly** | All properties use auto-properties with init accessors or getter-only |
| **Factory Methods** | MineralParams.CreateForMineral(), ThermoMineralParams.ComputeFrom() |
| **Serialization** | [Serializable], JSON export methods |
| **Data Transfer** | Classes serve as DTOs between View ↔ ViewModel ↔ Calculator |

## Thermodynamic State Hierarchy

```
MineralParams (static)
  ↓ [input P, T]
ThermoMineralParams (P,T-dependent computed state)
  ↓ [aggregation]
RockComposition (multi-mineral)
  ↓ [calculation]
ResultSummary (exportable output)
```

## Reference Database Accessors

Models access thermodynamic data via static methods:

| Data | Source | Access |
|------|--------|--------|
| 46 endmembers (SLB2011) | `SLB2011Endmembers.cs` | `MineralDatabase.GetMineral(name)` |
| Predefined rocks | `PredefinedRocks.cs` | `PredefinedRocks.Pyrolite`, etc. |
| Solid solutions | `SLB2011Solutions.cs` | `SolutionCalculator.GetSolution(type)` |
| PREM | `PREMModel.cs` | `PREMModel.Query(depth_km)` |
