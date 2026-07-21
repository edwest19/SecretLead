# SecretLead

Copyright (c) 2026 edwest19

Genetated by Claude.

**Status:** pre-build / spec-first · **License:** MIT · **Platform:** Windows (WinUI 3 / .NET 10, packaged MSIX)

> This README is written as a **spec**, not just documentation. Any AI (including future Claude sessions) or human contributor should be able to read this file, look at the code in this repo, and answer a binary question for every rule below: **does the code match the spec, yes or no.** If code and README ever disagree, that's a bug — fix the code to match the README, or open an issue proposing the README change. Never silently drift.

---

## 0. One-paragraph purpose

SecretLead is a **standalone, offline, tag-file-driven CRUD and CSV-import tool for Windows.** The owner writes a tag file describing any kind of record. 
SecretLead either imports a matching CSV in bulk or lets records be keyed in by hand, into a store that lives entirely in the app's local AppData and never touches a network. 
The pitch is not "an offline CSV importer for one dataset." It's: **import your data once, and from that point on the app architecturally cannot make a network call, ever, for anything.** 
That claim — not any single bundled dataset — is the actual product.

---

## 1. Relationship to other Secret* apps

- **SecretList** — a separate app, separate repo, **zero shared code**. SecretList is the hand-entry-only tool: the owner types records in one at a time. SecretLead does **not** feed SecretList, replace it, or depend on it. The two apps share only a conceptual approach (tag-file-driven, human-readable tagged-text storage) — nothing else.
- **SecretLetter** — a third, fully independent app (rough concept only, unstarted). Not a feature of SecretLead, not integrated with it at the code level.

If any prior document (including older revisions of this README, or older revisions of any tag file) used the word "schema," described a folder-picker-driven import, or described SecretLead as feeding SecretList or as a Contractor-only pre-populated database — that description is **superseded**. This file is the current spec.

---

## 2. Platform rules — non-negotiable

- **WinUI 3 / Windows App SDK, .NET 10, packaged MSIX.**
- **No third-party NuGet packages** — .NET BCL and Windows App SDK / COM only.
- **No network calls, ever.** No scraping, no telemetry, no sync, no update-check pings, no "phone home" of any kind. This is architectural, not a setting — there should be no code path in the app capable of opening a socket.
- **Offline-only.** Everything the app needs (tag files, records) lives on local disk.
- **No MSIX sandbox workarounds of any kind.** Read-only packaged assets via `Package.Current.InstalledLocation`; no attempts to write to the install folder; no elevation tricks.
- **No prefilled real-world data ships with the installer.** See §11 — the bundled demo is fictional only.
- If a feature seems to need a workaround to function, the answer is to find the correct platform-supported approach — not to bypass a rule above.

---

## 3. The Import screen

There is no folder picker on Import, and no "folder is a project" model. That's the rule this section exists to enforce — it's about how files get *into* AppData, not a blanket ban on folder pickers anywhere in the app (the Backup button, §14, uses one to choose where files get copied back *out*). Import works from **two independent file selections**, each its own file picker:

- **CSV file** — the data to import. Any filename, any location.
- **Tag file** — the `.md` file describing the entity's tags (§4). Any filename, any location. It does not need to sit next to the CSV, and it does not need to be named after the entity — though the bundled default (`construction.md`) follows that convention anyway.

**Validation runs automatically the moment both files are selected** — there's no separate "check" step. SecretLead matches every tag against the CSV's column headers, both directions, exactly as described in §9.

**The Import button's color *is* the validation result:**
- Gray/disabled — one or both files not yet selected, or they don't match.
- **Green/enabled** — both files selected and every tag matches a CSV column with nothing left over in either direction.

**A status area** holds validation messages and errors — reached via a button on the left (nav rail), not a box that's permanently visible on the Import screen. Same content rule as always: every mismatch is named explicitly (§9), never summarized.

**What happens when the green Import button is clicked:**

1. SecretLead reads the entity name from the tag file's `##` line (e.g. `## Construction`).
2. The tag file itself is copied into AppData alongside the entity's records (§5) — so the app always has its own reference copy of the tag file that produced `<EntityName>.txt`, independent of wherever the original file came from or whether it still exists afterward.
3. The CSV is converted into records per the tag file's rules (§4 role/split rules, §8 normalization) and written to `<EntityName>.txt` in AppData.
4. If `<EntityName>.txt` already exists, the new records are **appended** — nothing already stored is deleted or overwritten (§6).

Nothing outside AppData is ever written. The original CSV and tag file, wherever the owner picked them from, are only ever read, never modified or moved.

---

## 4. Tag file grammar

A tag file is plain Markdown, parsed as follows:

- `#` — title, a short description of the entity, version, date (informational; not machine-enforced beyond parsing past it)
- `##` — **entity name.** This is also the name of the AppData output file: `## Contractor` → `Contractor.txt`. `## Construction` → `Construction.txt`.
- `### TAG X` — a tag name, matched **case-insensitively** against CSV column headers when importing. Any column order is fine.

**Role rule (applies to every tag file):**
- The **first** `### TAG` plays the **Category** role — top-level grouping, and the field that gets exploded into multiple records when its value is semicolon-delimited on import (one input row → one output record per semicolon-separated value, all other fields identical).
- The **second** `### TAG` plays the **Nickname** role — the searchable identifying field used for browsing.
- Tags beyond position 2 can be declared in any order. CSV columns can be in any order too, matched by name.

That's the entire grammar: a name-matcher, two positional roles, and a filename — not a schema in the type/constraint sense.

---

## 5. Storage: AppData, entity-name-keyed, global

Everything SecretLead writes lives in the app's sandboxed AppData storage — **GUI-only**, never a location the user browses to or hand-edits directly. Each entity is a **pair** of files:

- `<EntityName>.md` — the reference copy of the tag file that produced this entity, copied in at Import time (§3).
- `<EntityName>.txt` — the actual records.

- **The entity name is the only key.** There is no per-source, per-CSV, or per-import-session isolation. If two different imports both declare `## Contractor`, they read and write the **same** `Contractor.txt` (and update the same `Contractor.md`).
- Keeping the tag file's own copy in AppData means an entity stays fully self-describing even if the original tag file the owner picked gets renamed, moved, or deleted afterward.

---

## 6. Existence-of-file is the mechanic

SecretLead's home/browse screen lists whatever entities currently exist in AppData — discovered by the `<EntityName>.txt` files present there, not by any folder the owner has open.

| State | Behavior |
|---|---|
| First run, AppData has no entities yet | The bundled Construction demo (§11) is seeded in automatically, so there's always at least one browsable entity out of the box. |
| Imported tag file names an entity with **no** existing `<EntityName>.txt` | Import creates `<EntityName>.md` and `<EntityName>.txt` fresh. |
| Imported tag file names an entity that **already exists** | Import appends the new records into the existing `<EntityName>.txt` and updates the stored `<EntityName>.md` copy. Existing records are never deleted or overwritten. |

**Still open, not decided (see §14):** whether SecretLead should compare the incoming tag file's tag set against the entity's already-stored `<EntityName>.md` before appending — flagging a mismatch as a possible different dataset reusing the same entity name — or continue relying purely on entity-name-as-key with no structural check.

---

## 7. CRUD + Import, together

Against whatever entity is selected, SecretLead supports both:

- **Manual CRUD** — Add / Edit / Delete records by hand, reached from the browse/list screen.
- **CSV Import** — bulk-convert a matching CSV into records, reached from the Import screen (§3) at any time — not gated behind any prior folder or file selection.

Same list/detail/print interaction model either way. This combination — any tag file, plus both hand-entry and bulk import, against one generic AppData store — is the actual generalization. It is not just "CSV import extended to more entity types."

---

## 8. Field normalization

Inferred purely from **tag name substring** (case-insensitive), no separate datatype field:

| Tag name contains | Rule |
|---|---|
| `date` | Reformat to `mm/dd/yyyy` regardless of source format |
| `email` | No validation or transform — stored as-is |
| `phone` | Strip to digits, then reformat by digit count: 10 digits → `(631) 555-1212`; 11 digits → `+1 (631) 555-1212`; 7 digits → `555-1212`; anything else → left as-is |

**Tag-file-specific exception:** Business Name falling back to `Last Name, First Name` when blank is specific to the bundled **Construction** demo tag file (§11) — it only makes sense where those three tags exist, and is **not** a generalized rule other tag files inherit automatically.

---

## 9. Import validation

Hard stop, both directions, no silent skipping — this is what the Import button's gray/green state (§3) reflects:

- Any tag with no matching CSV column → blocks import (button stays gray).
- Any CSV column with no matching tag → blocks import (button stays gray).
- Both cases are named explicitly, by name, in the status area (§3) — reached via the left nav button, not a screen the owner has to scroll past.

This logic is fully generalized — it never depended on Contractor/Construction specifically and needs no per-entity change.

---

## 10. Print layout

A4 portrait, two records per page, duplex, folded into an A5 handout. No requirement for front/back alignment between the two records on a sheet. This is a structural layout mechanic — whatever tags a tag file declares get printed — not something tied to contractor data specifically.

---

## 11. Bundled default: the Construction demo

SecretLead ships with one built-in default entity, **seeded directly into AppData on first run** (when AppData has no entities yet — §6), so a first-time user has something to browse immediately without running Import at all:

- `.\assets\construction.md` — the tag file, copied into AppData as `Construction.md`. Entity name `## Construction` (12 tags: Category, Business Name, License ID, Last Name, First Name, Address, Phone, Email, Status, Date Issued, Date Expire, Website).
- `.\assets\construction.txt` — **one fictional sample record**, copied into AppData as `Construction.txt`. Reserved `555` phone number, `example.com` email/website domain. No real business names, addresses, phone numbers, or license numbers ship with the installer, ever.

The real, hand-verified `contractor.csv` and the source `.xlsx` county exports it's built from are **strictly development-time references** — used to ground this spec in real data quirks, never bundled, never shipped.

The intent of this demo is explicit: show the app working out of the box on construction-trade data, then show that running Import with a different tag file adds a completely different, independent entity alongside it. SecretLead is a strict superset of what a Contractor-only tool would do — everything that design did is still possible here, it's just no longer the *only* thing possible.

---

## 12. Verification checklist (pass/fail)

An AI or human auditing this repo should be able to check each line below against the actual code:

- [ ] App makes zero network calls under any code path — no `HttpClient`, no sockets, nothing reachable from any menu or background timer.
- [ ] Zero third-party NuGet package references in any `.csproj`.
- [ ] Import screen has two independent file pickers (CSV, tag file) — no folder picker on Import.
- [ ] The two files can be selected from different locations with unrelated filenames and import still works.
- [ ] Import button's enabled/color state updates automatically the instant both files are selected — no separate "validate" action required.
- [ ] Status/error area is reached via a left-side button, not a permanently visible box.
- [ ] Successful import copies both `<EntityName>.md` and `<EntityName>.txt` into AppData — never anywhere else.
- [ ] Original CSV and tag file, wherever picked from, are left untouched — read-only access.
- [ ] Record store and tag-file-copy files live only under the app's sandboxed AppData path.
- [ ] `<EntityName>.txt` / `<EntityName>.md` filenames exactly match the `##` entity name from the imported tag file.
- [ ] Importing into an entity that already exists appends records rather than overwriting.
- [ ] First run with empty AppData seeds the Construction demo automatically, without requiring the owner to run Import.
- [ ] Import validates tags vs. CSV columns in both directions; mismatches are named explicitly in the status area.
- [ ] Semicolon-delimited values in the Category-role tag explode into one record per value on import.
- [ ] `date`-containing tags normalize to `mm/dd/yyyy`; `phone`-containing tags normalize by digit count per §8; `email`-containing tags are untouched.
- [ ] Business Name blank-fallback logic exists only in code paths specific to the Construction tag file, not as a generic rule.
- [ ] Print output is A4 portrait, duplex, two records per page.
- [ ] No installer asset contains real contractor names, addresses, phone numbers, emails, or license numbers.
- [ ] No MSIX sandbox workaround exists anywhere (no install-folder writes, no elevation).
- [ ] No use of the word "schema" anywhere in code, comments, or docs — "tag file" and "description" only.

Any unchecked item is either a bug to fix or a signal this README is stale and needs updating — treat divergence as a defect in one document or the other, never as acceptable drift.

---

## 13. Instructions for future AI sessions

If you are an AI (Claude or otherwise) opening this repo cold:

1. **This README is authoritative.** Where code and README disagree, that is a bug. Prefer fixing the code; if the README itself is what's out of date, say so explicitly and propose the edit rather than silently working around it.
2. **Never use the word "schema."** Call the file a **tag file** and call its contents a **description**. If you find "schema" anywhere in this repo, it's leftover wording from before the rename — fix it, don't propagate it.
3. **Don't reintroduce the folder-as-project model.** There is no folder picker on Import — it uses two independent file pickers (§3); that supersedes any earlier design where a single picked folder held both files. This is not a blanket ban on folder pickers anywhere in the app: the rule is about how files get *into* AppData (never trust a picked folder's contents wholesale), not about the Backup button's picker, which only copies files *out* of AppData for safekeeping (§14).
4. **Don't reintroduce the old Contractor-only, pre-populated-database design.** That model is superseded (§0, §1). SecretLead does not ship real contractor data, does not vet licenses at runtime, and is not scoped to contractors at all — Construction is a bundled demo entity, not the product.
5. **Don't add network calls, sync, telemetry, or NuGet dependencies** to "make something easier." If a task seems to need one, stop and flag it instead of implementing a workaround.
6. **Treat §12 as a literal checklist** before considering any implementation task complete.
7. **Flag open decisions honestly** rather than resolving them silently — see §14.

---

## 14. Open items — tracked honestly

- [ ] Whether SecretLead should validate an incoming tag file's tag set against an existing entity's stored `<EntityName>.md` before appending records, or continue relying purely on entity-name-as-key (§6).
- [x] **One-click backup out of AppData.** Built: the Backup button (`MainPageViewModel.BackupAsync`) opens a folder picker and copies every `<EntityName>.txt`/`.md` pair from AppData into the chosen folder. This is the one folder picker in the app — it's fine because it only copies files *out* of AppData for safekeeping; it never reads a picked folder's contents back in (that's what Import's two file pickers are for, §3). Worth having given precedent: SecretList's `records.txt` was lost with no recoverable trace after a Windows feature update.
- [ ] **Atomic write-then-swap on every save.** Still not built for either app — the one-click backup above is a manual safety net, not protection against a crash mid-write corrupting the live `.txt`/`.md` files.
- [ ] Splitting the bundled `Construction` demo tag file into separate real-usage tag files (`Construction`/general Home Improvement, `Electrician`, `Plumber`) once real data replaces the demo — future intent, not yet spec'd.
- [ ] SecretLetter's actual design — input format, output format, whether it reads another Secret-family app's `.txt` file or is fully self-contained.
- [ ] Whether any disclaimer mechanism is needed for arbitrary user-defined tag files, or only ever for a licensing-specific demo/dataset.
- [ ] `SampleData/README.md` — walkthrough of exporting source `.xlsx` data and converting it to CSV, for anyone building their own real dataset against the Construction tag file.
- [ ] Legal review of any disclaimer language, before any public sale.

---

## 15. Non-negotiables (summary)

- No third-party NuGet packages — .NET BCL and Windows App SDK / COM only
- No network calls, no scraping, no automated querying of any site, ever
- Offline-only, no cloud/sync/telemetry
- No MSIX sandbox workarounds — read-only packaged assets, no install-folder writes, no elevation tricks
- No real-world personal/business data ships with the installer
- A human reviews imported records before trusting them for anything
- No use of the word "schema" — tag file / description only
- No folder picker on Import — it uses two independent file pickers (§3). The Backup button (§14) is the one exception: it opens a folder picker to choose where files get copied *out* to, never to pull picked files back in.

---

## 16. Developer scripts

Not part of the shipped app itself — tooling for maintainers and end users to run manually from a terminal. Doesn't touch or relax any rule in §2; the app binary still makes zero network calls and has zero NuGet dependencies. These scripts do.

- `scripts\Reset-SecretLeadAppData.ps1` — deletes every stored entity (`<EntityName>.md` / `<EntityName>.txt`) from AppData, resetting SecretLead to first-run state. Existence-of-file is the whole mechanism (§6), so this is a complete, clean reset. Finds the AppData folder by matching the installed package's DisplayName ("SecretLead") rather than a hardcoded path or package identity, since the folder name is derived from the package's Identity Name and Publisher, which can change across builds/signing. Prompts for confirmation by default (supports `-WhatIf`/`-Confirm:$false`); pass `-Force` to skip the prompt. If Windows blocks it as a downloaded script ("running scripts is disabled on this system"), right-click the file → Properties → Unblock, or run `powershell -ExecutionPolicy Bypass -File .\scripts\Reset-SecretLeadAppData.ps1`.
