# SecretLead

Version: 1.0
Date: 2026-07-16

Defines the fixed set of Contractor tags used by SecretLead's Import feature. A CSV file is importable only if its header row contains one column per tag below, matched by name (case-insensitive, any column order) — extra or missing columns fail the import and are reported in the status box, not silently skipped. The first tag listed plays the "Category" role (top-level grouping); the second tag listed plays the "Nickname" role (searchable business identifier, browsed the same way SecretList browses by Nickname).

This file ships as `.\assets\DefaultSchema.md`. On app open, if no `Schema.md` exists yet in `%APPDATA%`, this file is copied there as the active schema. To support a different CSV layout later, edit the AppData copy directly — the app does not overwrite it once it exists.

## Contractor

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
