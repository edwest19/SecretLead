// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretLead.Services;
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>The tag file grammar (README §4): one <c>##</c> entity name, then <c>### TAG</c> names in order.</summary>
[TestClass]
public sealed class TagFileParserTests
{
    [TestMethod]
    public void BundledConstructionTagFile_HasTheTwelveTagsInOrder()
    {
        var tagFile = TagFileParser.Parse(ConstructionTagFileText());

        Assert.AreEqual("Construction", tagFile.EntityName);
        CollectionAssert.AreEqual(
            new[] { "Category", "Business Name", "License ID", "Last Name", "First Name", "Address", "Phone", "Email", "Status", "Date Issued", "Date Expire", "Website" },
            tagFile.Tags.ToArray());
    }

    [TestMethod]
    public void FirstTagIsCategory_SecondTagIsNickname()
    {
        var tagFile = TagFileParser.Parse(PetsTagFileText("Owner"));

        Assert.AreEqual("Kind", tagFile.CategoryTag);
        Assert.AreEqual("Name", tagFile.NicknameTag);
    }

    [TestMethod]
    public void TitleAndDescriptionLines_AreParsedPast_AndWindowsLineEndingsWork()
    {
        var text = "# My list\r\nVersion 1\r\n\r\n## Pets\r\n### TAG Kind\r\n### TAG Name\r\n";

        var tagFile = TagFileParser.Parse(text);

        Assert.AreEqual("Pets", tagFile.EntityName);
        CollectionAssert.AreEqual(new[] { "Kind", "Name" }, tagFile.Tags.ToArray());
    }

    [TestMethod]
    public void NoEntityName_IsRefused() =>
        Assert.ThrowsExactly<TagFileFormatException>(() => TagFileParser.Parse("# Title\n### TAG Kind\n### TAG Name\n"));

    [TestMethod]
    public void TwoEntityNames_AreRefused() =>
        Assert.ThrowsExactly<TagFileFormatException>(() => TagFileParser.Parse("## Pets\n## Cars\n### TAG Kind\n### TAG Name\n"));

    [TestMethod]
    public void FewerThanTwoTags_IsRefused() =>
        Assert.ThrowsExactly<TagFileFormatException>(() => TagFileParser.Parse("## Pets\n### TAG Kind\n"));

    // Bug 3 (evaluation 2026-11-07): records are stored as ":tag.TagName.Value", split at the first
    // period, so a tag name with a period ("Lic. No") came back under the wrong name.
    [TestMethod]
    public void TagNameWithAPeriod_IsRefused_AndNamed()
    {
        var error = Assert.ThrowsExactly<TagFileFormatException>(
            () => TagFileParser.Parse("## Licenses\n### TAG Category\n### TAG Lic. No\n"));

        Assert.Contains("'Lic. No'", error.Message);
    }

    // Bug 4 (evaluation 2026-11-07): the entity name becomes <EntityName>.md / .txt in AppData, so a
    // name with path parts wrote files outside AppData, and a name Windows can't use as a file name threw.
    [TestMethod]
    [DataRow("../../Escaped")]
    [DataRow("..\\Escaped")]
    [DataRow("a/b")]
    [DataRow("a\\b")]
    [DataRow("C:Escaped")]
    [DataRow("what?")]
    [DataRow("a*b")]
    [DataRow("a\"b")]
    [DataRow("a<b>")]
    [DataRow("a|b")]
    [DataRow("a\tb")]
    [DataRow(".")]
    [DataRow("..")]
    [DataRow("Name.")]
    [DataRow("CON")]
    [DataRow("nul")]
    [DataRow("Com1")]
    [DataRow("LPT9")]
    [DataRow("aux.backup")]
    public void EntityNameThatIsntAWindowsFileName_IsRefused(string entityName)
    {
        var error = Assert.ThrowsExactly<TagFileFormatException>(
            () => TagFileParser.Parse($"## {entityName}\n### TAG Kind\n### TAG Name\n"));

        Assert.Contains(entityName, error.Message);
    }

    [TestMethod]
    [DataRow("Construction")]
    [DataRow("Home Improvement")]
    [DataRow("Plumber-2026")]
    [DataRow("A_B (old)")]
    [DataRow("Café")]
    [DataRow("Lic.Holders")]
    [DataRow("Console")]
    [DataRow("COM10")]
    public void OrdinaryEntityNames_AreAccepted(string entityName) =>
        Assert.AreEqual(entityName, TagFileParser.Parse($"## {entityName}\n### TAG Kind\n### TAG Name\n").EntityName);
}
