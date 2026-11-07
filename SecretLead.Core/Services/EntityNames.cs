// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretLead is a project built with AI-generated code; see README.md.
// ============================================================================

namespace SecretLead.Services;

/// <summary>
/// Checks that an entity name can be the file name of <c>&lt;EntityName&gt;.md</c> and
/// <c>&lt;EntityName&gt;.txt</c> in the AppData store (README §4, §5). The rules are Windows'
/// own file naming rules (https://learn.microsoft.com/windows/win32/fileio/naming-a-file), so a
/// name with path parts can't reach outside the store and Windows never refuses the file.
/// </summary>
public static class EntityNames
{
    private static readonly char[] ReservedCharacters = ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];

    // Names Windows reserves for devices, also when followed by an extension ("NUL.txt" is NUL).
    private static readonly HashSet<string> ReservedDeviceNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM¹", "COM²", "COM³",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "LPT¹", "LPT²", "LPT³",
    };

    /// <summary>Why <paramref name="name"/> can't be an entity name, or null if it can.</summary>
    public static string? Problem(string name)
    {
        if (name.Length == 0)
        {
            return "The entity name is empty.";
        }

        if (name.Any(char.IsControl))
        {
            return $"Entity name '{name}' contains a control character, such as a tab. It's used as a file name, which can't.";
        }

        var reserved = name.IndexOfAny(ReservedCharacters);
        if (reserved >= 0)
        {
            return $"Entity name '{name}' contains '{name[reserved]}'. It's used as a file name, which can't contain any of < > : \" / \\ | ? *";
        }

        if (name.EndsWith('.') || name.EndsWith(' '))
        {
            return $"Entity name '{name}' ends with a period or a space. It's used as a file name, which can't.";
        }

        var beforeExtension = name.Split('.')[0];
        if (ReservedDeviceNames.Contains(beforeExtension))
        {
            return $"Entity name '{name}' can't be used as a file name: Windows reserves '{beforeExtension}' for a device.";
        }

        return null;
    }
}
