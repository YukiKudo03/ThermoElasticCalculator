# Sample Data

Demo datasets for the **SLB Parameter Fitter** (v1.1.0+).

## Files

| File | Mineral | Type | Points |
|------|---------|------|--------|
| `bridgmanite_synthetic.csv` | Mg-Perovskite (mpv) | Synthetic (SLB2011 forward model + Gaussian noise) | 15 |

## CSV Format

```
P(GPa), T(K), Vp(m/s), Vs(m/s), Density(g/cm3), SigmaVp, SigmaVs, SigmaDensity
```

- Lines starting with `#` are treated as comments.
- The first non-comment line is the header.
- Missing values: empty field or `NaN`.
- Delimiter: comma, tab, or semicolon (auto-detected).

## Synthetic Data Characterization

`bridgmanite_synthetic.csv` is generated from the SLB2011 database parameters for Mg-perovskite (K0=250.53 GPa, G0=172.90 GPa) at lower-mantle P-T conditions:

- **Pressures:** 25, 50, 75, 100, 125 GPa (670 km depth to CMB)
- **Temperatures:** 2000, 2500, 3000 K (cold slab to hot thermal boundary layer)
- **Noise:** ±0.5% (Vp, Vs), ±1.0% (density) — typical Brillouin scattering precision
- **Seed:** 42 (deterministic, reproducible)

This dataset supports **round-trip validation**: running the SLB Fitter on this CSV should recover the known K0 and G0 values within uncertainty.

## Regenerating Sample Data

```bash
dotnet run --project tools/SampleDataGenerator
```

Edit `tools/SampleDataGenerator/Program.cs` to change the mineral, P-T grid, or noise levels.
