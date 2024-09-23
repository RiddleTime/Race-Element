namespace RaceElement.Util.Settings;

public sealed class HudSettingsJson : IGenericSettingsJson
{
    public bool DemoMode { get; set; }
}

public sealed class HudSettings : AbstractSettingsJson<HudSettingsJson>
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
