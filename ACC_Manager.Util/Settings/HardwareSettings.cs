using ACCManager.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ACC_Manager.Util.Settings
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
