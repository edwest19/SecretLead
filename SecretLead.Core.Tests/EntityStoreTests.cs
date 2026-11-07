// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>The AppData record store (README §5, §6): one <c>.md</c> + <c>.txt</c> pair per entity name.</summary>
[TestClass]
public sealed class EntityStoreTests
{
    [TestMethod]
    public void EmptyStore_HasNoEntities_AndNoRecords()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);

        Assert.IsEmpty(store.ListEntityNames());
        Assert.IsEmpty(store.GetRecords("Pets"));
        Assert.IsFalse(store.EntityExists("Pets"));
    }

    [TestMethod]
    public void FirstImport_WritesTheTagFileAndRecordsFiles_NamedAfterTheEntity()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);

        store.SaveTagFile("Pets", PetsTagFileText());
        store.AppendRecords("Pets", [Record(("Kind", "Dog"), ("Name", "Rex"))]);

        Assert.IsTrue(File.Exists(Path.Combine(store.RootFolder, "Pets.md")));
        Assert.IsTrue(File.Exists(Path.Combine(store.RootFolder, "Pets.txt")));
        Assert.IsTrue(store.EntityExists("Pets"));
        Assert.AreEqual("Pets", store.GetTagFile("Pets").EntityName);
    }

    [TestMethod]
    public void SecondImport_AppendsAndKeepsEarlierRecords()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        store.SaveTagFile("Pets", PetsTagFileText());

        store.AppendRecords("Pets", [Record(("Kind", "Dog"), ("Name", "Rex"))]);
        store.AppendRecords("Pets", [Record(("Kind", "Cat"), ("Name", "Tom")), Record(("Kind", "Cat"), ("Name", "Kit"))]);

        var names = store.GetRecords("Pets").Select(r => r.GetValue("Name")).ToArray();
        CollectionAssert.AreEqual(new[] { "Rex", "Tom", "Kit" }, names);
    }

    [TestMethod]
    public void AppendingWithoutATagFile_IsRefused()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);

        Assert.ThrowsExactly<FileNotFoundException>(() => store.AppendRecords("Pets", [Record(("Kind", "Dog"), ("Name", "Rex"))]));
    }

    [TestMethod]
    public void SaveRecords_ReplacesTheWholeList()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        store.SaveTagFile("Pets", PetsTagFileText());
        store.AppendRecords("Pets", [Record(("Kind", "Dog"), ("Name", "Rex")), Record(("Kind", "Cat"), ("Name", "Tom"))]);

        var remaining = store.GetRecords("Pets").Where(r => r.GetValue("Name") != "Rex").ToList();
        store.SaveRecords("Pets", remaining);

        var names = store.GetRecords("Pets").Select(r => r.GetValue("Name")).ToArray();
        CollectionAssert.AreEqual(new[] { "Tom" }, names);
    }

    // Bug 1 (evaluation 2026-11-07), for values typed or pasted by hand: a line break in a value
    // split the stored record, and everything after it was lost on reading back.
    [TestMethod]
    public void ValueWithLineBreaks_IsStoredOnOneLine_AndReadsBackWhole()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        store.SaveTagFile("Pets", PetsTagFileText("Address"));

        store.AppendRecords("Pets",
        [
            Record(("Kind", "Dog"), ("Name", "Rex"), ("Address", "1 Elm\r\n\r\nRear")),
            Record(("Kind", "Cat"), ("Name", "Tom"), ("Address", "2 Oak")),
        ]);

        var records = store.GetRecords("Pets");
        Assert.HasCount(2, records);
        Assert.AreEqual("1 Elm Rear", records[0].GetValue("Address"));
        Assert.AreEqual("Tom", records[1].GetValue("Name"));
    }

    // Bug 2 (evaluation 2026-11-07): importing a tag file with fewer tags under an existing entity
    // name replaced the stored tag file, and the next edit rewrote every record without the missing tags.
    [TestMethod]
    public void ReimportWithDifferentTags_IsRefused_AndStoredRecordsKeepEveryField()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        store.SaveTagFile("Construction", "## Construction\n### TAG Category\n### TAG Business Name\n### TAG Phone\n### TAG Email\n");
        store.AppendRecords("Construction",
            [Record(("Category", "Plumber"), ("Business Name", "Acme"), ("Phone", "(631) 555-1212"), ("Email", "a@example.com"))]);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => store.SaveTagFile("Construction", "## Construction\n### TAG Category\n### TAG Business Name\n"));

        // The Browse screen's Edit saves every record with the stored tag file.
        var records = store.GetRecords("Construction").ToList();
        records[0].SetValue("Business Name", "Acme Plumbing");
        store.SaveRecords("Construction", records);

        var reread = store.GetRecords("Construction")[0];
        Assert.AreEqual("Acme Plumbing", reread.GetValue("Business Name"));
        Assert.AreEqual("(631) 555-1212", reread.GetValue("Phone"));
        Assert.AreEqual("a@example.com", reread.GetValue("Email"));
    }

    [TestMethod]
    public void ReimportWithTheSameTagsInADifferentOrder_IsRefused()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        store.SaveTagFile("Pets", PetsTagFileText("Owner"));

        // The first two tags decide Category and Nickname, so the order must stay the same.
        Assert.ThrowsExactly<InvalidOperationException>(
            () => store.SaveTagFile("Pets", "## Pets\n### TAG Name\n### TAG Kind\n### TAG Owner\n"));
    }

    [TestMethod]
    public void ReimportWithTheSameTags_DifferingOnlyInCase_IsAllowed()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        store.SaveTagFile("Pets", PetsTagFileText("Owner"));

        store.SaveTagFile("Pets", "# Pets, version 2\n## Pets\n### TAG kind\n### TAG NAME\n### TAG Owner\n");

        CollectionAssert.AreEqual(new[] { "kind", "NAME", "Owner" }, store.GetTagFile("Pets").Tags.ToArray());
    }

    // Bug 4 (evaluation 2026-11-07): the store is the last place an entity name becomes a path.
    [TestMethod]
    public void EntityNameOutsideTheStoreFolder_IsRefused_AndNothingIsWritten()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);

        Assert.ThrowsExactly<ArgumentException>(() => store.SaveTagFile("../Escaped", "## Escaped\n### TAG Kind\n### TAG Name\n"));

        Assert.IsFalse(File.Exists(folder.PathOf("Escaped.md")));
    }

    [TestMethod]
    public void Entities_AreFoundByTheirTxtFiles_SortedByName()
    {
        using var folder = new TempFolder();
        var store = StoreIn(folder);
        foreach (var name in new[] { "zoo", "Birds", "apples" })
        {
            store.SaveTagFile(name, $"## {name}\n### TAG A\n### TAG B\n");
            store.AppendRecords(name, [Record(("A", "1"), ("B", "2"))]);
        }

        // A tag file with no records file isn't an entity yet.
        store.SaveTagFile("Pending", PetsTagFileText());

        CollectionAssert.AreEqual(new[] { "apples", "Birds", "zoo" }, store.ListEntityNames().ToArray());
    }
}
