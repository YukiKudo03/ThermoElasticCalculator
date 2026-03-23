<!-- Generated: 2026-03-24 | Files scanned: 35 views + 35 ViewModels | Token estimate: ~1200 -->

# UI Layer Codemap

**Version:** v1.0.0
**Last Updated:** 2026-03-24
**Scope:** ThermoElastic.Desktop MVVM Views and ViewModels

## MVVM Architecture

```
View (AXAML)                    ViewModel (C#)                   Model/Calculator (C#)
├─ TextBox                      ├─ ObservableProperty            ├─ MineralParams
├─ ComboBox (binding)           ├─ RelayCommand                  ├─ PTProfile
├─ DataGrid (ItemsSource)       │   └─ public void ShowXyz()     ├─ RockComposition
├─ Canvas (OxyPlot)             ├─ Validation logic              ├─ Calculator
└─ Button (Command)             └─ Threading (Task.Run)          └─ Database

     INotifyPropertyChanged        ObservableObject              Static methods
            ↓↓↓ binding ↓↓↓         (source-gen MVVM)
```

**Pattern:** CommunityToolkit.Mvvm 8.4.0
- `[ObservableProperty] private T _field;` → auto-generates Property, OnPropertyChanged()
- `[RelayCommand] private void Command()` → auto-generates ICommand, CanExecute()
- All ViewModels inherit `ObservableObject`

## View Hierarchy & Navigation

```
MainWindow (App root)
  └─ MainWindowViewModel
      ├─ CurrentView: object? (bound in View)
      └─ RelayCommands (35 total)
          ├─ ShowMineralEditor()        → MineralEditorViewModel
          ├─ ShowPTProfile()            → PTProfileViewModel
          ├─ ShowMixture()              → MixtureViewModel
          ├─ ShowRockCalculator()       → RockCalculatorViewModel
          ├─ ShowResults()              → ResultsViewModel
          ├─ ShowDatabase()             → MineralDatabaseViewModel
          ├─ ShowChart()                → ChartViewModel
          ├─ ShowHugoniot()             → HugoniotViewModel
          ├─ ShowEOSFitter()            → EOSFitterViewModel
          ├─ ShowThermoElasticFitter()  → ThermoElasticFitterViewModel
          ├─ ShowVerificationDashboard() → VerificationDashboardViewModel
          ├─ ShowPhaseDiagram()         → PhaseDiagramExplorerViewModel
          ├─ ShowPostPerovskite()       → PostPerovskiteViewModel
          ├─ ShowClassicalGeobarometry() → ClassicalGeobarometryViewModel
          ├─ ShowGeobarometry()         → GeobarometryViewModel
          ├─ ShowSensitivityKernel()    → SensitivityKernelViewModel
          ├─ ShowAnelasticity()         → AnelasticityViewModel
          ├─ **ShowQProfile()**         → **QProfileViewModel** ← NEW
          ├─ ShowLLSVP()                → LLSVPViewModel
          ├─ ShowULVZ()                 → ULVZViewModel
          ├─ ShowSlabModel()            → SlabModelViewModel
          ├─ ShowPlanetaryInterior()    → PlanetaryInteriorViewModel
          ├─ ShowSpinCrossover()        → SpinCrossoverViewModel
          ├─ ShowThermalConductivity()  → ThermalConductivityViewModel
          ├─ ShowElectricalConductivity() → ElectricalConductivityViewModel
          ├─ ShowElasticTensor()        → ElasticTensorViewModel
          ├─ ShowOxygenFugacity()       → OxygenFugacityViewModel
          ├─ ShowCompositionInverter()  → CompositionInverterViewModel
          ├─ ShowIronPartitioning()     → IronPartitioningViewModel
          ├─ ShowWaterContent()         → WaterContentViewModel
          ├─ ShowMagmaOcean()           → MagmaOceanViewModel
          ├─ ShowMLData()               → MLDataViewModel
          ├─ ShowBayesianInversion()    → BayesianInversionViewModel
          └─ ShowLookupTable()          → LookupTableViewModel
```

**Navigation Pattern:**
1. User clicks button in MainWindow.axaml
2. MainWindowViewModel.ShowXyzCommand executes
3. `CurrentView = _xyzViewModel` (sets property)
4. View's DataTemplate for XyzViewModel is applied
5. UI displays XyzView with data bindings

## 34 Views & ViewModels (Organized by Category)

### Category 1: Core Mineralogy (3 Views)

#### 1.1 Mineral Editor
**File:** `Views/MineralEditorView.axaml` | `ViewModels/MineralEditorViewModel.cs`

**Purpose:** Create/edit single mineral EOS parameters (MineralParams)

**Controls:**
- TextBox: MineralName, PaperName, NumAtoms, MolarVolume, MolarWeight
- TextBox: KZero, K1Prime, K2Prime (bulk modulus parameters)
- TextBox: GZero, G1Prime, G2Prime (shear modulus parameters)
- TextBox: DebyeTempZero, GammaZero, QZero, EhtaZero (thermal)
- TextBox: Tc0, VD, SD (Landau parameters)
- TextBox: SpinQuantumNumber, MagneticAtomCount (magnetic)
- Button: Load JSON, Save JSON, Calculate Test, Import CSV

**ViewModel Commands:**
- `LoadMineralCommand` → File dialog + JSON deserialize
- `SaveMineralCommand` → File dialog + JSON serialize
- `CalculateTestCommand` → MieGruneisenEOSOptimizer(mineral, P_test, T_test)
- `ImportCsvCommand` → Bulk CSV import

**Bindings:**
- `SelectedMineral` ↔ TextBox inputs
- `TestResult` ↔ DataGrid (calculated properties at test P-T)
- `IsLoading` ↔ Button.IsEnabled

#### 1.2 Mineral Database
**File:** `Views/MineralDatabaseView.axaml` | `ViewModels/MineralDatabaseViewModel.cs`

**Purpose:** Browse SLB2011 46 endmembers + solid solutions

**Controls:**
- ComboBox: Select endmember or solid solution
- DataGrid: Display 46 SLB2011 minerals with all parameters
- TextBox: Search/filter by name
- Button: Copy to clipboard, Export CSV

**ViewModel Properties:**
- `EndmemberList: ObservableCollection<MineralParams>` (46 items from SLB2011Endmembers.cs)
- `SolutionList: ObservableCollection<SolidSolution>` (olivine, cpx, opx, spinel, garnet)
- `SelectedMineral` (property binding)

**Database Access:**
```csharp
var all46 = MineralDatabase.GetAllEndmembers();
var olivine = MineralDatabase.GetSolidSolution("Olivine");
```

#### 1.3 P-T Profile Calculator
**File:** `Views/PTProfileView.axaml` | `ViewModels/PTProfileViewModel.cs`

**Purpose:** Batch calculate mineral properties along P-T profile

**Controls:**
- ComboBox: Select mineral (loaded from file)
- DataGrid: P-T profile input (add/remove rows)
- Button: Load PTProfile JSON, Add Row, Remove Row, Calculate, Export
- ProgressBar: Calculation progress
- ResultsDataGrid: Display 18-column results (P, T, V, K, G, Vp, Vs, ρ, α, Cp, S, H, G, etc.)

**ViewModel Logic:**
```csharp
[RelayCommand]
private async void Calculate()
{
    var calculator = new PTProfileCalculator(SelectedMineral, LoadedProfile);
    ResultsList = await Task.Run(() => calculator.DoProfileCalculationAsSummary());
    CsvOutput = calculator.DoProfileCalculationAsCSV();
}
```

**Output Formats:**
- DataGrid display (ResultsList: List<ResultSummary>)
- CSV export (ResultSummary.ExportSummaryAsColumn())

---

### Category 2: Mixture & Rock Calculations (2 Views)

#### 2.1 Mixture Calculator
**File:** `Views/MixtureView.axaml` | `ViewModels/MixtureViewModel.cs`

**Purpose:** Calculate 2-mineral mixing (Voigt/Reuss/Hill/HS bounds)

**Controls:**
- ComboBox: Mineral1, Mineral2
- Slider: Volume fraction (0-100%, linked to TextBox)
- ComboBox: Mixing model (Voigt, Reuss, Hill, HS)
- TextBox: Pressure (GPa), Temperature (K)
- Button: Calculate, Save Result
- ResultsDataGrid: Aggregate properties (K_mix, G_mix, Vp_mix, Vs_mix, ρ_mix)

**ViewModel:**
```csharp
[ObservableProperty] private MineralParams _mineral1;
[ObservableProperty] private MineralParams _mineral2;
[ObservableProperty] private double _fraction1 = 0.5;
[ObservableProperty] private string _mixingModel = "Hill";
[RelayCommand] private void Calculate()
{
    var calc = new MixtureCalculator();
    var result = calc.CalculateMixture(Mineral1, Mineral2, Fraction1, MixingModel, Pressure, Temperature);
    Results = result;
}
```

#### 2.2 Rock Calculator
**File:** `Views/RockCalculatorView.axaml` | `ViewModels/RockCalculatorViewModel.cs`

**Purpose:** Multi-mineral rock composition (arbitrary n minerals)

**Controls:**
- DataGrid: Mineral + Fraction (add/remove rows dynamically)
- ComboBox: Predefined rocks (Pyrolite, Harzburgite, MORB, Piclogite)
- TextBox: P (GPa), T (K)
- ComboBox: Mixing model
- Button: Calculate, Save as .rock, Load .rock, Clear, Add Mineral
- ResultsDataGrid: Rock properties

**ViewModel:**
```csharp
[ObservableProperty] private ObservableCollection<RockComponent> _minerals;
[RelayCommand] private void LoadPredefinedRock(string rockName)
{
    var rock = MineralDatabase.GetPredefinedRock(rockName);
    Minerals = new(rock.Minerals.Select((m, i) => new RockComponent(m, rock.Fractions[i])));
}
[RelayCommand] private void Calculate()
{
    var calc = new RockCalculator();
    var result = calc.CalculateRockProperties(new RockComposition(Minerals, Pressure, Temperature, MixingModel));
}
```

**File Format (.rock):**
```json
{
  "Name": "Pyrolite",
  "MixingModel": "Hill",
  "Minerals": [
    { "Name": "Ol90", "Fraction": 0.57 },
    { "Name": "Opx", "Fraction": 0.21 },
    { "Name": "Cpx", "Fraction": 0.16 },
    { "Name": "Gt", "Fraction": 0.06 }
  ]
}
```

---

### Category 3: Phase Equilibria (4 Views)

#### 3.1 Phase Diagram Explorer
**File:** `Views/PhaseDiagramExplorerView.axaml` | `ViewModels/PhaseDiagramExplorerViewModel.cs`

**Purpose:** Interactive phase boundary mapping and phase diagram visualization

**Controls:**
- Canvas: Interactive 2D P-T plot (mouse hover → query phase)
- ComboBox: Transition pair (e.g., "Olivine ↔ Wadsleyite")
- TextBox: P_min, P_max, T_min, T_max (region bounds)
- Slider: Resolution (number of P/T steps)
- Button: Calculate Phase Boundary, Zoom Reset, Export SVG

**ViewModel:**
```csharp
[ObservableProperty] private List<(double P, double T)> _phaseBoundary;
[RelayCommand] private async void CalculateBoundary()
{
    var calc = new PhaseDiagramCalculator();
    PhaseBoundary = await Task.Run(() => calc.CalcPhaseBoundary(
        Phase1, Phase2, P_Min, P_Max, T_Min, T_Max, Resolution));
    PlotDiagram();
}
```

#### 3.2 Verification Dashboard
**File:** `Views/VerificationDashboardView.axaml` | `ViewModels/VerificationDashboardViewModel.cs`

**Purpose:** Cross-validate calculations vs. BurnMan + literature data

**Controls:**
- ComboBox: Select mineral to verify
- Button: Run Verification Suite
- DataGrid: Verification results (Property, Calc, BurnMan, Δ%, Status)
- TextBox: Tolerance threshold (%)

**ViewModel:**
```csharp
[RelayCommand] private async void RunVerification()
{
    var verifier = new ThermodynamicVerifier();
    var results = await Task.Run(() => verifier.Verify(SelectedMineral, TestP, TestT));
    VerificationResults = results; // VerificationResult[]
}
```

#### 3.3-3.4 Classical Geobarometry & Geobarometry
**Files:** `Views/{ClassicalGeobarometry,Geobarometry}View.axaml`

**Purpose:** Mineral equilibrium barometry and thermometry

**Controls:**
- TextBox: Phase compositions (garnet, clinopyroxene, orthopyroxene)
- TextBox: Reference temperature (K)
- Button: Calculate Pressure, Calculate Temperature
- ResultsDataGrid: P (GPa), T (K), Uncertainty (±)

---

### Category 4: EOS & Shock Compression (4 Views)

#### 4.1 Hugoniot (Shock Curve)
**File:** `Views/HugoniotView.axaml` | `ViewModels/HugoniotViewModel.cs`

**Purpose:** Calculate shock compression Hugoniot curve

**Controls:**
- ComboBox: Select mineral
- TextBox: Initial P (GPa), V (cm³/mol), T (K)
- Slider/TextBox: Shock velocity (km/s) range
- Button: Calculate Hugoniot
- Canvas: Plot P-V Hugoniot curve
- DataGrid: Hugoniot points (P, V, T, sound_speed, particle_velocity)

**ViewModel:**
```csharp
[RelayCommand] private async void CalculateHugoniot()
{
    var calc = new HugoniotCalculator(SelectedMineral, V0, P0, T0);
    HugoniotCurve = await Task.Run(() =>
        calc.CalcHugoniot(ShockVelocityMin, ShockVelocityMax, NumPoints));
    PlotCurve();
}
```

#### 4.2 EOS Fitter
**File:** `Views/EOSFitterView.axaml` | `ViewModels/EOSFitterViewModel.cs`

**Purpose:** Fit EOS parameters (K₀, K₀', K₀'') to P-V data

**Controls:**
- Button: Load CSV (V, P columns)
- DataGrid: Data points (V, P, residual)
- Button: Fit EOS, Clear Data
- TextBox: Fitted K₀, K₀', K₀'' (read-only output)
- Plot: Fitted curve vs. data points

#### 4.3 ThermoElastic Fitter
**File:** `Views/ThermoElasticFitterView.axaml` | `ViewModels/ThermoElasticFitterViewModel.cs`

**Purpose:** Fit combined thermoelastic properties (K, G, α vs. P, T)

---

### Category 5: Mantle & Deep Earth (6 Views)

#### 5.1-5.2 LLSVP & ULVZ Calculators
**Purpose:** Model large low-shear-velocity provinces and ultra-low-velocity zones

**ViewModel Logic:**
```csharp
// LLSVP: density vs. seismic reduction
var llsvpCalc = new LLSVPCalculator();
var profile = llsvpCalc.CalcLLSVP(DepthRange, Geotherm);

// ULVZ: CMB composition, partial melt
var ulvzCalc = new ULVZCalculator();
var composition = ulvzCalc.CalcULVZ(P_CMB, T_CMB);
```

#### 5.3 Slab Model
**File:** `Views/SlabModelView.axaml` | `ViewModels/SlabModelViewModel.cs`

**Purpose:** Subducting slab cooling model

**Controls:**
- TextBox: Plate age (Myr), subduction rate (cm/yr)
- Slider: Depth range (0-700 km)
- Button: Calculate, Plot T(depth)

#### 5.4 Planetary Interior
**File:** `Views/PlanetaryInteriorView.axaml` | `ViewModels/PlanetaryInteriorViewModel.cs`

**Purpose:** Mass-radius relation for exoplanets/rocky bodies

**Controls:**
- TextBox: Planet mass (M_earth), radius (R_earth)
- ComboBox: Core composition, mantle composition
- Button: Calculate interior structure
- Canvas: Density profile vs. radius

#### 5.5 Post-Perovskite
**File:** `Views/PostPerovskiteView.axaml` | `ViewModels/PostPerovskiteViewModel.cs`

**Purpose:** pPv stability field (D phase) vs. depth

**ViewModel:**
```csharp
var calc = new PostPerovskiteCalculator();
var stability = calc.CalcPostPerovskite(Depth_km, Geotherm);
// Returns: pPv fraction, Clapeyron slope dP/dT
```

#### 5.6 Sensitivity Kernel
**File:** `Views/SensitivityKernelView.axaml` | `ViewModels/SensitivityKernelViewModel.cs`

**Purpose:** Seismic wave sensitivity dVp/dρ vs. depth

**Controls:**
- Button: Calculate kernel
- Canvas: Plot depth kernels for Vp, Vs
- TextBox: Ray parameter, frequency

---

### Category 6: Material Properties (5 Views)

#### 6.1 Thermal Conductivity
**File:** `Views/ThermalConductivityView.axaml` | `ViewModels/ThermalConductivityViewModel.cs`

**Controls:**
- ComboBox: Select mineral
- Slider: Temperature (K), Pressure (GPa)
- Button: Calculate κ(P,T)
- Canvas: Plot thermal conductivity contour

#### 6.2 Electrical Conductivity
**File:** `Views/ElectricalConductivityView.axaml` | `ViewModels/ElectricalConductivityViewModel.cs`

**Controls:**
- ComboBox: Select mineral + redox buffer
- Slider: T, P, fO₂
- Button: Calculate σ(P,T,fO₂)

#### 6.3 Elastic Tensor
**File:** `Views/ElasticTensorView.axaml` | `ViewModels/ElasticTensorViewModel.cs`

**Purpose:** 6×6 elastic stiffness tensor (Voigt notation)

**Controls:**
- ComboBox: Crystal system (cubic, hexagonal, orthorhombic, triclinic)
- TextBox: K, G, anisotropy parameters
- DataGrid: 6×6 tensor display
- Button: Calculate, Export

#### 6.4 Anelasticity (Enhanced)
**File:** `Views/AnelasticityView.axaml` | `ViewModels/AnelasticityViewModel.cs`

**Purpose:** Seismic attenuation Q⁻¹(P, T, f) with multi-tier models, grain size, water & melt effects

**Controls (Enhanced):**
- ComboBox: **Model selection** (Tier 1: Simple | Tier 2: Parametric | Tier 3a: Extended Burgers | Tier 3b: Andrade)
- Slider: Temperature, Pressure, Frequency
- **Slider: Grain size [μm]** (NEW)
- **Slider: Water content [ppm H/Si]** (NEW)
- **Slider: Melt fraction [%]** (NEW)
- Button: Calculate Q⁻¹
- Canvas: Plot Q⁻¹ vs. frequency
- **Button: Show Q Profile** (NEW) → navigates to QProfileView

**ViewModel (Enhanced):**
```csharp
[ObservableProperty] private string _selectedModel = "Tier2_Parametric";
[ObservableProperty] private double _grainSize_um = 10.0;
[ObservableProperty] private double _waterContent_ppm = 0.0;
[ObservableProperty] private double _meltFraction = 0.0;

[RelayCommand] private async void Calculate()
{
    var prms = new AnelasticityParams
    {
        GrainSize_m = GrainSize_um * 1e-6,
        WaterContent_ppm = WaterContent_ppm,
        MeltFraction = MeltFraction
    };

    // Select calculator based on SelectedModel
    IAnelasticityModel calc = SelectedModel switch
    {
        "Tier2_Parametric" => new ParametricQCalculator(),
        "Tier3a_ExtendedBurgers" => new ExtendedBurgersCalculator(),
        "Tier3b_Andrade" => new AndradeCalculator(),
        _ => new AnelasticityCalculator()
    };

    Results = await Task.Run(() => calc.ApplyCorrection(
        Vp_ref, Vs_ref, K_ref, G_ref, Temperature, Pressure, Frequency, prms));
}
```

#### 6.5 Q Profile (NEW)
**File:** `Views/QProfileView.axaml` | `ViewModels/QProfileViewModel.cs`

**Purpose:** Build 1D depth-dependent Q profile along geothermal gradient with water/melt decorators

**Controls (NEW):**
- ComboBox: Geotherm profile (Adiabatic, Continental Shield, Mid-ocean ridge, Subduction zone)
- ComboBox: Anelasticity model (same as Anelasticity view)
- **Slider: Water content [ppm H/Si]** (applies to all depths)
- **Slider: Melt fraction [%]** (applies to all depths, depth-dependent optional)
- TextBox: Frequency [Hz]
- TextBox: Grain size [μm]
- Button: Build Profile, Export, Plot
- DataGrid: QProfilePoint[] (Depth, T, Q_inv, Model, GrainSize, WaterContent, MeltFraction)
- Canvas: Q⁻¹(depth) plot with shaded geotherm background

**ViewModel (NEW):**
```csharp
[ObservableProperty] private string _selectedGeotherm = "Adiabatic";
[ObservableProperty] private double _waterContent_ppm = 0.0;
[ObservableProperty] private double _meltFraction = 0.0;
[ObservableProperty] private double _frequency_Hz = 1.0;
[ObservableProperty] private ObservableCollection<QProfilePoint> _profilePoints = new();

[RelayCommand] private async void BuildProfile()
{
    var prms = new AnelasticityParams
    {
        WaterContent_ppm = WaterContent_ppm,
        MeltFraction = MeltFraction
    };

    var geotherm = LoadGeotherm(SelectedGeotherm);
    var builder = new QProfileBuilder();

    ProfilePoints = new(await Task.Run(() =>
        builder.BuildQProfile(geotherm, Frequency_Hz, prms)));
}
```

#### 6.6 Oxygen Fugacity
**File:** `Views/OxygenFugacityView.axaml` | `ViewModels/OxygenFugacityViewModel.cs`

**Purpose:** log(fO₂) from redox equilibrium

**Controls:**
- ComboBox: Buffer reaction (QFM, IW, MH)
- Slider: Temperature
- Button: Calculate fO₂

---

### Category 7: Composition & Fluids (4 Views)

#### 7.1 Iron Partitioning
**File:** `Views/IronPartitioningView.axaml` | `ViewModels/IronPartitioningViewModel.cs`

**Purpose:** Fe²⁺/Fe³⁺ distribution (KD) among phases

**ViewModel:**
```csharp
[RelayCommand] private void Calculate()
{
    var solver = new IronPartitioningSolver();
    var kd = solver.CalcPartitioning(Pressure, Temperature, Phases);
}
```

#### 7.2 Spin Crossover
**File:** `Views/SpinCrossoverView.axaml` | `ViewModels/SpinCrossoverViewModel.cs`

**Purpose:** High-spin ↔ Low-spin transition (Fe²⁺)

**Controls:**
- Slider: Pressure (GPa)
- Button: Calculate spin state
- Canvas: Plot spin fraction vs. P

#### 7.3 Water Content
**File:** `Views/WaterContentView.axaml` | `ViewModels/WaterContentViewModel.cs`

**Purpose:** H₂O abundance estimation

#### 7.4 Magma Ocean
**File:** `Views/MagmaOceanView.axaml` | `ViewModels/MagmaOceanViewModel.cs`

**Purpose:** Early Earth magma ocean crystallization

**Controls:**
- Slider: Adiabat temperature, mantle depth
- Button: Calculate melt fraction
- Canvas: Melt fraction vs. depth

---

### Category 8: Inversion & ML (4 Views)

#### 8.1 Composition Inverter
**File:** `Views/CompositionInverterView.axaml` | `ViewModels/CompositionInverterViewModel.cs`

**Purpose:** Invert seismic velocity → rock composition

**Controls:**
- TextBox: Vp (km/s), Vs (km/s)
- Slider: Depth (km)
- Button: Invert composition
- DataGrid: Estimated mineral fractions + uncertainty

#### 8.2 Bayesian Inversion
**File:** `Views/BayesianInversionView.axaml` | `ViewModels/BayesianInversionViewModel.cs`

**Purpose:** Full Bayesian inverse problem with uncertainty

**Controls:**
- Button: Load data, Set priors, Run MCMC
- ProgressBar: MCMC chains progress
- Canvas: Posterior distribution plot

#### 8.3 ML Data Generator
**File:** `Views/MLDataView.axaml` | `ViewModels/MLDataViewModel.cs`

**Purpose:** Generate synthetic training data

**Controls:**
- TextBox: n_samples, P_range, T_range
- Button: Generate dataset
- Button: Export to CSV

**ViewModel:**
```csharp
[RelayCommand] private async void GenerateData()
{
    var gen = new TrainingDataGenerator();
    TrainingData = await Task.Run(() =>
        gen.GenerateDataset(NumSamples, P_Min, P_Max, T_Min, T_Max, Composition));
}
```

#### 8.4 Lookup Table
**File:** `Views/LookupTableView.axaml` | `ViewModels/LookupTableViewModel.cs`

**Purpose:** Pre-compute properties grid for fast evaluation

**Controls:**
- TextBox: P/T resolution, property to tabulate
- Button: Generate table, Save, Load
- ProgressBar: Generation progress
- Button: Export to binary format

---

### Category 9: Utility Views (3 Views)

#### 9.1 Results
**File:** `Views/ResultsView.axaml` | `ViewModels/ResultsViewModel.cs`

**Purpose:** Display + export calculation results

**Controls:**
- DataGrid: Results table (18-column ResultSummary)
- Button: Export CSV, Export JSON, Clear
- ComboBox: Sort column
- TextBox: Filter rows

#### 9.2 Chart
**File:** `Views/ChartView.axaml` | `ViewModels/ChartViewModel.cs`

**Purpose:** Visualize results (Vp/Vs vs. P-T, density contours, etc.)

**Controls:**
- ComboBox: X-axis, Y-axis, Color (property selection)
- Canvas: OxyPlot chart
- Legend: Interactive

#### 9.3 Main Window
**File:** `Views/MainWindow.axaml` | `ViewModels/MainWindowViewModel.cs`

**Purpose:** Navigation hub and application shell

**Controls:**
- Menu/Ribbon: Category buttons (Core Tools, EOS & Shock, Phase, etc.)
- ContentControl: `{Binding CurrentView}` (shows active ViewModel)
- Status bar: Version, status messages

**ViewModel (Main):**
```csharp
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private object? _currentView;
    [ObservableProperty] private string _title = "ThermoElasticCalculator v1.0.0";

    // 33 ViewModel instances (lazy-loaded)
    private readonly MineralEditorViewModel _mineralEditorViewModel = new();
    private readonly PTProfileViewModel _ptProfileViewModel = new();
    // ... 31 more

    // 33 RelayCommands
    [RelayCommand] private void ShowMineralEditor() => CurrentView = _mineralEditorViewModel;
    [RelayCommand] private void ShowPTProfile() => CurrentView = _ptProfileViewModel;
    // ... 31 more
}
```

---

## Input Validation Rules

All TextBox inputs follow these patterns:

| Control | Validation | Example |
|---------|-----------|---------|
| Temperature (K) | Range [100, 10000] | Default: 1600 K |
| Pressure (GPa) | Range [0.001, 360] | Default: 1 GPa |
| Volume Fraction | Range [0, 1.0] | Auto-scale: sum → 1.0 |
| Mineral Count | Range [2, 20] | Rock with 2-20 minerals |
| Debye Temperature | Range [100, 1500] | Material-dependent |
| Elastic Moduli | Range [10, 500] GPa | Reasonable bounds |

## File I/O Integration

**ViewModel Pattern:**
```csharp
[RelayCommand]
private async void LoadFile()
{
    var dialog = new OpenFileDialog { Filters = new[] { new FileDialogFilter { Name = "Minerals", Extensions = { "mine" } } } };
    var result = await dialog.ShowAsync(Window);
    if (result?.Length > 0)
    {
        SelectedMineral = MineralParams.LoadFromJson(result[0]);
    }
}

[RelayCommand]
private async void SaveFile()
{
    var dialog = new SaveFileDialog { Filters = new[] { new FileDialogFilter { Name = "Minerals", Extensions = { "mine" } } } };
    var result = await dialog.ShowAsync(Window);
    if (!string.IsNullOrEmpty(result))
    {
        SelectedMineral.SaveToJson(result);
    }
}
```

## Threading Model

**Long-running calculations (async):**
```csharp
[RelayCommand]
private async void CalculateAsync()
{
    IsCalculating = true;
    ErrorMessage = null;

    try
    {
        Results = await Task.Run(() =>
        {
            var calc = new HugoniotCalculator(...);
            return calc.CalcHugoniot(...);
        });
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Calculation failed: {ex.Message}";
    }
    finally
    {
        IsCalculating = false;
    }
}
```

---

**Next:** See [dependencies.md](./dependencies.md) for external packages and [testing.md](./testing.md) for test structure.
