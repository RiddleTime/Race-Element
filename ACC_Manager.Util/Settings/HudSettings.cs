using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static HudSettingsJson hudSettingsJson;

        public override HudSettingsJson Default()
        {
            return new HudSettingsJson()
            {
                DemoMode = false
            };
        }

        public override HudSettingsJson LoadJson()
        {
            if (hudSettingsJson != null)
                return hudSettingsJson;

            return base.LoadJson();
        }

        public override void SaveJson(HudSettingsJson genericJson)
        {
            base.SaveJson(genericJson);
            hudSettingsJson = genericJson;
        }
    }
}
