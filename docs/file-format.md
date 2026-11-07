# SecretLead file formats

> AI-generated: this file was written by Claude (Anthropic) from the code in `SecretLead.Core` as of 2026-11-07. SecretLead is a project built with AI-generated code; see [README.md](../README.md).

SecretLead stores each entity as two text files in its data folder (README §5):

| File | What it is | Written by |
|---|---|---|
| `<EntityName>.md` | The tag file: the entity name and its tags, in order | `EntityStore.SaveTagFile`, a copy of the tag file picked at Import |
| `<EntityName>.txt` | The records | `EntityStore.AppendRecords` (Import, Add) and `EntityStore.SaveRecords` (Edit, Delete) |

Both formats are locked by the known-answer tests in `SecretLead.Core.Tests/FileFormatTests.cs`. **Changing either format needs the owner's approval**, a new version of this document, and a plan for files already on disk. Never change a known-answer test's expected values to make it pass.

## Both files

- **Encoding.** Written as UTF-8 with no byte-order mark. Read as UTF-8, or as UTF-16 or UTF-32 if the file starts with that byte-order mark.
- **Line endings.** Written with LF (`\n`). Read with LF or CRLF.
- **Names are compared ignoring case** (ordinal, case-insensitive): tag names against CSV columns, against each other, and in record lines.

## The tag file (`<EntityName>.md`)

The grammar is README §4. Line by line:

| A line that starts with… | Means |
|---|---|
| `## ` (two `#`, a space) | The entity name: the rest of the line, trimmed. Exactly one per file. |
| `### TAG ` (three `#`, a space, `TAG` in capitals, a space) | A tag name: the rest of the line, trimmed. At least two per file. The first is the Category role, the second the Nickname role. |
| anything else, including `#`, `####`, plain text, or a `##`/`### TAG` line with leading spaces | Ignored |

Trailing whitespace on each line is ignored. A tag file is refused (`TagFileFormatException`, with the reason in the status area) when:

- it has no `## ` line, or more than one
- it has fewer than two `### TAG ` lines
- a tag name contains a period (`.`), because record lines split at the first period
- the entity name isn't a usable Windows file name: it contains any of `< > : " / \ | ? *` or a control character, ends with a period or a space, or is a reserved device name (CON, PRN, AUX, NUL, COM1–COM9, LPT1–LPT9, COM¹–COM³, LPT¹–LPT³), also when followed by an extension

When a tag file is imported for an entity that already exists, it must have the same tags in the same order as the stored copy (README §6). The stored copy is then replaced with the new text, which may differ in its other lines.

## The records file (`<EntityName>.txt`)

### Writing

- One block per record. Records are separated by one blank line. The file ends with a line break after the last record's last line.
- A block has one line per tag, **in the stored tag file's order**, every tag every time:

  ```
  :tag.<TagName>.<Value>
  ```

  `:tag.` is literal. `<TagName>` is the tag name as it's spelled in the stored tag file. `<Value>` is the rest of the line.
- A tag with no value is written with nothing after the period: `:tag.Website.`
- A value can contain periods (URLs, emails). It can't contain a line break: any value with one is made one line first. Each line is trimmed, blank lines are dropped, and the lines are joined with one space (`FieldNormalizer.FlattenLineBreaks`).
- Appending (Import, Add) never rewrites existing records. It removes any line breaks at the end of the file, then adds one blank line and the new blocks.

### Reading

- Trailing whitespace on each line is ignored, so a value can't end with spaces.
- A blank line ends the current record. Several blank lines in a row are the same as one, and a record with no fields isn't created.
- A line that doesn't start with `:tag.`, or that has no period after the tag name, is skipped.
- The tag name is the text between `:tag.` and the next period. The value is everything after that period.
- If a block has the same tag twice, the last line wins.
- A line for a tag that isn't in the stored tag file is read, but isn't written back the next time the records are saved.

### Worked example (known-answer test)

This tag file, stored as `Contacts.md`:

```
## Contacts
### TAG Group
### TAG Name
### TAG Website
### TAG Notes
```

Import two records:

| Group | Name | Website | Notes |
|---|---|---|---|
| Friends | Zoë Example | https://www.example.com/a.b | |
| Work | Sam | | Line one *(line break)* Line two |

Then import (append) one more: Group `Work`, Name `Lee`, no Website, no Notes.

`Contacts.txt` is then exactly these UTF-8 bytes, with an LF at the end of every line, including the last:

```
:tag.Group.Friends
:tag.Name.Zoë Example
:tag.Website.https://www.example.com/a.b
:tag.Notes.

:tag.Group.Work
:tag.Name.Sam
:tag.Website.
:tag.Notes.Line one Line two

:tag.Group.Work
:tag.Name.Lee
:tag.Website.
:tag.Notes.
```

The file has 226 bytes. `ë` is the two bytes `C3 AB`, and it doesn't start with a byte-order mark. `FileFormatTests.RecordsFile_KnownAnswer` builds this file through `EntityStore` and compares it byte for byte, and checks that this document still shows it.
