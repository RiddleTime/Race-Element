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
using static ACCManager.HUD.Overlay.Internal.OverlayConfiguration;

namespace ACCManager.HUD.Overlay.Internal
{
    public class OverlayOptions
    {
        public class OverlaySettings
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


        public static OverlaySettings LoadOverlaySettings(string overlayName)
        {
            DirectoryInfo overlayDir = GetOverlayDirectory();

            FileInfo[] overlayFiles = overlayDir.GetFiles($"*.json");
            foreach (FileInfo overlayFile in overlayFiles)
            {
                if (overlayFile.Name.Replace(".json", "") == overlayName)
                {
                    OverlaySettings overlay = LoadSettings(overlayFile);

                    if(overlay == null)
                    {
                        overlay = new OverlaySettings();
                    }

                    return overlay;
                }
            }

            return null;
        }

        public static OverlaySettings SaveOverlaySettings(string overlayName, OverlaySettings settings)
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

            string jsonString = JsonConvert.SerializeObject(settings, Formatting.Indented);

            overlaySettingsFile.Delete();
            File.WriteAllText(overlaySettingsFile.FullName, jsonString);

            return settings;
        }


        private static OverlaySettings LoadSettings(FileInfo file)
        {
            if (!file.Exists)
                return null;

            try
            {
                using (FileStream fileStream = file.OpenRead())
                {
                    OverlaySettings settings = LoadSettings(fileStream);
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

        private static OverlaySettings LoadSettings(Stream stream)
        {
            string jsonString = string.Empty;
            OverlaySettings settings = null;
            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    jsonString = reader.ReadToEnd();
                    jsonString = jsonString.Replace("\0", "");
                    reader.Close();
                    stream.Close();
                }

                settings = JsonConvert.DeserializeObject<OverlaySettings>(jsonString);
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
