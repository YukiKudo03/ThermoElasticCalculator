<!-- Generated: 2026-03-18 | Files scanned: 55+ | Token estimate: ~700 -->
# Architecture

## System Overview

```
┌──────────────────────────────────────────────┐
│  ThermoElastic.Desktop (Avalonia 11 / MVVM)  │
│  8 Views + 8 ViewModels                      │
│  Windows / macOS / Linux                     │
├──────────────────────────────────────────────┤
│  ThermoElastic.Core (.NET 8 Library)         │
│  Models (11) │ Calculations (13) │ DB (3)    │
│  Debye+BM3+MGD EOS │ Landau │ Mixing        │
├──────────────────────────────────────────────┤
│  MathNet.Numerics (SVD, linear algebra)      │
└──────────────────────────────────────────────┘
     ↑ test
┌──────────────────────────────────────────────┐
│  ThermoElastic.Core.Tests (xUnit 2.9)       │
│  24 test classes │ BurnMan cross-validation  │
└──────────────────────────────────────────────┘
```

## Data Flow: P,T → Mineral Properties

```
User Input (P, T, Mineral)
  → MieGruneisenEOSOptimizer.ExecOptimize()
    → BM3Finite(refP) → finite strain f
    → ThermoMineralParams(f, T, mineral)
      → DebyeFunctionCalculator: E_th, Cv (Simpson D₃ integral)
      → LandauCalculator: phase transition correction
      → Magnetic contribution: -T·r·R·ln(2S+1)
      → KT, KS, GS, Vp, Vs, ρ, α, F, G, S
    → iterate until |P_total - P_target| < 1e-5
  → ResultSummary (CSV/JSON export)
```

## Key Directories

| Path | Purpose | Files |
|------|---------|-------|
| `src/ThermoElastic.Core/Models/` | Data structures (MineralParams, ThermoMineralParams, etc.) | 11 |
| `src/ThermoElastic.Core/Calculations/` | EOS solver, Debye, Landau, mixing, phase equilibrium | 13 |
| `src/ThermoElastic.Core/Database/` | SLB2011 mineral parameters (42 endmembers) | 3 |
| `src/ThermoElastic.Core/IO/` | JSON/CSV file I/O | 2 |
| `src/ThermoElastic.Desktop/Views/` | Avalonia AXAML UI definitions | 8 |
| `src/ThermoElastic.Desktop/ViewModels/` | MVVM logic (CommunityToolkit.Mvvm) | 8 |
| `tests/ThermoElastic.Core.Tests/` | Unit + verification tests | 24 |
| `thermo-dynamics/` | Legacy WinForms app (historical reference) | ~14 |

## Solutions

- `ThermoElasticCalculator.sln` — main (Core + Desktop + Tests)
- `thermo-dynamics.sln` — legacy Windows-only
