using ACCManager.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.HUD.Overlay.Configuration.OverlayConfiguration;

namespace ACCManager.HUD.Overlay.Configuration
{
    public class OverlaySettings
    {
        public class OverlaySettingsJson
        {
            public bool Enabled;
            public int X, Y;
            public List<ConfigField> Config;
        }

        private static DirectoryInfo GetOverlayDirectory()
        {
            DirectoryInfo overlayDir = new DirectoryInfo(FileUtil.AccManagerOverlayPath);

            if (!overlayDir.Exists)
            {
                overlayDir.Create();
            }

            return overlayDir;
        }


        public static OverlaySettingsJson LoadOverlaySettings(string overlayName)
        {
            DirectoryInfo overlayDir = GetOverlayDirectory();

            FileInfo[] overlayFiles = overlayDir.GetFiles($"*.json");
            foreach (FileInfo overlayFile in overlayFiles)
            {
                if (overlayFile.Name.Replace(".json", "") == overlayName)
                {
                    OverlaySettingsJson overlay = LoadSettings(overlayFile);

                    if (overlay == null)
                    {
                        overlay = new OverlaySettingsJson();
                    }

                    return overlay;
                }
            }

            return new OverlaySettingsJson(); ;
        }

        public static OverlaySettingsJson SaveOverlaySettings(string overlayName, OverlaySettingsJson settings)
        {
            DirectoryInfo tagDir = GetOverlayDirectory();

            FileInfo[] tagFiles = tagDir.GetFiles($"{overlayName}.json");
            FileInfo overlaySettingsFile = null;

            if (tagFiles.Length == 0)
            {
                overlaySettingsFile = new FileInfo($"{FileUtil.AccManagerOverlayPath}{overlayName}.json");
            }
            else
            {
                foreach (FileInfo file in tagFiles)
                {
                    if (file.Name == $"{overlayName}.json")
                    {
                        overlaySettingsFile = file;
                        break;
                    }
                }
            }

            if (overlaySettingsFile == null)
                overlaySettingsFile = new FileInfo($"{FileUtil.AccManagerOverlayPath}{overlayName}.json");

            string jsonString = JsonConvert.SerializeObject(settings, Formatting.Indented);

            try
            {
                if (overlaySettingsFile != null)
                {
                    overlaySettingsFile.Delete();
                }

                File.WriteAllText(overlaySettingsFile.FullName, jsonString);
            }
            catch (Exception)
            {
                return settings;
            }

            return settings;
        }


        private static OverlaySettingsJson LoadSettings(FileInfo file)
        {
            if (!file.Exists)
                return null;

            try
            {
                using (FileStream fileStream = file.OpenRead())
                {
                    OverlaySettingsJson settings = LoadSettings(fileStream);
                    fileStream.Close();
                    return settings;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        private static OverlaySettingsJson LoadSettings(Stream stream)
        {
            string jsonString = string.Empty;
            OverlaySettingsJson settings = null;
            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    jsonString = reader.ReadToEnd();
                    jsonString = jsonString.Replace("\0", "");
                    reader.Close();
                    stream.Close();
                }

                settings = JsonConvert.DeserializeObject<OverlaySettingsJson>(jsonString);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

            return settings;
        }
    }
}
