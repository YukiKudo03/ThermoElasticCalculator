# Sample Data

Demo datasets for the **SLB Parameter Fitter** (v1.1.0+).

## Files

| File | Mineral | Type | Points |
|------|---------|------|--------|
| `bridgmanite_synthetic.csv` | Mg-Perovskite (mpv) | Synthetic (SLB2011 forward model + Gaussian noise) | 15 |
| `murakami2007_mgsio3_pv.csv` | Mg-Perovskite | Model curve from Murakami et al. (2007) EPSL parameters | 13 |
| `murakami2012_bridgmanite.csv` | Mg-Perovskite | **Real** Brillouin Vs measurements, Murakami 2012 Nature | 20 |
| `murakami_periclase.csv` | MgO (periclase) | **Real** Brillouin Vs measurements, Murakami 2009 EPSL | 17 |

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

## Murakami 2007 Parameterization

`murakami2007_mgsio3_pv.csv` is a **model curve** generated from the EoS parameters reported in:

> Murakami, M., Sinogeikin, S.V., Hellwig, H., Bass, J.D., & Li, J. (2007).
> "Sound velocity of MgSiO3 perovskite to Mbar pressure."
> *Earth and Planetary Science Letters* **256**, 47–54.
> DOI: [10.1016/j.epsl.2007.01.011](https://doi.org/10.1016/j.epsl.2007.01.011)

**Parameter source:**
- G0 = 172.9 GPa, G' = 1.56 — Brillouin measurement reported in Murakami 2007
- K0 = 253 GPa, K' = 3.9 — Fiquet/Stixrude EoS adopted by the paper

**Important caveat:** In the original paper, Vp could not be measured directly because the Brillouin signal overlapped the diamond anvil. Vs was the primary measurement; Vp was derived from the adopted bulk modulus EoS. Vp values in this CSV reflect that same derivation.

Covers 0.001–96 GPa at 300 K (13 points). Pure MgSiO3 perovskite (no Al).

Ambient anchor values validate against paper:
- Vs(0 GPa) = 6489 m/s — paper reports 6490 ± 30 m/s ✓
- Vp(0 GPa) = 10879 m/s — paper reports 10850 ± 30 m/s ✓

## Real Brillouin Measurements

`murakami2012_bridgmanite.csv` and `murakami_periclase.csv` contain **actual measured Vs values** extracted from the BurnMan reference dataset (ExoPlex repository mirror), originally published by:

- **`murakami2012_bridgmanite.csv`** — Murakami, M., Ohishi, Y., Hirao, N., & Hirose, K. (2012). "A perovskitic lower mantle inferred from high-pressure, high-temperature sound velocity data." *Nature* **485**, 90–94. DOI: [10.1038/nature11004](https://doi.org/10.1038/nature11004). 20 measurements of Vs for MgSiO3 bridgmanite, 42–124 GPa at 300 K.
- **`murakami_periclase.csv`** — Likely Murakami, M. et al. (2009). "Elasticity of MgO to 130 GPa: Implications for lower mantle mineralogy." *EPSL* **277**, 123–129. 17 measurements of Vs for pure MgO, 34–121 GPa at 300 K.

**Important:** These CSVs contain **only Vs measurements** (Vp and density columns are blank). Use `FitTarget.VsOnly` in `FittingOptions` when running the fitter, or provide density from a compressible EoS as auxiliary input.

These datasets are intended for validating the SLB Parameter Fitter against published experimental results from the Murakami lab.

## Regenerating Sample Data

```bash
dotnet run --project tools/SampleDataGenerator
```

Edit `tools/SampleDataGenerator/Program.cs` to change the mineral, P-T grid, or noise levels.
