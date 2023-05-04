namespace RaceElement.Util.Settings
{
    public class AccManagerSettingsJson : IGenericSettingsJson
    {
        public bool MinimizeToSystemTray { get; set; }
        public bool TelemetryRecordDetailed { get; set; }
        public int TelemetryDetailedHerz { get; set; }
    }

    public class AccManagerSettings : AbstractSettingsJson<AccManagerSettingsJson>
    {
        public override string Path => FileUtil.RaceElementSettingsPath;

        public override string FileName => "AccManager.json";

        public override AccManagerSettingsJson Default() => new AccManagerSettingsJson()
        {
            MinimizeToSystemTray = false,
            TelemetryRecordDetailed = false,
            TelemetryDetailedHerz = 20,
        };

    }
}
