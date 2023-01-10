using RaceElement.Util;
using System;

namespace RaceElement.Util.Settings
{
    public class AccSettingsJson : IGenericSettingsJson
    {
        public bool AutoRecordReplay { get; set; } = false;
        public Guid UnlistedAccServer { get; set; }
    }

    public class AccSettings : AbstractSettingsJson<AccSettingsJson>
    {
        public override string Path => FileUtil.RaceElementSettingsPath;
        public override string FileName => "ACC.json";

        public override AccSettingsJson Default() => new AccSettingsJson()
        {
            UnlistedAccServer = Guid.Empty,
            AutoRecordReplay = false,
        };
    }
}
