// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================

namespace SecretLead.Services;

/// <summary>
/// The bundled Construction demo (README §6, §11): the Construction tag file and one fictional
/// record, built into this library from <c>Seed/construction.md</c> and <c>Seed/construction.txt</c>.
/// On first run, when AppData has no entities yet, they're copied in as <c>Construction.md</c> and
/// <c>Construction.txt</c>, so there's something to browse without importing first.
/// </summary>
public static class ConstructionDemo
{
    private const string TagFileResource = "SecretLead.Seed.construction.md";
    private const string RecordsResource = "SecretLead.Seed.construction.txt";

    /// <summary>The bundled tag file's text.</summary>
    public static string TagFileText => ReadResource(TagFileResource);

    /// <summary>The bundled sample record, in the <c>:tag.TagName.Value</c> record format.</summary>
    public static string RecordsText => ReadResource(RecordsResource);

    /// <summary>
    /// Copies the demo into <paramref name="store"/> when it has no entities yet.
    /// Returns true if it did, false if the store already had an entity.
    /// </summary>
    public static bool SeedIfEmpty(EntityStore store)
    {
        if (store.ListEntityNames().Count > 0)
        {
            return false;
        }

        var tagFileText = TagFileText;
        var entityName = TagFileParser.Parse(tagFileText).EntityName;

        // Tag file first: the store writes records with its tags (README §3).
        store.SaveTagFile(entityName, tagFileText);
        store.AppendRecords(entityName, EntityRecordSerializer.Parse(RecordsText));
        return true;
    }

    private static string ReadResource(string name)
    {
        using var stream = typeof(ConstructionDemo).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"SecretLead is missing its built-in file '{name}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
