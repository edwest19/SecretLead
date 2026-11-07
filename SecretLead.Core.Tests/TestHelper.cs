// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic), following
// TestHelper in edwest19/SecretLetter.
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretLead.Models;
using SecretLead.Services;

namespace SecretLead.Core.Tests;

/// <summary>Shared test values and helpers.</summary>
internal static class TestHelper
{
    /// <summary>Finds the repository root (the folder containing SecretLead.slnx) from the test's output folder.</summary>
    public static string RepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SecretLead.slnx")))
            {
                return dir.FullName;
            }
        }

        throw new InvalidOperationException("Could not find SecretLead.slnx above " + AppContext.BaseDirectory);
    }

    /// <summary>Reads a file in the repository, given its path parts from the root.</summary>
    public static string ReadRepoFile(params string[] path) =>
        File.ReadAllText(Path.Combine([RepoRoot(), .. path]));

    /// <summary>The bundled Construction tag file, as it is in the repository (built into SecretLead.Core).</summary>
    public static string ConstructionTagFileText() => ReadRepoFile("SecretLead.Core", "Seed", "construction.md");

    /// <summary>A small two-tag-plus tag file for tests that don't need Construction's twelve tags.</summary>
    public static string PetsTagFileText(params string[] extraTags)
    {
        var lines = new List<string> { "# Pets (test tag file)", "", "## Pets", "", "### TAG Kind", "", "### TAG Name", "" };
        foreach (var tag in extraTags)
        {
            lines.Add($"### TAG {tag}");
            lines.Add("");
        }

        return string.Join("\n", lines);
    }

    /// <summary>A record with the given tag/value pairs.</summary>
    public static EntityRecord Record(params (string Tag, string Value)[] fields)
    {
        var record = new EntityRecord();
        foreach (var (tag, value) in fields)
        {
            record.SetValue(tag, value);
        }

        return record;
    }

    /// <summary>An EntityStore pointed at a test's own temporary folder instead of AppData.</summary>
    public static EntityStore StoreIn(TempFolder folder) => new(folder.PathOf("AppData"));
}

/// <summary>A private temporary folder for one test, deleted afterwards.</summary>
internal sealed class TempFolder : IDisposable
{
    public TempFolder()
    {
        FullPath = Path.Combine(Path.GetTempPath(), "secretlead-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FullPath);
    }

    public string FullPath { get; }

    /// <summary>The path of a file or folder named <paramref name="name"/> in this folder.</summary>
    public string PathOf(string name) => Path.Combine(FullPath, name);

    public void Dispose()
    {
        try
        {
            Directory.Delete(FullPath, recursive: true);
        }
        catch (IOException)
        {
            // A leftover temp folder is harmless.
        }
    }
}
