using ACCManager.Util;

namespace ACC_Manager.Util.Settings
{
    public class HudSettingsJson : IGenericSettingsJson
    {
        public bool DemoMode { get; set; }
    }

    public class HudSettings : AbstractSettingsJson<HudSettingsJson>
    {
        public override string Path => FileUtil.AccManangerSettingsPath;

        public override string FileName => "HudSettings.json";

        public override HudSettingsJson Default()
        {
            return new HudSettingsJson()
            {
                DemoMode = false
            };
        }
    }
}
