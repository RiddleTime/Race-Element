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
    public class HardwareSettingsJson
    {
        public bool UseHardwareSteeringLock = false;

        public static HardwareSettingsJson Default()
        {
            var settings = new HardwareSettingsJson()
            {
                UseHardwareSteeringLock = false
            };
            return settings;
        }
    }

    public class HardwareSettings
    {
        private const string FileName = "Hardware.json";
        private static FileInfo StreamSettingsFile => new FileInfo(FileUtil.AccManangerSettingsPath + FileName);

        public static HardwareSettingsJson LoadJson()
        {
            if (!StreamSettingsFile.Exists)
                return HardwareSettingsJson.Default();

            try
            {
                using (FileStream fileStream = StreamSettingsFile.OpenRead())
                {
                    return ReadJson(fileStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return HardwareSettingsJson.Default();
        }

        public static HardwareSettingsJson ReadJson(Stream stream)
        {
            string jsonString = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    jsonString = reader.ReadToEnd();
                    jsonString = jsonString.Replace("\0", "");
                    reader.Close();
                    stream.Close();
                }

                return JsonConvert.DeserializeObject<HardwareSettingsJson>(jsonString);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return HardwareSettingsJson.Default();
        }

        public static void SaveJson(HardwareSettingsJson streamSettings)
        {
            string jsonString = JsonConvert.SerializeObject(streamSettings, Formatting.Indented);

            if (!StreamSettingsFile.Exists)
                if (!Directory.Exists(FileUtil.AccManangerSettingsPath))
                    Directory.CreateDirectory(FileUtil.AccManangerSettingsPath);

            File.WriteAllText(FileUtil.AccManangerSettingsPath + FileName, jsonString);
        }
    }
}
