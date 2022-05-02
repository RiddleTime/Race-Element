using ACCSetupApp.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay.Internal
{
    internal class OverlayOptions
    {

        public class OverlaySettings
        {
            bool Enabled;
            int X, Y;
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


            OverlaySettings overlaySettings = new OverlaySettings();
            return overlaySettings;
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

            File.WriteAllText(overlaySettingsFile.FullName, jsonString);

            return settings;
        }

    }
}
