<!-- Generated: 2026-03-23 | v1.0.0 | 42 calculator classes, 25 model classes, 4 database files -->
# Backend (ThermoElastic.Core)

## Calculation Pipeline (v1.0.0)

```
User Input → Phase 1-5: Core EOS → Phase 6-7: Mixtures → Phase 8-9: Advanced → ResultSummary
             BM3 + Debye       HS bounds + Gibbs    Isentropes, Hugoniots   (18-col export)
             + Landau          SolutionCalculator   PlanetaryInteriorSolver
```

## 42 Calculator Classes (Organized by Phase)

### Phase 1-5: Core Thermodynamics (5 classes)
| Class | Purpose |
|-------|---------|
| **MieGruneisenEOSOptimizer** | Iterative P,T→f solver (500 iter, 1e-5 tol), convergence report |
| **DebyeFunctionCalculator** | D₃(x) Simpson 500-pt, thermal contributions E_th, Cv, S_th |
| **LandauCalculator** | Displacive transitions: Q(T), Tc(P), G_Landau |
| **Optimizer** | ReglaFalsi + Secant root finders |
| **EOSFitter** | Least-squares BM3/MGD parameter fitting |

### Phase 6-7: Mixtures, Solutions & Equilibria (5 classes)
| Class | Purpose |
|-------|---------|
| **MixtureCalculator** | N-component mixing: Voigt/Reuss/Hill/Hashin-Shtrikman bounds |
| **SolutionCalculator** | Ideal + van Laar solutions, activity coefficients, per-endmember EOS |
| **GibbsMinimizer** | Phase equilibrium via SVD + stability analysis |
| **EquilibriumAggregateCalculator** | Re-equilibrate + mix along P-T paths |
| **VProfileCalculator** | Binary composition profiles + elastic bounds |

### Phase 8: Specialized Thermodynamics (8 classes)
| Class | Purpose |
|-------|---------|
| **IsentropeCalculator** | Adiabatic T(P) via bisection on S(P,T)=const |
| **PhaseDiagramCalculator** | Phase boundary tracing + stability detection |
| **HugoniotCalculator** | Shock equations of state (Hugoniot curves) |
| **IsomekeCalculator** | Constant entropy curves |
| **DepthConverter** | PREM-based depth↔pressure conversion |
| **PTProfileCalculator** | Loop calculator for P-T point sequences |
| **RockCalculator** | Multi-mineral orchestrator |
| **LookupTableGenerator** | Pre-computed interpolation tables |

### Phase 9: Advanced & Planetary (9 classes)
| Class | Purpose |
|-------|---------|
| **PlanetaryInteriorSolver** | Self-consistent interior profiles (mass, radius) |
| **MarsInteriorModel** | Mars 3-layer interior + core size estimation |
| **MagmaOceanCalculator** | Early crystallization + fractionation sequences |
| **PostPerovskiteCalculator** | D''-phase properties (P, T) |
| **ULVZCalculator** | Ultra-low velocity zone modeling |
| **LLSVPCalculator** | Large low shear velocity province anomalies |
| **SlabThermalModel** | Subduction slab thermal structure |
| **AnelasticityCalculator** | Seismic attenuation (Q⁻¹) |
| **ElectricalConductivityCalculator** | Mantle electrical conductivity |

### Inversion & Machine Learning (4 classes)
| Class | Purpose |
|-------|---------|
| **LevenbergMarquardtOptimizer** | Non-linear least-squares fitting |
| **MCMCSampler** | MCMC uncertainty quantification |
| **MLSurrogateModel** | Neural net surrogate for fast evaluation |
| **TrainingDataGenerator** | ML training set generation |

### Inverse Geochemistry (5 classes)
| Class | Purpose |
|-------|---------|
| **IronPartitioningSolver** | Fe²⁺/Fe³⁺ + Fe/Mg partitioning equilibria |
| **SpinCrossoverCalculator** | Fe²⁺ spin state transitions |
| **OxygenFugacityCalculator** | log fO₂ via redox buffers |
| **CompositionInverter** | Back-calculate composition from density |
| **ClassicalGeobarometer** | Multi-equilibrium pressure estimation |

### Thermal & Transport Properties (3 classes)
| Class | Purpose |
|-------|---------|
| **ThermalConductivityCalculator** | κ from density + correlations |
| **ElasticTensorCalculator** | 4th-rank stiffness tensor computation |
| **SensitivityKernelCalculator** | Travel time & waveform sensitivity kernels |

### Verification & Utilities (3 classes)
| Class | Purpose |
|-------|---------|
| **ThermodynamicVerifier** | G-H-S consistency + Gibbs-Duhem checks |
| **JointLikelihood** | Joint probability likelihood function |
| **WaterContentEstimator** | Invert Vp for volatile content |

### Models (25 classes)
Core: `MineralParams`, `ThermoMineralParams`, `RockComposition`, `PREMModel`
Results: `ResultSummary`, `PhaseAssemblage`, `PTProfile`
Specialized: `HugoniotPoint`, `ElasticTensor`, `RadialProfile`, `LookupTable`
Thermodynamic: `SolidSolution`, `MeltParams`, `SensitivityKernel`
Statistical: `MCMCChain`, `InversionResult`, `VerificationResult`
(see core-models.md for complete 25-item listing)

### Database (4 files)
| File | Content |
|------|---------|
| **SLB2011Endmembers.cs** | 46 endmembers (BurnMan-verified) |
| **SLB2011Solutions.cs** | Pre-defined solid solutions |
| **PredefinedRocks.cs** | Pyrolite, Harzburgite, MORB, Lower Mantle Peridotite |
| **MineralDatabase.cs** | Static accessor with search/lookup |

## Thermodynamic Equations

```
BM3 EOS:     P(f) = 3K₀f(1+2f)^(5/2)[1 + 3(K'₀-4)f/2]
Debye:       D₃(x) = (3/x³)∫₀ˣ t³/(eᵗ-1)dt  [500-pt Simpson]
Thermal P:   ΔP = (γ/V)·ΔE_th  [J/cm³ → GPa via /1000]
Velocities:  Vp = 1000√[(KS+4G/3)/ρ],  Vs = 1000√[G/ρ]
Gibbs:       G = F₀ + F_cold + F_thermal + F_Landau + F_mag + PV
HS lower:    K_HS = <1/(K_i+ζ_K)>⁻¹ - ζ_K,  ζ_K = 4G_min/3
Isentrope:   Find T at each P where S(P,T) = S₀ (bisection)
```

## Unit Conventions

| Quantity | Unit | Notes |
|----------|------|-------|
| Pressure | GPa | |
| Volume | cm³/mol | |
| Temperature | K | |
| Density | g/cm³ | |
| Velocity | m/s | |
| Free energy | kJ/mol | 1 GPa·cm³ = 1 kJ |
| Entropy | J/(mol·K) | |
| Cv, GetCv | J/(mol_atom·K) | per atom of formula unit |
| Depth | km | PREM reference |
