// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretLead.Services;
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>The <c>:tag.TagName.Value</c> record format used by every <c>&lt;EntityName&gt;.txt</c>.</summary>
[TestClass]
public sealed class EntityRecordSerializerTests
{
    private static readonly string[] Tags = ["Kind", "Name", "Website"];

    [TestMethod]
    public void Serialize_WritesOneLinePerTag_InTagFileOrder_WithABlankLineBetweenRecords()
    {
        var records = new[]
        {
            Record(("Name", "Rex"), ("Kind", "Dog"), ("Website", "www.example.com")),
            Record(("Kind", "Cat"), ("Name", "Tom")),
        };

        var text = EntityRecordSerializer.Serialize(records, Tags);

        Assert.AreEqual(
            ":tag.Kind.Dog\n:tag.Name.Rex\n:tag.Website.www.example.com\n" +
            "\n" +
            ":tag.Kind.Cat\n:tag.Name.Tom\n:tag.Website.\n",
            text);
    }

    [TestMethod]
    public void Parse_ReadsBackWhatSerializeWrote_IncludingValuesWithPeriods()
    {
        var original = new[]
        {
            Record(("Kind", "Dog"), ("Name", "Rex"), ("Website", "https://www.example.com/a.b")),
            Record(("Kind", "Cat"), ("Name", "Tom"), ("Website", "")),
        };

        var records = EntityRecordSerializer.Parse(EntityRecordSerializer.Serialize(original, Tags));

        Assert.HasCount(2, records);
        Assert.AreEqual("https://www.example.com/a.b", records[0].GetValue("Website"));
        Assert.AreEqual("Tom", records[1].GetValue("Name"));
        Assert.AreEqual(string.Empty, records[1].GetValue("Website"));
    }

    [TestMethod]
    public void Parse_SkipsLinesThatAreNotTagLines_AndWindowsLineEndingsWork()
    {
        var records = EntityRecordSerializer.Parse("stray text\r\n:tag.Kind.Dog\r\n:tag.Name.Rex\r\n\r\n\r\n:tag.Kind.Cat\r\n");

        Assert.HasCount(2, records);
        Assert.AreEqual("Rex", records[0].GetValue("Name"));
        Assert.AreEqual("Cat", records[1].GetValue("Kind"));
    }
}
