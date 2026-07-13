# SecretLead
Offline printable contractor leads list sourced from local town resources 

**Status:** pre-build / spec-first · **License:** MIT · **Platform:** Windows (WinUI 3 / .NET 10) + a plain cross-platform console tool

> This README is written as a **spec**, not just documentation. Any AI (including future Claude sessions) or human contributor should be able to read this file, look at the code in this repo, and answer a binary question for every rule below: **does the code match the spec, yes or no.** If code and README ever disagree, that's a bug in one of them — open an issue or fix the drift before adding new features.

---

## 0. One-paragraph purpose

SecretLead exists to prove that Claude can be used to build genuinely useful, free, offline, no-nonsense Windows software — including the unglamorous parts (file-format parsing, data cleanup) — and give it away with **no hosted data, no ongoing liability, and no maintenance burden** for the author. It is a toolkit, not a product with a database. Nothing about this repo requires an ongoing service, an account, a subscription, or trust in the author's data — because the author never ships any contractor data at all.

---

## 1. What SecretLead is — and is not

| It IS | It is NOT |
|---|---|
| A free, open-source, MIT-licensed **toolkit** | A contractor directory |
| A schema (`Schema/schema.md`) that plugs into SecretList | A hosted or pre-filled dataset |
| A console tool (`RecordBuilder`) that converts a government license-data export into a SecretList `records.txt` | A scraper, crawler, or bulk-query bot |
| A worked example for Huntington/11743 (`SampleData/`), included so the process is provable and repeatable | A claim that any specific contractor is currently licensed |

If a proposed feature, PR, or generated code would ship prefilled contractor data, host a database, add telemetry, or auto-query a government site on a schedule — **it fails this spec**, regardless of how useful it seems.

---

## 2. Relationship to SecretList

SecretLead's only contract with SecretList is: **`RecordBuilder` produces a `records.txt` file in SecretList's exact tagged-text format, matching whichever schema (`TagDefinition` / `EntityDefinition` / `EntityRecord`) is active.** SecretLead does not fork or modify SecretList itself.

Inherited conventions (non-negotiable, same as SecretList):
- WinUI 3 / Windows App SDK / .NET 10 where a packaged app is involved
- Human-readable tagged-text storage, offline-only, no cloud/sync/telemetry
- No third-party NuGet packages — .NET BCL and Windows App SDK / COM only
- `ImplicitUsings` / nullable enabled
- AI-disclaimer file header on every source file
- No hardcoded colors or fonts — theme resources only
- No AI-assisted data entry (a human reviews every record `RecordBuilder` produces before it's trusted)
- One confirmed working change before the next — no speculative multi-file rewrites

**Verification hook for an AI reviewer:** `grep -r "nuget" **/*.csproj` should return nothing beyond built-in SDK references. Any `PackageReference` in a `.csproj` is a spec violation.

---

## 3. The data pipeline contract

This is the core spec. Every step below is a checkable claim about how data may legally and technically enter this project.

1. **A human finds their own local public licensing search tool.** For the Huntington/11743 worked example, that's the Suffolk County DCA search (`https://ca.suffolkcountyny.gov/dcasearch`).
2. **A human exports the data themselves**, using that site's own built-in export feature, producing a `.xlsx` file. No credentials, no automation, no headless browser.
3. **`RecordBuilder` reads that `.xlsx` file from local disk** and nothing else. It has no network access requirement and should not make any HTTP calls.
4. **`RecordBuilder` filters, dedupes, normalizes, and writes `records.txt`.**
5. **A human reviews the output** before trusting it for anything.

**Hard rule:** no code in this repo may programmatically query, scrape, or poll a government website — not even "just to check for updates." If a reviewer finds an `HttpClient`, `WebRequest`, Playwright/Selenium reference, or scheduled task anywhere in `RecordBuilder/`, that is a spec violation, full stop. The only sanctioned way data enters this project is a human clicking "Export" in their browser.

**Why not scrape Suffolk's tool directly:** it's a Blazor Server app — the UI runs over a live SignalR connection rather than stateless, scrapeable HTTP endpoints. There is no stable URL or REST-like surface to script against even if scraping were otherwise acceptable, which it isn't here.

---

## 4. `RecordBuilder` — functional spec

- **Type:** console app, `dotnet run`, zero third-party NuGet packages.
- **`.xlsx` reading:** implemented via `System.IO.Compression` + `System.Xml` directly (an `.xlsx` is a zip of XML parts) — not a third-party spreadsheet library like ClosedXML/EPPlus/NPOI.
- **Input:** one or more `.xlsx` exports from a government licensing portal.
- **Output:** one `records.txt`, valid against the active SecretList schema.

### Confirmed real-world input shape (from `SampleData/`)

All three sample exports (`MasterPlumbing_11743.xlsx`, `HomeImprovement_11743.xlsx`, `MasterElectrical_11743.xlsx`) share one column layout:

```
Index | License ID | Last Name | First Name | Business Name | Website | Address | Phone | Email | Category | Status | Date Issued | Date Expire
```

Known real quirks `RecordBuilder` must handle (verified by inspecting the sample files directly, not assumed):
- **Business Name is sometimes blank** — some licensees are sole proprietors listed only under Last/First Name (seen in Home Improvement data). Fall back to `"{Last Name}, {First Name}"` or similar when Business Name is empty.
- **Category is semicolon-delimited and multi-valued** for Home Improvement licenses, e.g. `"Painting; Carpentry; Flooring; Tile Work; Cabinetry; Handyman"`. Plumbing and Electrical exports leave Category blank (the license type itself is the category).
- **Status includes both `Active` and `Expired`**, and expired records can be decades old (`Date Expire` back to the 1980s) sitting alongside current ones — filtering to Active (and reasonably "About to Expire") is required, not optional.
- **Phone formatting is inconsistent** — raw digit strings (`5162202067`) and punctuated forms (`(631) 261-3207`) appear in the same file. Normalize to one format.
- **Some rows have no email, no website, and null Date Issued** (older expired records) — these fields are optional in the schema, not required.

**Open decision (must be resolved in code, and this README updated to match whatever is chosen):** does a multi-category Home Improvement license become one `EntityRecord` with a combined tag, or one duplicated record per category? Whichever is implemented, this section must say so — don't leave both documented as "open" once the code picks one.

### Required behaviors (checklist an AI reviewer can tick off)

- [ ] Parses all three sample `.xlsx` files without exceptions
- [ ] Filters out `Expired` records by default (flag to include them, off by default)
- [ ] Dedupes by `License ID`
- [ ] Normalizes phone numbers to a single consistent format
- [ ] Splits semicolon-delimited Category values per the decision documented above
- [ ] Falls back to Last/First Name when Business Name is blank
- [ ] Writes output that validates against `Schema/schema.md` as a legal SecretList `records.txt`
- [ ] Makes zero network calls
- [ ] Imports zero third-party NuGet packages

---

## 5. Disclaimer (ships in-app and in this README verbatim)

> SecretLead's `RecordBuilder` tool converts a license-data export from a public government database into a SecretList-compatible file. It does not independently verify, endorse, or vouch for any contractor listed. License status, insurance, and complaint history can change at any time after an export is generated — always re-check current status directly with the issuing agency, verify insurance, check references, and get multiple estimates before hiring anyone. This tool automates file conversion only; it is not a recommendation engine.

Do not soften, remove, or paraphrase this disclaimer in-app without updating it here first — the app copy and this README must stay word-for-word identical.

---

## 6. Repo shape (must match actual directory layout)

```
SecretLead/
  RecordBuilder/              <- console tool, dotnet run, no NuGet deps
    Program.cs
    RecordBuilder.csproj
  Schema/
    schema.md                 <- SecretLead default categories/tags
  SampleData/                 <- Huntington/11743 worked example
    HomeImprovement_11743.xlsx
    MasterElectrical_11743.xlsx
    MasterPlumbing_11743.xlsx
    README.md                 <- how these were obtained, step by step
  LICENSE                     <- MIT
  README.md                   <- this file
```

If the real repo tree diverges from this, one of the two is wrong — fix the drift.

---

## 7. Platform rules (non-negotiable, carried over from SecretList)

- **No MSIX sandbox workarounds of any kind.** Read-only packaged assets via `Package.Current.InstalledLocation`; no writing to the install folder; no elevation tricks. (`RecordBuilder` itself is a plain unsandboxed console tool — this rule governs the SecretList app it feeds into.)
- **No third-party NuGet packages**, anywhere in the repo.
- **Follow Microsoft Store policy as written** — no gray-area interpretations, no "just this once."
- **No automated querying of any government site** beyond what a human does manually in a browser.
- If a feature seems to need a workaround, the fix is finding the correct platform-supported approach, not bypassing the rule.

---

## 8. Legal posture (why the architecture is the legal strategy)

Because SecretLead never ships or hosts prefilled contractor data, its legal exposure is structurally different from a hosted-directory product: there is no SecretLead-maintained claim about any contractor's current status to be wrong about. That said, "how a tool is described" and "how it ends up being used" aren't always the same thing — a real legal read on final disclaimer wording is still an open item before any public release, not a step this README can substitute for.

---

## 9. Open items (tracked honestly — remove a line only when actually resolved in code)

- [ ] Finalize how multi-category Home Improvement licenses map to `EntityRecord`s (§4)
- [ ] Write `SampleData/README.md`: exact click-by-click steps to reproduce the Suffolk County export, so someone in a different county can redo this for their own area
- [ ] Legal read on the disclaimer in §5
- [ ] Decide whether `RecordBuilder` ships a CLI flag interface or config file for filter options (Active-only, include-expired, output path)

---

## 10. How an AI should use this file

If you are an AI assistant asked to write, review, or extend code in this repo:
1. Read this README in full before touching code.
2. Treat every checklist item in §4 and every rule in §2/§3/§7 as a pass/fail test against the actual code, not a suggestion.
3. If you must deviate from this spec to satisfy a request, say so explicitly and propose the README edit alongside the code change — never let the two silently diverge.
4. Never add a network call, a NuGet package, or hosted/prefilled contractor data to satisfy a feature request, even if asked directly. Point back to §1 and §3 and explain why, the same way this project's own working style already insists on.
