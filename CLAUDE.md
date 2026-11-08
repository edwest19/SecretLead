# CLAUDE.md

This file provides guidance to Claude Code (or any AI agent) working in this repository.

## The spec

**`README.md` is the single, authoritative spec.** Read it in full before writing code — it's written specifically so an AI can verify whether the code matches it (see its own §13, "Instructions for future AI sessions," and the §12 verification checklist). If code and README disagree, that's a bug in one of them, not a judgment call: fix the code, or if the README itself is what's stale, say so explicitly and propose the edit rather than silently working around it.

There used to be a second document, `SecretLead-PROJECT-BRIEF.md`, describing an earlier, superseded architecture (folder-as-project model, "schema" terminology, `Contractor` entity name). It's been removed. If you find a reference to it anywhere — including old commit history, or a stray local copy someone saved by mistake — that reference is stale; don't treat it as current.

## Repo status: built, not pre-build

<!-- AI-generated (Claude): build commands and layout updated for the SecretLead.Core / SecretLead.Core.Tests / SecretLead.UI split. -->
This is a working WinUI 3 / .NET 10 app, not a spec-only repo. Build and test with:

- Tests (any OS, including Linux): `dotnet test SecretLead.Core.Tests/SecretLead.Core.Tests.csproj`
- The WinUI app (Windows only): `dotnet build SecretLead.UI/SecretLead.UI.csproj -c Release -r win-x64`

Current layout:
- `SecretLead.Core/` — everything that isn't the window. Plain .NET, no Windows dependencies:
  - `Models/` — `EntityRecord`, `TagFileDefinition`, `CsvTable`, `ImportValidationResult`
  - `Services/` — tag file parsing (`TagFileParser`), AppData read/write (`EntityStore`, `EntityRecordSerializer`, `EntityNames`), CSV import (`CsvParser`, `CsvImporter`, `FieldNormalizer`, `ConstructionFixups`), export/print (`RecordPrintFormatter`, `PrintHtmlFormatter`), first-run demo (`ConstructionDemo`)
  - `Seed/construction.md`, `Seed/construction.txt` — the bundled Construction tag file and its one fictional record, built into the library and copied into AppData on first run (README.md §11)
- `SecretLead.Core.Tests/` — MSTest tests for `SecretLead.Core`, plus `RepositoryRulesTests` (Microsoft packages only, no network API in app code)
- `SecretLead.UI/` — the WinUI 3 app, packaged as MSIX. It calls `SecretLead.Core` for everything except the window:
  - `ViewModels/` — one per page (`MainPageViewModel`, `ImportPageViewModel`, `BrowsePageViewModel`), the hand-rolled MVVM base (`ObservableObject`, `RelayCommand` — see below), and the shared `StatusService`
  - `MainWindow.xaml`, `MainPage.xaml` (app shell: nav rail + content frame), `ImportPage.xaml`, `BrowsePage.xaml` — page files live at the project root, there is no `Views/` folder
  - `Assets/` — the app's icons and tiles
- `docs/file-format.md` — the exact `<EntityName>.md` and `<EntityName>.txt` formats, locked by `FileFormatTests`
- `Directory.Build.props` — the version, shared by every project
- `global.json` — selects a .NET 10 SDK
- `CHANGELOG.md` — release notes
- `scripts/Reset-SecretLeadAppData.ps1` — resets AppData to first-run state (README.md §16)
- `scripts/Test-Msix.ps1` — checks a packaged MSIX: identity, self-contained, permissions (used by both workflows)
- `.github/workflows/build.yml` — every pull request: tests, Windows build, unsigned MSIX packaging; `release.yml` — on a `v*` tag: check, test, package, sign (after approval), publish

## Non-negotiables (README.md §2/§15 is the full, authoritative list — this is a reminder, not a substitute)

- No third-party NuGet packages — .NET BCL and Windows App SDK / COM only. `CommunityToolkit.Mvvm` was deliberately removed early on for exactly this reason; the hand-rolled `ObservableObject`/`RelayCommand` in `ViewModels/` replace it. Don't reintroduce it or any other MVVM/utility package.
- No network calls, ever — no `HttpClient`, no telemetry, no update checks.
- Never use the word "schema" in code, comments, or docs — "tag file" / "description" only.
- No folder picker on Import — it uses two independent file pickers (README.md §3). This is about how files get *into* AppData (never trust a picked folder's contents wholesale), not a ban on folder pickers everywhere: the Backup button uses one to choose a destination it copies files *out* to (README.md §14).
- No MSIX sandbox workarounds.
- Never change a file format silently. Both formats are specified in `docs/file-format.md` and locked by the known-answer tests in `FileFormatTests`. Never edit their expected values to make a test pass; a deliberate change needs the owner's approval, a new spec version, and a plan for files already on disk. <!-- AI-generated (Claude) -->
- Ship one confirmed working change before starting the next — avoid speculative multi-file rewrites.

## Releases <!-- AI-generated (Claude) -->

- Releases only through `.github/workflows/release.yml`, on a pushed `vX.Y.Z` or `vX.Y.Z-rc.N` tag, with the owner's approval of the sign job. Never sign or publish by hand.
- The version lives in `Directory.Build.props` (`X.Y.Z`), `SecretLead.UI/Package.appxmanifest` (`X.Y.Z.0`) and `CHANGELOG.md` (`## [X.Y.Z] - YYYY-MM-DD`).
- The manifest's `Publisher` must equal `EXPECTED_SIGNER` in `release.yml`. Workflow actions are pinned to full commit SHAs.
- Signing trusts one immutable OIDC subject, `repo:edwest19@5809459/SecretLead@1299459679:environment:release` (README "Releases"). Renaming the repo, its owner or the `release` environment breaks signing until the Azure federated credential is updated.
- Enforced by `ReleaseRulesTests`. Every pull request packages an unsigned MSIX and checks it with `scripts/Test-Msix.ps1`.

## Sibling projects (not in this repo)

- **SecretList** — like SecretLead, but entirely offline with no import: records are typed in by hand. Separate repo, zero shared code. <!-- AI-generated (Claude): SecretLetter removed; it has no relationship to SecretLead. -->
