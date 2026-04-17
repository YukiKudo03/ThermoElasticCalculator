# TODOS

## Post-v1.1.0

### CsvHelper library for flexible experimental data import
**What:** Replace hand-rolled CSV parsing in ExperimentalDataset with CsvHelper NuGet package.
**Why:** When multiple labs send data with different CSV column headers/formats, flexible column mapping becomes necessary. Hand-rolled parsing breaks on unexpected formats.
**Pros:** Handles edge cases (quoted commas, encoding, BOM), column mapping by header name, type conversion.
**Cons:** Adds a NuGet dependency (~200KB). Overkill for v1.1.0 where the format is controlled.
**Context:** v1.1.0 uses a simple CSV format (P,T,Vp,Vs,rho,sigma_Vp,sigma_Vs,sigma_rho). If Murakami or other labs use different column orders or names, CsvHelper's `ClassMap` feature handles this cleanly.
**Depends on:** ExperimentalDataset implementation (Phase 2 of v1.1.0 plan).
