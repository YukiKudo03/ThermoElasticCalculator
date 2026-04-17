<!-- Generated: 2026-03-24 | Files scanned: 54 calculators + 30 models | Token estimate: ~1400 -->

# Core Engine Codemap

**Version:** v1.0.0
**Last Updated:** 2026-03-24
**Scope:** ThermoElastic.Core calculation engines and data models

## Calculation Engines Overview (43 Classes)

### Phase 1-5: Core Thermodynamics & EOS

Fundamental mineral property calculation at arbitrary P-T.

| Class | Purpose | Key Method | Input | Output |
|-------|---------|-----------|-------|--------|
| **MieGruneisenEOSOptimizer** | Birch-Murnaghan 3rd-order finite strain EOS with Mie-Gruneisen thermal correction | `ExecOptimize()` | P, T, MineralParams | ThermoMineralParams (V, K, G, elastic constants) |
| **ThermoMineralParams** (model) | Intermediate result: elastic & thermodynamic properties at P-T | properties only | Finite strain, T | Vp, Vs, α, Cp, S, H, G, etc. |
| **DebyeFunctionCalculator** | Debye model thermal energy from Debye temperature | `IntegralD3()` | θD, T | Internal energy, Cv, Debye function D₃ |
| **LandauCalculator** | Phase transition order-disorder modeling (α↔β) | `CalcLandau()` | T, Landau params (Tc0, VD, SD) | Excess G, S, V |
| **Optimizer** | Legacy/general root-finding optimizer (deprecable) | `Optimize()` | — | — |

**File locations:**
- `src/ThermoElastic.Core/Calculations/MieGruneisenEOSOptimizer.cs`
- `src/ThermoElastic.Core/Calculations/DebyeFunctionCalculator.cs`
- `src/ThermoElastic.Core/Calculations/LandauCalculator.cs`
- `src/ThermoElastic.Core/Models/ThermoMineralParams.cs`

### Phase 6-7: Mixtures, Solutions & Equilibria

Multi-phase composition calculations with mixing models and activity coefficients.

| Class | Purpose | Key Method | Input | Output |
|-------|---------|-----------|-------|--------|
| **MixtureCalculator** | 2-phase mixing with Voigt/Reuss/Hill/HS models | `CalculateMixture()` | mineral1, mineral2, fractions, model | Mixed elastic moduli, density |
| **SolutionCalculator** | Solid solution (van Laar) with interaction parameters | `CalcSolution()` | endmembers, site occupancies | Activity coefficients, excess G |
| **GibbsMinimizer** | Find stable mineral assemblage at fixed P-T | `MinimizeEnergy()` | candidate phases, P, T | PhaseAssemblage (stable phases + fractions) |
| **EquilibriumAggregateCalculator** | Multi-phase coexistence with constraints | `CalculateEquilibrium()` | phases, P, T | PhaseAssemblage |
| **VProfileCalculator** | Molar volume as function of pressure | `CalculateVProfile()` | P_array, MineralParams | V(P) profile for equation of state work |

**File locations:**
- `src/ThermoElastic.Core/Calculations/MixtureCalculator.cs`
- `src/ThermoElastic.Core/Calculations/SolutionCalculator.cs`
- `src/ThermoElastic.Core/Calculations/GibbsMinimizer.cs`
- `src/ThermoElastic.Core/Calculations/EquilibriumAggregateCalculator.cs`
- `src/ThermoElastic.Core/Calculations/VProfileCalculator.cs`

### Phase 8: Specialized Thermodynamics

Advanced EOS and geothermal models.

| Class | Purpose | Key Method | Input | Output |
|-------|---------|-----------|-------|--------|
| **PTProfileCalculator** | Batch calculation of properties along P-T profile | `DoProfileCalculation()` | MineralParams, PTProfile | List<ThermoMineralParams> |
| **IsentropeCalculator** | Adiabatic temperature profile (S = const) | `CalcIsentrope()` | T0, P0, dP step | T(P) geotherm array |
| **IsomekeCalculator** | Isomeke path (constant kinetic energy / shock Hugoniot) | `CalcIsomeke()` | P_initial, U_initial | P(U) shock locus |
| **HugoniotCalculator** | Shock compression Hugoniot curve | `CalcHugoniot()` | P0, V0, shock velocity | HugoniotPoint (P, V, T, sound speed) |
| **PhaseDiagramCalculator** | Phase boundary mapping across P-T | `CalcPhaseBoundary()` | 2 phases, P_range, T_range | Phase boundary line (locus of equilibrium) |
| **DepthConverter** | PREM model depth ↔ pressure conversion | `DepthToPressure()` | depth_km | pressure_GPa |
| **RockCalculator** | Multi-mineral rock properties from composition | `CalculateRockProperties()` | RockComposition, P, T | Aggregate Vp, Vs, density |
| **EOSFitter** | Fit equation-of-state parameters to PV data | `FitEOS()` | V_data, P_data | Optimized KZero, K1Prime, K2Prime |

**File locations:**
- `src/ThermoElastic.Core/Calculations/PTProfileCalculator.cs`
- `src/ThermoElastic.Core/Calculations/IsentropeCalculator.cs`
- `src/ThermoElastic.Core/Calculations/IsomekeCalculator.cs`
- `src/ThermoElastic.Core/Calculations/HugoniotCalculator.cs`
- `src/ThermoElastic.Core/Calculations/PhaseDiagramCalculator.cs`
- `src/ThermoElastic.Core/Calculations/DepthConverter.cs`
- `src/ThermoElastic.Core/Calculations/RockCalculator.cs`
- `src/ThermoElastic.Core/Calculations/EOSFitter.cs`

### Phase 9: Planetary & Deep Earth

Interior models, mantle structures, and specialized solvers.

| Class | Purpose | Key Method | Input | Output |
|-------|---------|-----------|-------|--------|
| **PlanetaryInteriorSolver** | Mass-radius relation for planets | `CalcInteriorStructure()` | core_comp, mantle_comp, M, R | Density profile, moment of inertia |
| **MarsInteriorModel** | Mars-specific interior structure | `CalcMarsModel()` | Mars parameters | MarsSCSV model profile |
| **MagmaOceanCalculator** | Early Earth/primordial magma ocean | `CalcMagmaOcean()` | T_surface, depth | Melt fraction, density along adiabat |
| **PostPerovskiteCalculator** | Post-perovskite (pPv) phase D stability | `CalcPostPerovskite()` | depth, geotherm | pPv fraction, Clapeyron slope |
| **ULVZCalculator** | Ultra-low velocity zone composition | `CalcULVZ()` | P, T at CMB | ULVZ material (FeO-rich, partial melt) |
| **LLSVPCalculator** | Large low shear-velocity province | `CalcLLSVP()` | depth_range | Vp, Vs reduction, thermal vs. compositional |
| **SlabThermalModel** | Subducting slab cooling | `CalcSlabTemperature()` | age, depth, subduction_rate | T(depth) in slab |
| **SensitivityKernelCalculator** | Seismic wave sensitivity to structure | `CalcKernel()` | ray_path, depth_range | dVp/dρ, dVs/dρ kernel array |
| **PlanetaryInteriorSolver** | (see above) | `CalcInteriorStructure()` | — | — |

**File locations:**
- `src/ThermoElastic.Core/Calculations/PlanetaryInteriorSolver.cs`
- `src/ThermoElastic.Core/Calculations/MarsInteriorModel.cs`
- `src/ThermoElastic.Core/Calculations/MagmaOceanCalculator.cs`
- `src/ThermoElastic.Core/Calculations/PostPerovskiteCalculator.cs`
- `src/ThermoElastic.Core/Calculations/ULVZCalculator.cs`
- `src/ThermoElastic.Core/Calculations/LLSVPCalculator.cs`
- `src/ThermoElastic.Core/Calculations/SlabThermalModel.cs`
- `src/ThermoElastic.Core/Calculations/SensitivityKernelCalculator.cs`

### Optimization & Inversion (6 Classes)

Inverse problem solvers and machine learning support.

| Class | Purpose | Key Method | Input | Output |
|-------|---------|-----------|-------|--------|
| **LevenbergMarquardtOptimizer** | Non-linear least-squares parameter fitting | `Minimize()` | observed, Jacobian, lambda | OptimizationResult (parameters, residuals, covariance) |
| **MCMCSampler** | Markov Chain Monte Carlo uncertainty estimation | `SamplePosterior()` | likelihood_func, priors, n_iterations | MCMCChain (samples, acceptance rate) |
| **ThermoElasticFitter** | Fit seismic velocity vs. composition | `FitModel()` | Vp_obs, Vs_obs, composition | ModelParameters, goodness-of-fit |
| **CompositionInverter** | Invert seismic velocity → rock composition | `InvertComposition()` | Vp, Vs, depth | Estimated mineral fractions |
| **TrainingDataGenerator** | Generate synthetic data for ML training | `GenerateDataset()` | n_samples, P_range, T_range | TrainingDataPoint array |
| **MLSurrogateModel** | Neural network surrogate for fast evaluation | `Predict()` | input_features | output_properties (fast approximation) |

**File locations:**
- `src/ThermoElastic.Core/Calculations/LevenbergMarquardtOptimizer.cs`
- `src/ThermoElastic.Core/Calculations/MCMCSampler.cs`
- `src/ThermoElastic.Core/Calculations/ThermoElasticFitter.cs`
- `src/ThermoElastic.Core/Calculations/CompositionInverter.cs`
- `src/ThermoElastic.Core/Calculations/TrainingDataGenerator.cs`
- `src/ThermoElastic.Core/Calculations/MLSurrogateModel.cs`

### Inverse Geochemistry (5 Classes)

Elemental partitioning and spin-state thermodynamics.

| Class | Purpose | Key Method | Input | Output |
|-------|---------|-----------|-------|--------|
| **IronPartitioningSolver** | Fe²⁺/Fe³⁺ distribution among phases | `CalcPartitioning()` | P, T, phase_list | KD (partition coefficients) |
| **SpinCrossoverCalculator** | High-spin ↔ Low-spin transition for Fe²⁺ | `CalcSpinState()` | P, T | Spin-state fraction, density change |
| **ElementPartitioningModel** | General trace element partitioning (D_olivine/melt) | `CalcPartition()` | element, T | LogKD |
| **OxygenFugacityCalculator** | Oxygen fugacity from redox equilibria | `CalcfO2()` | buffer_reaction, T, pressure | log(fO2) relative to QFM |
| **WaterContentEstimator** | H₂O abundance from volatile speciation | `EstimateWaterContent()` | IR spectrum / Raman | wt% H₂O in mineral |

**File locations:**
- `src/ThermoElastic.Core/Calculations/IronPartitioningSolver.cs`
- `src/ThermoElastic.Core/Calculations/SpinCrossoverCalculator.cs`
- `src/ThermoElastic.Core/Calculations/OxygenFugacityCalculator.cs`
- `src/ThermoElastic.Core/Calculations/WaterContentEstimator.cs`

### Material Transport Properties (4 Classes)

Thermal and electrical conductivity, elastic tensor components.

| Class | Purpose | Key Method | Input | Output |
|-------|---------|-----------|-------|--------|
| **ThermalConductivityCalculator** | Heat conductivity κ(P, T) | `CalcConductivity()` | P, T, mineral | κ [W/m/K] |
| **ElectricalConductivityCalculator** | Electrical conductivity σ(P, T, redox) | `CalcConductivity()` | P, T, fO2 | σ [S/m] |
| **ElasticTensorCalculator** | 6×6 elastic stiffness tensor Cᵢⱼₖₗ | `CalcTensor()` | K, G, crystal symmetry | Full tensor components (Voigt notation) |
| **AnelasticityCalculator** | Seismic quality factor Q⁻¹(P, T, frequency) | `CalcAttenuation()` | P, T, f_Hz | Q⁻¹ |

**File locations:**
- `src/ThermoElastic.Core/Calculations/ThermalConductivityCalculator.cs`
- `src/ThermoElastic.Core/Calculations/ElectricalConductivityCalculator.cs`
- `src/ThermoElastic.Core/Calculations/ElasticTensorCalculator.cs`
- `src/ThermoElastic.Core/Calculations/AnelasticityCalculator.cs` (now implements IAnelasticityModel)

### Enhanced Anelasticity (11 Classes) — NEW

Advanced seismic quality factor modeling with multi-tier complexity, viscoelastic corrections, and material-specific effects.

| Class | Purpose | Key Method | Input | Output | Tier |
|-------|---------|-----------|-------|--------|------|
| **IAnelasticityModel** (interface) | Strategy interface for Q models | `ComputeQS()`, `ApplyCorrection()` | T, P, f, AnelasticityParams | QS, AnelasticResult | — |
| **ParametricQCalculator** | Grain-size & T,P,f dependent Q(d,T,P,f) | `ComputeQS()` | grain_size, T, P, f, prms | Q⁻¹ | Tier 2 |
| **AndradeCalculator** | Analytical J*(ω) compliance via Andrade creep | `ComputeComplianceSpectrum()` | T, P, freq_array, prms | J* (complex) | Tier 3b |
| **ExtendedBurgersCalculator** | Numerical J*(ω) via log-normal relaxation | `ComputeComplianceSpectrum()` | T, P, freq_array, prms | J* (complex) | Tier 3a |
| **WaterQCorrector** | Decorator: water effect on Q (enhanced mobility) | `CorrectForWater()` | Q_dry, H2O_ppm, T, prms | Q_wet (reduced) | Decorator |
| **MeltQCorrector** | Decorator: partial melt effect on Q (damping) | `CorrectForMelt()` | Q_solid, melt_frac, T, P, prms | Q_melt (reduced) | Decorator |
| **QProfileBuilder** | Build depth-dependent Q profile along geotherm | `BuildQProfile()` | geotherm (T vs depth), freq, prms | QProfilePoint[] | Composite |
| **AnelasticCorrectionHelper** | Shared: convert J*(ω) → velocity corrections | `ApplyCompliance()` | J*(ω), Vp_0, Vs_0 | ΔVp, ΔVs | Helper |
| **AnelasticityDatabase** | Mineral-specific Q parameters (olivine, bridgmanite, etc.) | `GetParameters()` | mineral_name | AnelasticityParams | Database |

**File locations:**
- `src/ThermoElastic.Core/Calculations/IAnelasticityModel.cs`
- `src/ThermoElastic.Core/Calculations/ParametricQCalculator.cs`
- `src/ThermoElastic.Core/Calculations/AndradeCalculator.cs`
- `src/ThermoElastic.Core/Calculations/ExtendedBurgersCalculator.cs`
- `src/ThermoElastic.Core/Calculations/WaterQCorrector.cs`
- `src/ThermoElastic.Core/Calculations/MeltQCorrector.cs`
- `src/ThermoElastic.Core/Calculations/QProfileBuilder.cs`
- `src/ThermoElastic.Core/Calculations/AnelasticCorrectionHelper.cs`
- `src/ThermoElastic.Core/Database/AnelasticityDatabase.cs`

**Architecture Pattern:**
```
IAnelasticityModel (Strategy)
  ├─ AnelasticityCalculator (Tier 1: basic, frequency-independent)
  ├─ ParametricQCalculator (Tier 2: grain-size, T, P, f dependent)
  ├─ AndradeCalculator (Tier 3b: analytical J*(ω))
  └─ ExtendedBurgersCalculator (Tier 3a: numerical J*(ω))

Decorators (layered corrections):
  ├─ WaterQCorrector ─→ reduces Q via H2O-enhanced diffusion
  └─ MeltQCorrector ─→ reduces Q via viscous melt damping

Composite:
  └─ QProfileBuilder ─→ assembles 1D Q(depth) using geotherm + decorators
```

**Key Features:**
- Grain-size dependent: Q ∝ d^m (m=3 for diffusion creep)
- Frequency dependent: Q⁻¹ ∝ f^α (α≈0.27 for olivine)
- Water effect: relaxation time τ ∝ (H2O)^r (r=1.0-2.0)
- Melt effect: ΔQ_inv ∝ φ_melt (partial damping)

### Verification & Quality Assurance (4 Classes)

Cross-validation and uncertainty quantification.

| Class | Purpose | Key Method | Input | Output |
|-------|---------|-----------|-------|--------|
| **ThermodynamicVerifier** | Compare this code vs. BurnMan vs. literature | `Verify()` | mineral, P, T | VerificationResult (Δ%, pass/fail) |
| **JointLikelihood** | Combine multi-dataset likelihood | `CalcLikelihood()` | data1, data2, weights | Joint log-likelihood |
| **ClassicalGeobarometer** | Mineral equilibrium barometer (garnet-cpx, etc.) | `CalcPressure()` | mineral_composition | P [GPa] estimate |
| **LookupTableGenerator** | Pre-compute properties for fast retrieval | `GenerateTable()` | P_range, T_range, res | LookupTable (interpolatable) |

**File locations:**
- `src/ThermoElastic.Core/Calculations/ThermodynamicVerifier.cs`
- `src/ThermoElastic.Core/Calculations/JointLikelihood.cs`
- `src/ThermoElastic.Core/Calculations/ClassicalGeobarometer.cs`
- `src/ThermoElastic.Core/Calculations/LookupTableGenerator.cs`

## Core Data Models (27 Classes)

### Input Configuration Models

| Class | Purpose | Key Properties | File |
|-------|---------|-----------------|------|
| **MineralParams** | Single mineral EOS + thermodynamic parameters | MineralName, K₀, K₀', K₀'', G₀, G₀', G₀'', θD, γ, q, η, Landau params, Spin params | `Models/MineralParams.cs` |
| **PTProfile** | List of P-T points for batch calculation | Profile (List<PTData>), name | `Models/PTProfile.cs` |
| **PTData** | Single (P, T) point | Pressure (GPa), Temperature (K) | `Models/PTData.cs` |
| **RockComposition** | Multi-mineral rock with fractions | Minerals (MineralParams[]), Fractions (double[]), Name, MixingModel | `Models/RockComposition.cs` |
| **SolidSolution** | Solid solution with site occupancies | Endmembers, SolutionSites, Name | `Models/SolidSolution.cs` |
| **SolutionSite** | Crystallographic site with occupancies | Site name, occupancy multiplicity | `Models/SolutionSite.cs` |
| **InteractionParam** | van Laar binary interaction coefficient | Species pair, Wab, Wba, T-dependence | `Models/InteractionParam.cs` |
| **PlanetaryConfig** | Planet physical parameters | Mass (M_earth), Radius (R_earth), core_radius | `Models/PlanetaryConfig.cs` |
| **FittingConfig** | EOS fitting configuration | data_file, parameter_bounds, optimizer_settings | `Models/FittingConfig.cs` |

### Output/Result Models

| Class | Purpose | Key Properties | File |
|-------|---------|-----------------|------|
| **ThermoMineralParams** | Calculated mineral properties at P-T | P, T, V, K_S, G, Vp, Vs, ρ, α, Cp, S, H, G, IsConverged, Iterations, PressureResidual | `Models/ThermoMineralParams.cs` |
| **ResultSummary** | Flattened result for CSV export | 18-column: P, T, V, K, G, Vp, Vs, density, α, Cp, etc. | `Models/ResultSummary.cs` |
| **PhaseAssemblage** | Stable mineral assemblage | Phases (PhaseEntry[]), Pressure, Temperature, TotalGibbsEnergy | `Models/PhaseAssemblage.cs` |
| **PhaseEntry** | Single phase in assemblage | MineName, Fraction, BulkComposition, Status | `Models/PhaseEntry.cs` |
| **OptimizationResult** | Parameter optimization output | FittedParameters (double[]), Residuals, Covariance, Chi2, Status | `Models/OptimizationResult.cs` |
| **InversionResult** | Inverse problem solution | EstimatedComposition, Uncertainty, Misfit, Iterations | `Models/InversionResult.cs` |

### Specialized Thermodynamic Models

| Class | Purpose | Key Properties | File |
|-------|---------|-----------------|------|
| **HugoniotPoint** | Single Hugoniot point | P, V, T, U (internal energy), particle_velocity, sound_speed | `Models/HugoniotPoint.cs` |
| **ElasticTensor** | 6×6 stiffness tensor (Voigt notation) | C11...C66 (36 components → 21 independent), Symmetry class | `Models/ElasticTensor.cs` |
| **AnelasticResult** | Q⁻¹ vs. frequency/temperature (NOW with J* compliance) | Q_inv, frequency_Hz, T_K, **ComplexCompliance_Jp**, **ComplexCompliance_Jpp** | `Models/AnelasticResult.cs` |
| **SensitivityKernel** | dVp/dρ, dVs/dρ depth profile | Kernel_Vp (double[]), Kernel_Vs (double[]), Depth_km (double[]) | `Models/SensitivityKernel.cs` |
| **MeltParams** | Melt/liquid properties | Density, Viscosity, Composition, T | `Models/MeltParams.cs` |

### Enhanced Anelasticity Models (NEW)

| Class | Purpose | Key Properties | File |
|-------|---------|-----------------|------|
| **AnelasticityParams** | Immutable record: Q model parameters (grain size, water, melt, etc.) | GrainSize_m, ActivationEnergy, FrequencyExponent, GrainSizeExponent, WaterContent_ppm, MeltFraction, AndradeBeta, RelaxationStrength, DistributionWidth, etc. | `Models/AnelasticityParams.cs` |
| **ViscoelasticResult** | Extended result with complex compliance J*(ω) | Q_inv, frequency_Hz, **Jp_ReflectiveCompliance**, **Jpp_LossCompliance**, **Vp_corrected**, **Vs_corrected** | `Models/ViscoelasticResult.cs` |
| **QProfilePoint** | Single point in depth-dependent Q profile | Depth_km, Temperature_K, Q_inv, Grain_size, WaterContent, MeltFraction, Model_used | `Models/QProfilePoint.cs` |

### Lookup & Training Data Models

| Class | Purpose | Key Properties | File |
|-------|---------|-----------------|------|
| **LookupTable** | Pre-computed properties grid | P_array, T_array, properties_3D (e.g., Vp[i,j]), interpolate() method | `Models/LookupTable.cs` |
| **TrainingDataPoint** | ML training example | Input (P, T, composition), Output (Vp, Vs, density), Weight | `Models/TrainingDataPoint.cs` |
| **MCMCChain** | MCMC posterior samples | Samples (List<double[]>), LogProbability (List<double>), AcceptanceRate | `Models/MCMCChain.cs` |
| **FittingDataPoint** | Single EOS fitting point | V, P, T, weight | `Models/FittingDataPoint.cs` |

### Earth Model Reference Data

| Class | Purpose | Key Properties | File |
|-------|---------|-----------------|------|
| **PREMModel** | PREM 1-D Earth reference model | Depth_km, Density, Vp, Vs, Q_mu lookup | `Models/PREMModel.cs` |
| **RadialProfile** | Any radial profile (T, density, etc.) | Radius, Values (interpolatable) | `Models/RadialProfile.cs` |
| **VerificationResult** | Cross-validation vs. literature | ComparisonData, PercentDifference, Status (PASS/FAIL) | `Models/VerificationResult.cs` |

## Database Module (6 Files)

**Location:** `src/ThermoElastic.Core/Database/`

| File | Content | Size |
|------|---------|------|
| `SLB2011Endmembers.cs` | 46 mineral endmember parameters (Stixrude & Lithgow-Bertelloni 2011) | ~4000 lines (hardcoded) |
| `SLB2011Solutions.cs` | Solid solution mixing models (olivine, pyroxene, spinel, garnet) | ~1500 lines |
| `PredefinedRocks.cs` | 4 standard rock compositions (Pyrolite, Harzburgite, MORB, Piclogite) | ~500 lines |
| `MineralDatabase.cs` | Wrapper/accessor for mineral lookups | ~200 lines |
| `SingleCrystalElasticConstants.cs` | Elastic stiffness tensor components for major minerals | ~800 lines |
| `AnelasticityDatabase.cs` | **NEW:** Mineral-specific Q parameters (olivine, bridgmanite, wadsleyite, etc.) with Tier defaults | ~600 lines |

**Key Features:**
- **SLB2011 Coverage:** 46 endmembers across olivine, pyroxene, garnet, spinel, perovskite, post-perovskite phases
- **Binary Solutions:** Olivine (Mg-Fe), opx (En-Fs-Wo), cpx (Di-Hd-Ac-Jd), spinel (Mg-Fe), garnet (Alm-Pyr-Grs)
- **Accessible Methods:**
  - `GetEndmember(name: string)` → MineralParams
  - `GetSolidSolution(name: string)` → SolidSolution
  - `GetPredefinedRock(name: string)` → RockComposition
  - `GetElasticTensor(mineral: string)` → ElasticTensor

## Key Equations Implemented

### Birch-Murnaghan 3rd-Order Finite Strain EOS

```
P = 3K₀f(1 + 2f)^(5/2) [1 + 3(K₀' - 4)f/2 + 3K₀(K₀'' - 4)f²/2]
where f = [(V₀/V)^(2/3) - 1] / 2  (Eulerian finite strain)
```

### Mie-Gruneisen Thermal Correction

```
ΔP_th = (γ/V) × ΔE_th
Elastic moduli: K(P,T), G(P,T) via derivatives
```

### Debye Model Thermodynamics

```
E_th = 3nRT × D₃(θD/T)
Cv = 3nR × [4D₃(x) - 3x/(e^x - 1)]
D₃(x) = (3/x³) ∫₀ˣ t³/(e^t - 1) dt  [Simpson 500-point integration]
```

### Landau Phase Transition

```
Q(T) = (1 - T/Tc)^(1/4)  (T < Tc, trilinear model)
Tc(P) = Tc₀ + VD × P / SD
G_Landau = SD × [(T - Tc)Q² + Tc × Q⁶/3]
```

### Hashin-Shtrikman Bounds (Mixtures)

```
K_HS+ = K_0 + f_1 f_2 (K_1 - K_0)² / [f_1(K_1 + 4G_0/3) + f_2(K_0 + 4G_0/3)]
Similar for G with appropriate moduli
```

### Seismic Velocities

```
Vp = √[(K_S + 4G/3) / ρ]    [bulk sound speed + shear]
Vs = √[G / ρ]                [shear wave]
K_S = K_T + α² T V Cp / ρ    [adiabatic correction]
```

## Unit Conventions

| Quantity | Unit | Notes |
|----------|------|-------|
| Pressure | GPa | 1 GPa = 10 kbar |
| Temperature | K | Kelvin |
| Density | g/cm³ | Or kg/m³ in some outputs |
| Seismic velocity | km/s | Vp, Vs (use √(elastic/density)) |
| Elastic moduli | GPa | K (bulk), G (shear) |
| Thermal expansion | 1/K | α = (∂V/∂T)_P / V |
| Heat capacity | J/mol/K | Cp = (∂H/∂T)_P |
| Entropy | J/mol/K | S = -∂G/∂T |
| Gibbs energy | kJ/mol | G = H - TS |
| Thermal conductivity | W/m/K | κ |
| Electrical conductivity | S/m | σ |

## Calculation Flow Example: PT Profile

```
Input: MineralParams (Fo90 olivine), PTProfile [6 P-T pairs]
  ↓ PTProfileCalculator.DoProfileCalculation()
  ├─ For each PTData (P₁, T₁) to (P₆, T₆):
  │   ├─ Create MieGruneisenEOSOptimizer(mineral, P, T)
  │   ├─ ExecOptimize() [iterative solver 500 max iterations]
  │   │   ├─ BM3Finite strain calculation
  │   │   ├─ ThermoMineralParams instantiation
  │   │   ├─ Check P convergence (< 1e-5 GPa)
  │   │   └─ Return converged ThermoMineralParams
  │   └─ Append to results list
  ↓
Output: List<ThermoMineralParams> (6 entries, one per P-T)
  ↓
ResultSummary conversion for CSV
  ↓
Export: "P,T,V,K,G,Vp,Vs,density,alpha,Cp,S,H,G,..."
```

---

**Next:** See [ui-layer.md](./ui-layer.md) for ViewModel and View mapping.
