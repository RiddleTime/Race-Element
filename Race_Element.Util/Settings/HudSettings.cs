using RaceElement.Util;

namespace RaceElement.Util.Settings;

public class HudSettingsJson : IGenericSettingsJson
{
    public bool DemoMode { get; set; }
}

public class HudSettings : AbstractSettingsJson<HudSettingsJson>
{
    public override string Path => FileUtil.RaceElementSettingsPath;

    public override string FileName => "HudSettings.json";

    public override HudSettingsJson Default()
    {
        return new HudSettingsJson()
        {
            DemoMode = false
        };
    }
}
