<!-- Generated: 2026-03-18 | Files scanned: 18 | Token estimate: ~500 -->
# Frontend (ThermoElastic.Desktop)

## Technology

- Avalonia 11.2 (cross-platform XAML)
- CommunityToolkit.Mvvm 8.4 (source-generated ObservableObject, RelayCommand)
- .NET 8.0

## View Hierarchy

```
MainWindow
├── MineralEditorView     — Edit mineral params, test calc at single P,T
├── MineralDatabaseView   — Browse SLB2011 endmember library
├── PTProfileView         — Build P-T profile, calc along path
├── MixtureView           — Two-mineral binary mixing
├── RockCalculatorView    — Multi-mineral rock composition
├── ResultsView           — Display/export result tables (CSV)
└── ChartView             — Property vs P-T visualization
```

## ViewModel → Core Mapping

| ViewModel | Core Classes Used |
|-----------|------------------|
| MineralEditorVM | MieGruneisenEOSOptimizer, MineralParams |
| MineralDatabaseVM | MineralDatabase.GetAll() |
| PTProfileVM | PTProfileCalculator, PTProfile |
| MixtureVM | MixtureCalculator (Voigt/Reuss/Hill) |
| RockCalculatorVM | RockCalculator, RockComposition |
| ResultsVM | ResultSummary (CSV export) |
| ChartVM | ResultSummary data binding |

## Navigation

MainWindowViewModel manages sub-VMs via `[RelayCommand]` navigation methods.
Each View binds to its ViewModel through Avalonia DataContext.

## File Formats

| Extension | Purpose | Format |
|-----------|---------|--------|
| `.mine` | Mineral parameters | JSON |
| `.ptpf` | P-T profile | JSON |
| `.vpf` | Composition profile | JSON |
| `.rock` | Rock composition | JSON |
| `.csv` | Import/export data | CSV |
