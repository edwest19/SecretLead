# SecretLead — Project Brief

Copyright (c) 2026 edwest19
Last updated: July 16, 2026 — **major revision: generalized architecture**

## What SecretLead is now

SecretLead is a **standalone WinUI 3 app** — schema-driven, human-readable tagged-text storage, offline-only, no third-party NuGet packages, no cloud/sync/telemetry, no MSIX sandbox workarounds. That part hasn't changed. What's changed: **SecretLead is no longer fixed to a single Contractor entity.** It is now a generic, offline CRUD tool with an import pipeline — the owner defines a schema for *any* kind of record, and SecretLead can either import a matching CSV in bulk or let records be keyed in by hand, into a store that lives entirely in AppData and never touches a network. This is the same repo, same name, same app — the earlier Contractor-only spec is superseded, not forked.

**Why this matters:** the pitch isn't just "an offline CSV importer." It's "import your data once, and from that point on it never touches a network again, because the app architecturally cannot make a network call." That's a stronger, more general claim than anything specific to contractors, and it's worth stating as the app's actual selling point going forward.

## Relationship to SecretList — unchanged

SecretList stays exactly as it is: its own repo, own codebase, zero shared code, manual-entry-only. It is **not** being replaced and is **not** absorbed into this generalization — the owner explicitly wants to keep it as the tool where records get typed in by hand. SecretLead and SecretList share only the underlying approach (schema-driven, tagged-text storage), the same as before.

## The Contractor-only design is now the bundled default demo, not a separate product

The original fixed-schema SecretLead (Category / Business Name / License ID / … / Website, CSV-import-only) becomes the app's **built-in default schema** — used whenever the active folder has no `schema.md` of its own. The intent is explicitly a demo: show it working out of the box on contractor data, then show that swapping in any other `schema.md` makes it work on completely different data. SecretLead is now a strict superset of what the original spec described — everything the Contractor-only version did is still possible, it's just no longer the *only* thing possible.

## The folder-as-project model

There is no in-app list of named projects. **A folder is a project.** The owner points SecretLead's folder picker at a folder, and:

- If that folder contains a `schema.md`, that becomes the active schema.
- If it doesn't, SecretLead falls back to its bundled default (Contractor) schema.
- The folder also holds the `.csv` file to import, when importing.
- Switching projects is just pointing the picker at a different folder. No migration step, no named-project management UI.

The picked folder **never** holds the actual record store — only `schema.md` and the source `.csv` live there.

## Schema

Same schema grammar as before, now understood to apply to any domain, not just contractors:

- `#` = schema title, description, version, date
- `##` = entity name — **also the name of the AppData output file.** `## Contractor` → `Contractor.txt`; `## AIAssistant` → `AIAssistant.txt`
- `### TAG X` = a tag name matched case-insensitively against CSV column headers when importing

**Role rule (generalized, applies to every schema, not just Contractor):** the first `### TAG` plays the "Category" role — top-level grouping, and the field that gets exploded into multiple records when its value is semicolon-delimited on import. The second `### TAG` plays the "Nickname" role — the searchable identifying field for browsing. Tags beyond position 2 can be in any order; CSV columns can be in any order too, matched by name.

## Storage: AppData, entity-name-keyed, global — not scoped per folder

The actual `<EntityName>.txt` record store always lives in the app's sandboxed AppData storage — GUI-only, never a location the user browses to or hand-edits directly. **The entity name is the only key.** There is no per-folder or per-project isolation: if two different folders' schemas both declare `## Contractor`, they read and write the *same* `Contractor.txt`, on purpose.

## Existence-of-file is the mechanic — both for import-prompting and for collision detection

- **No `<EntityName>.txt` yet** → first time this entity has been used → SecretLead knows to prompt Import (there's nothing to browse yet).
- **`<EntityName>.txt` already exists** → skip straight to browse/list mode. Import can still be run again later, manually, to add more records under the same schema into the same existing store.
- **A genuine collision — a schema declaring an entity name that already belongs to a different, unrelated dataset** — is an **error**, not something the app tries to merge or isolate around. SecretLead does not inspect or reconcile tag sets; the mere existence of the file under that name means the name is taken. Resolution is on the owner: pick a different entity name in `schema.md`. This is consistent with the app's existing hard-stop validation philosophy — no silent skip, no clever disambiguation.

## CRUD + Import, together — this is the actual "SecretList 2.0" claim

SecretLead now supports both of the following against whatever schema is active:

- **Manual CRUD** — Add / Edit / Delete records by hand, the way SecretList already works
- **CSV Import** — bulk-convert a matching CSV into records, the way the original SecretLead worked

Same list/detail/print interaction model either way. This combination — any schema, plus both hand-entry and bulk import — is the actual generalization, not just "CSV import for more entity types."

## Field normalization — generalized, some rules stay schema-specific

The substring-based normalization rules (inferred purely from tag name, no datatype field) generalize cleanly to any schema:

| Name contains | Rule |
|---|---|
| `date` | Reformat to `mm/dd/yyyy` regardless of source format |
| `email` | No validation or transform |
| `phone` | Strip to digits, reformat by digit count (10 → `(631) 555-1212`, 11 → `+1 (631) 555-1212`, 7 → `555-1212`, else left as-is) |

**Business Name fallback to `Last Name, First Name` when blank remains specific to the Contractor demo schema** — it only makes sense where those three tags exist, and is not a generalized rule other schemas inherit automatically.

## Import validation — unchanged, already generalized

Hard stop, both directions: any schema tag with no matching CSV column, or any CSV column with no matching schema tag, blocks the import and is named explicitly in the status box. This logic never depended on Contractor specifically and needs no change.

## Print layout — unchanged, already generalized

A4 portrait, two records per page, duplex, folded into an A5 handout. This is a structural layout mechanic (whatever tags a schema defines get printed), not something tied to contractor data specifically.

## SecretLetter — standalone, separate product, not integrated

A third, fully independent app. Not a feature bolted onto SecretLead or SecretList, and not integrated with either at the code level. Rough concept only at this point — generating a formatted Markdown (or print-ready) letter/report — actual design is unstarted; see open items.

## Non-negotiables (carried over, still apply)

- No third-party NuGet packages — .NET BCL and Windows App SDK / COM only
- No network calls, no scraping, no automated querying of any site
- Offline-only, no cloud/sync/telemetry
- No MSIX sandbox workarounds — read-only packaged assets, no install-folder writes, no elevation tricks
- A human still reviews imported records before trusting them for anything

## ⚠️ Needs a decision: what does "sample data" mean for the bundled demo?

The default-schema demo is meant to ship with sample data so a first-time user sees it work immediately. But this bumps into an existing rule: the original brief says *"no prefilled contractor data shipped with the repo — sample `.xlsx` files exist only as a worked example, not a dataset."* That rule was written when the sample files were purely a development-time reference, not something installed on an end user's machine. Two different things could be meant by "sample data" now, and they have very different implications:

- **Fictional placeholder data** (like the `Example Home Design Llc` / `555` / `example.com` record already used elsewhere in this project) — safe, no conflict with the existing non-negotiable, no real names or license numbers ship with the installer.
- **The real, hand-verified `contractor.csv`** — actual business names and license numbers for real Huntington-area contractors — bundled into a distributable app. This would directly conflict with the "no prefilled contractor data shipped" rule and reintroduces the exact liability question that rule existed to avoid.

Not resolved here on purpose — this is exactly the kind of thing to flag rather than pick silently.

## Open items — tracked honestly

- [ ] **Sample data for the bundled demo** — fictional vs. real (see flag above)
- [ ] Whether SecretLead should validate an existing entity file's tag structure against the active schema before appending, or continue relying purely on entity-name uniqueness discipline
- [ ] SecretLetter's actual design — input format, output format, whether it reads another Secret-family app's `.txt` file or is fully self-contained
- [ ] Whether the licensing-specific disclaimer (§ in the prior brief) is scoped only to when the Contractor demo schema is active, or whether arbitrary user-defined schemas need any disclaimer mechanism at all
- [ ] `SampleData/README.md` — walkthrough of exporting the county `.xlsx` and converting it to CSV
- [ ] Legal review of the Contractor-demo disclaimer
