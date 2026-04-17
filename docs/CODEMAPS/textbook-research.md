<!-- Generated: 2026-03-24 | Textbook: 31 chapters + 20k lines | Research: 31+12 JSON topic files -->

# Textbook & Research Materials

**Last Updated:** 2026-03-24
**Scope:** Graduate-level educational content + research data foundations

## Overview

The ThermoElasticCalculator project includes a comprehensive 31-chapter graduate textbook that systematically covers all project features, from foundational thermodynamics to advanced computational methods. Accompanying research data in JSON format provides topic outlines and chapter-level reference materials.

## Textbook Structure

**Location:** `textbook/`

| File | Size | Purpose |
|------|------|---------|
| `README.md` | ~6 KB | Table of contents, 8-part organization, learning objectives |
| `chapters/ch01_thermodynamics_high_pressure.md` | ~1 KB × 31 | Individual chapter files (ch01–ch31) |

**Total:** 31 chapters, ~20,300 lines, ~1.5 MB

### 8-Part Curriculum Organization

| Part | Chapters | Topics | Key Equations |
|------|----------|--------|----------------|
| **I. Foundations** | 1-4 | BM3 EOS, Debye model, Gruneisen parameter, elastic moduli | Birch-Murnaghan 3rd-order, Debye thermal energy, seismic velocities (Vp, Vs) |
| **II. SLB2011 Framework** | 5-9 | Complete SLB2011 formulation, EOS solver, mineral DB, Landau transitions, magnetic contributions | Stixrude-Lithgow-Bertelloni parametrization, Mie-Gruneisen, phase diagrams |
| **III. Mixtures** | 10-11 | Voigt-Reuss-Hill bounds, HS mixing, rock compositions | Elasticity mixing (VRH, HS), multi-mineral aggregates |
| **IV. Phase Equilibria** | 12-14 | Gibbs free energy minimization, phase diagrams, post-perovskite stability | Chemical potential, phase equilibrium conditions |
| **V. Seismic Interpretation** | 15-18 | PREM, anelasticity/Q factor, sensitivity kernels, composition inversion | Seismic wave sensitivity, Q⁻¹ attenuation, kernel derivatives |
| **VI. Deep Earth** | 19-21 | Slab models, LLSVP/ULVZ, planetary interiors | Cooling models, lateral heterogeneity, mass-radius relations |
| **VII. Material Properties** | 22-26 | Spin crossover, Fe partitioning, water content, thermal/electrical conductivity, elastic anisotropy | Spin-pairing energy, KD partitioning, transport properties |
| **VIII. Advanced Methods** | 27-31 | Shock compression (Hugoniot), Levenberg-Marquardt fitting, MCMC Bayesian inversion, geobarometry, oxygen fugacity | EOS fitting, parameter estimation, fO₂ buffers |

### Chapter Reference

**Mapping to Project Features:**

| Feature | Chapter(s) | Implementation |
|---------|-----------|-----------------|
| Birch-Murnaghan 3 EOS | Ch01–04 | `MieGruneisenEOSOptimizer` (Phase 1) |
| Debye Model | Ch02 | `DebyeFunctionCalculator` (Phase 2) |
| SLB2011 Parameters | Ch05–07 | `SLB2011Database`, 46 endmembers |
| Phase Transitions | Ch08 | `LandauCalculator` (Phase 3) |
| Mixtures | Ch10–11 | `MixtureCalculator` (Phase 6) |
| Phase Diagrams | Ch12–14 | `GibbsMinimizerCalculator` (Phase 7) |
| Anelasticity | Ch15 | `AnelasticityCalculator`, 4-tier framework |
| Hugoniot | Ch27 | `HugoniotCalculator` (Phase 8) |
| Geobarometry | Ch30 | `GeobarometryCalculator` (Phase 9) |
| Bayesian Inversion | Ch29 | `MCMCInversionCalculator` (advanced methods) |

## Research Data Materials

**Location:** `research/`

### Textbook Research Data

**Path:** `research/textbook/`

| File | Format | Purpose |
|------|--------|---------|
| `TEXTBOOK_PLAN.md` | Markdown | Project-level outline and chapter responsibilities |
| `outline.yaml` | YAML | Structured 31-chapter outline with section breakdowns |
| `fields.yaml` | YAML | Physics terminology mappings (Japanese↔English, symbols) |
| `results/` | Directory | Chapter-level research summaries (31 JSON files, one per chapter) |

**JSON Structure:** Each `chNN_summary.json` contains:
- Chapter title (JP + EN)
- Learning objectives
- Key equations with LaTeX notation
- Code reference mappings
- Cross-chapter dependencies
- Recommended reading order notes

**Total:** 31 JSON summaries, ~50 KB

### Anelasticity Research Data

**Path:** `research/anelasticity/`

| File | Format | Purpose |
|------|--------|---------|
| `IMPLEMENTATION_PLAN.md` | Markdown | 4-tier anelasticity framework design |
| `outline.yaml` | YAML | Anelasticity model hierarchy (Simple→Parametric→Extended→Andrade) |
| `fields.yaml` | YAML | Q-factor terminology and unit conversions |
| `results/` | Directory | Topic research summaries (12 JSON files) |

**JSON Topics (12 files):**
1. Q-factor-definition.json
2. Frequency-dependence-models.json
3. Temperature-dependence.json
4. Pressure-dependence.json
5. Composition-effects.json
6. Burgers-model.json
7. Andrade-creep.json
8. Jackson-Faul-data.json
9. Seismic-attenuation-frequency-ranges.json
10. Experimental-measurements.json
11. Mantle-Q-profiles.json
12. Calibration-uncertainty.json

**Total:** 12 JSON topic files, ~100 KB

## Cross-References

### From Code to Textbook

Example: `MieGruneisenEOSOptimizer.cs` (Phase 1 calculator)
- **Textbook:** Chapter 1 (High pressure thermodynamics) + Chapter 5 (SLB2011 formulation)
- **Research:** `research/textbook/results/ch01_summary.json`, `ch05_summary.json`
- **Equations:** BM3 finite strain, Mie-Gruneisen thermal correction

### From Textbook to Views/ViewModels

Example: Chapter 27 (Hugoniot shock compression)
- **UI:** `HugoniotView.axaml` + `HugoniotViewModel.cs`
- **Calculator:** `HugoniotCalculator` (Phase 8)
- **Tests:** `HugoniotCalculatorTests.cs`

### From Research to Implementation

Example: Anelasticity framework
- **Research:** `research/anelasticity/IMPLEMENTATION_PLAN.md` + JSON models
- **Code:** `AnelasticityCalculator` (9 classes), `AnelasticityDatabase`
- **Tests:** `AnelasticityTests.cs` (37 methods)
- **UI:** `AnelasticityView.axaml` + `AnelasticityViewModel.cs`
- **Textbook:** Chapter 15 (Anelasticity and Q)

## Usage Guidelines

### For Students/Learners
1. Start with **Textbook README.md** for chapter overview
2. Read chapters in 8-part order (Part I foundations → Part VIII advanced)
3. Reference **research/textbook/results/** for key equations and code mappings
4. Run `ThermoElasticCalculator` UI to interactively validate chapter examples

### For Researchers
1. Check **research/** JSON files for topic-specific literature outlines
2. Cross-reference **research/anelasticity/** for advanced implementation details
3. Review chapter research summaries for unresolved questions
4. Access code calculators from textbook Chapter→Feature mapping table above

### For Developers
1. Update **textbook/chapters/** when adding major features (Phase N calculator)
2. Add research JSON files when significant refactoring occurs
3. Keep chapter content synchronized with implementation via mapping table
4. Update file paths in codemaps (INDEX.md, architecture.md) when adding chapters/sections

## File Statistics

| Category | Count | Location |
|----------|-------|----------|
| **Textbook** | | |
| Chapters | 31 | `textbook/chapters/` |
| Lines of content | ~20,300 | `textbook/chapters/*.md` |
| Markdown size | ~1.5 MB | Total |
| **Research — Textbook** | | |
| JSON summaries | 31 | `research/textbook/results/` |
| YAML outlines | 2 | `research/textbook/` |
| Markdown docs | 1 | `research/textbook/TEXTBOOK_PLAN.md` |
| **Research — Anelasticity** | | |
| JSON topic files | 12 | `research/anelasticity/results/` |
| YAML outlines | 2 | `research/anelasticity/` |
| Markdown docs | 1 | `research/anelasticity/IMPLEMENTATION_PLAN.md` |

## Freshness & Sync

**Last Synchronized:** 2026-03-24
- Textbook content complete (31/31 chapters)
- Research data complete (31+12 JSON files)
- Cross-references verified with code calculators

**To Update:**
1. Modify textbook chapter files → Update corresponding `research/textbook/results/chNN_summary.json`
2. Add calculator code → Add textbook chapter + research JSON
3. Change Feature→Chapter mapping → Update table in this document + INDEX.md

---

**See also:** [architecture.md](./architecture.md) for code structure, [core-engine.md](./core-engine.md) for calculator details, [dependencies.md](./dependencies.md) for external references.
