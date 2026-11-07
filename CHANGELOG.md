# Changelog

> AI-generated: this file was written by Claude (Anthropic). SecretLead is a project built with AI-generated code; see [README.md](README.md).

Every release has a heading `## [X.Y.Z] - YYYY-MM-DD`. Changes waiting for the next release are listed under `## [Unreleased]`.

## [Unreleased]

### New
- **The Construction demo is added on first run.** When AppData has no entities yet, SecretLead adds the Construction tag file and one fictional record (Example Plumbing & Heating, a 555-01xx phone number, example.com addresses), so there's something to browse right away. The README described this before, but it wasn't built. Both files are built into the app.

### Fixes
- **Line breaks no longer lose text.** A quoted CSV value with a line break (a two-line address, say) was stored as-is, and everything after the first line was lost when the records were read back. Each line is now trimmed and the lines are joined with one space, on import and for values typed or pasted by hand.
- **Re-importing with a different tag file is blocked.** Importing a tag file with other tags under an existing entity name replaced the stored tag file, and the next edit or delete erased the missing fields from every record. Now the Import button stays gray and Status names each tag that differs. The same tags in a different order are blocked too, because the order decides Category and Nickname.
- **Tag names with a period are refused.** A tag like `Lic. No` was read back under the wrong name. The tag file is now refused, naming the tag.
- **Entity names must be usable Windows file names.** A name like `../../x` wrote files outside AppData, and names Windows can't use as file names (`a:b`, `what?`, `CON`) weren't checked. Such tag files are now refused with the reason. Import errors weren't caught; now, if an import fails for any reason, the reason goes to Status and the app stays open.

### Project
- The tag-file, CSV import, record store, print and export code moved into a new library, `SecretLead.Core`, which has no Windows dependencies. The window app moved to `SecretLead.UI` and calls the library.
- New MSTest tests in `SecretLead.Core.Tests` cover the library, and check that only Microsoft packages are used and that no app code uses a network API. The Build workflow runs them.
- The version is set once, in `Directory.Build.props` (0.2.0). `global.json` selects a .NET 10 SDK.

## [0.1.2] - 2026-07-21

- Released as `SecretLead-win-x64.zip`: an unpackaged x64 build. It needs the .NET 10 Desktop Runtime and the Windows App SDK runtime installed separately, and it isn't signed.
