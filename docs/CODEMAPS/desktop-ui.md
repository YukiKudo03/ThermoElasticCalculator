<!-- Generated: 2026-03-23 | v1.0.0 | 13 views + 13 viewmodels -->

# Desktop UI (Avalonia)

## Technology Stack

- **Avalonia 11.2.3** — Cross-platform XAML UI framework
- **CommunityToolkit.Mvvm 8.4** — Source-generated ObservableObject + RelayCommand
- **.NET 9.0** — Target framework
- **Platforms** — Windows, macOS, Linux

## View Hierarchy & Navigation

```
MainWindow
├── MainWindowViewModel [navigation hub]
│
├── MineralEditorView → MineralEditorViewModel
│   Edit single mineral params + quick calc at (P,T)
│
├── MineralDatabaseView → MineralDatabaseViewModel
│   Browse SLB2011 endmember library (46 minerals)
│   Search + filter by property
│
├── PTProfileView → PTProfileViewModel
│   Design P-T path (isentrope, geotherm, etc.)
│   Build & export profile
│
├── PhaseDiagramExplorerView → PhaseDiagramExplorerViewModel
│   Trace phase boundaries in P-T space
│   Visualize stability regions
│
├── MixtureView → MixtureViewModel
│   Binary mixing: elastic averaging + HS bounds
│   Composition vs. property interpolation
│
├── RockCalculatorView → RockCalculatorViewModel
│   Multi-mineral rock composition
│   Predefined rock templates (Pyrolite, MORB, etc.)
│   Elastic aggregate + density
│
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
├── HugoniotView → HugoniotViewModel
│   Shock equation of state curves
│   Hugoniot construction + comparison
│
├── LookupTableView → LookupTableViewModel
│   Pre-compute interpolation tables
│   Export for fast evaluation
│   Progress indication
│
├── PlanetaryInteriorView → PlanetaryInteriorViewModel
│   Configure planet: mass, radius, composition
│   Solve self-consistent interior
│   Output radial profiles
│
└── SensitivityKernelView → SensitivityKernelViewModel
    Compute seismic sensitivity kernels
    Travel time derivatives vs. depth
```

## ViewModel-to-Calculator Mapping

| ViewModel | Core Classes Used | Key Commands |
|-----------|-------------------|--------------|
| **MineralEditorViewModel** | MieGruneisenEOSOptimizer, MineralParams | Calculate, SaveMineral, LoadMineral |
| **MineralDatabaseViewModel** | MineralDatabase.GetAll() | Search, Filter, SelectMineral |
| **PTProfileViewModel** | PTProfileCalculator, PTProfile, IsentropeCalculator | AddPoint, ComputeProfile, Export |
| **PhaseDiagramExplorerViewModel** | PhaseDiagramCalculator, GibbsMinimizer | TraceBoundary, DetectTransition, Visualize |
| **MixtureViewModel** | MixtureCalculator (Voigt/Reuss/Hill/HS), VProfileCalculator | ComputeProfile, ToggleBounds |
| **RockCalculatorViewModel** | RockCalculator, RockComposition, PredefinedRocks | AddMineral, RemoveMineral, LoadTemplate |
| **ResultsViewModel** | ResultSummary, file I/O | ExportCSV, ExportJSON, Copy |
| **ChartViewModel** | ResultSummary data binding, plot configuration | ConfigureAxis, ToggleSeries, Zoom |
| **HugoniotViewModel** | HugoniotCalculator | ComputeHugoniot, AddCurve |
| **LookupTableViewModel** | LookupTableGenerator, LookupTable | GenerateTable, ProgressReport, Export |
| **PlanetaryInteriorViewModel** | PlanetaryInteriorSolver, RadialProfile, MarsInteriorModel | ConfigurePlanet, Solve, VisualizeMass |
| **SensitivityKernelViewModel** | SensitivityKernelCalculator, SensitivityKernel | ComputeKernel, PlotDepthProfile |
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

### Views (AXAML)
```
src/ThermoElastic.Desktop/Views/
├── MainWindow.axaml
├── ChartView.axaml
├── HugoniotView.axaml
├── LookupTableView.axaml
├── MineralDatabaseView.axaml
├── MineralEditorView.axaml
├── MixtureView.axaml
├── PTProfileView.axaml
├── PhaseDiagramExplorerView.axaml
├── PlanetaryInteriorView.axaml
├── ResultsView.axaml
├── RockCalculatorView.axaml
└── SensitivityKernelView.axaml
```

### ViewModels (C#)
```
src/ThermoElastic.Desktop/ViewModels/
├── MainWindowViewModel.cs
├── ChartViewModel.cs
├── HugoniotViewModel.cs
├── LookupTableViewModel.cs
├── MineralDatabaseViewModel.cs
├── MineralEditorViewModel.cs
├── MixtureViewModel.cs
├── PTProfileViewModel.cs
├── PhaseDiagramExplorerViewModel.cs
├── PlanetaryInteriorViewModel.cs
├── ResultsViewModel.cs
├── RockCalculatorViewModel.cs
└── SensitivityKernelViewModel.cs
```

## Navigation Pattern

MainWindowViewModel manages active view via [RelayCommand] navigation methods:

```csharp
[RelayCommand]
public void NavigateToMineralEditor() => ActiveView = "MineralEditor";

[RelayCommand]
public void NavigateToPTProfile() => ActiveView = "PTProfile";
// ... (similar for all 13 views)
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

45 E2E tests verify:
- Navigation between all 13 views
- Input validation error handling
- Calculation correctness vs. expected results
- File I/O (save/load all formats)
- Property binding refresh
- Export CSV/JSON formatting

Tests located in: `tests/ThermoElastic.Desktop.E2E/`
