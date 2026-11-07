// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretLead.Services;

namespace SecretLead.Core.Tests;

/// <summary>Normalization by tag-name substring (README §8).</summary>
[TestClass]
public sealed class FieldNormalizerTests
{
    [TestMethod]
    [DataRow("631.555.1212", "(631) 555-1212")]
    [DataRow("(631) 555 1212", "(631) 555-1212")]
    [DataRow("1-631-555-1212", "+1 (631) 555-1212")]
    [DataRow("555 1212", "555-1212")]
    [DataRow("ext 12", "ext 12")]
    [DataRow("", "")]
    public void Phone_IsFormattedByDigitCount(string raw, string expected) =>
        Assert.AreEqual(expected, FieldNormalizer.Normalize("Phone", raw));

    [TestMethod]
    public void PhoneRule_MatchesTheTagNameIgnoringCase() =>
        Assert.AreEqual("(631) 555-1212", FieldNormalizer.Normalize("OWNER PHONE NUMBER", "6315551212"));

    [TestMethod]
    [DataRow("2024-03-04", "03/04/2024")]
    [DataRow("3/4/2024", "03/04/2024")]
    [DataRow("03/04/2024", "03/04/2024")]
    [DataRow("not a date", "not a date")]
    [DataRow("", "")]
    public void Date_IsReformattedToMonthDayYear(string raw, string expected) =>
        Assert.AreEqual(expected, FieldNormalizer.Normalize("Date Issued", raw));

    [TestMethod]
    public void Email_IsStoredAsIs() =>
        Assert.AreEqual(" A@Example.COM ", FieldNormalizer.Normalize("Email", " A@Example.COM "));

    [TestMethod]
    public void OtherTags_AreStoredAsIs() =>
        Assert.AreEqual(" 12 Main St ", FieldNormalizer.Normalize("Address", " 12 Main St "));

    // Bug 1 (evaluation 2026-11-07): records are stored one line per field, so a value
    // can't keep a line break. Each line is trimmed and the lines are joined with one space.
    [TestMethod]
    [DataRow("12 Main St\nSuite 4", "12 Main St Suite 4")]
    [DataRow("1 Elm\r\n\r\nRear", "1 Elm Rear")]
    [DataRow("a\rb", "a b")]
    [DataRow("Acme \n  Plumbing\n", "Acme Plumbing")]
    public void LineBreaksInAnyValue_BecomeOneSpace(string raw, string expected) =>
        Assert.AreEqual(expected, FieldNormalizer.Normalize("Address", raw));

    [TestMethod]
    public void LineBreaksInAPhoneOrDate_AreRemovedBeforeFormatting()
    {
        Assert.AreEqual("(631) 555-1212", FieldNormalizer.Normalize("Phone", "631\n555-1212"));
        Assert.AreEqual("03/04/2024", FieldNormalizer.Normalize("Date Issued", "2024-03-04\r\n"));
    }
}
