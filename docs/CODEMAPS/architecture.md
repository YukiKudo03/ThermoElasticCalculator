<!-- Generated: 2026-03-19 | Files scanned: 80+ | Token estimate: ~750 -->
# Architecture

## System Overview

```
┌──────────────────────────────────────────────┐
│  ThermoElastic.Desktop (Avalonia 11 / MVVM)  │
│  8 Views + 8 ViewModels                      │
│  Windows / macOS / Linux                     │
├──────────────────────────────────────────────┤
│  ThermoElastic.Core (.NET 8 Library)         │
│  Models (12) │ Calculations (15) │ DB (4)    │
│  BM3+MGD EOS │ Debye │ Landau │ Gibbs min   │
│  HS bounds │ PREM │ Isentrope │ Mixing       │
├──────────────────────────────────────────────┤
│  MathNet.Numerics (SVD, linear algebra)      │
└──────────────────────────────────────────────┘
     ↑ test
┌──────────────────────────────────────────────┐
│  ThermoElastic.Core.Tests (xUnit 2.9)       │
│  29 test classes │ 286 tests                 │
│  BurnMan cross-validation + SLB2011 lit. ref │
└──────────────────────────────────────────────┘
```

## Data Flow: P,T → Mineral Properties

```
User Input (P, T, Mineral)
  → MieGruneisenEOSOptimizer.ExecOptimize()
    → BM3Finite(refP) → finite strain f
    → ThermoMineralParams(f, T, mineral)
      → DebyeFunctionCalculator: E_th, Cv, S_th (Simpson D₃ integral)
      → LandauCalculator: phase transition correction
      → Magnetic contribution: -T·r·R·ln(2S+1)
      → KT, KS, GS, Vp, Vs, ρ, α, F, G, S
    → iterate until |P_total - P_target| < 1e-5
    → convergence status: IsConverged, Iterations, PressureResidual
  → ResultSummary (CSV 18-col / JSON export)
```

## Key Directories

| Path | Purpose | Files |
|------|---------|-------|
| `src/ThermoElastic.Core/Models/` | Data structures (MineralParams, PREM, etc.) | 12 |
| `src/ThermoElastic.Core/Calculations/` | EOS, Debye, Landau, mixing, isentrope, depth | 15 |
| `src/ThermoElastic.Core/Database/` | SLB2011 minerals (46), predefined rocks, solutions | 4 |
| `src/ThermoElastic.Core/IO/` | JSON/CSV file I/O | 2 |
| `src/ThermoElastic.Desktop/Views/` | Avalonia AXAML UI definitions | 8 |
| `src/ThermoElastic.Desktop/ViewModels/` | MVVM logic (CommunityToolkit.Mvvm) | 8 |
| `tests/ThermoElastic.Core.Tests/` | Unit + verification + literature tests | 29 |

## Solution

- `ThermoElasticCalculator.sln` — Core + Desktop + Tests (3 projects)
