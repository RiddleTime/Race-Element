using ACCManager.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACC_Manager.Util.Settings
{
    public class AccSettingsJson : IGenericSettingsJson
    {
        public Guid UnlistedAccServer { get; set; }
    }

    public class AccSettings : AbstractSettingsJson<AccSettingsJson>
    {
        public override string Path => FileUtil.AccManangerSettingsPath;
        public override string FileName => "ACC.json";

        public override AccSettingsJson Default()
        {
            return new AccSettingsJson()
            {
                UnlistedAccServer = Guid.Empty,
            };
        }
    }
}
