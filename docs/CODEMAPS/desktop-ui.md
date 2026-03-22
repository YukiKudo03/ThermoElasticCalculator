<!-- Generated: 2026-03-23 | v1.0.0-ui | 556 tests passing -->

# Desktop UI (Avalonia)

## Technology Stack

- **Avalonia 11.2.3** — Cross-platform XAML UI framework
- **CommunityToolkit.Mvvm 8.4** — Source-generated ObservableObject + RelayCommand
- **.NET 9.0** — Target framework
- **Platforms** — Windows, macOS, Linux

## View Hierarchy & Navigation (33 Views)

```
MainWindow
├── MainWindowViewModel [navigation hub]
│
├── ─── CORE MINERALOGY ───
├── MineralEditorView → MineralEditorViewModel
│   Edit single mineral params + quick calc at (P,T)
│
├── MineralDatabaseView → MineralDatabaseViewModel
│   Browse SLB2011 endmember library (46 minerals)
│   Search + filter by property
│
├── MineralPropertiesView → MineralPropertiesViewModel
│   Compare mineral properties across P-T
│
├── MineralSpecsView → MineralSpecsViewModel
│   Detailed mineral parameters + metadata
│
├── ─── EOS & SHOCK ───
├── PTProfileView → PTProfileViewModel
│   Design P-T path (isentrope, geotherm, etc.)
│   Build & export profile
│
├── HugoniotView → HugoniotViewModel
│   Shock equation of state curves
│   Hugoniot construction + comparison
│
├── ShockComparisonView → ShockComparisonViewModel
│   Compare Hugoniot curves for multiple minerals
│
├── HugoniotAnalysisView → HugoniotAnalysisViewModel
│   Shock wave behavior analysis
│
├── ─── PHASE EQUILIBRIA ───
├── PhaseDiagramExplorerView → PhaseDiagramExplorerViewModel
│   Trace phase boundaries in P-T space
│   Visualize stability regions
│
├── PhaseTransitionView → PhaseTransitionViewModel
│   Map phase transitions between polymorphs
│
├── PhaseStabilityView → PhaseStabilityViewModel
│   Gibbs energy surfaces + stability fields
│
├── EquilibriumAnalysisView → EquilibriumAnalysisViewModel
│   Multi-phase equilibrium solver
│
├── ─── MANTLE & DEEP EARTH ───
├── PlanetaryInteriorView → PlanetaryInteriorViewModel
│   Configure planet: mass, radius, composition
│   Solve self-consistent interior
│   Output radial profiles
│
├── PREMProfileView → PREMProfileViewModel
│   Earth velocity + density 1-D model
│
├── IsotopeProfileView → IsotopeProfileViewModel
│   Trace isotope fractionation vs. depth
│
├── PostPerovskiteView → PostPerovskiteViewModel
│   Ultra-low velocity zone (ULVZ) modeling
│
├── ─── MATERIAL PROPERTIES ───
├── MixtureView → MixtureViewModel
│   Binary mixing: elastic averaging + HS bounds
│   Composition vs. property interpolation
│
├── RockCalculatorView → RockCalculatorViewModel
│   Multi-mineral rock composition
│   Predefined rock templates (Pyrolite, MORB, etc.)
│   Elastic aggregate + density
│
├── ElasticTensorView → ElasticTensorViewModel
│   View/edit full elastic compliance/stiffness
│
├── AnelasticityView → AnelasticityViewModel
│   Seismic attenuation + quality factor
│
├── ─── COMPOSITION & FLUIDS ───
├── MeltPropertiesView → MeltPropertiesViewModel
│   Melt thermodynamics + density
│
├── SolutionView → SolutionViewModel
│   Solid solution thermodynamics (van Laar)
│
├── WaterContentView → WaterContentViewModel
│   Water solubility estimation
│
├── FluidPhaseView → FluidPhaseViewModel
│   H2O-CO2 fluid interaction modeling
│
├── ─── INVERSION & ML ───
├── InversionView → InversionViewModel
│   Levenberg-Marquardt + MCMC optimization
│
├── SensitivityKernelView → SensitivityKernelViewModel
│   Compute seismic sensitivity kernels
│   Travel time derivatives vs. depth
│
├── MCMCView → MCMCViewModel
│   Bayesian uncertainty quantification
│
├── ─── RESULTS & VISUALIZATION ───
├── ResultsView → ResultsViewModel
│   Display calculation results (18-column table)
│   Export to CSV, JSON
│   Sort/filter operations
│
├── ChartView → ChartViewModel
│   Property vs. (P, T) visualization
│   Multiple series overlay
│   Axis configuration
│
├── LookupTableView → LookupTableViewModel
│   Pre-compute interpolation tables
│   Export for fast evaluation
│   Progress indication
│
└── DataExportView → DataExportViewModel
    Batch export + file format conversion
```

## ViewModel-to-Calculator Mapping (33 pairs)

| ViewModel | Core Classes Used | Key Commands |
|-----------|-------------------|--------------|
| **MineralEditorViewModel** | MieGruneisenEOSOptimizer, MineralParams | Calculate, SaveMineral, LoadMineral |
| **MineralDatabaseViewModel** | MineralDatabase.GetAll() | Search, Filter, SelectMineral |
| **MineralPropertiesViewModel** | MineralPropertyCalculator, PropertyComparison | CompareMinerals, SelectProperties |
| **MineralSpecsViewModel** | MineralDatabase, ThermoMineralParams | LoadSpecs, EditParameters, ViewMetadata |
| **PTProfileViewModel** | PTProfileCalculator, IsentropeCalculator | AddPoint, ComputeProfile, Export |
| **HugoniotViewModel** | HugoniotCalculator | ComputeHugoniot, AddCurve, Compare |
| **ShockComparisonViewModel** | HugoniotCalculator | LoadCurves, CompareMinerals, PlotOverlay |
| **HugoniotAnalysisViewModel** | HugoniotCalculator, ShockPropertyAnalyzer | AnalyzeShock, ComputeDerivatives |
| **PhaseDiagramExplorerViewModel** | PhaseDiagramCalculator, GibbsMinimizer | TraceBoundary, DetectTransition, Visualize |
| **PhaseTransitionViewModel** | PhaseDiagramCalculator, TransitionDetector | MapPolymorph, EstimateBoundary |
| **PhaseStabilityViewModel** | GibbsMinimizer, EnergyCalculator | ComputeGibbsSurface, PlotStability |
| **EquilibriumAnalysisViewModel** | GibbsMinimizer, EquilibriumAggregateCalculator | SolveEquilibrium, IterateComposition |
| **PlanetaryInteriorViewModel** | PlanetaryInteriorSolver, RadialProfile, MarsInteriorModel | ConfigurePlanet, Solve, VisualizeMass |
| **PREMProfileViewModel** | PREMModel, DepthConverter | LoadPREM, CompareWithCalc, ExportProfile |
| **IsotopeProfileViewModel** | IsotopeCalculator, FractionationModel | ComputeFractionation, PlotDepthProfile |
| **PostPerovskiteViewModel** | PostPerovskiteCalculator, ULVZCalculator | ComputeULVZ, EstimateVelocity |
| **MixtureViewModel** | MixtureCalculator (Voigt/Reuss/Hill/HS), VProfileCalculator | ComputeProfile, ToggleBounds |
| **RockCalculatorViewModel** | RockCalculator, RockComposition, PredefinedRocks | AddMineral, RemoveMineral, LoadTemplate |
| **ElasticTensorViewModel** | ElasticTensorCalculator, TensorVisualizer | ViewTensor, EditCompliance, RotateFrame |
| **AnelasticityViewModel** | AnelasticityCalculator, QFactorComputer | ComputeQ, PlotAttenuation |
| **MeltPropertiesViewModel** | MeltCalculator, MeltParams | ComputeMeltDensity, EstimateViscosity |
| **SolutionViewModel** | SolutionCalculator, VanLaarCalculator | ComputeActivity, EstimateComposition |
| **WaterContentViewModel** | WaterContentEstimator, SolubilityModel | EstimateWater, PlotVsDepth |
| **FluidPhaseViewModel** | FluidMixingCalculator, H2OCO2Calculator | ComputeFluidDensity, EstimateFugacity |
| **InversionViewModel** | LevenbergMarquardtOptimizer, MCMCSampler | RunInversion, ViewUncertainty, ExportChain |
| **SensitivityKernelViewModel** | SensitivityKernelCalculator, SensitivityKernel | ComputeKernel, PlotDepthProfile |
| **MCMCViewModel** | MCMCSampler, PostProcessor, JointLikelihood | RunMCMC, ConvergenceDiagnostics, PlotTrace |
| **ResultsViewModel** | ResultSummary, file I/O | ExportCSV, ExportJSON, Copy |
| **ChartViewModel** | ResultSummary data binding, plot configuration | ConfigureAxis, ToggleSeries, Zoom |
| **LookupTableViewModel** | LookupTableGenerator, LookupTable | GenerateTable, ProgressReport, Export |
| **DataExportViewModel** | BatchExporter, FileFormatConverter | ExportBatch, SelectFormat, ConfigureOutput |
| **MainWindowViewModel** | All above (coordinator) | NavigateTo(view), IsViewActive(name) |

## Data Binding Flow

```
View [AXAML]
  ↓
ViewModel [ObservableObject, RelayCommand]
  ↓ (data context binding)
Model [ThermoMineralParams, ResultSummary, etc.]
  ↓
Calculator [MieGruneisenEOSOptimizer, etc.]
  ↓ (returns result)
ViewModel [updates properties]
  ↓ (property changed notification)
View [UI refresh]
```

## File I/O Formats

| Extension | Associated ViewModel | Format | Contents |
|-----------|---------------------|--------|----------|
| `.mine` | MineralEditorViewModel | JSON | MineralParams (K0, Debye, etc.) |
| `.ptpf` | PTProfileViewModel | JSON | PTProfile (list of PTData points) |
| `.rock` | RockCalculatorViewModel | JSON | RockComposition (minerals + fractions) |
| `.csv` | ResultsViewModel | CSV | 18-column tabular results + headers |
| `.json` | ResultsViewModel | JSON | Serialized ResultSummary with metadata |

## File Locations

### Views (AXAML — 33 total)
```
src/ThermoElastic.Desktop/Views/
├── MainWindow.axaml
├── AnelasticityView.axaml
├── ChartView.axaml
├── DataExportView.axaml
├── ElasticTensorView.axaml
├── EquilibriumAnalysisView.axaml
├── FluidPhaseView.axaml
├── HugoniotAnalysisView.axaml
├── HugoniotView.axaml
├── IsotopeProfileView.axaml
├── LookupTableView.axaml
├── MCMCView.axaml
├── MeltPropertiesView.axaml
├── MineralDatabaseView.axaml
├── MineralEditorView.axaml
├── MineralPropertiesView.axaml
├── MineralSpecsView.axaml
├── MixtureView.axaml
├── PhaseStabilityView.axaml
├── PhaseTransitionView.axaml
├── PhaseDiagramExplorerView.axaml
├── PlanetaryInteriorView.axaml
├── PostPerovskiteView.axaml
├── PREMProfileView.axaml
├── PTProfileView.axaml
├── ResultsView.axaml
├── RockCalculatorView.axaml
├── SensitivityKernelView.axaml
├── ShockComparisonView.axaml
├── SolutionView.axaml
├── WaterContentView.axaml
└── InversionView.axaml
```

### ViewModels (C# — 33 total)
```
src/ThermoElastic.Desktop/ViewModels/
├── MainWindowViewModel.cs
├── AnelasticityViewModel.cs
├── ChartViewModel.cs
├── DataExportViewModel.cs
├── ElasticTensorViewModel.cs
├── EquilibriumAnalysisViewModel.cs
├── FluidPhaseViewModel.cs
├── HugoniotAnalysisViewModel.cs
├── HugoniotViewModel.cs
├── IsotopeProfileViewModel.cs
├── LookupTableViewModel.cs
├── MCMCViewModel.cs
├── MeltPropertiesViewModel.cs
├── MineralDatabaseViewModel.cs
├── MineralEditorViewModel.cs
├── MineralPropertiesViewModel.cs
├── MineralSpecsViewModel.cs
├── MixtureViewModel.cs
├── PhaseStabilityViewModel.cs
├── PhaseTransitionViewModel.cs
├── PhaseDiagramExplorerViewModel.cs
├── PlanetaryInteriorViewModel.cs
├── PostPerovskiteViewModel.cs
├── PREMProfileViewModel.cs
├── PTProfileViewModel.cs
├── ResultsViewModel.cs
├── RockCalculatorViewModel.cs
├── SensitivityKernelViewModel.cs
├── ShockComparisonViewModel.cs
├── SolutionViewModel.cs
├── WaterContentViewModel.cs
└── InversionViewModel.cs
```

## Navigation Pattern

MainWindowViewModel manages active view via [RelayCommand] navigation methods:

```csharp
[RelayCommand]
public void NavigateToMineralEditor() => ActiveView = "MineralEditor";

[RelayCommand]
public void NavigateToPTProfile() => ActiveView = "PTProfile";

[RelayCommand]
public void NavigateToHugoniot() => ActiveView = "Hugoniot";

[RelayCommand]
public void NavigateToInversion() => ActiveView = "Inversion";

// ... (similar for all 33 views)
```

Each View binds DataContext to corresponding ViewModel in MainWindowViewModel.

## Common UI Controls

| Control | Usage | Examples |
|---------|-------|----------|
| **TextBox** | P, T, composition input | MineralEditor, RockCalculator |
| **ComboBox** | Mineral selection, geotherm type | MineralDatabase, PTProfile |
| **DataGrid** | Result tables, mineral lists | Results, MineralDatabase |
| **Canvas/Plot** | Property vs. P-T charts, boundaries | Chart, PhaseDiagram |
| **Button** | Compute, Export, Clear | All views |
| **ProgressBar** | Long calculations (Lookup table generation) | LookupTableView |
| **Slider** | Parameter sweeps (composition, T range) | Mixture, PTProfile |

## Validation Rules

All ViewModels implement input validation before calculation:

- P range: 0 - 365 GPa (Earth's core pressure)
- T range: 0 - 5000 K
- Composition: sum to 1.0 (with tolerance 1e-6)
- Mineral selection: only valid SLB2011 minerals

## Threading Model

Long-running calculations (Hugoniot, Lookup Table, Planetary Solve) run on background thread to prevent UI freeze:

```csharp
await Task.Run(() => { /* calculation */ });
// Then marshal back to UI thread for property updates
```

## E2E Testing Coverage

77 E2E tests verify:
- Navigation between all 33 views
- Input validation error handling
- Calculation correctness vs. expected results
- File I/O (save/load all formats)
- Property binding refresh
- Export CSV/JSON formatting
- Category-based view switching

Tests located in: `tests/ThermoElastic.Desktop.E2E/`
