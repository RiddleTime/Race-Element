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
    public class AccSettingsJson
    {
        public Guid? UnlistedAccServer { get; set; }

        public static AccSettingsJson Default()
        {
            return new AccSettingsJson()
            {
                UnlistedAccServer = null,
            };
        }
    }

    public class AccSettings
    {
        private const string FileName = "ACC.json";
        private static FileInfo AccSettingsFile => new FileInfo(FileUtil.AccManangerSettingsPath + FileName);

        public static AccSettingsJson LoadJson()
        {
            if (!AccSettingsFile.Exists)
                return AccSettingsJson.Default();

            try
            {
                using (FileStream fileStream = AccSettingsFile.OpenRead())
                {
                    return ReadJson(fileStream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return AccSettingsJson.Default();
        }

        public static AccSettingsJson ReadJson(Stream stream)
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

                return JsonConvert.DeserializeObject<AccSettingsJson>(jsonString);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return AccSettingsJson.Default();
        }

        public static void SaveJson(AccSettingsJson accSettings)
        {
            string jsonString = JsonConvert.SerializeObject(accSettings, Formatting.Indented);

            if (!AccSettingsFile.Exists)
                if (!Directory.Exists(FileUtil.AccManangerSettingsPath))
                    Directory.CreateDirectory(FileUtil.AccManangerSettingsPath);

            File.WriteAllText(FileUtil.AccManangerSettingsPath + FileName, jsonString);
        }
    }
}
