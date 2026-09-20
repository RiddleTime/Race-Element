using Newtonsoft.Json;
using RaceElement.Data.Games;
using RaceElement.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

    /// <summary>Game root overlay folder (live / last-applied settings).</summary>
    public static DirectoryInfo GetOverlayDirectory(Game gameWhenStarted = Game.Any)
    {
        if (gameWhenStarted == Game.Any)
            gameWhenStarted = GameManager.CurrentGame;

        DirectoryInfo overlayDir = new(FileUtil.RaceElementOverlayPath + gameWhenStarted.ToFriendlyName());
        if (!overlayDir.Exists)
            overlayDir.Create();
        return overlayDir;
    }

    /// <summary>Profiles root for a game: …\Overlay\{Game}\Profiles\</summary>
    public static DirectoryInfo GetProfilesDirectory(Game gameWhenStarted = Game.Any)
    {
        DirectoryInfo dir = new(Path.Combine(GetOverlayDirectory(gameWhenStarted).FullName, "Profiles"));
        if (!dir.Exists)
            dir.Create();
        return dir;
    }

    /// <summary>One profile folder: …\Profiles\{profileName}\</summary>
    public static DirectoryInfo GetProfileDirectory(string profileName, Game gameWhenStarted = Game.Any)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            profileName = profileName.Replace(c, '_');

        DirectoryInfo dir = new(Path.Combine(GetProfilesDirectory(gameWhenStarted).FullName, profileName.Trim()));
        if (!dir.Exists)
            dir.Create();
        return dir;
    }

    public static OverlaySettingsJson LoadOverlaySettings(string overlayName, Game gameWhenStarted = Game.Any)
    {
        DirectoryInfo overlayDir = GetOverlayDirectory(gameWhenStarted);

        foreach (FileInfo overlayFile in overlayDir.GetFiles("*.json"))
        {
            if (overlayFile.Name.Replace(".json", "") == overlayName)
            {
                OverlaySettingsJson overlay = LoadSettings(overlayFile);
                return overlay ?? new OverlaySettingsJson();
            }
        }

        return new OverlaySettingsJson();
    }

    /// <summary>
    /// Load one HUD settings file from a specific profile folder (not the live root).
    /// Does not change “current profile” — caller chooses the folder.
    /// </summary>
    public static OverlaySettingsJson LoadOverlaySettingsFromDirectory(string overlayName, DirectoryInfo directory)
    {
        if (directory is null || !directory.Exists)
            return new OverlaySettingsJson();

        FileInfo file = new(Path.Combine(directory.FullName, overlayName + ".json"));
        OverlaySettingsJson overlay = LoadSettings(file);
        return overlay ?? new OverlaySettingsJson();
    }

    public static OverlaySettingsJson SaveOverlaySettings(string overlayName, OverlaySettingsJson settings, Game gameWhenStarted = Game.Any)
    {
        DirectoryInfo dir = GetOverlayDirectory(gameWhenStarted);
        FileInfo overlaySettingsFile = new(Path.Combine(dir.FullName, overlayName + ".json"));

        string jsonString = JsonConvert.SerializeObject(settings, Formatting.Indented);

        try
        {
            if (overlaySettingsFile.Exists)
                overlaySettingsFile.Delete();

            File.WriteAllText(overlaySettingsFile.FullName, jsonString);
            Debug.WriteLine($"Written to {overlaySettingsFile.FullName}\n - Game: {gameWhenStarted.ToFriendlyName()}");
        }
        catch (Exception)
        {
            return settings;
        }

        return settings;
    }

    /// <summary>
    /// Save one HUD settings file into a specific profile folder (snapshot).
    /// Live root is unchanged unless the caller also calls SaveOverlaySettings.
    /// </summary>
    public static OverlaySettingsJson SaveOverlaySettingsToDirectory(string overlayName, OverlaySettingsJson settings, DirectoryInfo directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory));

        if (!directory.Exists)
            directory.Create();

        FileInfo file = new(Path.Combine(directory.FullName, overlayName + ".json"));
        string jsonString = JsonConvert.SerializeObject(settings, Formatting.Indented);

        try
        {
            if (file.Exists)
                file.Delete();

            File.WriteAllText(file.FullName, jsonString);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        return settings;
    }

    /// <summary>HUD json files in the live game root (excludes nothing under Profiles\).</summary>
    public static IReadOnlyList<string> ListOverlayNames(Game gameWhenStarted = Game.Any)
    {
        DirectoryInfo dir = GetOverlayDirectory(gameWhenStarted);
        return dir.GetFiles("*.json")
            .Select(f => f.Name.Replace(".json", ""))
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static OverlaySettingsJson LoadSettings(FileInfo file)
    {
        if (!file.Exists)
            return null;

        try
        {
            using FileStream fileStream = file.OpenRead();
            return LoadSettings(fileStream);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        return null;
    }

    private static OverlaySettingsJson LoadSettings(Stream stream)
    {
        OverlaySettingsJson settings = null;
        try
        {
            using StreamReader reader = new(stream);
            string jsonString = reader.ReadToEnd().Replace("\0", "");
            settings = JsonConvert.DeserializeObject<OverlaySettingsJson>(jsonString);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return settings;
    }
}