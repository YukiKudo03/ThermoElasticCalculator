<!-- Generated: 2026-03-24 | Files scanned: 246 C# files | Token estimate: ~900 -->

# Architecture Codemap

**Version:** v1.0.0
**Last Updated:** 2026-03-24
**Platform:** .NET 9.0 | Windows/macOS/Linux

## System Overview

```
┌────────────────────────────────────────────────────────┐
│  ThermoElastic.Desktop                                 │
│  (.NET 9.0 Avalonia 11.2.3 | MVVM)                     │
│  • 35 Views (AXAML) + 35 ViewModels (new: QProfile)    │
│  • Categories: Core, EOS&Shock, Phase, Mantle, Props   │
│  • Threading: async/await for long calculations        │
├────────────────────────────────────────────────────────┤
│  ThermoElastic.Core                                    │
│  (.NET 9.0 Class Library)                              │
│  • 54 Calculation Engines (Phase 1-9 + Enhanced Q)     │
│  • 30 Data Models (Input/Output/Intermediate + Q)      │
│  • 6 Database Modules (SLB2011 + Q params)             │
│  • 2 I/O Helpers (JSON/CSV serialization)              │
├────────────────────────────────────────────────────────┤
│  External Dependencies                                 │
│  • MathNet.Numerics 5.0.0 (SVD, linear algebra)        │
│  • CommunityToolkit.Mvvm 8.4.0 (source-gen MVVM)       │
└────────────────────────────────────────────────────────┘
     ↓ test
┌────────────────────────────────────────────────────────┐
│  ThermoElastic.Core.Tests (xUnit 2.9.0)                │
│  • 57 Test Classes (unit tests, new: Anelasticity)     │
│  • ~507 Test Methods (Fact/Theory)                     │
│  • Verification: BurnMan + Jackson & Faul 2010         │
│  • Coverage: ~95.6% on Core library                    │
├────────────────────────────────────────────────────────┤
│  ThermoElastic.Desktop.E2E (xUnit 2.9.0)               │
│  • 8 E2E test files (new: expanded FullStackE2ETests)  │
│  • ~84 Test Methods (ViewModel + Visual tests)         │
│  • Full UI flow validation (Avalonia Headless)         │
└────────────────────────────────────────────────────────┘
```

## Data Flow: User Input → Calculation → Output

```
┌─────────────────────────────────────────────────────────┐
│  User Input Layer (View)                                │
│  • TextBox: Pressure (GPa), Temperature (K), etc.       │
│  • ComboBox: Mineral selection, mixing models           │
│  • DataGrid: P-T profiles, composition tables           │
│  • Slider: Volume fraction, parameters                  │
└─────────────────────────────────────────────────────────┘
               ↓ INotifyPropertyChanged (MVVM)
┌─────────────────────────────────────────────────────────┐
│  Business Logic Layer (ViewModel)                       │
│  • RelayCommand: Calculate, Export, Load, Save          │
│  • ObservableProperty: Results binding                  │
│  • Validation: Range checks, unit conversion            │
│  • Threading: Task.Run for non-blocking UI              │
└─────────────────────────────────────────────────────────┘
               ↓ Invoke Calculator
┌─────────────────────────────────────────────────────────┐
│  Calculation Engine (Core Library)                      │
│  • Phase 1-5: EOS (BM3) + Thermodynamics               │
│  • Phase 6-7: Mixtures (HS bounds) + Solutions         │
│  • Phase 8-9: Specialized (Hugoniot, Isentrope, etc.)  │
│  • Database: SLB2011 mineral parameters                │
└─────────────────────────────────────────────────────────┘
               ↓ ResultSummary (18 columns CSV)
┌─────────────────────────────────────────────────────────┐
│  Output Layer (Results Binding)                         │
│  • CSV/JSON Export: save to disk                        │
│  • DataGrid: display results table                      │
│  • Canvas/OxyPlot: plot Vp, Vs, density curves         │
│  • Status: "✓ Converged" or "✗ Did not converge"       │
└─────────────────────────────────────────────────────────┘
```

## Core Calculation Pipeline Hierarchy

```
Input (P, T, Composition)
  ↓
MieGruneisenEOSOptimizer
  ├─ BM3 Finite Strain EOS (iterative P solver)
  ├─ K(P), G(P), elastic constants
  └─→ ThermoMineralParams (intermediate)
  ↓
DebyeFunctionCalculator
  ├─ Debye model thermal energy
  ├─ Simpson integral (500 points)
  └─→ Internal energy, heat capacity
  ↓
LandauCalculator (phase transitions)
  ├─ α↔β transition order-disorder
  ├─ Landau free energy
  └─→ Excess G, S, V
  ↓
[Optional: Mixture Models]
  ├─ MixtureCalculator (Voigt/Reuss/Hill/HS)
  ├─ SolutionCalculator (van Laar activity)
  └─→ Aggregate elastic moduli
  ↓
[Optional: Specialized]
  ├─ HugoniotCalculator (shock EOS)
  ├─ IsentropeCalculator (adiabatic T profile)
  └─→ Specialized results
  ↓
Output (Vp, Vs, ρ, K, G, α, Cp, S, H, G, etc.)
```

## Directory Structure & File Counts

| Path | Purpose | Files | Key Content |
|------|---------|-------|-------------|
| `src/ThermoElastic.Core/Models/` | Data models (input/output) | 30 | MineralParams, ThermoMineralParams, PTProfile, RockComposition, PhaseAssemblage, ElasticTensor, **AnelasticityParams**, **ViscoelasticResult**, **QProfilePoint**, etc. |
| `src/ThermoElastic.Core/Calculations/` | Calculation engines | 54 | Phase 1-9 organized: BM3, Debye, Landau, Mixture, Gibbs, Hugoniot, planetary solvers, **Enhanced Anelasticity** (11 classes), etc. |
| `src/ThermoElastic.Core/Database/` | SLB2011 minerals + rocks + Q params | 6 | MineralDatabase, SLB2011Endmembers (46), SLB2011Solutions, PredefinedRocks, SingleCrystalElasticConstants, **AnelasticityDatabase** |
| `src/ThermoElastic.Core/IO/` | File I/O utilities | 2 | JSON/CSV serialization helpers |
| `src/ThermoElastic.Desktop/Views/` | UI pages (AXAML) | 35 | MainWindow + 34 category views (new: QProfileView) |
| `src/ThermoElastic.Desktop/ViewModels/` | MVVM ViewModels | 35 | MainWindowViewModel + 34 paired with Views (new: QProfileViewModel) |
| `tests/ThermoElastic.Core.Tests/` | Unit tests | 57 | Test classes covering all calculators, models, DB (new: Anelasticity tests) |
| `tests/ThermoElastic.Desktop.E2E/` | E2E tests | 8 | ViewModelE2ETests, VisualScreenshotTests, FullStackE2ETests (expanded) |

## Solution Layout

**File:** `ThermoElasticCalculator.sln` (unified solution, 4 projects)

```
ThermoElasticCalculator.sln
├── ThermoElastic.Core (net9.0 library)
│   └── Reference: MathNet.Numerics
├── ThermoElastic.Desktop (net9.0 WinExe)
│   ├── Reference: ThermoElastic.Core
│   ├── Reference: Avalonia 11.2.3 + Fluent theme
│   └── Reference: CommunityToolkit.Mvvm 8.4.0
├── ThermoElastic.Core.Tests (net9.0 test)
│   ├── Reference: ThermoElastic.Core
│   └── Reference: xunit 2.9.0 + SDK 17.8.0
└── ThermoElastic.Desktop.E2E (net9.0 test)
    ├── Reference: ThermoElastic.Desktop
    ├── Reference: ThermoElastic.Core
    └── Reference: xunit 2.9.0
```

## Technology Stack

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| Runtime | .NET | 9.0 | Target framework |
| **Core Library** | — | — | — |
| Math/Linear Algebra | MathNet.Numerics | 5.0.0 | SVD, matrix operations, iterative solvers |
| **Desktop UI** | — | — | — |
| UI Framework | Avalonia | 11.2.3 | Cross-platform XAML (Windows/macOS/Linux) |
| Themes | Avalonia.Themes.Fluent | 11.2.3 | Modern Fluent Design |
| Data Grid | Avalonia.Controls.DataGrid | 11.2.3 | Results tables |
| MVVM Framework | CommunityToolkit.Mvvm | 8.4.0 | Source-gen ObservableObject + RelayCommand |
| **Testing** | — | — | — |
| Test Framework | xunit | 2.9.0 | Fact/Theory test execution |
| Test Runner | Microsoft.NET.Test.Sdk | 17.8.0 | Visual Studio integration |
| Code Coverage | coverlet.collector | 6.0.2 | Line coverage metrics |

## Key Architectural Patterns

1. **MVVM (Model-View-ViewModel)**
   - **View** (.axaml): UI markup, data bindings
   - **ViewModel**: RelayCommand handlers, ObservableProperty, validation
   - **Model**: Business logic in Calculator classes

2. **Separation of Concerns**
   - Core (no UI dependencies) → reusable library
   - Desktop (Avalonia only) → replaceable UI layer
   - Tests independent → can run headless

3. **Calculator Factory Pattern**
   - Each tool (MieGruneisenEOSOptimizer, RockCalculator, etc.) is a standalone class
   - Stateless: instantiate with input data, call method, get result

4. **Result Streaming**
   - List<T> (P-T profiles) → batch calculations
   - Single values → instant calculations
   - CSV/JSON export → file serialization

## Threading Model

- **UI Thread**: View updates, user input
- **Background Tasks**: `Task.Run(() => Calculator.Execute())` in ViewModel
- **Async Await**: RelayCommand with cancellation support (where needed)
- **Long Operations**: Progress callback (future enhancement)

## File Formats

| Extension | Purpose | Format | Example |
|-----------|---------|--------|---------|
| `.mine` | Mineral parameters | JSON | `{ "MineralName": "Fo90", "KZero": 128.5, ... }` |
| `.ptpf` | P-T profile | JSON | `{ "Profile": [ {"P": 1.0, "T": 1600}, ... ] }` |
| `.rock` | Rock composition | JSON | `{ "Minerals": [{ "Name": "Ol", "Fraction": 0.5 }, ...] }` |
| `.csv` | Results table | CSV | Headers: P, T, Vp, Vs, Density, K, G, ... |
| `.json` | General data | JSON | ResultSummary serialization |

## Build & Deploy

**Build:**
```bash
dotnet build ThermoElasticCalculator.sln
```

**Run (debug):**
```bash
dotnet run --project src/ThermoElastic.Desktop
```

**Publish (platform-specific):**
```bash
dotnet publish src/ThermoElastic.Desktop -r win-x64 -c Release
dotnet publish src/ThermoElastic.Desktop -r osx-arm64 -c Release
dotnet publish src/ThermoElastic.Desktop -r linux-x64 -c Release
```

**Test:**
```bash
dotnet test                    # All tests
dotnet test --filter "Category=UnitTest"  # Unit only
dotnet test --filter "Category=E2E"  # E2E only
```

## Key Statistics

| Metric | Count |
|--------|-------|
| Total C# Source Files | 246 |
| Core Model Classes | 30 (+3 anelasticity) |
| Core Calculator Classes | 54 (+11 anelasticity) |
| Core Database Files | 6 (+1 anelasticity) |
| Core I/O Files | 2 |
| Desktop Views | 35 (+1 Q profile) |
| Desktop ViewModels | 35 (+1 Q profile) |
| Unit Test Classes | 57 (+2 anelasticity) |
| Unit Test Methods | ~507 (+37 anelasticity) |
| E2E Test Classes | 8 (expanded) |
| E2E Test Methods | ~84 (+6 anelasticity) |
| **Total Test Methods** | **~591** |
| Test Code Coverage | 95.6% (Core) |
| SLB2011 Endmembers | 46 |
| Predefined Rocks | 4 (Pyrolite, Harzburgite, MORB, Piclogite) |
| Anelasticity Tiers | 4 (Simple, Parametric, Extended Burgers, Andrade) |
| NuGet Direct Dependencies | 8 |
| Supported Platforms | 3 (Win/Mac/Linux) |

---

**Next:** See [core-engine.md](./core-engine.md) for detailed calculator descriptions.
