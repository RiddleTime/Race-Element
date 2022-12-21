using ACCManager.Util;
using System;

namespace ACC_Manager.Util.Settings
{
    public class AccSettingsJson : IGenericSettingsJson
    {
        public bool AutoRecordReplay { get; set; }
        public Guid UnlistedAccServer { get; set; }
    }

    public class AccSettings : AbstractSettingsJson<AccSettingsJson>
    {
        public override string Path => FileUtil.AccManangerSettingsPath;
        public override string FileName => "ACC.json";

        public override AccSettingsJson Default() => new AccSettingsJson()
        {
            UnlistedAccServer = Guid.Empty,
            AutoRecordReplay = false,
        };
    }
}
