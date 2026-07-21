# CLAUDE.md

This file provides guidance to Claude Code (or any AI agent) working in this repository.

## The spec

**`README.md` is the single, authoritative spec.** Read it in full before writing code — it's written specifically so an AI can verify whether the code matches it (see its own §13, "Instructions for future AI sessions," and the §12 verification checklist). If code and README disagree, that's a bug in one of them, not a judgment call: fix the code, or if the README itself is what's stale, say so explicitly and propose the edit rather than silently working around it.

There used to be a second document, `SecretLead-PROJECT-BRIEF.md`, describing an earlier, superseded architecture (folder-as-project model, "schema" terminology, `Contractor` entity name). It's been removed. If you find a reference to it anywhere — including old commit history, or a stray local copy someone saved by mistake — that reference is stale; don't treat it as current.

## Repo status: built, not pre-build

This is a working WinUI 3 / .NET 10 app, not a spec-only repo. Build and run with:

```
dotnet build
dotnet run
```

Current layout:
- `Models/` — `EntityRecord`, `TagFileDefinition`, `CsvTable`, `ImportValidationResult`
- `Services/` — tag file parsing (`TagFileParser`), AppData read/write (`EntityStore`, `EntityRecordSerializer`), CSV import (`CsvParser`, `CsvImporter`, `FieldNormalizer`, `ConstructionFixups`), export/print (`RecordPrintFormatter`, `PrintHtmlFormatter`)
- `ViewModels/` — one per page (`MainPageViewModel`, `ImportPageViewModel`, `BrowsePageViewModel`), the hand-rolled MVVM base (`ObservableObject`, `RelayCommand` — see below), and the shared `StatusService`
- `MainWindow.xaml`, `MainPage.xaml` (app shell: nav rail + content frame), `ImportPage.xaml`, `BrowsePage.xaml` — page files live at the project root, there is no `Views/` folder
- `Assets/construction.md` — the bundled default tag file (entity name `Construction`)
- `scripts/Reset-SecretLeadAppData.ps1` — resets AppData to first-run state (README.md §16)

## Non-negotiables (README.md §2/§15 is the full, authoritative list — this is a reminder, not a substitute)

- No third-party NuGet packages — .NET BCL and Windows App SDK / COM only. `CommunityToolkit.Mvvm` was deliberately removed early on for exactly this reason; the hand-rolled `ObservableObject`/`RelayCommand` in `ViewModels/` replace it. Don't reintroduce it or any other MVVM/utility package.
- No network calls, ever — no `HttpClient`, no telemetry, no update checks.
- Never use the word "schema" in code, comments, or docs — "tag file" / "description" only.
- No folder picker on Import — it uses two independent file pickers (README.md §3). This is about how files get *into* AppData (never trust a picked folder's contents wholesale), not a ban on folder pickers everywhere: the Backup button uses one to choose a destination it copies files *out* to (README.md §14).
- No MSIX sandbox workarounds.
- Ship one confirmed working change before starting the next — avoid speculative multi-file rewrites.

## Sibling projects (not in this repo)

- **SecretList** — separate repo, zero shared code, manual-entry-only.
- **SecretLetter** — a third, unstarted, independent app/concept. Don't build it speculatively.
