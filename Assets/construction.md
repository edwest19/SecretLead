# SecretLead — Bundled Default Schema

Version: 1.1
Date: 2026-07-17

Defines the fixed set of tags for SecretLead's bundled default schema — the schema SecretLead falls back to whenever the folder picked via the folder-as-project model has no `schema.md` of its own. This is the original Contractor-only schema from the pre-generalization spec, renamed **Construction** so its entity name and its record-store filename (`Construction.txt`) stay consistent with every other schema under the generalized architecture. A CSV file is importable only if its header row contains one column per tag below, matched by name (case-insensitive, any column order) — extra or missing columns fail the import and are reported in the status box, not silently skipped. The first tag listed plays the "Category" role (top-level grouping, and the field exploded into multiple records when semicolon-delimited on import); the second tag listed plays the "Nickname" role (searchable business identifier).

This file ships as `.\assets\construction.md` — a read-only packaged asset. It is used directly as the fallback schema whenever the active folder has no `schema.md` of its own. Unlike the pre-generalization design, it is **not** copied into AppData on first run — schemas now live per-folder, not globally, so there's nothing to copy.

## Construction

### TAG Category

### TAG Business Name

### TAG License ID

### TAG Last Name

### TAG First Name

### TAG Address

### TAG Phone

### TAG Email

### TAG Status

### TAG Date Issued

### TAG Date Expire

### TAG Website
