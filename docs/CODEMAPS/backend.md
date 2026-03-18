<!-- Generated: 2026-03-19 | Files scanned: 33 | Token estimate: ~950 -->
# Backend (ThermoElastic.Core)

## Calculation Pipeline

```
MineralParams ──→ MieGruneisenEOSOptimizer ──→ ThermoMineralParams ──→ ResultSummary
    (static)         (P,T solver)                (computed props)        (output)
                     IsConverged/Iterations       AnalyticalEntropy       18-col CSV
```

## Key Files

### Models/
| File | Lines | Purpose |
|------|-------|---------|
| ThermoMineralParams.cs | 259 | P,T-dependent: ρ, KS, KT, GS, Vp, Vs, α, F, G, S + convergence status |
| MineralParams.cs | 205 | BM3 EOS params + BM3Finite/BM3KT/BM3GT methods |
| PREMModel.cs | 117 | PREM reference Earth model (0-2891 km depth) |
| ResultSummary.cs | 45 | Output DTO with 18-column CSV/JSON export (incl. F, G, S) |
| RockComposition.cs | 51 | Multi-mineral assemblage + volume ratios |
| SolidSolution.cs | 33 | Endmembers + sites + interaction params |
| PhaseAssemblage.cs | 21 | Phase collection for Gibbs minimization |

### Calculations/
| File | Lines | Purpose |
|------|-------|---------|
| MieGruneisenEOSOptimizer.cs | 51 | Iterative P,T→f solver (500 iter, 1e-5 tol) + convergence report |
| DebyeFunctionCalculator.cs | 161 | D₃(x) Simpson 500-pt, E_th, Cv, S_th, F_th per atom |
| MixtureCalculator.cs | 141 | Voigt/Reuss/Hill + **N-component Hashin-Shtrikman lower bound** |
| SolutionCalculator.cs | 237 | Ideal S_conf, van Laar G_ex, activity coeff, **rigorous per-endmember EOS** |
| GibbsMinimizer.cs | 221 | Phase equilibrium by Gibbs minimization (SVD) |
| IsentropeCalculator.cs | 122 | **Adiabatic T(P) profile** via bisection on S(P,T) = const |
| EquilibriumAggregateCalculator.cs | 144 | Re-equilibrate + mix (Voigt/Reuss/Hill/**HS**) along P-T path |
| PhaseDiagramCalculator.cs | 90 | Phase boundary detection/tracing |
| VProfileCalculator.cs | 130 | 2-component composition profile with HS bounds |
| DepthConverter.cs | 41 | **Depth↔Pressure conversion** via PREM bisection |
| RockCalculator.cs | 87 | Multi-mineral orchestrator → MixtureCalculator |
| LandauCalculator.cs | 63 | Displacive transition: Q(T), Tc(P), G_Landau |
| PTProfileCalculator.cs | 32 | Loop over P-T points |
| Optimizer.cs | 148 | ReglaFalsi + Secant root finders |

### Database/
| File | Lines | Purpose |
|------|-------|---------|
| SLB2011Endmembers.cs | 439 | 46 endmembers (BurnMan-verified params) |
| SLB2011Solutions.cs | 75 | Pre-defined solid solutions (olivine, garnet, etc.) |
| PredefinedRocks.cs | 98 | **Pyrolite, Harzburgite, MORB, Lower Mantle Peridotite** |
| MineralDatabase.cs | 26 | Static accessor with search/lookup |

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
