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
