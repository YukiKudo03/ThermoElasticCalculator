<!-- Generated: 2026-03-19 | Files scanned: 5 | Token estimate: ~400 -->
# Dependencies

## NuGet Packages

### ThermoElastic.Core
| Package | Version | Purpose |
|---------|---------|---------|
| MathNet.Numerics | 5.0.0 | Linear algebra (SVD for Gibbs minimizer) |

### ThermoElastic.Desktop
| Package | Version | Purpose |
|---------|---------|---------|
| Avalonia | 11.2.3 | Cross-platform XAML UI |
| Avalonia.Desktop | 11.2.3 | Desktop platform host |
| Avalonia.Controls.DataGrid | 11.2.3 | Data grid control |
| Avalonia.Themes.Fluent | 11.2.3 | Windows Fluent theme |
| Avalonia.Fonts.Inter | 11.2.3 | Inter font family |
| CommunityToolkit.Mvvm | 8.4.0 | MVVM source generators |

### Tests
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.NET.Test.Sdk | 17.8.0 | Test framework host |
| xunit | 2.9.0 | Test framework |
| xunit.runner.visualstudio | 2.8.2 | VS test runner adapter |

## External Validation

- **BurnMan** (Python, pip) — SLB2011 reference implementation for cross-validation
- **PREM** (Dziewonski & Anderson, 1981) — built-in reference Earth model
- Reference CSV files in `tests/ThermoElastic.Core.Tests/TestData/`

## CI/CD

- GitHub Actions: `.github/workflows/ci.yml`
- Matrix: ubuntu-latest, windows-latest, macos-latest
- Pipeline: restore → build (Release) → test → **publish (self-contained)** → **upload artifacts**
- Artifacts: win-x64, linux-x64, osx-x64 (30-day retention)
