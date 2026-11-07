// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretLead.Services;
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>Print (README §10: A4, two records per page) and the Markdown export.</summary>
[TestClass]
public sealed class PrintAndExportTests
{
    [TestMethod]
    public void Print_PutsTwoRecordsOnEachA4Sheet()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText());
        var records = new[]
        {
            Record(("Kind", "Dog"), ("Name", "Rex")),
            Record(("Kind", "Cat"), ("Name", "Tom")),
            Record(("Kind", "Cat"), ("Name", "Kit")),
        };

        var html = PrintHtmlFormatter.BuildDocument(tagFile, records);

        Assert.Contains("@page { size: A4;", html);
        Assert.AreEqual(2, CountOf(html, "<div class=\"sheet\">"));
        Assert.AreEqual(3, CountOf(html, "<div class=\"record\">"));
    }

    [TestMethod]
    public void Print_EncodesValuesAsText()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText());

        var html = PrintHtmlFormatter.BuildDocument(tagFile, [Record(("Kind", "Dog"), ("Name", "<b>Rex</b> & co"))]);

        Assert.Contains("&lt;b&gt;Rex&lt;/b&gt; &amp; co", html);
        Assert.DoesNotContain("<b>Rex</b>", html);
    }

    [TestMethod]
    public void Export_WritesTheEntityHeadingThenOneTagValueLinePerTag()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText());

        var text = RecordPrintFormatter.FormatRecord(tagFile, Record(("Kind", "Dog"), ("Name", "Rex")));

        Assert.AreEqual("## Pets\n\nKind: Dog\n\nName: Rex\n", text);
    }

    private static int CountOf(string text, string value)
    {
        var count = 0;
        for (var i = text.IndexOf(value, StringComparison.Ordinal); i >= 0; i = text.IndexOf(value, i + value.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }
}
