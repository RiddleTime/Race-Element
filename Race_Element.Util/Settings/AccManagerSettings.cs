namespace RaceElement.Util.Settings;

public class AccManagerSettingsJson : IGenericSettingsJson
{
    public bool MinimizeToSystemTray { get; set; }
    public bool TelemetryRecordDetailed { get; set; }
    public int TelemetryDetailedHerz { get; set; }
    public bool Generate4kDDS { get; set; }
}

public class AccManagerSettings : AbstractSettingsJson<AccManagerSettingsJson>
{
    public override string Path => FileUtil.RaceElementSettingsPath;

    public override string FileName => "AccManager.json";

    public override AccManagerSettingsJson Default() => new()
    {
        MinimizeToSystemTray = false,
        TelemetryRecordDetailed = false,
        TelemetryDetailedHerz = 20,
        Generate4kDDS = false,
    };

}
