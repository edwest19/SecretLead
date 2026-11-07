# Changelog

> AI-generated: this file was written by Claude (Anthropic). SecretLead is a project built with AI-generated code; see [README.md](README.md).

Every release has a heading `## [X.Y.Z] - YYYY-MM-DD`. Changes waiting for the next release are listed under `## [Unreleased]`.

## [Unreleased]

### Project
- The tag-file, CSV import, record store, print and export code moved into a new library, `SecretLead.Core`, which has no Windows dependencies. The window app moved to `SecretLead.UI` and calls the library.
- New MSTest tests in `SecretLead.Core.Tests` cover the library, and check that only Microsoft packages are used and that no app code uses a network API. The Build workflow runs them.
- The version is set once, in `Directory.Build.props` (0.2.0). `global.json` selects a .NET 10 SDK.

## [0.1.2] - 2026-07-21

- Released as `SecretLead-win-x64.zip`: an unpackaged x64 build. It needs the .NET 10 Desktop Runtime and the Windows App SDK runtime installed separately, and it isn't signed.
