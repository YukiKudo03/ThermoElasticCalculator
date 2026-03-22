<!-- Generated: 2026-03-23 | v1.0.0 | 42 calculator classes -->

# Core Calculations

## Overview

42 specialized calculator classes organized by thermodynamic phase (1-9). Each handles distinct physics domains.

## Phase 1-5: Core Thermodynamics & EOS

| Class | File | Key Public Methods | Purpose |
|-------|------|-------------------|---------|
| **MieGruneisenEOSOptimizer** | `MieGruneisenEOSOptimizer.cs` | `ExecOptimize(P, T, mineral)` → ThermoMineralParams | Iterative P,T→finite strain solver (500 iter, 1e-5 tol) |
| **DebyeFunctionCalculator** | `DebyeFunctionCalculator.cs` | `GetInternalEnergy()`, `GetCv()`, `GetEntropy()` | D₃(x) Simpson 500-pt integration; thermal contributions |
| **LandauCalculator** | `LandauCalculator.cs` | `GetDeltaF()`, `GetCritTemp()` | Displacive phase transitions: Q(T), Tc(P) |
| **Optimizer** | `Optimizer.cs` | `ReglaFalsi()`, `Secant()` | Root finders for EOS inversion |
| **EOSFitter** | `EOSFitter.cs` | `FitBM3()`, `FitMGEOS()` | Least-squares EOS parameter estimation |

## Phase 6-7: Mixtures, Solutions & Equilibria

| Class | File | Key Public Methods | Purpose |
|-------|------|-------------------|---------|
| **MixtureCalculator** | `MixtureCalculator.cs` | `Voigt()`, `Reuss()`, `Hill()`, `HashinShtrikmanLower()` | Binary/N-component elastic mixing; HS bounds |
| **SolutionCalculator** | `SolutionCalculator.cs` | `GetActivityCoeff()`, `GetExcessGibbs()`, `GetEntropy()` | Ideal + van Laar solutions; per-endmember EOS |
| **GibbsMinimizer** | `GibbsMinimizer.cs` | `Minimize(phases, P, T)` → PhaseAssemblage | Phase equilibrium via SVD + stability analysis |
| **EquilibriumAggregateCalculator** | `EquilibriumAggregateCalculator.cs` | `ReequilibrateAndMix()` | Path calculations along P-T with re-equilibration |
| **VProfileCalculator** | `VProfileCalculator.cs` | `ComputeProfile(x1_range, P, T)` | Binary composition profiles + elastic bounds |

## Phase 8: Specialized Thermodynamics

| Class | File | Key Public Methods | Purpose |
|-------|------|-------------------|---------|
| **IsentropeCalculator** | `IsentropeCalculator.cs` | `ComputeIsentrope(S0, P_range)` → PTProfile | Adiabatic T(P) via bisection on S(P,T)=const |
| **PhaseDiagramCalculator** | `PhaseDiagramCalculator.cs` | `TraceBoundary()`, `DetectTransition()` | Phase boundary tracing and stability |
| **HugoniotCalculator** | `HugoniotCalculator.cs` | `ComputeHugoniot(E0, V0, P_range)` | Shock equations of state (Hugoniot curves) |
| **IsomekeCalculator** | `IsomekeCalculator.cs` | `ComputeIsomeke()` | Constant entropy curves |
| **DepthConverter** | `DepthConverter.cs` | `PressureToDepth(P)`, `DepthToPressure(d)` | PREM-based depth↔pressure conversion via bisection |
| **PTProfileCalculator** | `PTProfileCalculator.cs` | `ComputeProfile(path_spec)` | Loop calculator for P-T point sequences |

## Phase 9: Advanced & Planetary

| Class | File | Key Public Methods | Purpose |
|-------|------|-------------------|---------|
| **PlanetaryInteriorSolver** | `PlanetaryInteriorSolver.cs` | `SolveStructure(config)` → RadialProfile | Self-consistent interior profiles (mass, radius) |
| **MarsInteriorModel** | `MarsInteriorModel.cs` | `ComputeProfile()`, `EstimateCoreSize()` | Mars-specific 3-layer interior models |
| **MagmaOceanCalculator** | `MagmaOceanCalculator.cs` | `CoolingModel()`, `CrystallizationSequence()` | Early magma ocean crystallization + fractionation |
| **PostPerovskiteCalculator** | `PostPerovskiteCalculator.cs` | `GetProperties(P, T)` | D''-phase elastic & thermodynamic properties |
| **ULVZCalculator** | `ULVZCalculator.cs` | `GetVelocityGradient()`, `GetDensity()` | Ultra-low velocity zone modeling |
| **LLSVPCalculator** | `LLSVPCalculator.cs` | `ComputeVpVsVariation()` | Large low shear velocity province anomalies |
| **SlabThermalModel** | `SlabThermalModel.cs` | `ComputeTProfile()` | Subduction zone slab thermal structure |

## Inversion & Machine Learning

| Class | File | Key Public Methods | Purpose |
|-------|------|-------------------|---------|
| **LevenbergMarquardtOptimizer** | `LevenbergMarquardtOptimizer.cs` | `Optimize(objective, params0)` | Non-linear least-squares fitting |
| **MCMCSampler** | `MCMCSampler.cs` | `Sample(niter)` → MCMCChain | Markov chain Monte Carlo uncertainty quantification |
| **MLSurrogateModel** | `MLSurrogateModel.cs` | `Predict(input)`, `Train(data)` | Neural net surrogate for fast evaluation |
| **LookupTableGenerator** | `LookupTableGenerator.cs` | `Generate(mineral, P_range, T_range)` | Pre-computed interpolation tables |

## Inverse Geochemistry & Tracers

| Class | File | Key Public Methods | Purpose |
|-------|------|-------------------|---------|
| **IronPartitioningSolver** | `IronPartitioningSolver.cs` | `SolvePartition(T, P, composition)` | Fe²⁺/Fe³⁺ and Fe/Mg partitioning equilibria |
| **SpinCrossoverCalculator** | `SpinCrossoverCalculator.cs` | `GetCrossoverPressure()`, `GetExcessEntropy()` | Fe²⁺ spin state transitions in minerals |
| **OxygenFugacityCalculator** | `OxygenFugacityCalculator.cs` | `GetFugacity(T, P)` | log fO₂ via redox buffers |
| **CompositionInverter** | `CompositionInverter.cs` | `InvertToComposition(density_target, P, T)` | Back-calculate mineral composition from density |
| **ClassicalGeobarometer** | `ClassicalGeobarometer.cs` | `CalibratePressure(minerals, activities)` | Multi-equilibrium pressure estimation |

## Thermal Properties & Conductivity

| Class | File | Key Public Methods | Purpose |
|-------|------|-------------------|---------|
| **ThermalConductivityCalculator** | `ThermalConductivityCalculator.cs` | `GetThermalConductivity(T, P, mineral)` | κ from density + mineral physics correlations |
| **AnelasticityCalculator** | `AnelasticityCalculator.cs` | `GetQInverse()`, `GetAttenuationFactor()` | Seismic attenuation (Q⁻¹) temperature/freq dependence |
| **ElectricalConductivityCalculator** | `ElectricalConductivityCalculator.cs` | `GetConductivity(T, P)` | Electrical conductivity from mantle models |
| **ElasticTensorCalculator** | `ElasticTensorCalculator.cs` | `Compute()` → ElasticTensor | 4th-rank stiffness tensor computation |

## Advanced Seismology

| Class | File | Key Public Methods | Purpose |
|-------|------|-------------------|---------|
| **SensitivityKernelCalculator** | `SensitivityKernelCalculator.cs` | `ComputeVelocityKernel()` | Travel time & waveform sensitivity kernels |
| **ThermodynamicVerifier** | `ThermodynamicVerifier.cs` | `VerifyConsistency()` | G-H-S consistency checks + Gibbs-Duhem relations |
| **JointLikelihood** | `JointLikelihood.cs` | `LogLikelihood(params)` | Joint probability of observations vs. model |

## Training & Data Generation

| Class | File | Key Public Methods | Purpose |
|-------|------|-------------------|---------|
| **TrainingDataGenerator** | `TrainingDataGenerator.cs` | `GenerateDataset(P_range, T_range)` → List<TrainingDataPoint> | ML training set creation for surrogate models |
| **WaterContentEstimator** | `WaterContentEstimator.cs` | `EstimateH2OContent(density, Vp)` | Invert seismic velocities for volatile content |
| **RockCalculator** | `RockCalculator.cs` | `ComputeAggregateProperties()` | Multi-mineral rock elastic averaging |

## Unit Conventions & Constants

All calculators use SI-derived units where applicable:

| Quantity | Unit | Notes |
|----------|------|-------|
| Pressure | GPa | |
| Volume | cm³/mol | molar basis |
| Temperature | K | |
| Density | g/cm³ | |
| Velocity | m/s | Vp, Vs |
| Free Energy | kJ/mol | 1 GPa·cm³ = 1 kJ |
| Entropy | J/(mol·K) | molar |
| Heat Capacity | J/(mol_atom·K) | per atom of formula unit |
| Depth | km | referenced to PREM |

## Key Equations

```
BM3 EOS:        P(f) = 3K₀f(1+2f)^(5/2)[1 + 3(K'₀-4)f/2]
Debye D₃(x):    D₃(x) = (3/x³)∫₀ˣ t³/(eᵗ-1)dt  [Simpson 500-pt]
Thermal P:      ΔP = (γ/V)·ΔE_th  [J/cm³ → GPa /1000]
Velocities:     Vp = 1000√[(KS+4G/3)/ρ],  Vs = 1000√[G/ρ]
HS lower bound: K_HS = <1/(K_i+ζ_K)>⁻¹ - ζ_K,  ζ_K = 4G_min/3
Gibbs equilibrium: min(ΣμᵢNᵢ) subject to stoichiometric & activity constraints
Hugoniot:       P_shock(V) from conservation laws + Mie-Gruneisen relation
```

## File Locations

All 42 calculator classes are located in:
```
src/ThermoElastic.Core/Calculations/*.cs
```

Each is fully documented with XML comments describing parameters, return types, equations, and references.
