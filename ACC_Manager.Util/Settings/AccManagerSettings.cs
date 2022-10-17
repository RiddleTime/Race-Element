using ACCManager.Util;

namespace ACC_Manager.Util.Settings
{
    public class AccManagerSettingsJson : IGenericSettingsJson
    {
        public bool TelemetryRecordDetailed { get; set; } = false;
        public int TelemetryDetailedHerz { get; set; }
    }

    public class AccManagerSettings : AbstractSettingsJson<AccManagerSettingsJson>
    {
        public override string Path => FileUtil.AccManangerSettingsPath;

        public override string FileName => "AccManager.json";

        public override AccManagerSettingsJson Default()
        {
            var settings = new AccManagerSettingsJson()
            {
                TelemetryRecordDetailed = true,
                TelemetryDetailedHerz = 20,
            };

            return settings;
        }
    }
}
