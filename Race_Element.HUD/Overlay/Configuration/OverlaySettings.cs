using RaceElement.Data.Games;
using RaceElement.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.HUD.Overlay.Configuration;

public class OverlaySettings
{
    public class OverlaySettingsJson
    {
        [JsonInclude] public bool Enabled;
        [JsonInclude] public int X, Y;
        [JsonInclude] public List<ConfigField> Config;
    }

    /// <summary>
    /// Shared STJ options for live HUD json and profile HUD snapshots.
    /// IncludeFields matches OverlaySettingsJson public fields (Newtonsoft default).
    /// ConfigField.Value is coerced to CLR primitives via ConfigFieldValueConverter.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        options.Converters.Add(new ConfigFieldValueConverter());
        return options;
    }

    public static string SerializeSettings(OverlaySettingsJson settings)
    {
        settings ??= new OverlaySettingsJson();
        settings.Config ??= [];
        return JsonSerializer.Serialize(settings, JsonOptions);
    }

    public static OverlaySettingsJson DeserializeSettings(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new OverlaySettingsJson();

        try
        {
            json = json.Replace("\0", "");
            OverlaySettingsJson settings = JsonSerializer.Deserialize<OverlaySettingsJson>(json, JsonOptions);
            if (settings is null)
                return new OverlaySettingsJson();
            settings.Config ??= [];
            return settings;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return new OverlaySettingsJson();
        }
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
        return WriteFile(overlaySettingsFile, settings, gameWhenStarted);
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
        return WriteFile(file, settings, Game.Any);
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

    private static OverlaySettingsJson WriteFile(FileInfo overlaySettingsFile, OverlaySettingsJson settings, Game gameWhenStarted)
    {
        settings ??= new OverlaySettingsJson();
        settings.Config ??= [];
        string jsonString = SerializeSettings(settings);

        try
        {
            if (overlaySettingsFile.Exists)
                overlaySettingsFile.Delete();

            File.WriteAllText(overlaySettingsFile.FullName, jsonString);
            Debug.WriteLine($"Written to {overlaySettingsFile.FullName}\n - Game: {gameWhenStarted.ToFriendlyName()}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
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
        try
        {
            using StreamReader reader = new(stream);
            string jsonString = reader.ReadToEnd();
            return DeserializeSettings(jsonString);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }
    }
}