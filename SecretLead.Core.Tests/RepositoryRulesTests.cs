// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic), following
// RepositoryRulesTests in edwest19/SecretLetter and edwest19/SecretKey.
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>
/// Enforces two of the README's non-negotiables (§2, §12, §15): Microsoft packages only, and
/// no network API anywhere in the app's code. If one of these fails, a change broke a rule;
/// fix the change, don't loosen the test without the owner's approval.
/// </summary>
[TestClass]
public sealed class RepositoryRulesTests
{
    // Every NuGet package the repo may use. All are published by Microsoft.
    private static readonly string[] AllowedPackages =
    [
        "Microsoft.WindowsAppSDK",                // WinUI 3 runtime for SecretLead.UI
        "Microsoft.Windows.SDK.BuildTools",       // Windows SDK build tools for SecretLead.UI (MSIX packaging)
        "Microsoft.Windows.SDK.BuildTools.WinApp", // `dotnet run` with package identity for SecretLead.UI
        "MSTest",                                 // Microsoft's test framework (tests only)
    ];

    // The app's code. The test project is left out because it names these APIs itself.
    private static readonly string[] AppProjectFolders = ["SecretLead.Core", "SecretLead.UI"];

    [TestMethod]
    public void OnlyAllowedMicrosoftPackagesAreReferenced()
    {
        var root = RepoRoot();
        var used = new List<string>();

        foreach (var file in BuildFiles(root))
        {
            foreach (var reference in XDocument.Load(file).Descendants().Where(e => e.Name.LocalName == "PackageReference"))
            {
                var id = (string?)reference.Attribute("Include") ?? (string?)reference.Attribute("Update") ?? "";
                used.Add($"{Path.GetRelativePath(root, file)}: {id}");
                Assert.Contains(id, AllowedPackages, $"Package '{id}' in {Path.GetRelativePath(root, file)} is not on the allowed list.");
            }
        }

        Assert.IsNotEmpty(used, "Expected to find the project's package references.");
    }

    [TestMethod]
    public void AppCodeHasNoNetworkAccess()
    {
        // System.Net on its own is allowed: PrintHtmlFormatter uses System.Net.WebUtility.HtmlEncode,
        // which only encodes text. Its namespaces that can reach a network are not.
        var network = new Regex(
            @"\bSystem\.Net\.(Http|Sockets|WebSockets|NetworkInformation|Mail|Quic|Security)\b" +
            @"|\bHttpClient\b|\bWebClient\b|\bWebRequest\b|\bSockets?\b|\bTcpClient\b|\bUdpClient\b|\bWebSocket\b" +
            @"|\bWebView2?\b|\bWindows\.Web\b|\bWindows\.Networking\b");
        var root = RepoRoot();
        var filesChecked = 0;

        foreach (var folder in AppProjectFolders)
        {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(root, folder), "*.*", SearchOption.AllDirectories)
                         .Where(f => f.EndsWith(".cs", StringComparison.Ordinal) || f.EndsWith(".xaml", StringComparison.Ordinal))
                         .Where(f => !IsBuildOutput(root, f)))
            {
                filesChecked++;
                var lines = File.ReadAllLines(file);
                for (var i = 0; i < lines.Length; i++)
                {
                    Assert.IsFalse(network.IsMatch(lines[i]),
                        $"Network API found in {Path.GetRelativePath(root, file)} line {i + 1}: {lines[i].Trim()}");
                }
            }
        }

        Assert.IsGreaterThan(0, filesChecked, "Expected to scan the app's source files.");
    }

    private static IEnumerable<string> BuildFiles(string root) =>
        Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".csproj", StringComparison.Ordinal) || f.EndsWith(".props", StringComparison.Ordinal) || f.EndsWith(".targets", StringComparison.Ordinal))
            .Where(f => !IsBuildOutput(root, f));

    private static bool IsBuildOutput(string root, string file)
    {
        var parts = Path.GetRelativePath(root, file).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(p => p is "bin" or "obj" or ".git" or ".vs" or "TestResults");
    }
}
