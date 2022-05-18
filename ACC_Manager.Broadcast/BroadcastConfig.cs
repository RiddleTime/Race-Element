using ACCManager.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACC_Manager.Broadcast
{
    public class BroadcastConfig
    {
        public class Root
        {
            public int updListenerPort { get; set; }
            public string connectionPassword { get; set; }
            public string commandPassword { get; set; }
        }

        public static Root GetConfiguration()
        {
            FileInfo broadcastingConfig = new FileInfo(FileUtil.ConfigPath + "broadcasting.json");

            if (broadcastingConfig.Exists)
            {
                try
                {
                    using (FileStream fileStream = broadcastingConfig.OpenRead())
                    {
                        Root config = GetConfiguration(fileStream);
                        if (config.updListenerPort == 0)
                            LogWriter.WriteToLog($"Please change the port number in \"{FileUtil.ConfigPath}broadcasting.json\" .change it to something higher than 0.");

                        return config;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
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
