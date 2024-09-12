using Newtonsoft.Json;
using RaceElement.Data.Games;
using RaceElement.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.HUD.Overlay.Configuration;

public class OverlaySettings
{
    public class OverlaySettingsJson
    {
        public bool Enabled;
        public int X, Y;
        public List<ConfigField> Config;
    }

    private static DirectoryInfo GetOverlayDirectory(Game gameWhenStarted = Game.Any)
    {
        if (gameWhenStarted == Game.Any) gameWhenStarted = GameManager.CurrentGame;
        DirectoryInfo overlayDir = new(FileUtil.RaceElementOverlayPath + gameWhenStarted.ToFriendlyName());
        if (!overlayDir.Exists) overlayDir.Create();
        return overlayDir;
    }

    public static OverlaySettingsJson LoadOverlaySettings(string overlayName, Game gameWhenStarted = Game.Any)
    {
        DirectoryInfo overlayDir = GetOverlayDirectory(gameWhenStarted);

        FileInfo[] overlayFiles = overlayDir.GetFiles($"*.json");
        foreach (FileInfo overlayFile in overlayFiles)
        {
            if (overlayFile.Name.Replace(".json", "") == overlayName)
            {
                OverlaySettingsJson overlay = LoadSettings(overlayFile);

                overlay ??= new OverlaySettingsJson();

                return overlay;
            }
        }

        return new OverlaySettingsJson(); ;
    }

    public static OverlaySettingsJson SaveOverlaySettings(string overlayName, OverlaySettingsJson settings, Game gameWhenStarted = Game.Any)
    {
        FileInfo[] tagFiles = GetOverlayDirectory(gameWhenStarted).GetFiles($"{overlayName}.json");
        FileInfo overlaySettingsFile = null;

        if (tagFiles.Length == 0)
        {
            overlaySettingsFile = new FileInfo($"{GetOverlayDirectory(gameWhenStarted).FullName}{Path.DirectorySeparatorChar}{overlayName}.json");
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

        overlaySettingsFile ??= new FileInfo($"{GetOverlayDirectory(gameWhenStarted).FullName}{Path.DirectorySeparatorChar}{overlayName}.json");

        string jsonString = JsonConvert.SerializeObject(settings, Formatting.Indented);

        try
        {
            if (overlaySettingsFile != null)
            {
                overlaySettingsFile.Delete();
            }

            File.WriteAllText(overlaySettingsFile.FullName, jsonString);
            Debug.WriteLine($"Written to {overlaySettingsFile.FullName}\n - Game: {gameWhenStarted.ToFriendlyName()}");
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
            using (StreamReader reader = new(stream))
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
