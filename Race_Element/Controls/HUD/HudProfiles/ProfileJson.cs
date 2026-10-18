using System;
using System.Collections.Generic;

namespace RaceElement.Controls.HUD.HudProfiles;

/// <summary>
/// On-disk schema for profile.json (metadata + conditions only).
/// HUD settings live as sibling *.json files in the same folder.
/// </summary>
public sealed class ProfileJson
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public List<ProfileConditionDto> Conditions { get; set; } = [];
}