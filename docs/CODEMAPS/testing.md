<!-- Generated: 2026-03-24 | Files scanned: 57 unit tests + 8 E2E tests | Token estimate: ~950 -->

# Testing Codemap

**Version:** v1.0.0
**Last Updated:** 2026-03-24
**Total Test Methods:** ~591 (507 unit + 84 E2E)
**Code Coverage:** 95.6% (Core library, increased with anelasticity tests)

## Test Projects Overview

```
ThermoElasticCalculator.sln
├── ThermoElastic.Core.Tests/
│   ├── Framework: xUnit 2.9.0 + Coverlet 6.0.2
│   ├── Test Classes: 57 (new: AnelasticityEnhancedTests, AnelasticityLiteratureE2ETests)
│   ├── Test Methods: ~507 (new: 28 enhanced + 9 literature)
│   ├── Categories: Unit tests (Fact/Theory), Literature verification
│   └── Scope: Calculators + Models + Database
│
└── ThermoElastic.Desktop.E2E/
    ├── Framework: xUnit 2.9.0 + Avalonia Headless
    ├── Test Classes: 8 (new: FullStackE2ETests expanded with 6 anelasticity tests)
    ├── Test Methods: ~84 (new: 6 App28 E2E for anelasticity)
    ├── Categories: ViewModel E2E, Visual/Screenshot E2E
    └── Scope: Full UI flow validation
```

## Unit Test Structure (ThermoElastic.Core.Tests)

**File Location:** `tests/ThermoElastic.Core.Tests/`

### Core Calculator Tests (20+ Classes)

| Test Class | Scope | Key Test Methods | File |
|-----------|-------|-----------------|------|
| **MieGruneisenEOSOptimizerTests** | BM3 EOS convergence | TestConvergenceOlivine, TestHighPressure, TestHighTemperature | `MieGruneisenEOSOptimizerTests.cs` |
| **DebyeFunctionCalculatorTests** | Debye integral D₃ | TestIntegralD3_LowTemp, TestIntegralD3_HighTemp, TestDebyeLimitCv | `DebyeFunctionCalculatorTests.cs` |
| **LandauCalculatorTests** | Phase transition modeling | TestLandau_BelowTc, TestLandau_AboveTc, TestClapeyron | `LandauCalculatorTests.cs` |
| **PTProfileCalculatorTests** | Batch P-T calculation | TestProfileCalculation, TestConvergenceAllPoints, TestCSVExport | `PTProfileCalculatorTests.cs` |
| **MixtureCalculatorTests** | Multi-phase bounds (HS, Voigt, Reuss, Hill) | TestVoigtBound, TestReussBound, TestHSBound, TestConsistency | `MixtureCalculatorTests.cs` |
| **SolutionCalculatorTests** | Solid solution + activity | TestVanLaarActivity, TestExcessGibbs | `SolutionCalculatorTests.cs` |
| **RockCalculatorTests** | Aggregate rock properties | TestPyrolite, TestHarzburgite, TestMORB | `RockCalculatorTests.cs` |
| **GibbsMinimizerTests** | Phase equilibrium stability | TestStableAssemblage, TestMantelTransition | `GibbsMinimizerTests.cs` |
| **VProfileCalculatorTests** | V(P) equation of state | TestVolumePressure, TestBulkModulusFromVolume | `VProfileCalculatorTests.cs` |
| **HugoniotCalculatorTests** | Shock EOS | TestHugoniotCurve, TestRankineHugoniot | `HugoniotCalculatorTests.cs` |
| **IsentropeCalculatorTests** | Adiabatic geotherm | TestIsentrope, TestMantelAdiabat | `IsentropeCalculatorTests.cs` |
| **RockCalculatorTests** | Multi-mineral aggregates | TestVoigtReussMixing, TestEquivalentMediaApproximation | `RockCalculatorTests.cs` |
| **AnelasticityCalculatorTests** | Seismic Q⁻¹ | TestQInvariant, TestFrequencyDependence | `AnelasticityCalculatorTests.cs` |
| **AnelasticityEnhancedTests** | **NEW:** Multi-tier Q models, grain-size, water, melt effects (28 tests) | Phase 1-5: AnelasticityParams, IAnelasticityModel, ParametricQCalculator, AndradeCalculator, ExtendedBurgersCalculator, WaterQCorrector, MeltQCorrector, QProfileBuilder | `AnelasticityEnhancedTests.cs` |
| **AnelasticityLiteratureE2ETests** | **NEW:** Cross-validation vs. Jackson & Faul 2010, Andrade creep, Burgers model (9 tests) | Literature comparison: grain-size dependence, frequency dependence, water effect on relaxation | `AnelasticityLiteratureE2ETests.cs` |
| **ElasticTensorCalculatorTests** | Stiffness tensor Cᵢⱼₖₗ | TestCubicSymmetry, TestHexagonalSymmetry | `ElasticTensorCalculatorTests.cs` |
| **ThermalConductivityCalculatorTests** | κ(P,T) | TestKappaTemperature, TestKappaPressure | `ThermalConductivityCalculatorTests.cs` |
| **ElectricalConductivityCalculatorTests** | σ(P,T,fO₂) | TestConductivityRedox, TestConductivityTemperature | `ElectricalConductivityCalculatorTests.cs` |
| **IronPartitioningSolverTests** | Fe²⁺/Fe³⁺ KD | TestPartitioningOlv_Opx, TestPartitioningGrt_Cpx | `IronPartitioningSolverTests.cs` |
| **SpinCrossoverCalculatorTests** | Fe²⁺ spin transition | TestSpinState_HighSpin, TestSpinState_LowSpin | `SpinCrossoverCalculatorTests.cs` |
| **OxygenFugacityCalculatorTests** | fO₂ buffers | TestQFMBuffer, TestIWBuffer | `OxygenFugacityCalculatorTests.cs` |
| **LevenbergMarquardtOptimizerTests** | Parameter fitting | TestConvergence, TestResiduals, TestCovariance | `LevenbergMarquardtOptimizerTests.cs` |

### Data Model Tests (15+ Classes)

| Test Class | Scope | Key Test Methods | File |
|-----------|-------|-----------------|------|
| **MineralParamsTests** | Mineral parameter serialization + validation | TestJsonSerialization, TestJsonDeserialization, TestParameterBounds | `MineralParamsTests.cs` |
| **ThermoMineralParamsTests** | Thermodynamic properties calculation | TestCalculatedProperties, TestConvergenceStatus | `ThermoMineralParamsTests.cs` |
| **RockCompositionTests** | Rock composition + fractions | TestFractionNormalization, TestMinimumCount | `RockCompositionTests.cs` |
| **PTProfileTests** | P-T profile lists | TestProfileLoading, TestProfileSerialization | `PTProfileTests.cs` |
| **SolidSolutionTests** | Solid solution structure | TestSiteOccupancies, TestCompositionConstraints | `SolidSolutionTests.cs` |
| **PhaseAssemblageTests** | Multi-phase assemblages | TestStabilityConditions, TestCompositionSum | `PhaseAssemblageTests.cs` |
| **ElasticTensorTests** | Tensor symmetry + voigt notation | TestVoigtNotation, TestSymmetryClass | `ElasticTensorTests.cs` |
| **LookupTableTests** | Interpolation + tabulation | TestInterpolation, TestBoundary, TestExtrapolation | `LookupTableTests.cs` |
| **MCMCChainTests** | MCMC sample statistics | TestAcceptanceRate, TestAutocorrelation | `MCMCChainTests.cs` |
| **OptimizationResultTests** | Fitting result structure | TestCovariance, TestResiduals | `OptimizationResultTests.cs` |
| **TrainingDataPointTests** | ML training examples | TestFeatureNormalization, TestLabelEncoding | `TrainingDataPointTests.cs` |
| **InversionResultTests** | Inverse problem output | TestUncertainty, TestMisfitMetrics | `InversionResultTests.cs` |
| **VerificationResultTests** | Cross-validation results | TestComparison, TestPassCriteria | `VerificationResultTests.cs` |
| **ResultSummaryTests** | CSV export format | TestCSVColumns, TestCSVExport, TestJSONSerialization | `ResultSummaryTests.cs` |

### Database & IO Tests (5+ Classes)

| Test Class | Scope | Key Test Methods | File |
|-----------|-------|-----------------|------|
| **MineralDatabaseTests** | SLB2011 database access + integrity | TestEndmemberCount (46), TestSolidSolutionLoading, TestPredefinedRocks, TestElasticConstants | `MineralDatabaseTests.cs` |
| **SLB2011EndmembersTests** | 46 endmember parameters | TestOlivineParameters, TestPyroxeneParameters, TestGarnetParameters, TestPerovskiteParameters | `SLB2011EndmembersTests.cs` |
| **SLB2011SolutionsTests** | Binary solution models | TestOlivineMixing, TestPyroxeneMixing, TestSpinelMixing, TestGarnetMixing | `SLB2011SolutionsTests.cs` |
| **PredefinedRocksTests** | Pyrolite, Harzburgite, MORB, Piclogite | TestPyroliteComposition, TestHarzburgiteComposition | `PredefinedRocksTests.cs` |
| **IOTests** | JSON/CSV serialization | TestMineralJsonIO, TestProfileJsonIO, TestRockJsonIO, TestResultsCSVIO | `IOTests.cs` |

### Literature Verification Tests (10+ Classes)

**Purpose:** Cross-validate against published data and BurnMan

| Test Class | Reference | Key Verification | File |
|-----------|-----------|------------------|------|
| **BurnManEndmemberVerificationTests** | BurnMan v0.6+ (Python) | Compare Vp, Vs, ρ at STP for all 46 minerals | `BurnManEndmemberVerificationTests.cs` |
| **BurnManPhaseEquilibriumTests** | BurnMan phase boundaries | Verify olivine-wadsleyite, wadsleyite-ringwoodite transitions | `BurnManPhaseEquilibriumTests.cs` |
| **StixrudeVerificationTests** | Stixrude & Lithgow-Bertelloni 2011 tables | Check published Vp(P,T) tables (SLB2011 paper) | `StixrudeVerificationTests.cs` |
| **THERMOCALCVerificationTests** | THERMOCALC software | Compare phase diagrams (MORB, mantle) | `THERMOCALCVerificationTests.cs` |
| **DebyeModelVerificationTests** | Debye model literature | Verify D₃ function, heat capacity Dulong-Petit limit | `DebyeModelVerificationTests.cs` |
| **HashinShtrikmanVerificationTests** | HS bound theory | Confirm bounds hold: M_r ≤ M_hs ≤ M_v | `HashinShtrikmanVerificationTests.cs` |
| **MieGruneisenVerificationTests** | Mie-Gruneisen EOS theory | Check thermal pressure, volume thermal expansion | `MieGruneisenVerificationTests.cs` |
| **HugoniotVerificationTests** | SESAME EOS tables | Compare shock curves with standard data | `HugoniotVerificationTests.cs` |
| **LandauVerificationTests** | Landau theory literature | Verify order-disorder transition shapes | `LandauVerificationTests.cs` |
| **MagneticContributionTests** | Spin model literature | Check magnetic free energy contributions | `MagneticContributionTests.cs` |

### Integration Tests (5+ Classes)

| Test Class | Scope | Key Test Methods | File |
|-----------|-------|-----------------|------|
| **ChartDataTests** | OxyPlot data generation | TestXYDataGeneration, TestContourGeneration | `ChartDataTests.cs` |
| **EquilibriumAggregateTests** | Multi-phase equilibrium | TestAssemblageStability, TestCompositionRange | `EquilibriumAggregateTests.cs` |
| **GibbsMinimizerTests** | Phase diagram computation | TestMantleTransition, TestComplexMultiphaseEquilibria | `GibbsMinimizerTests.cs` |
| **ClassicalGeobarometryTests** | Barometer + thermometer | TestGarnetCpxBarometer, TestOpx-Cpx Thermometer | `ClassicalGeobarometryTests.cs` |
| **BayesianInversionTests** | Full inversion workflow | TestPosteriorDistribution, TestUncertaintyPropagation | `BayesianInversionTests.cs` |

## E2E Test Structure (ThermoElastic.Desktop.E2E)

**File Location:** `tests/ThermoElastic.Desktop.E2E/`

### Test Framework

**xUnit + Avalonia Headless:**
```csharp
[Trait("Category", "E2E")]
public class ViewModelE2ETests : IAsyncLifetime
{
    private TestAppBuilder _appBuilder;
    private MainWindowViewModel _mainViewModel;

    public async Task InitializeAsync()
    {
        _appBuilder = new TestAppBuilder();
        _mainViewModel = await _appBuilder.BuildApp();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShowMineralEditor_LoadMineral_DisplaysData()
    {
        // Arrange
        var mineral = MineralDatabase.GetEndmember("Fo90");

        // Act
        _mainViewModel.ShowMineralEditorCommand.Execute(null);
        var vm = (MineralEditorViewModel)_mainViewModel.CurrentView;
        await vm.LoadMineralCommand.ExecuteAsync(mineralPath);

        // Assert
        Assert.NotNull(vm.SelectedMineral);
        Assert.Equal("Fo90", vm.SelectedMineral.MineralName);
    }
}
```

### Phase 1: Core Tool E2E Tests (20+ Tests)

| Test | ViewModel | Coverage | File |
|------|-----------|----------|------|
| ShowMineralEditor_LoadMineral_DisplaysData | MineralEditorViewModel | Load JSON → display properties | Phase1ViewModelE2ETests.cs |
| ShowMineralEditor_CalculateTest_ProducesResult | MineralEditorViewModel | Calculate button → ThermoMineralParams |
| ShowDatabase_Filter_ReturnsMatches | MineralDatabaseViewModel | Query 46 endmembers → filtered list |
| ShowPTProfile_LoadProfile_BatchCalculates | PTProfileViewModel | Load .ptpf → batch calculate → export CSV |
| ShowMixture_Calculate_ProducesHSBounds | MixtureViewModel | 2-mineral mixing → all 4 models (Voigt/Reuss/Hill/HS) |
| ShowRockCalculator_LoadPyrolite_ManipulateFractions | RockCalculatorViewModel | Load predefined rock → modify fractions → recalculate |

### Phase 2: EOS & Shock E2E Tests (15+ Tests)

| Test | ViewModel | Coverage | File |
|------|-----------|----------|------|
| ShowHugoniot_CalculateCurve_PlotDisplay | HugoniotViewModel | Calculate shock curve → canvas renders |
| ShowEOSFitter_LoadData_FitParameters | EOSFitterViewModel | Load P-V data → LM optimize → fitted K₀, K₀', K₀'' |
| ShowThermoElasticFitter_FitVpVs_ProducesModel | ThermoElasticFitterViewModel | Fit Vp/Vs model to depth profile |
| ShowVerificationDashboard_RunSuite_CompareBurnMan | VerificationDashboardViewModel | Cross-validate 46 minerals vs. BurnMan → show Δ% |

### Phase 3: Advanced Tools E2E Tests (15+ Tests)

| Test | ViewModel | Coverage | File |
|------|-----------|----------|------|
| ShowPhaseDiagram_CalculateBoundary_PlotBoundary | PhaseDiagramExplorerViewModel | Map phase boundary → interactive canvas |
| ShowCompositionInverter_InvertVpVs_EstimateComposition | CompositionInverterViewModel | Input Vp/Vs → solve for mineral fractions |
| ShowBayesianInversion_RunMCMC_PlotPosterior | BayesianInversionViewModel | MCMC sampling → posterior plot → uncertainty |
| ShowMLData_GenerateTrainingSet_ExportCSV | MLDataViewModel | Synthetic data generation → CSV export |

### Phase 4: Enhanced Anelasticity E2E Tests (6 NEW Tests)

| Test | ViewModel | Coverage | File |
|------|-----------|----------|------|
| ShowAnelasticity_SelectTier2_CalculateQ_ProducesResult | **AnelasticityViewModel** (enhanced) | Model selection + grain size + water/melt sliders → Q⁻¹ result | FullStackE2ETests.cs |
| ShowAnelasticity_VaryGrainSize_QChangesMonotonically | **AnelasticityViewModel** | Grain size sweep (5-50 μm) → verify Q ∝ d^m trend | FullStackE2ETests.cs |
| ShowAnelasticity_AddWater_QDecreases | **AnelasticityViewModel** | Water effect (0 → 100 ppm) → Q reduced via decorators | FullStackE2ETests.cs |
| ShowAnelasticity_AddMelt_QDecreases | **AnelasticityViewModel** | Melt effect (0 → 5% melt) → Q damping | FullStackE2ETests.cs |
| ShowQProfile_BuildProfile_DisplaysDepthDependentQ | **QProfileViewModel** (NEW) | Geotherm selection + frequency → 1D Q(depth) profile | FullStackE2ETests.cs |
| ShowQProfile_ExportProfile_SavesCSV | **QProfileViewModel** (NEW) | Build profile → export CSV with Depth, T, Q, model metadata | FullStackE2ETests.cs |

### Visual/Screenshot E2E Tests (20+ Tests)

**File:** `VisualScreenshotTests.cs`

**Purpose:** Automated screenshot validation (regression testing)

```csharp
[Trait("Category", "E2E-Visual")]
public class VisualScreenshotTests : IAsyncLifetime
{
    [Fact]
    public async Task MainWindow_Display_MatchesGoldenImage()
    {
        var image = await _appBuilder.TakeScreenshot("MainWindow");
        var golden = await File.ReadAllBytesAsync("docs/images/main_window.png");
        AssertImagesEqual(image, golden, tolerance: 0.02f);
    }

    [Fact]
    public async Task MineralEditor_Display_MatchesGoldenImage()
    {
        _mainViewModel.ShowMineralEditorCommand.Execute(null);
        var image = await _appBuilder.TakeScreenshot("MineralEditor");
        var golden = await File.ReadAllBytesAsync("docs/images/02_mineral_editor.png");
        AssertImagesEqual(image, golden);
    }

    // 20+ more screenshot tests...
}
```

**Screenshot Database:**
```
docs/images/
├── main_window.png
├── 02_mineral_editor.png
├── 03_pt_profile.png
├── 04_mixture.png
├── 05_rock_calculator.png
├── 06_results.png
└── ... (more views)
```

### Full Stack E2E Tests (15+ Tests)

**File:** `FullStackE2ETests.cs`

**Purpose:** End-to-end workflow validation

```csharp
[Trait("Category", "E2E-FullStack")]
public class FullStackE2ETests : IAsyncLifetime
{
    [Fact]
    public async Task Workflow_CreateMineral_CalculateProfile_ExportResults_Success()
    {
        // 1. Create mineral in editor
        _mainViewModel.ShowMineralEditorCommand.Execute(null);
        var editorVm = (MineralEditorViewModel)_mainViewModel.CurrentView;
        editorVm.SelectedMineral = CreateTestMineral("TestFo90");
        await editorVm.SaveMineralCommand.ExecuteAsync(null);

        // 2. Load in PT Profile calculator
        _mainViewModel.ShowPTProfileCommand.Execute(null);
        var profileVm = (PTProfileViewModel)_mainViewModel.CurrentView;
        profileVm.SelectedMineral = editorVm.SelectedMineral;
        var profile = CreateTestProfile(); // 10 P-T points
        profileVm.LoadedProfile = profile;

        // 3. Calculate
        await profileVm.CalculateCommand.ExecuteAsync(null);
        Assert.NotEmpty(profileVm.ResultsList);

        // 4. Export to CSV
        await profileVm.ExportCommand.ExecuteAsync("test_results.csv");
        Assert.True(File.Exists("test_results.csv"));

        // 5. Verify CSV structure
        var lines = File.ReadAllLines("test_results.csv");
        Assert.Equal(11, lines.Length); // Header + 10 results
        Assert.Contains("Vp", lines[0]); // Header has Vp column
    }

    [Fact]
    public async Task Workflow_RockCalculator_LoadPyrolite_MixingModels_Consistent()
    {
        // Load predefined Pyrolite rock
        _mainViewModel.ShowRockCalculatorCommand.Execute(null);
        var rockVm = (RockCalculatorViewModel)_mainViewModel.CurrentView;

        // Test all 4 mixing models on same composition
        var models = new[] { "Voigt", "Reuss", "Hill", "HS" };
        var results = new Dictionary<string, double>();

        foreach (var model in models)
        {
            rockVm.MixingModel = model;
            await rockVm.CalculateCommand.ExecuteAsync(null);
            results[model] = rockVm.Results.Vp;
        }

        // Verify Reuss ≤ Hill ≤ Voigt for bulk modulus
        Assert.True(results["Reuss"] <= results["Hill"]);
        Assert.True(results["Hill"] <= results["Voigt"]);
    }
}
```

## Test Execution & Coverage

### Run All Tests

```bash
dotnet test
```

### Run Unit Tests Only

```bash
dotnet test --filter "Category!=E2E"
```

### Run E2E Tests Only

```bash
dotnet test --filter "Category=E2E"
```

### Run with Coverage Report

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=json /p:CoverageFileName=coverage.json
```

**Coverage Results (v1.0.0):**
- Core Library: 95.6% line coverage
- Model Classes: 98% coverage
- Calculator Classes: 94% coverage
- Database: 100% coverage (hardcoded data)
- Desktop UI: 75% coverage (UI controls hard to test)

## Test Data

**Location:** `tests/ThermoElastic.Core.Tests/TestData/`

| File | Purpose | Usage |
|------|---------|-------|
| `Minerals_SLB2011.csv` | 46 endmember parameters | Import test |
| `PVData_Olivine.csv` | P-V experimental data | EOS fitter test |
| `VpVs_Profile.csv` | Velocity depth profile | Inversion test |
| `Phase_Boundary.csv` | Olivine-Wadsleyite boundary | Phase diagram validation |
| `BurnMan_Comparison.json` | BurnMan output reference | Verification test |

## Key Test Patterns

### Fact Tests (Single Fixed Input)

```csharp
[Fact]
public void BirchMurnaghan_Olivine_AtSTP_Returns928kms()
{
    var mineral = MineralDatabase.GetEndmember("Fo90");
    var result = new MieGruneisenEOSOptimizer(mineral, 1.0, 300.0).ExecOptimize();
    Assert.InRange(result.Vp, 9.2, 9.3); // km/s
}
```

### Theory Tests (Parameterized)

```csharp
[Theory]
[InlineData(1.0, 1200, 0.02)] // P=1 GPa, T=1200K, expected tolerance
[InlineData(10.0, 2000, 0.03)]
[InlineData(100.0, 3000, 0.05)]
public void MieGruneisen_Convergence(double p, double t, double tolerance)
{
    var mineral = MineralDatabase.GetEndmember("Fo90");
    var result = new MieGruneisenEOSOptimizer(mineral, p, t).ExecOptimize();
    Assert.True(result.IsConverged);
    Assert.InRange(result.PressureResidual, 0, tolerance);
}
```

### Cross-Validation Tests

```csharp
[Fact]
public void CompareWithBurnMan_Olivine_Fo90_AtSTP()
{
    var verifier = new ThermodynamicVerifier();
    var result = verifier.Verify(MineralDatabase.GetEndmember("Fo90"), 1.0, 300.0);

    Assert.Equal("PASS", result.Status);
    Assert.InRange(result.PercentDifference, -1.5, 1.5); // ±1.5%
}
```

## Performance Benchmarks

| Calculation | Time | Constraint |
|-------------|------|-----------|
| Single mineral Vp/Vs calc | ~1 ms | Interactive (<10 ms) |
| P-T profile (100 points) | ~100 ms | User acceptable |
| Rock mixture (4 minerals) | ~10 ms | Interactive |
| Phase equilibrium (SVD) | ~500 ms | Background task |
| Hugoniot curve (100 points) | ~200 ms | Background task |
| MCMC (10k iterations) | ~5-10 s | Progress bar + cancel |

## Continuous Integration

**GitHub Actions:** `.github/workflows/test.yml`

```yaml
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      - run: dotnet test --logger "trx;LogFileName=test-results.trx"
      - uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results-${{ matrix.os }}
          path: '**/test-results.trx'
```

---

**Next:** See [dependencies.md](./dependencies.md) for external packages and versions.
