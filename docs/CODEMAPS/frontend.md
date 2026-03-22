<!-- Generated: 2026-03-23 | v1.0.0 | 13 views + 13 viewmodels, .NET 9 -->

# Frontend (ThermoElastic.Desktop)

## Technology

- Avalonia 11.2.3 (cross-platform XAML)
- CommunityToolkit.Mvvm 8.4 (source-generated ObservableObject, RelayCommand)
- .NET 9.0
- Version: v1.0.0

## View Hierarchy (13 Views)

```
MainWindow
├── MineralEditorView           — Edit mineral params, quick calc at (P,T)
├── MineralDatabaseView         — Browse SLB2011 library (46 minerals)
├── PTProfileView               — Design P-T paths (isentrope, geotherm)
├── PhaseDiagramExplorerView    — Trace phase boundaries (NEW in v1.0)
├── MixtureView                 — Binary mixing + HS bounds
├── RockCalculatorView          — Multi-mineral composition + templates
├── ResultsView                 — Display/export 18-col results
├── ChartView                   — Property visualization
├── HugoniotView                — Shock EOS curves (NEW in v1.0)
├── LookupTableView             — Pre-compute interpolation tables (NEW in v1.0)
├── PlanetaryInteriorView       — Interior profiles + mass-radius (NEW in v1.0)
├── SensitivityKernelView       — Seismic sensitivity kernels (NEW in v1.0)
└── MainWindow                  — Navigation hub
```

## ViewModel → Core Mapping (13 pairs)

| ViewModel | Core Classes Used | Key Features |
|-----------|------------------|--------------|
| MineralEditorVM | MieGruneisenEOSOptimizer, MineralParams | Single mineral at (P,T) |
| MineralDatabaseVM | MineralDatabase.GetAll() | 46 minerals, search/filter |
| PTProfileVM | PTProfileCalculator, IsentropeCalculator | Isentropes, geotherms |
| PhaseDiagramExplorerVM | PhaseDiagramCalculator, GibbsMinimizer | Boundary tracing + stability |
| MixtureVM | MixtureCalculator (Voigt/Reuss/Hill/HS) | HS bounds included (NEW) |
| RockCalculatorVM | RockCalculator, PredefinedRocks | Multi-mineral + templates |
| ResultsVM | ResultSummary, file I/O | CSV/JSON export |
| ChartVM | ResultSummary plotting | Multiple series |
| HugoniotVM | HugoniotCalculator | Shock curves (NEW) |
| LookupTableVM | LookupTableGenerator | Table generation (NEW) |
| PlanetaryInteriorVM | PlanetaryInteriorSolver, MarsInteriorModel | Radial profiles (NEW) |
| SensitivityKernelVM | SensitivityKernelCalculator | Seismic kernels (NEW) |
| MainWindowVM | All above (coordinator) | Navigation hub |

## Navigation Pattern

```csharp
MainWindowViewModel manages 13 views via [RelayCommand] navigation:
NavigateToMineralEditor(), NavigateToPTProfile(), ... NavigateToSensitivityKernel()
Each View's DataContext → corresponding ViewModel
Property changes → UI refresh via INotifyPropertyChanged
```

## File Formats

| Extension | Purpose | Format | Views |
|-----------|---------|--------|-------|
| `.mine` | Mineral parameters | JSON | MineralEditor |
| `.ptpf` | P-T profile | JSON | PTProfile |
| `.vpf` | Composition profile | JSON | Mixture |
| `.rock` | Rock composition | JSON | RockCalculator |
| `.csv` | Tabular data | 18-column | Results, export |
| `.json` | Serialized results | JSON | Results, all exporters |
