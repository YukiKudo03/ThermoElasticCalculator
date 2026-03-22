<!-- Generated: 2026-03-23 | v1.0.0-ui | 556 tests passing -->

# ThermoElasticCalculator Codemaps Index

**Version:** v1.0.0-ui
**Last Updated:** 2026-03-23
**Status:** Complete — All 42 calculators, 25 models, 33 views documented

## Quick Navigation

### 1. [architecture.md](./architecture.md)
**Overview of the entire system**

- High-level system diagram (Desktop UI → Core → Tests)
- Data flow from user input → calculation → output
- Technology stack (.NET 9, Avalonia 11.2, MathNet.Numerics)
- Directory structure with file counts
- Solution layout

**Target audience:** New developers, architects, project leads

---

### 2. [core-calculations.md](./core-calculations.md)
**All 42 calculator classes organized by thermodynamic phase**

- **Phase 1-5:** Core thermodynamics & EOS (5 classes)
  - MieGruneisenEOSOptimizer, DebyeFunctionCalculator, LandauCalculator, Optimizer, EOSFitter

- **Phase 6-7:** Mixtures, solutions & equilibria (5 classes)
  - MixtureCalculator, SolutionCalculator, GibbsMinimizer, EquilibriumAggregateCalculator, VProfileCalculator

- **Phase 8:** Specialized thermodynamics (8 classes)
  - IsentropeCalculator, PhaseDiagramCalculator, HugoniotCalculator, IsomekeCalculator, DepthConverter, etc.

- **Phase 9:** Advanced & planetary (9 classes)
  - PlanetaryInteriorSolver, MarsInteriorModel, MagmaOceanCalculator, PostPerovskiteCalculator, ULVZCalculator, etc.

- **Inversion & ML:** 4 classes (LevenbergMarquardtOptimizer, MCMCSampler, MLSurrogateModel, TrainingDataGenerator)

- **Inverse Geochemistry:** 5 classes (IronPartitioningSolver, SpinCrossoverCalculator, etc.)

- **Thermal Properties:** 3 classes (ThermalConductivityCalculator, ElasticTensorCalculator, SensitivityKernelCalculator)

- **Verification:** 3 classes (ThermodynamicVerifier, JointLikelihood, WaterContentEstimator)

**Includes:** Key public methods, equations, unit conventions

**Target audience:** Algorithm developers, scientific validation team

---

### 3. [core-models.md](./core-models.md)
**All 25 data model classes**

- **Input/Configuration:** MineralParams, RockComposition, SolidSolution, SolutionSite, InteractionParam, PlanetaryConfig

- **Output/Results:** ThermoMineralParams, ResultSummary, PhaseAssemblage, PhaseEntry, OptimizationResult

- **Specialized Thermodynamic:** HugoniotPoint, ElasticTensor, AnelasticResult, SensitivityKernel, MeltParams

- **Profiles & Paths:** PTProfile, PTData, RadialProfile, PREMModel

- **Lookup & Interpolation:** LookupTable, TrainingDataPoint

- **Statistical & Uncertainty:** MCMCChain, InversionResult, VerificationResult

**Includes:** Properties, design patterns, serialization, factory methods

**Target audience:** Backend developers, data structure designers

---

### 4. [desktop-ui.md](./desktop-ui.md)
**All 33 Avalonia views + 33 ViewModels**

- Full view hierarchy organized by 6 categories (EOS&Shock, Phase Equilibria, Mantle&Deep Earth, Material Properties, Composition&Fluids, Inversion&ML)
- ViewModel-to-Calculator mapping (33 pairs)
- Data binding flow (View → ViewModel → Model → Calculator)
- File I/O formats (.mine, .ptpf, .vpf, .rock, .csv, .json)
- MVVM patterns (RelayCommand, ObservableObject)
- UI controls used (TextBox, ComboBox, DataGrid, Canvas, Slider)
- Input validation rules
- Threading model (background tasks for long calculations)
- E2E testing coverage (77 tests)

**New Views (19 added):**
- Core Mineralogy: MineralProperties, MineralSpecs
- EOS & Shock: ShockComparison, HugoniotAnalysis
- Phase Equilibria: PhaseTransition, PhaseStability, EquilibriumAnalysis
- Mantle & Deep Earth: PREMProfile, IsotopeProfile, PostPerovskite
- Material Properties: ElasticTensor, Anelasticity
- Composition & Fluids: MeltProperties, Solution, WaterContent, FluidPhase
- Inversion & ML: Inversion, MCMC
- Results & Visualization: DataExport

**Target audience:** UI developers, frontend engineers, QA testers

---

### 5. [dependencies.md](./dependencies.md)
**External dependencies and integration points**

- **.NET Framework:** 9.0

- **NuGet Packages:**
  - Core: MathNet.Numerics 5.0.0
  - Desktop: Avalonia 11.2.3, CommunityToolkit.Mvvm 8.4.0, supporting packages
  - Tests: xunit 2.9.0, Microsoft.NET.Test.Sdk 17.8.0

- **External Data Sources:**
  - SLB2011 (46 endmembers) — Stixrude & Lithgow-Bertelloni 2005
  - PREM (1-D Earth) — Dziewonski & Anderson 1981
  - BurnMan (cross-validation) — Python implementation
  - Literature reference data (CSV test files)

- **CI/CD Pipeline:**
  - GitHub Actions matrix: Ubuntu, Windows, macOS
  - Self-contained publishable artifacts (win-x64, linux-x64, osx-x64)

- **Licensing:** All MIT/Apache 2.0 compatible

- **Version maintenance status:** As of 2026-03-23

**Target audience:** DevOps engineers, dependency managers, IT/deployment teams

---

### 6. [backend.md](./backend.md) — Updated
**Core library thermodynamic engine (supplemental to core-calculations.md)**

- Calculation pipeline diagram
- 42-calculator overview (organized by phase 1-9)
- 25-model overview
- Thermodynamic equations (BM3, Debye, HS bounds, Gibbs, Hugoniot, etc.)
- Unit conventions table
- Database file summary

**Target audience:** Scientists, thermodynamic algorithm specialists

---

### 7. [frontend.md](./frontend.md) — Updated
**Desktop UI layer (supplemental to desktop-ui.md)**

- Technology stack (.NET 9, Avalonia 11.2.3)
- 33 View Hierarchy (organized by 6 categories)
- 33 ViewModel mapping
- Navigation pattern
- File formats

**Target audience:** UI developers, application architects

---

## File Locations

```
docs/CODEMAPS/
├── INDEX.md                      [THIS FILE]
├── architecture.md               [System overview]
├── core-calculations.md          [42 calculators]
├── core-models.md                [25 data models]
├── desktop-ui.md                 [13 views + 13 VMs]
├── dependencies.md               [NuGet + external data]
├── backend.md                    [Calculation engine supplement]
└── frontend.md                   [UI layer supplement]
```

## Key Statistics

| Metric | Count | Status |
|--------|-------|--------|
| Calculator Classes (Phase 1-9) | 42 | Complete |
| Model Classes | 25 | Complete |
| Avalonia Views | 33 | Complete (19 new in v1.0-ui) |
| ViewModels | 33 | 1:1 with Views |
| Test Classes | 55 | All documented |
| Test Methods | 556 | Full coverage |
| E2E Tests | 77 | All views covered |
| Database Files | 4 | SLB2011 + rocks |
| Endmembers | 46 | Pre-loaded |
| Direct NuGet Dependencies | 8 | Listed |
| Platforms | 3 | Win/Mac/Linux |
| .NET Version | 9.0 | Current |

## Documentation Maintenance Schedule

**Update triggers:**
- New calculator class added
- Model structure changed
- View added/removed
- Dependency version updated
- Major architectural refactor

**Last maintenance check:** 2026-03-23
**Automated generation:** Via analysis scripts (see project docs)

## How to Use These Codemaps

### For Understanding Structure
Start with **architecture.md** → then dive into specific areas:
- **Calculation logic?** → core-calculations.md + backend.md
- **Data models?** → core-models.md
- **UI flow?** → desktop-ui.md + frontend.md
- **Building/deploying?** → dependencies.md

### For Contributing
1. New calculator? Add to core-calculations.md with phase category
2. New model? Add to core-models.md with properties table
3. New view? Add to desktop-ui.md with ViewModel mapping
4. New dependency? Update dependencies.md

### For Code Review
Cross-reference class names with file locations in codemaps.
Each codemap links to actual source files:
```
src/ThermoElastic.Core/Calculations/ClassName.cs
src/ThermoElastic.Core/Models/ClassName.cs
src/ThermoElastic.Desktop/Views/ViewName.axaml
src/ThermoElastic.Desktop/ViewModels/ViewModelName.cs
```

## Related Documentation

See also:
- `docs/requirements.md` — Feature specifications
- `docs/basic-design.md` — Design rationale
- `docs/detailed-design.md` — Implementation details
- `docs/user-guide-en.md` — End-user documentation
- `tests/README.md` — Test strategy

## Version History

| Version | Date | Key Changes |
|---------|------|-------------|
| v1.0.0-ui | 2026-03-23 | Added 19 new views (33 total); organized by 6 categories; 77 E2E tests; 556 test methods |
| v1.0.0 | 2026-03-22 | Added 5 new views (Phase Diagram, Hugoniot, Lookup, Planetary, Kernel); expanded to 42 calculators |
| v0.5.0 | 2026-03-21 | Initial comprehensive mapping (8 views, 15 calculators) |
| Earlier | — | Basic structure documentation |

---

**Generated with automated codemap analysis tool**
**For questions or updates, refer to project maintainers**
