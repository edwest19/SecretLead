// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretLead.Services;
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>Import validation (README §9) and CSV-to-record conversion (README §4, §8).</summary>
[TestClass]
public sealed class CsvImporterTests
{
    [TestMethod]
    public void Validate_ColumnsInAnyOrderAndCase_IsValid()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText("Owner"));

        var result = CsvImporter.Validate(tagFile, ["OWNER", "name", "Kind"]);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Messages);
    }

    [TestMethod]
    public void Validate_NamesEveryMismatch_InBothDirections()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText("Owner"));

        var result = CsvImporter.Validate(tagFile, ["Kind", "Name", "Breed"]);

        Assert.IsFalse(result.IsValid);
        CollectionAssert.AreEqual(new[] { "Owner" }, result.MissingColumns.ToArray());
        CollectionAssert.AreEqual(new[] { "Breed" }, result.UnmatchedColumns.ToArray());
        CollectionAssert.AreEqual(
            new[] { "Tag 'Owner' has no matching CSV column.", "CSV column 'Breed' has no matching tag." },
            result.Messages.ToArray());
    }

    [TestMethod]
    public void Import_SemicolonCategory_BecomesOneRecordPerValue()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText());
        var table = CsvParser.Parse("Kind,Name\n\"Dog; Cat ;\",Rex\n");

        var records = CsvImporter.Import(tagFile, table);

        Assert.HasCount(2, records);
        Assert.AreEqual("Dog", records[0].GetValue("Kind"));
        Assert.AreEqual("Cat", records[1].GetValue("Kind"));
        Assert.AreEqual("Rex", records[0].GetValue("Name"));
        Assert.AreEqual("Rex", records[1].GetValue("Name"));
    }

    [TestMethod]
    public void Import_BlankCategory_StillGivesOneRecord()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText());
        var table = CsvParser.Parse("Kind,Name\n,Rex\n");

        var records = CsvImporter.Import(tagFile, table);

        Assert.HasCount(1, records);
        Assert.AreEqual("Rex", records[0].GetValue("Name"));
    }

    [TestMethod]
    public void Import_NormalizesPhoneAndDateTags()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText("Owner Phone", "Date Adopted"));
        var table = CsvParser.Parse("Kind,Name,Owner Phone,Date Adopted\nDog,Rex,631.555.1212,2024-03-04\n");

        var record = CsvImporter.Import(tagFile, table)[0];

        Assert.AreEqual("(631) 555-1212", record.GetValue("Owner Phone"));
        Assert.AreEqual("03/04/2024", record.GetValue("Date Adopted"));
    }

    // Bug 1 (evaluation 2026-11-07): a quoted CSV value with a line break was stored as-is, and
    // everything after the first line was lost when the records were read back.
    [TestMethod]
    public void Import_LineBreaksInAValue_BecomeOneSpace()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText("Address"));
        var table = CsvParser.Parse("Kind,Name,Address\nDog,Rex,\"12 Main St\r\nSuite 4\"\n");

        var record = CsvImporter.Import(tagFile, table)[0];

        Assert.AreEqual("12 Main St Suite 4", record.GetValue("Address"));
    }

    [TestMethod]
    public void Import_WhenColumnsDontMatch_IsRefused()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText("Owner"));
        var table = CsvParser.Parse("Kind,Name\nDog,Rex\n");

        Assert.ThrowsExactly<InvalidOperationException>(() => CsvImporter.Import(tagFile, table));
    }
}
