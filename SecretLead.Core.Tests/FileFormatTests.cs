// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text;
using SecretLead.Services;
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>
/// Known-answer tests that lock the two file formats in docs/file-format.md. Never change an
/// expected value here to make a test pass: a format change needs the owner's approval, a new
/// version of the spec, and a plan for files already on disk.
/// </summary>
[TestClass]
public sealed class FileFormatTests
{
    private const string ContactsTagFile =
        "## Contacts\n### TAG Group\n### TAG Name\n### TAG Website\n### TAG Notes\n";

    // The worked example in docs/file-format.md, exactly.
    private const string ContactsRecordsFile =
        ":tag.Group.Friends\n" +
        ":tag.Name.Zoë Example\n" +
        ":tag.Website.https://www.example.com/a.b\n" +
        ":tag.Notes.\n" +
        "\n" +
        ":tag.Group.Work\n" +
        ":tag.Name.Sam\n" +
        ":tag.Website.\n" +
        ":tag.Notes.Line one Line two\n" +
        "\n" +
        ":tag.Group.Work\n" +
        ":tag.Name.Lee\n" +
        ":tag.Website.\n" +
        ":tag.Notes.\n";

    [TestMethod]
    public void RecordsFile_KnownAnswer()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        store.SaveTagFile("Contacts", ContactsTagFile);

        store.AppendRecords("Contacts",
        [
            Record(("Group", "Friends"), ("Name", "Zoë Example"), ("Website", "https://www.example.com/a.b"), ("Notes", "")),
            Record(("Group", "Work"), ("Name", "Sam"), ("Notes", "Line one\nLine two")),
        ]);
        store.AppendRecords("Contacts", [Record(("Group", "Work"), ("Name", "Lee"))]);

        var bytes = File.ReadAllBytes(Path.Combine(store.RootFolder, "Contacts.txt"));
        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(ContactsRecordsFile), bytes);
        Assert.HasCount(226, bytes);
        Assert.AreNotEqual(0xEF, bytes[0], "The records file must not start with a byte-order mark.");
    }

    [TestMethod]
    public void TagFileCopy_IsStoredAsGiven_AsUtf8WithoutAByteOrderMark()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        var text = "# Café contacts\n" + ContactsTagFile;

        store.SaveTagFile("Contacts", text);

        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(text), File.ReadAllBytes(Path.Combine(store.RootFolder, "Contacts.md")));
    }

    [TestMethod]
    public void RecordsFile_ReadingRules()
    {
        var text =
            "stray text\r\n" +
            ":tag.Group.Work   \r\n" +   // trailing whitespace is dropped
            ":tag.NoPeriodHere\r\n" +    // no period after the tag name: skipped
            ":tag.name.First\r\n" +
            ":tag.NAME.Second\r\n" +     // same tag again, any case: the last one wins
            ":tag.Website.www.example.com\r\n" +
            "\r\n\r\n\r\n" +             // several blank lines are one separator
            ":tag.Group.Friends\r\n";

        var records = EntityRecordSerializer.Parse(text);

        Assert.HasCount(2, records);
        Assert.AreEqual("Work", records[0].GetValue("group"));
        Assert.AreEqual("Second", records[0].GetValue("Name"));
        Assert.AreEqual("www.example.com", records[0].GetValue("Website"));
        Assert.IsNull(records[0].GetValue("NoPeriodHere"));
        Assert.AreEqual("Friends", records[1].GetValue("Group"));
    }

    [TestMethod]
    public void TagFile_KnownAnswer()
    {
        var text =
            "# Title, version and date lines are ignored\n" +
            "Plain text is ignored\n" +
            "## Contacts\n" +
            "#### Four hashes is ignored\n" +
            "  ### TAG Indented is ignored\n" +
            "### tag lowercase TAG is ignored\n" +
            "### TAG   Group   \n" +
            "### TAG Name\n";

        var tagFile = TagFileParser.Parse(text);

        Assert.AreEqual("Contacts", tagFile.EntityName);
        CollectionAssert.AreEqual(new[] { "Group", "Name" }, tagFile.Tags.ToArray());
    }

    [TestMethod]
    public void Spec_ShowsTheKnownAnswerExample()
    {
        var spec = ReadRepoFile("docs", "file-format.md").Replace("\r\n", "\n");

        Assert.Contains("```\n" + ContactsTagFile + "```", spec);
        Assert.Contains("```\n" + ContactsRecordsFile + "```", spec);
        Assert.Contains("The file has 226 bytes.", spec);
    }
}
