// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretLead.Services;

namespace SecretLead.Core.Tests;

/// <summary>The hand-rolled CSV reader: header row, quoted fields, line endings.</summary>
[TestClass]
public sealed class CsvParserTests
{
    [TestMethod]
    public void HeaderRowAndDataRows_AreRead_AndColumnsMatchIgnoringCase()
    {
        var table = CsvParser.Parse("Name,Phone\nAcme,5551212\nBeta,5550000\n");

        CollectionAssert.AreEqual(new[] { "Name", "Phone" }, table.Headers.ToArray());
        Assert.HasCount(2, table.Rows);
        Assert.AreEqual("Acme", table.Rows[0]["name"]);
        Assert.AreEqual("5550000", table.Rows[1]["PHONE"]);
    }

    [TestMethod]
    public void QuotedFields_KeepCommasAndDoubledQuotes()
    {
        var table = CsvParser.Parse("A,B\n\"x, y\",\"say \"\"hi\"\"\"\n");

        Assert.AreEqual("x, y", table.Rows[0]["A"]);
        Assert.AreEqual("say \"hi\"", table.Rows[0]["B"]);
    }

    [TestMethod]
    public void QuotedField_CanSpanLines()
    {
        var table = CsvParser.Parse("A,B\n\"12 Main St\nSuite 4\",x\n");

        Assert.AreEqual("12 Main St\nSuite 4", table.Rows[0]["A"]);
        Assert.AreEqual("x", table.Rows[0]["B"]);
    }

    [TestMethod]
    public void WindowsLineEndings_Work()
    {
        var table = CsvParser.Parse("A,B\r\n1,2\r\n3,4\r\n");

        Assert.HasCount(2, table.Rows);
        Assert.AreEqual("4", table.Rows[1]["B"]);
    }

    [TestMethod]
    public void BlankTrailingLine_IsNotARecord()
    {
        var table = CsvParser.Parse("A,B\n1,2\n\n");

        Assert.HasCount(1, table.Rows);
    }

    [TestMethod]
    public void ShortRow_GetsEmptyValuesForMissingColumns()
    {
        var table = CsvParser.Parse("A,B,C\n1\n");

        Assert.AreEqual("1", table.Rows[0]["A"]);
        Assert.AreEqual(string.Empty, table.Rows[0]["B"]);
        Assert.AreEqual(string.Empty, table.Rows[0]["C"]);
    }

    [TestMethod]
    public void EmptyFile_IsRefused() =>
        Assert.ThrowsExactly<CsvFormatException>(() => CsvParser.Parse(string.Empty));
}
