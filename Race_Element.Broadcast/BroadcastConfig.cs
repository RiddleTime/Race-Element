using RaceElement.Util;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace RaceElement.Broadcast
{
    public class BroadcastConfig
    {
        public class Root
        {
            [JsonProperty("updListenerPort")]
            public int UpdListenerPort { get; set; }
            [JsonProperty("connectionPassword")]
            public string ConnectionPassword { get; set; }
            [JsonProperty("commandPassword")]
            public string CommandPassword { get; set; }
        }

        private static string _lock = String.Empty;

        public static Root GetConfiguration()
        {
            lock (_lock)
            {
                FileInfo broadcastingConfig = new FileInfo(FileUtil.AccConfigPath + "broadcasting.json");

                if (broadcastingConfig.Exists)
                {
                    try
                    {
                        using (FileStream fileStream = broadcastingConfig.OpenRead())
                        {
                            Root config = GetConfiguration(fileStream);

                            if (config.UpdListenerPort == 0)
                            {
                                config.UpdListenerPort = 9000;
                                File.WriteAllText(broadcastingConfig.FullName, JsonConvert.SerializeObject(config, Formatting.Indented));
                                LogWriter.WriteToLog($"Auto-Changed the port number in \"{FileUtil.AccConfigPath}broadcasting.json\" from 0 to 9000.");
                            }

                            return config;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
            return null;
        }


        private static Root GetConfiguration(Stream stream)
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

                return JsonConvert.DeserializeObject<Root>(jsonString);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return null;
        }

    }
}
