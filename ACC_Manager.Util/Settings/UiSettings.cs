using ACCManager.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACC_Manager.Util.Settings
{
    public class UiSettingsJson
    {
        public int SelectedTabIndex;
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public static UiSettingsJson Default()
        {
            var settings = new UiSettingsJson()
            {
                SelectedTabIndex = 0,
                X = (int)SystemParameters.PrimaryScreenWidth / 2,
                Y = (int)SystemParameters.PrimaryScreenHeight / 2
            };

            return settings;
        }
    }

    public class UiSettings
    {
        private const string FileName = "UI.json";
        private static FileInfo StreamSettingsFile => new FileInfo(FileUtil.AccManangerSettingsPath + FileName);

        public static UiSettingsJson LoadJson()
        {
            if (!StreamSettingsFile.Exists)
                return UiSettingsJson.Default();

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
            return UiSettingsJson.Default();
        }

        public static UiSettingsJson ReadJson(Stream stream)
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

                return JsonConvert.DeserializeObject<UiSettingsJson>(jsonString);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return UiSettingsJson.Default();
        }

        public static void SaveJson(UiSettingsJson streamSettings)
        {
            string jsonString = JsonConvert.SerializeObject(streamSettings, Formatting.Indented);

            if (!StreamSettingsFile.Exists)
                if (!Directory.Exists(FileUtil.AccManangerSettingsPath))
                    Directory.CreateDirectory(FileUtil.AccManangerSettingsPath);

            File.WriteAllText(FileUtil.AccManangerSettingsPath + FileName, jsonString);
        }
    }
}
