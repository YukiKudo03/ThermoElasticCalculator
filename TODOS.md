# TODOS

## Post-v1.1.0

### CsvHelper library for flexible experimental data import
**What:** Replace hand-rolled CSV parsing in ExperimentalDataset with CsvHelper NuGet package.
**Why:** When multiple labs send data with different CSV column headers/formats, flexible column mapping becomes necessary. Hand-rolled parsing breaks on unexpected formats.
**Pros:** Handles edge cases (quoted commas, encoding, BOM), column mapping by header name, type conversion.
**Cons:** Adds a NuGet dependency (~200KB). Overkill for v1.1.0 where the format is controlled.
**Context:** v1.1.0 uses a simple CSV format (P,T,Vp,Vs,rho,sigma_Vp,sigma_Vs,sigma_rho). If Murakami or other labs use different column orders or names, CsvHelper's `ClassMap` feature handles this cleanly.
**Depends on:** ExperimentalDataset implementation (Phase 2 of v1.1.0 plan).

### Apple Developer ID signing + notarization for macOS binaries
**What:** Sign the `osx-x64` and `osx-arm64` release binaries with an Apple Developer ID certificate and submit them to Apple's notary service.
**Why:** Current v1.1.0 macOS binaries are ad-hoc signed (self-signed). When users download from GitHub via Safari, the quarantine attribute triggers Gatekeeper's "developer cannot be verified" dialog — a scary first impression for research-lab users (e.g., Murakami lab demo).
**Pros:** Clean first-run experience, no manual `xattr` workaround, looks professional.
**Cons:** Requires Apple Developer Program membership ($99/year). Adds 2-3 extra steps to release.yml (import cert to keychain, codesign, notarytool submit, staple). ~5 min of CI time per build.
**Workaround for v1.1.0:** Document `xattr -dr com.apple.quarantine <path>` in release notes, or provide USB transfer option for the Murakami demo.
**Context:** `codesign -dv` on v1.1.0 osx-arm64 binary reports `Signature=adhoc` and `Format=Mach-O thin (arm64)`. Only the `com.apple.provenance` xattr is set on tar-extracted files (no quarantine), so local extraction works fine — the issue appears only on direct browser downloads.
**Priority:** v1.2.0 or later. Not blocking for research-audience release.
