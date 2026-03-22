<!-- Generated: 2026-03-23 | v1.0.0 | .NET 9, 8 direct NuGet packages -->

# Dependencies

## .NET Framework

| Framework | Version | Projects |
|-----------|---------|----------|
| .NET | 9.0 | Core, Desktop, All tests |

## NuGet Packages

### ThermoElastic.Core

| Package | Version | Purpose | Used For |
|---------|---------|---------|----------|
| MathNet.Numerics | 5.0.0 | Scientific computing: SVD, linear algebra, matrix ops | Gibbs minimization (SVD factorization), MCMC, inversion |

### ThermoElastic.Desktop

| Package | Version | Purpose | Used For |
|---------|---------|---------|----------|
| Avalonia | 11.2.3 | Cross-platform XAML UI framework | Application shell, layout engine |
| Avalonia.Desktop | 11.2.3 | Desktop platform integration (Windows/macOS/Linux) | Native windowing, rendering |
| Avalonia.Controls.DataGrid | 11.2.3 | Tabular data display control | Results tables, mineral database browse |
| Avalonia.Themes.Fluent | 11.2.3 | Windows Fluent Design System theme | Modern UI styling |
| Avalonia.Fonts.Inter | 11.2.3 | Inter font family system | Typography |
| CommunityToolkit.Mvvm | 8.4.0 | MVVM source generators + templates | ViewModels: ObservableObject, RelayCommand generation |

### Test Projects

| Package | Version | Purpose | Used For |
|---------|---------|---------|----------|
| Microsoft.NET.Test.Sdk | 17.8.0 | Test framework infrastructure | Test discovery, execution, reporting |
| xunit | 2.9.0 | Unit test framework | Test assertions, [Fact], [Theory] attributes |
| xunit.runner.visualstudio | 2.8.2 | Visual Studio test runner adapter | IDE integration (Test Explorer) |

## External Data & Validation

### Thermodynamic Reference Data

| Source | Type | Usage | Integration |
|--------|------|-------|-------------|
| **SLB2011** (Stixrude & Lithgow-Bertelloni, 2005) | EOS parameters (46 endmembers) | Mineral database, default calculations | Built-in: `SLB2011Endmembers.cs` |
| **PREM** (Dziewonski & Anderson, 1981) | 1-D Earth reference model (0-2891 km) | Depth↔pressure conversion | Built-in: `PREMModel.cs` |
| **BurnMan** (Python/pip) | Independent EOS implementation | Cross-validation, literature verification | Test data CSV files in `tests/ThermoElastic.Core.Tests/TestData/` |

### Test Reference Data

| File | Source | Purpose |
|------|--------|---------|
| `TestData/*.csv` | BurnMan output | Numerical cross-validation (decimated to key points) |
| `TestData/literature_*.csv` | Published papers | Literature value verification (SLB2011, Fei, Saxena, etc.) |

## Dependency Graph

```
ThermoElastic.sln
├── ThermoElastic.Core (Library)
│   └─ MathNet.Numerics 5.0.0
│
├── ThermoElastic.Desktop (App)
│   ├─ Avalonia 11.2.3
│   ├─ Avalonia.Desktop 11.2.3
│   ├─ Avalonia.Controls.DataGrid 11.2.3
│   ├─ Avalonia.Themes.Fluent 11.2.3
│   ├─ Avalonia.Fonts.Inter 11.2.3
│   ├─ CommunityToolkit.Mvvm 8.4.0
│   └─ [depends on] ThermoElastic.Core
│
├── ThermoElastic.Core.Tests (Test)
│   ├─ Microsoft.NET.Test.Sdk 17.8.0
│   ├─ xunit 2.9.0
│   ├─ xunit.runner.visualstudio 2.8.2
│   └─ [depends on] ThermoElastic.Core
│
└── ThermoElastic.Desktop.E2E (Test)
    ├─ xunit 2.9.0
    ├─ xunit.runner.visualstudio 2.8.2
    └─ [depends on] ThermoElastic.Desktop
```

## Transitive Dependencies

Avalonia 11.2.3 transitively brings:
- System.* NuGet packages (collection, memory, etc.) — .NET runtime built-in
- SkiaSharp (graphics rendering)
- HarfBuzz (text layout)

MathNet.Numerics 5.0.0 transitively brings:
- System libraries (no external)

## CI/CD Pipeline

### GitHub Actions Workflow

**File:** `.github/workflows/ci.yml`

| Stage | Command | Platforms |
|-------|---------|-----------|
| Restore | `dotnet restore` | ubuntu-latest, windows-latest, macos-latest |
| Build | `dotnet build -c Release` | All three |
| Test | `dotnet test --no-build -c Release` | All three |
| Publish | `dotnet publish -c Release` | All three (self-contained) |
| Upload | `upload-artifact@v3` | GitHub Artifacts (30-day retention) |

**Artifacts Published:**
- `win-x64/` (Windows standalone)
- `linux-x64/` (Linux standalone)
- `osx-x64/` (macOS standalone)

All builds are **self-contained** (bundled .NET runtime, no system .NET required).

## License & Compliance

| Package | License | Notes |
|---------|---------|-------|
| MathNet.Numerics | MIT | Permissive |
| Avalonia | MIT | Permissive |
| CommunityToolkit.Mvvm | MIT | Permissive |
| xunit | Apache 2.0 | Permissive |
| Microsoft.NET | MIT | Permissive |

All dependencies are compatible with MIT/Apache 2.0 licensing.

## Version Maintenance

As of v1.0.0:
- **Critical** updates tracked for security patches
- **Minor** updates considered for functionality/performance
- **Major** updates evaluated for breaking changes

**Last checked:** 2026-03-23

| Package | Current | Latest | Status |
|---------|---------|--------|--------|
| .NET | 9.0 | 9.0 | Latest |
| Avalonia | 11.2.3 | 11.2+ | Minor updates available |
| MathNet.Numerics | 5.0.0 | 5.1+ | Minor updates available |
| CommunityToolkit.Mvvm | 8.4.0 | 8.4+ | Current |
| xunit | 2.9.0 | 2.9+ | Current |

## Offline/Air-Gapped Deployment

For environments without internet access:
1. Download NuGet packages via `dotnet add package --offline`
2. Use `--packages ./packages` restore flag
3. Self-contained publishable apps require no external dependencies at runtime
