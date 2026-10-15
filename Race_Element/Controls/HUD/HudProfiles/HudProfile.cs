using System;
using System.Collections.Generic;
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;

namespace RaceElement.Controls.HUD.HudProfiles;

/// <summary>
/// In-memory representation of a HUD profile (one folder under Profiles\).
/// </summary>
public sealed class HudProfile
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>Absolute path to the profile folder.</summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Conditions for automatic activation. Empty for Step 2;
    /// populated from profile.json when condition types exist (Step 6).
    /// </summary>
    public List<ProfileConditionDto> Conditions { get; set; } = [];

    /// <summary>
    /// HUD name → settings. Key is the overlay name (same as OverlaySettings file name without .json).
    /// </summary>
    public Dictionary<string, OverlaySettingsJson> Huds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Serializable condition entry. Evaluation comes in Step 6;
/// for now we only round-trip the data.
/// </summary>
public sealed class ProfileConditionDto
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = [];
}