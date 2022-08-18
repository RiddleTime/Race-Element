using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACC_Manager.Util.Settings
{
    public class AccManagerSettingsJson : IGenericSettingsJson
    {
        public bool TelemetryRecordDetailed { get; set; }
    }

    public class AccManagerSettings : AbstractSettingsJson<AccManagerSettingsJson>
    {
        public override string Path => FileUtil.AccManangerSettingsPath;

        public override string FileName => "AccManager.json";

        public override AccManagerSettingsJson Default()
        {
            var settings = new AccManagerSettingsJson()
            {
                TelemetryRecordDetailed = false,
            };

            return settings;
        }
    }
}
