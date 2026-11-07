// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text.RegularExpressions;
using SecretLead.Services;
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>The bundled Construction demo, seeded on first run (README §6, §11, §12).</summary>
[TestClass]
public sealed class ConstructionDemoTests
{
    [TestMethod]
    public void FirstRun_WithNoEntities_SeedsConstructionWithOneRecord()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);

        Assert.IsTrue(ConstructionDemo.SeedIfEmpty(store));

        CollectionAssert.AreEqual(new[] { "Construction" }, store.ListEntityNames().ToArray());
        Assert.IsTrue(File.Exists(Path.Combine(store.RootFolder, "Construction.md")));
        Assert.HasCount(12, store.GetTagFile("Construction").Tags);
        var records = store.GetRecords("Construction");
        Assert.HasCount(1, records);
        Assert.AreEqual("Plumber", records[0].GetValue("Category"));
        Assert.AreEqual("Example Plumbing & Heating", records[0].GetValue("Business Name"));
    }

    [TestMethod]
    public void StoreWithAnEntity_IsNotSeeded()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        store.SaveTagFile("Pets", PetsTagFileText());
        store.AppendRecords("Pets", [Record(("Kind", "Dog"), ("Name", "Rex"))]);

        Assert.IsFalse(ConstructionDemo.SeedIfEmpty(store));

        CollectionAssert.AreEqual(new[] { "Pets" }, store.ListEntityNames().ToArray());
        Assert.IsFalse(File.Exists(Path.Combine(store.RootFolder, "Construction.md")));
    }

    [TestMethod]
    public void SecondRun_DoesNotSeedAgain()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);

        ConstructionDemo.SeedIfEmpty(store);
        Assert.IsFalse(ConstructionDemo.SeedIfEmpty(store));

        Assert.HasCount(1, store.GetRecords("Construction"));
    }

    [TestMethod]
    public void BuiltInFiles_AreTheRepositoryFiles()
    {
        Assert.AreEqual(ReadRepoFile("SecretLead.Core", "Seed", "construction.md"), ConstructionDemo.TagFileText);
        Assert.AreEqual(ReadRepoFile("SecretLead.Core", "Seed", "construction.txt"), ConstructionDemo.RecordsText);
    }

    [TestMethod]
    public void DemoRecord_HasEveryTag_InTheTagFileOrder()
    {
        var tags = TagFileParser.Parse(ConstructionDemo.TagFileText).Tags;

        var lineTags = ConstructionDemo.RecordsText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line[":tag.".Length..line.IndexOf('.', ":tag.".Length)])
            .ToArray();

        CollectionAssert.AreEqual(tags.ToArray(), lineTags);
    }

    // README §2, §11: no real-world data ships with the app. The phone is in 555-0100 to 555-0199,
    // the range reserved for fictional use; the email and website are on example.com.
    [TestMethod]
    public void DemoRecord_IsFictional()
    {
        var record = EntityRecordSerializer.Parse(ConstructionDemo.RecordsText).Single();
        string Value(string tag) => record.GetValue(tag) ?? string.Empty;

        Assert.IsTrue(Regex.IsMatch(Value("Phone"), @"^\(\d{3}\) 555-01\d\d$"), Value("Phone"));
        Assert.IsTrue(Value("Email").EndsWith("@example.com", StringComparison.Ordinal), Value("Email"));
        Assert.IsTrue(Value("Website").EndsWith("example.com", StringComparison.Ordinal), Value("Website"));
        Assert.IsTrue(Value("License ID").StartsWith("DEMO-", StringComparison.Ordinal), Value("License ID"));
    }
}
