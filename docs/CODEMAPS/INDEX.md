<!-- Generated: 2026-03-23 | Files scanned: 233 C# files | Token estimate: ~1200 -->

# ThermoElasticCalculator Codemaps Index

**Version:** v1.0.0
**Last Updated:** 2026-03-23
**Status:** Complete — All 43 calculators, 27 models, 34 views documented

## Overview

This codemap collection documents a .NET 9.0 Avalonia desktop application for computing thermoelastic properties of mantle minerals. The application implements **Stixrude & Lithgow-Bertelloni (2011) thermodynamics** with **MVVM architecture** and follows a strict separation of concerns (Core library + Desktop UI).

**Key Statistics:**
- **233 C# source files**
- **43 calculation engines** (Phase 1-9 organized)
- **27 data models** (input/output/intermediate)
- **34 Views + 34 ViewModels** (1:1 mapping, 6 categories)
- **56 unit test classes** (~479 test methods)
- **7 E2E test files** (~77 test methods)
- **95.6% code coverage** (Core library)
- **46 SLB2011 endmembers** built-in database

---

## Quick Navigation

### 1. [architecture.md](./architecture.md)
**System overview and technology stack**

**Read this if you need to understand:**
- How the entire system fits together (Desktop → Core → Tests)
- Data flow from user input → calculation → output
- Technology choices (.NET 9, Avalonia 11.2, MathNet.Numerics)
- Directory structure and file organization
- Solution layout and project dependencies

**Key Content:**
- 3-layer architecture diagram (Desktop UI | Core Library | Tests)
- Data flow visualization (User Input → ViewModel → Calculator → Output)
- Calculation pipeline hierarchy (Phase 1-9 organization)
- Directory structure table (27 models, 43 calculators, 34 views)
- Technology stack matrix (runtime, frameworks, versions)
- Key statistics table (233 files, 556 tests, 95.6% coverage)

**Target Audience:** New developers, architects, project leads, anyone onboarding

---

### 2. [core-engine.md](./core-engine.md)
**All 43 calculator classes and 27 data models**

**Read this if you need to understand:**
- How calculations work (thermodynamic equations, algorithms)
- Which calculator to use for a specific problem
- Input/output data structure for each calculation
- Thermodynamic theory behind the implementation
- Unit conventions and equation references

**Key Content:**
- **43 calculators organized by phase (1-9):**
  - Phase 1-5: Core thermodynamics (Mie-Gruneisen, Debye, Landau, EOS)
  - Phase 6-7: Mixtures & solutions (HS bounds, van Laar activity)
  - Phase 8: Specialized thermodynamics (Hugoniot, Isentrope, Phase diagrams)
  - Phase 9: Planetary & deep Earth (interior solvers, LLSVP, ULVZ)
  - Optimization & Inversion (LM, MCMC, ML surrogates)
  - Inverse Geochemistry (Fe partitioning, spin crossover)
  - Transport Properties (thermal/electrical conductivity, elastic tensor)
  - Verification & QA (cross-validation, geobarometers)

- **27 data models organized by purpose:**
  - Input configuration (MineralParams, PTProfile, RockComposition)
  - Output/results (ThermoMineralParams, ResultSummary, PhaseAssemblage)
  - Specialized (HugoniotPoint, ElasticTensor, AnelasticResult)
  - Lookup & training (LookupTable, TrainingDataPoint, MCMCChain)
  - Earth reference (PREMModel, RadialProfile)

- **Key equations:** BM3 EOS, Mie-Gruneisen thermal correction, Debye model, Landau phase transition, HS bounds, seismic velocities
- **Database:** SLB2011 (46 endmembers), solid solutions, predefined rocks
- **Unit conventions:** Table of all quantities and units

**Example Calculator Usage:**
```csharp
var mineral = MineralDatabase.GetEndmember("Fo90");
var calc = new MieGruneisenEOSOptimizer(mineral, pressure: 5.0, temperature: 1800.0);
var result = calc.ExecOptimize(); // → ThermoMineralParams
Console.WriteLine($"Vp = {result.Vp} km/s, Vs = {result.Vs} km/s");
```

**Target Audience:** Algorithm developers, thermodynamics specialists, scientific validation team

---

### 3. [ui-layer.md](./ui-layer.md)
**All 34 Views and 34 ViewModels (UI/UX Layer)**

**Read this if you need to understand:**
- How to build a new View or modify existing ones
- MVVM patterns used in this application
- Navigation and data binding patterns
- Which ViewModel handles which View
- Input validation rules and file I/O

**Key Content:**
- **MVVM Architecture Overview:**
  - Pattern: CommunityToolkit.Mvvm (source-generated ObservableObject + RelayCommand)
  - View (AXAML) ← binding → ViewModel (C#) ← method calls → Calculator/Model

- **View Hierarchy (34 Views in 6 categories):**
  1. **Core Mineralogy** (3 views):
     - MineralEditor — Create/edit mineral parameters
     - MineralDatabase — Browse 46 SLB2011 endmembers
     - PTProfile — Batch calculate properties along P-T profile

  2. **Mixture & Rock** (2 views):
     - Mixture — 2-mineral mixing (Voigt/Reuss/Hill/HS)
     - RockCalculator — Multi-mineral rock composition

  3. **Phase Equilibria** (4 views):
     - PhaseDiagram — Interactive phase boundary exploration
     - Verification — Cross-validate vs. BurnMan + literature
     - ClassicalGeobarometry — Mineral equilibrium barometer
     - Geobarometry — Detailed geothermometry

  4. **EOS & Shock** (4 views):
     - Hugoniot — Shock compression curves
     - EOSFitter — Fit EOS parameters to P-V data
     - ThermoElasticFitter — Fit combined thermoelastic properties

  5. **Mantle & Deep Earth** (6 views):
     - LLSVP — Large low-shear-velocity provinces
     - ULVZ — Ultra-low-velocity zones
     - SlabModel — Subducting slab cooling
     - PlanetaryInterior — Mass-radius relations
     - PostPerovskite — pPv stability vs. depth
     - SensitivityKernel — Seismic wave sensitivity

  6. **Material Properties** (5 views):
     - ThermalConductivity — κ(P,T)
     - ElectricalConductivity — σ(P,T,fO₂)
     - ElasticTensor — Stiffness tensor Cᵢⱼₖₗ
     - Anelasticity — Q⁻¹ seismic attenuation
     - OxygenFugacity — log(fO₂) buffers

  7. **Composition & Fluids** (4 views):
     - IronPartitioning — Fe²⁺/Fe³⁺ distribution (KD)
     - SpinCrossover — High-spin ↔ Low-spin transition
     - WaterContent — H₂O abundance estimation
     - MagmaOcean — Primordial magma ocean

  8. **Inversion & ML** (4 views):
     - CompositionInverter — Invert Vp/Vs → composition
     - BayesianInversion — Full MCMC inversion framework
     - MLData — Synthetic training data generation
     - LookupTable — Pre-computed property grids

  9. **Utility** (3 views):
     - Results — Display & export calculation results
     - Chart — Visualize results (OxyPlot)
     - MainWindow — Navigation hub

- **MVVM Pattern Details:**
  - `[ObservableProperty]` attributes → auto-generated INotifyPropertyChanged
  - `[RelayCommand]` attributes → auto-generated ICommand + CanExecute()
  - Async support: `[RelayCommand] private async Task MyCommand()`
  - Threading: `Task.Run()` for long calculations

- **File I/O Patterns:**
  - `.mine` (JSON) — MineralParams
  - `.ptpf` (JSON) — PTProfile
  - `.rock` (JSON) — RockComposition
  - `.csv` — Results table export
  - Serialization helpers in IO utilities

- **Input Validation:**
  - Temperature: [100, 10000] K
  - Pressure: [0.001, 360] GPa
  - Volume fractions: [0, 1.0], sum → 1.0
  - Material-specific bounds for elastic parameters

**Example ViewModel Pattern:**
```csharp
public partial class HugoniotViewModel : ObservableObject
{
    [ObservableProperty] private MineralParams? _selectedMineral;
    [ObservableProperty] private List<HugoniotPoint>? _hugoniotCurve;
    [ObservableProperty] private bool _isCalculating;

    [RelayCommand]
    private async Task Calculate()
    {
        IsCalculating = true;
        try
        {
            HugoniotCurve = await Task.Run(() =>
            {
                var calc = new HugoniotCalculator(SelectedMineral!);
                return calc.CalcHugoniot(VelMin, VelMax, NumPoints);
            });
        }
        finally { IsCalculating = false; }
    }
}
```

**Target Audience:** UI developers, frontend engineers, QA testers

---

### 4. [testing.md](./testing.md)
**Test structure, coverage, and validation strategy**

**Read this if you need to understand:**
- How to write a new test
- What test categories exist and where they are
- How to run tests and measure coverage
- What's being verified (unit, E2E, cross-validation)
- Literature verification approach

**Key Content:**
- **Test Structure (2 projects, 63 test files, ~556 tests):**
  - **Unit Tests (ThermoElastic.Core.Tests):** 56 classes, ~479 tests
    - Calculator tests (20+ classes covering all 43 calculators)
    - Data model tests (15+ classes validating input/output)
    - Database tests (5 classes for SLB2011 + IO)
    - Literature verification tests (10+ classes for cross-validation)
    - Integration tests (5+ classes for workflows)

  - **E2E Tests (ThermoElastic.Desktop.E2E):** 7 files, ~77 tests
    - ViewModel E2E tests (full View→ViewModel→Calculator flow)
    - Visual/Screenshot tests (golden image comparison)
    - Full Stack tests (end-to-end workflows)

- **Key Test Patterns:**
  - `[Fact]` — single fixed input
  - `[Theory]` — parameterized with `[InlineData(...)]`
  - Cross-validation against BurnMan + literature
  - Screenshot golden image matching

- **Coverage Report:**
  - Core library: 95.6%
  - Model classes: 98%
  - Calculator classes: 94%
  - Database: 100%
  - Desktop UI: 75% (UI controls hard to test)

- **Running Tests:**
  ```bash
  dotnet test                                  # All tests
  dotnet test --filter "Category!=E2E"        # Unit only
  dotnet test --filter "Category=E2E"         # E2E only
  dotnet test /p:CollectCoverage=true         # With coverage report
  ```

- **Performance Benchmarks:**
  - Single mineral calculation: ~1 ms
  - P-T profile (100 pts): ~100 ms
  - Phase equilibrium: ~500 ms
  - MCMC (10k iterations): 5-10 s with progress

**Target Audience:** QA engineers, test specialists, CI/CD maintainers

---

### 5. [dependencies.md](./dependencies.md)
**External packages and dependency management**

**Read this if you need to understand:**
- What external libraries are used and why
- How to update dependencies
- Build and publish process
- Cross-platform support (Windows/macOS/Linux)
- Licensing and compliance

**Key Content:**
- **.NET Framework:** 9.0 (latest LTS)
- **NuGet Packages (8 direct dependencies):**
  - `MathNet.Numerics 5.0.0` — Scientific computing (SVD, linear algebra)
  - `Avalonia 11.2.3` — Cross-platform XAML UI
  - `Avalonia.Desktop 11.2.3` — Platform integration
  - `Avalonia.Controls.DataGrid 11.2.3` — Tables
  - `Avalonia.Themes.Fluent 11.2.3` — Styling
  - `Avalonia.Fonts.Inter 11.2.3` — Typography
  - `CommunityToolkit.Mvvm 8.4.0` — MVVM source generation
  - `xunit 2.9.0` — Testing framework

- **External Data Sources:**
  - **SLB2011** — 46 mineral endmembers (Stixrude & Lithgow-Bertelloni 2005)
  - **PREM** — 1-D Earth reference model (Dziewonski & Anderson 1981)
  - **BurnMan** — Python library for cross-validation

- **Dependency Graph:** Visual representation of all project references
- **CI/CD:** GitHub Actions matrix (Ubuntu, Windows, macOS)
- **Licensing:** All MIT/Apache 2.0 compatible

**Target Audience:** DevOps engineers, dependency managers, IT/deployment teams

---

## File Locations

```
docs/CODEMAPS/
├── INDEX.md ← YOU ARE HERE
├── architecture.md           [System overview]
├── core-engine.md           [43 calculators + 27 models]
├── ui-layer.md              [34 Views + 34 ViewModels]
├── testing.md               [Test structure + coverage]
└── dependencies.md          [External packages + build]
```

## How to Use These Codemaps

### For Architecture Understanding
1. **Start:** `architecture.md` — System layers, data flow, tech stack
2. **Next:** `core-engine.md` — Where calculations live
3. **Then:** `ui-layer.md` — How UI invokes calculations
4. **Finally:** `testing.md` — How code is validated

### For Implementing a New Feature
1. **New calculator?**
   - Add class to `src/ThermoElastic.Core/Calculations/`
   - Follow Phase 1-9 organization (see core-engine.md)
   - Write unit tests in `tests/ThermoElastic.Core.Tests/`
   - Update `core-engine.md`

2. **New View (UI)?**
   - Create `.axaml` in `src/ThermoElastic.Desktop/Views/`
   - Create matching ViewModel in `src/ThermoElastic.Desktop/ViewModels/`
   - Add category button to MainWindowViewModel
   - Write E2E tests in `tests/ThermoElastic.Desktop.E2E/`
   - Update `ui-layer.md`

3. **New dependency?**
   - Update `.csproj` file(s)
   - Document in `dependencies.md`

### For Code Review
- Use file paths from codemaps to reference exact locations
- Cross-check class names against tables
- Verify method signatures match documented patterns

### For Testing a Feature
- See `testing.md` for test patterns and frameworks
- Run: `dotnet test --filter "ClassName"` for specific tests
- Check coverage: `dotnet test /p:CollectCoverage=true`

### For Deployment
- See `dependencies.md` for build & publish commands
- Self-contained artifacts include .NET runtime
- Cross-platform: Win-x64, macOS-x64, Linux-x64

---

## Key Statistics

| Metric | Count | Details |
|--------|-------|---------|
| **Codebase** | | |
| Total C# files | 233 | Source code only |
| Core library classes | 70 | 43 calculators + 27 models |
| Desktop UI classes | 68 | 34 Views + 34 ViewModels |
| Database files | 5 | 46 endmembers + solutions |
| I/O helper files | 2 | JSON/CSV utilities |
| **Testing** | | |
| Unit test classes | 56 | ThermoElastic.Core.Tests |
| Unit test methods | ~479 | Fact + Theory tests |
| E2E test files | 7 | ThermoElastic.Desktop.E2E |
| E2E test methods | ~77 | ViewModel + Visual tests |
| Total test methods | **~556** | **95.6% coverage (Core)** |
| **Data** | | |
| SLB2011 endmembers | 46 | Built-in mineral database |
| Predefined rocks | 4 | Pyrolite, Harzburgite, MORB, Piclogite |
| Solid solution models | 5 | Olivine, Opx, Cpx, Spinel, Garnet |
| **Dependencies** | | |
| Direct NuGet packages | 8 | Listed in dependencies.md |
| .NET version | 9.0 | Latest LTS |
| Platforms | 3 | Windows, macOS, Linux |

---

## Related Documentation

See also:
- `docs/requirements.md` — Feature specifications and v1.0.0 changelog
- `docs/basic-design.md` — Architecture rationale and design decisions
- `docs/detailed-design.md` — Implementation details and technical deep-dives
- `docs/user-guide-en.md` — End-user documentation (in English)
- `docs/user-guide-ja.md` — End-user documentation (in Japanese)
- `README.md` — Project overview and setup

---

## Version History

| Version | Date | Key Changes | Coverage |
|---------|------|------------|----------|
| v1.0.0 | 2026-03-23 | Full system: 43 calculators, 34 views, 556 tests | 95.6% |
| v0.5.0 | 2026-03-21 | Initial comprehensive mapping | 85% |
| Earlier | — | Basic structure | < 80% |

---

## Contributing to Codemaps

**When to update codemaps:**
- ✅ New calculator class added
- ✅ Model structure changed
- ✅ View added/removed
- ✅ Major refactoring
- ✅ Dependencies updated
- ❌ Code style changes (only update if structure changes)
- ❌ Documentation-only fixes

**How to update:**
1. Make code changes
2. Update relevant codemap(s)
3. Update freshness timestamp: `<!-- Generated: YYYY-MM-DD | ... -->`
4. Update version in INDEX.md if major change
5. Commit with message: "docs: Update codemaps for [feature name]"

---

## Search Tips

**Finding a class:**
- Calculator? → `core-engine.md` Phase table
- ViewModel? → `ui-layer.md` Category section
- Test? → `testing.md` Test Class table
- Model? → `core-engine.md` Data Models section

**Finding a file path:**
- All codemaps include file paths in tables/examples
- Pattern: `src/ThermoElastic.Core/Calculations/ClassName.cs`
- Pattern: `src/ThermoElastic.Desktop/Views/ViewName.axaml`

**Finding equations:**
- See `core-engine.md` "Key Equations Implemented" section
- Birch-Murnaghan 3rd-order, Mie-Gruneisen, Debye, Landau, HS bounds, Hugoniot, etc.

---

**Questions?** Refer to specific codemaps or project maintainers.

**Last generated:** 2026-03-23 | Next sync: When major changes occur
