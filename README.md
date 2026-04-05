# ThermoElasticCalculator

A cross-platform desktop application for computing thermoelastic properties of mantle minerals and multi-mineral assemblages (rocks) at high temperature and pressure conditions.

## Overview

Implements the Stixrude & Lithgow-Bertelloni (2011, 2024) thermoelastic framework:

- Elastic wave velocities (Vp, Vs), density, and elastic moduli at arbitrary P-T conditions
- **SLB Parameter Fitter**: fit SLB thermoelastic parameters to experimental Brillouin scattering data (Vp/Vs/density)
- SLB2011 mineral database (46 endmembers) + SLB2024 Fe3+ extensions (15 new species including native iron)
- Mixing models (Voigt / Reuss / Hill / Hashin-Shtrikman) for rock properties
- Gibbs free energy minimization for phase equilibria
- Fe2+ and Fe3+ spin crossover with explicit LS endmembers (wuls, hlbg)
- PREM reference model comparison
- MCMC Bayesian composition inversion with posterior distributions
- P-T profiles, isentropes, phase diagrams, and more

## Key Features

| Feature | Description |
|---------|-------------|
| **SLB Fitter** | Import experimental Vp/Vs/density data, fit 9 SLB parameters, compare with PREM |
| Mineral Library | Create, edit, and save mineral parameters (JSON / CSV) |
| Mineral Database | Built-in SLB2011 + SLB2024 database (61 endmembers + solid solution models) |
| P-T Profile | Compute properties along pressure-temperature paths |
| Mixture Calculator | Two-mineral mixing with Hashin-Shtrikman bounds |
| Rock Calculator | Multi-mineral rock properties with predefined compositions |
| Phase Equilibrium | Gibbs free energy minimization for stable phase assemblages |
| Composition Inverter | Estimate mantle composition (Mg#) from seismic velocities via MCMC |
| Spin Crossover | Fe2+ (wu/wuls) and Fe3+ (hebg/hlbg) spin transitions |

## v1.1.0 Highlights

- **SLB Parameter Fitter**: Import CSV data from Brillouin scattering experiments, fit SLB parameters with Levenberg-Marquardt optimization, and compare predicted mantle velocities with PREM
- **SLB2024 Fe3+ endmembers**: magnetite, hematite, Fe3+-bridgmanite (HS/LS), Fe3+-post-perovskite, native iron (alpha/gamma/epsilon), Cr-bearing phases
- **MCMC composition inversion**: Bayesian Mg# estimation with 95% credible intervals
- **Updated interaction parameters**: pe-wu W = 44.0 kJ/mol (SLB2024), new fp/bg/ppv interactions

## Requirements

- **Windows** 10 / 11
- **macOS** 12 (Monterey) or later (Intel and Apple Silicon)
- **Linux** (Ubuntu 22.04+, Fedora 38+)
- .NET 9.0 Runtime (or download self-contained binaries from [Releases](../../releases))

## Quick Start

```bash
# Clone
git clone <repository-url>
cd ThermoElasticCalculator

# Build and run
dotnet build
dotnet run --project src/ThermoElastic.Desktop

# Run tests
dotnet test
```

### macOS note

Downloaded binaries may be blocked by Gatekeeper. To allow execution:
```bash
xattr -cr ThermoElasticCalculator.app
```

## Project Structure

```
ThermoElasticCalculator/
├── src/
│   ├── ThermoElastic.Core/          # Computation engine (.NET 9 class library)
│   │   ├── Models/                  # Data models
│   │   ├── Calculations/           # EOS, fitting, inversion, spin crossover
│   │   └── Database/               # SLB2011 + SLB2024 mineral database
│   └── ThermoElastic.Desktop/      # Desktop UI (Avalonia, cross-platform)
│       ├── Views/                   # AXAML view definitions (34 views)
│       └── ViewModels/             # MVVM view models
└── tests/
    ├── ThermoElastic.Core.Tests/   # Unit tests (562 tests)
    └── ThermoElastic.Desktop.E2E/  # E2E tests (104 tests)
```

## References

- Stixrude, L. & Lithgow-Bertelloni, C. (2024). Thermodynamics of mantle minerals -- III: the role of iron. *Geophysical Journal International*, 237, 1699-1733.
- Stixrude, L. & Lithgow-Bertelloni, C. (2011). Thermodynamics of mantle minerals -- II. Phase equilibria. *Geophysical Journal International*, 184, 1180-1213.
- Stixrude, L. & Lithgow-Bertelloni, C. (2005). Thermodynamics of mantle minerals -- I. Physical properties. *Geophysical Journal International*, 162, 610-632.
- Cottaar, S., Heister, T., Rose, I. & Unterborn, C. (2014). BurnMan: A lower mantle mineral physics toolkit. *Geochemistry, Geophysics, Geosystems*, 15, 1164-1179.
- Dziewonski, A. M. & Anderson, D. L. (1981). Preliminary reference Earth model. *Physics of the Earth and Planetary Interiors*, 25, 297-356.

## License

MIT License. See [LICENSE](LICENSE) for details.

---

[日本語版 README](README_ja.md)
