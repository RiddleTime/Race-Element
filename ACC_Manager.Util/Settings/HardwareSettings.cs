using RaceElement.Util;

namespace RaceElement.Util.Settings
{
    public class HardwareSettingsJson : IGenericSettingsJson
    {
        public bool UseHardwareSteeringLock = false;
    }

    public class HardwareSettings : AbstractSettingsJson<HardwareSettingsJson>
    {
        public override string Path => FileUtil.AccManangerSettingsPath;

        public override string FileName => "Hardware.json";

        public override HardwareSettingsJson Default()
        {
            var settings = new HardwareSettingsJson()
            {
                UseHardwareSteeringLock = false
            };
            return settings;
        }
    }
}
