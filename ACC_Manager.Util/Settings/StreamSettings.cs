using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace ACCManager.Util.Settings
{
    public class StreamingSettingsJson
    {
        public string StreamingSoftware { get; set; }
        public string StreamingWebSocketIP { get; set; }
        public int StreamingWebSocketPort { get; set; }
        public string StreamingWebSocketPassword { get; set; }

        public static StreamingSettingsJson Default()
        {
            var settings = new StreamingSettingsJson()
            {
                StreamingSoftware = "OBS",
                StreamingWebSocketIP = "localhost",
                StreamingWebSocketPort = 4444,
                StreamingWebSocketPassword = String.Empty
            };
            return settings;
        }
    }

    public class StreamSettings
    {
        private const string FileName = "Streaming.json";
        private static FileInfo StreamSettingsFile => new FileInfo(FileUtil.AccManangerSettingsPath + FileName);

        public static StreamingSettingsJson LoadJson()
        {
            if (!StreamSettingsFile.Exists)
                return StreamingSettingsJson.Default();

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
            return null;
        }

        public static StreamingSettingsJson ReadJson(Stream stream)
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

                return JsonConvert.DeserializeObject<StreamingSettingsJson>(jsonString);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return null;
        }

        public static void SaveJson(StreamingSettingsJson streamSettings)
        {
            string jsonString = JsonConvert.SerializeObject(streamSettings, Formatting.Indented);

            if (!StreamSettingsFile.Exists)
            {
                if (!Directory.Exists(FileUtil.AccManangerSettingsPath))
                    Directory.CreateDirectory(FileUtil.AccManangerSettingsPath);
            }

            File.WriteAllText(FileUtil.AccManangerSettingsPath + FileName, jsonString);
        }
    }
}
