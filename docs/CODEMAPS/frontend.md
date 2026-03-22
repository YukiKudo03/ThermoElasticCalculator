<!-- Generated: 2026-03-23 | v1.0.0-ui | 556 tests passing -->

# Frontend (ThermoElastic.Desktop)

## Technology

- Avalonia 11.2.3 (cross-platform XAML)
- CommunityToolkit.Mvvm 8.4 (source-generated ObservableObject, RelayCommand)
- .NET 9.0
- Version: v1.0.0

## View Hierarchy (33 Views)

**Core Mineralogy:** MineralEditor, MineralDatabase, MineralProperties, MineralSpecs

**EOS & Shock:** PTProfile, Hugoniot, ShockComparison, HugoniotAnalysis

**Phase Equilibria:** PhaseDiagramExplorer, PhaseTransition, PhaseStability, EquilibriumAnalysis

**Mantle & Deep Earth:** PlanetaryInterior, PREMProfile, IsotopeProfile, PostPerovskite

**Material Properties:** Mixture, RockCalculator, ElasticTensor, Anelasticity

**Composition & Fluids:** MeltProperties, Solution, WaterContent, FluidPhase

**Inversion & ML:** Inversion, SensitivityKernel, MCMC

**Results & Visualization:** Results, Chart, LookupTable, DataExport

## ViewModel → Core Mapping (33 pairs)

| Category | ViewModel | Core Classes | Key Features |
|----------|-----------|-------------|--------------|
| **Core** | MineralEditorVM | MieGruneisenEOSOptimizer, MineralParams | Single mineral at (P,T) |
| | MineralDatabaseVM | MineralDatabase.GetAll() | 46 minerals, search/filter |
| | MineralPropertiesVM | MineralPropertyCalculator | Mineral property comparisons |
| | MineralSpecsVM | MineralDatabase | Detailed mineral metadata |
| **Shock** | PTProfileVM | PTProfileCalculator, IsentropeCalculator | Isentropes, geotherms |
| | HugoniotVM | HugoniotCalculator | Shock curves |
| | ShockComparisonVM | HugoniotCalculator | Multi-mineral shock comparison |
| | HugoniotAnalysisVM | HugoniotCalculator, ShockAnalyzer | Shock wave analysis |
| **Phase** | PhaseDiagramExplorerVM | PhaseDiagramCalculator, GibbsMinimizer | Boundary tracing + stability |
| | PhaseTransitionVM | PhaseDiagramCalculator | Polymorph transitions |
| | PhaseStabilityVM | GibbsMinimizer, EnergyCalculator | Gibbs energy surfaces |
| | EquilibriumAnalysisVM | GibbsMinimizer, EquilibriumAggregateCalculator | Multi-phase equilibrium |
| **Mantle** | PlanetaryInteriorVM | PlanetaryInteriorSolver, MarsInteriorModel | Interior profiles + mass-radius |
| | PREMProfileVM | PREMModel, DepthConverter | Earth velocity model |
| | IsotopeProfileVM | IsotopeCalculator, FractionationModel | Isotope fractionation vs. depth |
| | PostPerovskiteVM | PostPerovskiteCalculator, ULVZCalculator | ULVZ modeling |
| **Material** | MixtureVM | MixtureCalculator (Voigt/Reuss/Hill/HS) | HS bounds included |
| | RockCalculatorVM | RockCalculator, PredefinedRocks | Multi-mineral + templates |
| | ElasticTensorVM | ElasticTensorCalculator, TensorVisualizer | Elastic compliance/stiffness |
| | AnelasticityVM | AnelasticityCalculator, QFactorComputer | Seismic attenuation |
| **Fluids** | MeltPropertiesVM | MeltCalculator, MeltParams | Melt thermodynamics |
| | SolutionVM | SolutionCalculator, VanLaarCalculator | Solid solution modeling |
| | WaterContentVM | WaterContentEstimator, SolubilityModel | Water solubility |
| | FluidPhaseVM | FluidMixingCalculator, H2OCO2Calculator | Fluid mixing |
| **Inverse** | InversionVM | LevenbergMarquardtOptimizer, MCMCSampler | Parameter optimization |
| | SensitivityKernelVM | SensitivityKernelCalculator | Seismic kernels |
| | MCMCVM | MCMCSampler, PostProcessor | Bayesian uncertainty |
| **Results** | ResultsVM | ResultSummary, file I/O | CSV/JSON export |
| | ChartVM | ResultSummary plotting | Multiple series |
| | LookupTableVM | LookupTableGenerator | Table generation |
| | DataExportVM | BatchExporter, FileFormatConverter | Batch export + conversion |
| | MainWindowVM | All above (coordinator) | Navigation hub |

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
