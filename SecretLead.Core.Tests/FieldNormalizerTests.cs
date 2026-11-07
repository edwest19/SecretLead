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
}
