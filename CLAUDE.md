# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repo status: pre-build, spec-first

There is no source code in this repo yet — no `.csproj`, no `RecordBuilder/`, no WinUI project. The repo currently contains only spec documents (`README.md`, `SecretLead-PROJECT-BRIEF.md`, `Assets/DefaultSchema.md`) and `LICENSE`. There are no build, lint, or test commands to run because there is nothing to build yet. When code is added, update this section with the actual commands (e.g. `dotnet build`, `dotnet run`, `dotnet test`).

## Important: the spec has two documents that disagree, on purpose

- **`README.md`** describes the *original* spec: SecretLead as a Contractor-only console tool (`RecordBuilder`) that converts a government `.xlsx` license export into a `records.txt` file consumed by a separate app, SecretList.
- **`SecretLead-PROJECT-BRIEF.md`** (dated 2026-07-16, "major revision: generalized architecture") describes a **superseding** design: SecretLead is now a standalone WinUI 3 app — a generic, schema-driven, offline CRUD + CSV-import tool for *any* entity type, not just contractors. The original Contractor-only spec becomes the bundled default demo schema (`Assets/DefaultSchema.md`), not the whole product.

**Treat `SecretLead-PROJECT-BRIEF.md` as the current architecture** when the two conflict (e.g. "console tool only" vs. "WinUI 3 app with CRUD"; "no prefilled data" scope questions). `README.md` is not yet updated to match — per its own §10, code/README drift is supposed to be called out and fixed, not silently ignored. If you write code, prefer the brief's model, and flag to the user that `README.md` needs a rewrite to stop contradicting it.

## Architecture (per the current brief)

- **Folder-as-project model:** no in-app list of projects. Pointing the folder picker at a folder makes that folder's `schema.md` (if present) the active schema; otherwise the bundled default (`Assets/DefaultSchema.md`, entity `Contractor`) is used. The picked folder holds only `schema.md` and the source `.csv` — never the actual record store.
- **Storage:** the real <EntityName>.txt record store lives in AppData, keyed only by entity name, globally. The app does no tag-set inspection or reconciliation — existence-of-file is the entire mechanism. If two schemas declare the same entity name, they silently share the same file, whether that's intentional (same demo schema, different folders) or a mistake (two unrelated datasets that happen to collide). There is no runtime collision detection to build — avoiding accidental collisions is entirely a schema-naming discipline problem for the human author, not something to add validation for.
- **Schema grammar** (`schema.md` / `Assets/DefaultSchema.md`):
  - `#` — schema title/description/version/date
  - `##` — entity name, which is also the output filename (`## Contractor` → `Contractor.txt`)
  - `### TAG X` — a field, matched case-insensitively against CSV headers on import
  - The **first** tag is the "Category" role (top-level grouping; exploded into multiple records when semicolon-delimited on import). The **second** tag is the "Nickname" role (the searchable/browsable identifier). Tags beyond position 2 and CSV columns can be in any order.
- **Existence-of-file mechanic:** no `<EntityName>.txt` yet → prompt Import. File already exists → skip to browse/list mode (Import can still be re-run manually to append more records).
- **CRUD + Import both apply to whatever schema is active** — manual Add/Edit/Delete plus bulk CSV import, not import-only.
- **Field normalization is inferred from tag name substrings**, not a declared datatype: `date` → reformat to `mm/dd/yyyy`; `phone` → strip to digits then reformat by digit count; `email` → no transform. The "fall back to `Last Name, First Name` when Business Name is blank" rule is specific to the Contractor demo schema, not a generalized rule.
- **Import validation is a hard stop in both directions:** any schema tag with no matching CSV column, or any CSV column with no matching schema tag, blocks the import and must be named explicitly in the status box — never silently skipped.
- **Print layout:** A4 portrait, two records per page, duplex, folded into an A5 handout — driven structurally by whatever tags the active schema defines.
- **SecretLetter** is mentioned in the brief as a third, fully separate app/concept (not integrated with SecretLead or SecretList). It has no design yet — do not build it speculatively.

## Non-negotiable platform rules (apply to any code added to this repo)

- **No third-party NuGet packages, ever** — .NET BCL and Windows App SDK / COM only. E.g. `.xlsx` reading must go through `System.IO.Compression` + `System.Xml` directly, not ClosedXML/EPPlus/NPOI.
- **No network calls of any kind** — no `HttpClient`, `WebRequest`, scraping libraries (Playwright/Selenium), or scheduled polling. Data enters the system only via a human manually exporting/downloading a file and pointing the app at it on local disk.
- Offline-only: no cloud sync, no telemetry, no accounts.
- No MSIX sandbox workarounds — read-only packaged assets via `Package.Current.InstalledLocation`, no writes to the install folder, no elevation tricks.
- `ImplicitUsings` / nullable enabled; AI-disclaimer file header on every source file; no hardcoded colors/fonts (theme resources only) — these are inherited conventions from the sibling SecretList project.
- No AI-assisted data entry: a human reviews every record before it's trusted.
- Ship one confirmed working change before starting the next — avoid speculative multi-file rewrites.
- Never bundle real/prefilled contractor data (real business names, real license numbers) with the repo or an installer — sample data, if any, must be fictional placeholder data. This is an explicitly open/unresolved item in the brief (see its "Needs a decision" section) — don't resolve it unilaterally in code; flag it.

## Relationship to sibling projects (not in this repo)

- **SecretList**  is a separate repo/codebase (manual-entry-only, tagged-text storage). SecretLead does not fork or share code with it. Under the original spec they had a direct format contract (records.txt output feeding SecretList); the current brief drops that framing and describes only a shared approach (schema-driven, tagged-text storage) — not a binding compatibility requirement. Don't assume SecretLead's output format needs to match SecretList's file-for-file unless that gets restated explicitly somewhere.
- **SecretLetter** is a third, independent, unstarted app — do not conflate with SecretLead.
