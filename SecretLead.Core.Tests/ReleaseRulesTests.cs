// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic), following
// ReleaseRulesTests in edwest19/SecretLetter.
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static SecretLead.Core.Tests.TestHelper;

namespace SecretLead.Core.Tests;

/// <summary>
/// The release settings every app in the family shares: the signing publisher, the version in
/// Directory.Build.props and Package.appxmanifest, and x64 only.
/// </summary>
[TestClass]
public sealed class ReleaseRulesTests
{
    // The exact name Azure Artifact Signing (account edwest19, profile CodeSigning-Apps) signs as.
    // Case-sensitive: Windows refuses to sign an MSIX whose Publisher differs from the certificate subject.
    private const string SigningPublisher = "CN=Edwin West, O=Edwin West, L=Huntington, S=ny, C=US";

    private static XElement ManifestIdentity() =>
        XDocument.Parse(ReadRepoFile("SecretLead.UI", "Package.appxmanifest"))
            .Descendants().Single(e => e.Name.LocalName == "Identity");

    private static string ProjectVersion() =>
        XDocument.Parse(ReadRepoFile("Directory.Build.props")).Descendants("Version").Single().Value;

    [TestMethod]
    public void ManifestPublisher_IsTheNameReleasesAreSignedAs() =>
        Assert.AreEqual(SigningPublisher, (string?)ManifestIdentity().Attribute("Publisher"));

    [TestMethod]
    public void ManifestVersion_IsTheProjectVersionPlusZero()
    {
        var version = ProjectVersion();

        Assert.IsTrue(Regex.IsMatch(version, @"^\d+\.\d+\.\d+$"), $"Directory.Build.props Version '{version}' must be X.Y.Z.");
        Assert.AreEqual(version + ".0", (string?)ManifestIdentity().Attribute("Version"));
    }

    [TestMethod]
    public void ManifestPackageName_StaysTheSame()
    {
        // scripts/Reset-SecretLeadAppData.ps1 finds SecretLead's package by this name (README §16).
        const string packageName = "D3B25905-54E0-4737-BC59-F1CFA9E25E2B";

        Assert.AreEqual(packageName, (string?)ManifestIdentity().Attribute("Name"));
        Assert.Contains($"$packageName = '{packageName}'", ReadRepoFile("scripts", "Reset-SecretLeadAppData.ps1"));
    }

    [TestMethod]
    public void App_IsBuiltForX64Only()
    {
        var project = XDocument.Parse(ReadRepoFile("SecretLead.UI", "SecretLead.UI.csproj"));

        Assert.AreEqual("x64", project.Descendants("Platforms").Single().Value);
    }
}
