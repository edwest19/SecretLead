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
}
