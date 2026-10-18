using System.Collections.Generic;

namespace RaceElement.Util.Settings;

/// <summary>
/// UI state for HUD profiles 
/// </summary>
public sealed class HudProfileUiSettingsJson : IGenericSettingsJson
{
    public Dictionary<string, string> LastSelectedByGame { get; set; } = [];
}

public sealed class HudProfileUiSettings : AbstractSettingsJson<HudProfileUiSettingsJson>
{
    public override string Path => FileUtil.RaceElementSettingsPath;

    public override string FileName => "HudProfileUiSettings.json";

    public override HudProfileUiSettingsJson Default() => new()
    {
        LastSelectedByGame = []
    };
}