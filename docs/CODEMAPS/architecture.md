<!-- Generated: 2026-03-23 | v1.0.0 | 42 calculators, 25 models, 13 views, 55 test classes, 391 test methods -->

# Architecture

## System Overview

```
┌─────────────────────────────────────────────────┐
│  ThermoElastic.Desktop (Avalonia 11 / MVVM)     │
│  13 Views + 13 ViewModels                       │
│  Windows / macOS / Linux                        │
├─────────────────────────────────────────────────┤
│  ThermoElastic.Core (.NET 9 Library)            │
│  Models (25) │ Calculations (42) │ DB (4)       │
│  Phase 1-9: EOS → Debye → Landau → Gibbs min   │
│  Mixtures (HS bounds) → Isentropes → PREM      │
│  Phase diagrams → Hugoniots → Interior models  │
│  Advanced: spin crossover, LLSVP, planetary    │
├─────────────────────────────────────────────────┤
│  MathNet.Numerics | CommunityToolkit.Mvvm      │
└─────────────────────────────────────────────────┘
     ↑ test
┌─────────────────────────────────────────────────┐
│  ThermoElastic.Core.Tests (xUnit 2.9)           │
│  55 test classes │ 391 test methods             │
│  BurnMan cross-validation + SLB2011 verification│
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│  ThermoElastic.Desktop.E2E                      │
│  45 E2E tests covering all views                │
└─────────────────────────────────────────────────┘
```

## Data Flow: Input → Calculation Pipeline → Output

```
User Input (P, T, Composition)
  ↓
View (13 variants: Mineral, Database, Profile, etc.)
  ↓
ViewModel (data binding + validation)
  ↓
Calculator (phase 1-9 specialized engines)
  │
  ├─ Phase 1-5: Core thermodynamics
  │   MieGruneisenEOSOptimizer → BM3 finite strain
  │   ThermoMineralParams → Debye + Landau + magnetic
  │
  ├─ Phase 6-7: Mixtures & solutions
  │   MixtureCalculator (Voigt/Reuss/Hill/HS bounds)
  │   SolutionCalculator (van Laar + activity coeff)
  │   GibbsMinimizer (SVD phase equilibrium)
  │
  ├─ Phase 8-9: Planetary & specialized
  │   PlanetaryInteriorSolver → mass-radius profiles
  │   IsentropeCalculator → adiabatic geotherms
  │   HugoniotCalculator → shock equations of state
  │   SpinCrossoverCalculator → Fe²⁺ spin transitions
  │
  └─ Database lookup (SLB2011 endmembers, predefined rocks)
  ↓
ResultSummary (18-column CSV export, JSON, tables)
  ↓
UI Results + Charts
```

## Key Directories & File Counts

| Path | Purpose | Files | Latest |
|------|---------|-------|--------|
| `src/ThermoElastic.Core/Models/` | Data structures | 25 | ThermoMineralParams, ElasticTensor, MCMCChain |
| `src/ThermoElastic.Core/Calculations/` | Calculator engines (phase 1-9) | 42 | All phases + specialized solvers |
| `src/ThermoElastic.Core/Database/` | SLB2011 + rocks + solutions | 4 | 46 endmembers, predefined rocks |
| `src/ThermoElastic.Core/IO/` | File I/O utilities | 2 | JSON/CSV serialization |
| `src/ThermoElastic.Desktop/Views/` | Avalonia AXAML UI | 13 | All application pages |
| `src/ThermoElastic.Desktop/ViewModels/` | MVVM business logic | 13 | 1:1 mapping to views |
| `tests/ThermoElastic.Core.Tests/` | Unit + verification tests | 68 | 391 test methods |
| `tests/ThermoElastic.Desktop.E2E/` | Integration + E2E tests | variable | 45 E2E scenarios |

## Solution Structure

- `ThermoElasticCalculator.sln` — Unified solution with 4 projects:
  - ThermoElastic.Core (library)
  - ThermoElastic.Desktop (app)
  - ThermoElastic.Core.Tests
  - ThermoElastic.Desktop.E2E

## Technology Stack

| Layer | Framework | Version | Purpose |
|-------|-----------|---------|---------|
| Core | .NET | 9.0 | Scientific library |
| Math | MathNet.Numerics | 5.0.0 | SVD, linear algebra |
| UI | Avalonia | 11.2.3 | Cross-platform XAML |
| MVVM | CommunityToolkit.Mvvm | 8.4.0 | Source-generated ObservableObject |
| Test | xUnit | 2.9.0 | Test framework |
