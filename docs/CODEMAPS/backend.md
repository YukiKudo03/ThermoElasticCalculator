<!-- Generated: 2026-03-18 | Files scanned: 30 | Token estimate: ~900 -->
# Backend (ThermoElastic.Core)

## Calculation Pipeline

```
MineralParams ──→ MieGruneisenEOSOptimizer ──→ ThermoMineralParams ──→ ResultSummary
    (static)         (P,T solver)                (computed props)        (output)
```

## Key Files

### Models/
| File | Lines | Purpose |
|------|-------|---------|
| MineralParams.cs | 205 | BM3 EOS params + BM3Finite/BM3KT/BM3GT methods |
| ThermoMineralParams.cs | 243 | P,T-dependent: ρ, KS, KT, GS, Vp, Vs, α, F, G, S |
| ResultSummary.cs | 45 | Output DTO with CSV/JSON export |
| RockComposition.cs | 51 | Multi-mineral assemblage + volume ratios |
| SolidSolution.cs | 33 | Endmembers + sites + interaction params |
| PhaseAssemblage.cs | 21 | Phase collection for Gibbs minimization |

### Calculations/
| File | Lines | Purpose |
|------|-------|---------|
| MieGruneisenEOSOptimizer.cs | 44 | Iterative P,T→f solver (max 500 iter, tol 1e-5 GPa) |
| DebyeFunctionCalculator.cs | 143 | D₃(x) Simpson integration, E_th, Cv, F_th per atom |
| LandauCalculator.cs | 63 | Displacive transition: Q(T), Tc(P), G_Landau |
| MixtureCalculator.cs | 69 | Voigt/Reuss/Hill mechanical mixing |
| SolutionCalculator.cs | 197 | Ideal S_conf, van Laar G_excess, activity coefficients |
| GibbsMinimizer.cs | 221 | Phase equilibrium by Gibbs minimization (SVD) |
| RockCalculator.cs | 87 | Multi-mineral orchestrator → MixtureCalculator |
| PTProfileCalculator.cs | 32 | Loop over P-T points |
| PhaseDiagramCalculator.cs | 90 | Phase boundary detection |
| EquilibriumAggregateCalculator.cs | 139 | Re-equilibrate + mix along P-T path |
| VProfileCalculator.cs | 130 | Composition profile along P-T |
| Optimizer.cs | 148 | ReglaFalsi + Secant root finders |

### Database/
| File | Lines | Purpose |
|------|-------|---------|
| SLB2011Endmembers.cs | 439 | 42 endmembers (BurnMan-verified params) |
| SLB2011Solutions.cs | 75 | Pre-defined solid solutions (olivine, garnet, etc.) |
| MineralDatabase.cs | 26 | Static accessor with search/lookup |

## Thermodynamic Equations

```
BM3 EOS:     P(f) = 3K₀f(1+2f)^(5/2)[1 + 3(K'₀-4)f/2]
Debye:       D₃(x) = (3/x³)∫₀ˣ t³/(eᵗ-1)dt  [500-pt Simpson]
Thermal P:   ΔP = (γ/V)·ΔE_th  [J/cm³ → GPa via /1000]
Velocities:  Vp = 1000√[(KS+4G/3)/ρ],  Vs = 1000√[G/ρ]
Gibbs:       G = F₀ + F_cold + F_thermal + F_Landau + F_mag + PV
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
