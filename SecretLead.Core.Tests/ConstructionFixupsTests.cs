// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretLead.Services;
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>The Business Name fallback applies to the Construction tag file only (README §8, §12).</summary>
[TestClass]
public sealed class ConstructionFixupsTests
{
    private static Dictionary<string, string> Values(string businessName) => new(StringComparer.OrdinalIgnoreCase)
    {
        ["Business Name"] = businessName,
        ["Last Name"] = "Doe",
        ["First Name"] = "Jane",
    };

    [TestMethod]
    public void Construction_BlankBusinessName_BecomesLastCommaFirst()
    {
        var values = Values("  ");

        ConstructionFixups.Apply(TagFileParser.Parse(ConstructionTagFileText()), values);

        Assert.AreEqual("Doe, Jane", values["Business Name"]);
    }

    [TestMethod]
    public void Construction_BusinessNameGiven_IsKept()
    {
        var values = Values("Acme Plumbing");

        ConstructionFixups.Apply(TagFileParser.Parse(ConstructionTagFileText()), values);

        Assert.AreEqual("Acme Plumbing", values["Business Name"]);
    }

    [TestMethod]
    public void OtherEntity_WithTheSameTags_IsLeftAlone()
    {
        var tagFile = TagFileParser.Parse("## Contacts\n### TAG Business Name\n### TAG Last Name\n### TAG First Name\n");
        var values = Values(string.Empty);

        ConstructionFixups.Apply(tagFile, values);

        Assert.AreEqual(string.Empty, values["Business Name"]);
    }
}
