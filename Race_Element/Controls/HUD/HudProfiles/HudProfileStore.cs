using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;

namespace RaceElement.Controls.HUD.Profiles;

/// <summary>
/// Load / save / list HUD profile folders. No UI, no Apply.
/// </summary>
internal static class HudProfileStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        IncludeFields = true
    };

    private const string ProfilesFolderName = "Profiles";
    private const string ProfileJsonFileName = "profile.json";

    // ── Paths ─────────────────────────────────────────────────

    /// <summary>
    /// Root overlay directory for the current (or given) game.
    /// Reuses the same location OverlaySettings already uses.
    /// </summary>
    public static string GetGameOverlayDirectory(Game? game = null)
    {
        game ??= GameManager.CurrentGame;

        string root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Race Element",
            "Overlay",
            game.Value.ToFriendlyName());
        return root;
    }

    public static string GetProfilesRoot(Game? game = null)
        => Path.Combine(GetGameOverlayDirectory(game), ProfilesFolderName);

    public static string GetProfileFolder(string profileName, Game? game = null)
        => Path.Combine(GetProfilesRoot(game), SanitizeFolderName(profileName));

    private static string SanitizeFolderName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim();
    }


    public const string DefaultProfileName = "Default";

    /// <summary>
    /// If the game has root-level HUD settings but no Default profile yet,
    /// create Profiles\Default\ from the current root files.
    /// Does not overwrite an existing Default profile.
    /// Returns true when a new Default was created.
    /// </summary>
    public static bool EnsureDefaultProfile(Game? game = null)
    {
        game ??= GameManager.CurrentGame;
        if (game == Game.Any)
            return false;

        // Already have Default → nothing to do
        string defaultFolder = GetProfileFolder(DefaultProfileName, game);
        if (Directory.Exists(defaultFolder))
        {
            string profileJson = Path.Combine(defaultFolder, ProfileJsonFileName);
            if (File.Exists(profileJson))
                return false;
        }

        string overlayDir = GetGameOverlayDirectory(game);
        if (!Directory.Exists(overlayDir))
            return false;

        var rootHudFiles = Directory.GetFiles(overlayDir, "*.json")
            .Where(f => !IsUnderProfilesFolder(f, overlayDir))
            .ToList();

        if (rootHudFiles.Count == 0)
            return false; // nothing to migrate

        var profile = new HudProfile
        {
            Name = DefaultProfileName,
            Description = "Auto-created from existing HUD settings",
            IsDefault = true,
            LastModified = DateTime.UtcNow,
            Conditions = []
        };

        foreach (string file in rootHudFiles)
        {
            string hudName = Path.GetFileNameWithoutExtension(file);
            if (string.IsNullOrWhiteSpace(hudName))
                continue;

            try
            {
                OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(hudName, game.Value);
                profile.Huds[hudName] = settings;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HudProfileStore] Default migrate skip '{hudName}': {ex.Message}");
            }
        }

        if (profile.Huds.Count == 0)
            return false;

        Save(profile, game);
        Debug.WriteLine($"[HudProfileStore] Created Default profile for {game} ({profile.Huds.Count} HUDs)");
        return true;
    }

    private static bool IsUnderProfilesFolder(string filePath, string overlayDir)
    {
        string profilesRoot = Path.Combine(overlayDir, ProfilesFolderName);
        return filePath.StartsWith(profilesRoot, StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string> ListProfileNames(Game? game = null)
    {
        string root = GetProfilesRoot(game);
        if (!Directory.Exists(root))
            return [];

        return Directory.GetDirectories(root)
            .Select(Path.GetFileName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList()!;
    }

    public static HudProfile? Load(string profileName, Game? game = null)
    {
        string folder = GetProfileFolder(profileName, game);
        if (!Directory.Exists(folder))
            return null;

        string profileJsonPath = Path.Combine(folder, ProfileJsonFileName);
        ProfileJson meta;

        if (File.Exists(profileJsonPath))
        {
            try
            {
                string json = File.ReadAllText(profileJsonPath);
                meta = JsonSerializer.Deserialize<ProfileJson>(json, JsonOptions) ?? new ProfileJson();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HudProfileStore] Failed to read profile.json: {ex.Message}");
                meta = new ProfileJson { Name = profileName };
            }
        }
        else
        {
            meta = new ProfileJson { Name = profileName };
        }

        if (string.IsNullOrWhiteSpace(meta.Name))
            meta.Name = profileName;

        var profile = new HudProfile
        {
            Name = meta.Name,
            Description = meta.Description ?? string.Empty,
            IsDefault = meta.IsDefault,
            LastModified = meta.LastModified,
            FolderPath = folder,
            Conditions = meta.Conditions ?? []
        };

        // Discover HUD settings files (*.json except profile.json)
        foreach (string file in Directory.GetFiles(folder, "*.json"))
        {
            string fileName = Path.GetFileName(file);
            if (fileName.Equals(ProfileJsonFileName, StringComparison.OrdinalIgnoreCase))
                continue;

            string hudName = Path.GetFileNameWithoutExtension(file);
            try
            {
                string json = File.ReadAllText(file);
                var settings = JsonSerializer.Deserialize<OverlaySettingsJson>(json, JsonOptions);
                if (settings is not null)
                    profile.Huds[hudName] = settings;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HudProfileStore] Skip '{fileName}': {ex.Message}");
            }
        }

        return profile;
    }

    public static IReadOnlyList<HudProfile> LoadAll(Game? game = null)
    {
        var list = new List<HudProfile>();
        foreach (string name in ListProfileNames(game))
        {
            var p = Load(name, game);
            if (p is not null)
                list.Add(p);
        }
        return list;
    }

    public static void Save(HudProfile profile, Game? game = null)
    {
        if (profile is null || string.IsNullOrWhiteSpace(profile.Name))
            throw new ArgumentException("Profile must have a name.", nameof(profile));

        string folder = GetProfileFolder(profile.Name, game);
        Directory.CreateDirectory(folder);
        profile.FolderPath = folder;
        profile.LastModified = DateTime.UtcNow;

        // profile.json
        var meta = new ProfileJson
        {
            Name = profile.Name,
            Description = profile.Description ?? string.Empty,
            IsDefault = profile.IsDefault,
            LastModified = profile.LastModified,
            Conditions = profile.Conditions ?? []
        };

        string profileJsonPath = Path.Combine(folder, ProfileJsonFileName);
        File.WriteAllText(profileJsonPath, JsonSerializer.Serialize(meta, JsonOptions));

        // HUD settings files
        foreach (var kv in profile.Huds)
        {
            string hudPath = Path.Combine(folder, kv.Key + ".json");
            File.WriteAllText(hudPath, JsonSerializer.Serialize(kv.Value, JsonOptions));
        }
    }

    /// <summary>
    /// Builds a HudProfile from the current root-level OverlaySettings files
    /// for the game (the same files the app uses today). Does not write until Save is called.
    /// </summary>
    public static HudProfile CaptureCurrentState(string profileName, Game? game = null)
    {
        game ??= GameManager.CurrentGame;
        string overlayDir = GetGameOverlayDirectory(game);

        var profile = new HudProfile
        {
            Name = profileName,
            Description = string.Empty,
            IsDefault = false,
            LastModified = DateTime.UtcNow,
            Conditions = []
        };

        if (!Directory.Exists(overlayDir))
            return profile;

        foreach (string file in Directory.GetFiles(overlayDir, "*.json"))
        {
            // Skip anything that is not a HUD settings file at root
            string fileName = Path.GetFileName(file);
            if (fileName.Equals(ProfileJsonFileName, StringComparison.OrdinalIgnoreCase))
                continue;

            string hudName = Path.GetFileNameWithoutExtension(file);
            try
            {
                // Prefer the existing OverlaySettings loader so format stays identical
                var settings = OverlaySettings.LoadOverlaySettings(hudName);
                if (settings is not null)
                    profile.Huds[hudName] = settings;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HudProfileStore] Capture skip '{hudName}': {ex.Message}");
            }
        }

        return profile;
    }

    /// <summary>
    /// Convenience: capture current state and write the profile folder in one call.
    /// </summary>
    public static HudProfile CaptureAndSave(string profileName, string? description = null, bool isDefault = false, Game? game = null)
    {
        var profile = CaptureCurrentState(profileName, game);
        profile.Description = description ?? string.Empty;
        profile.IsDefault = isDefault;
        Save(profile, game);
        return profile;
    }

    public static bool Delete(string profileName, Game? game = null)
    {
        string folder = GetProfileFolder(profileName, game);
        if (!Directory.Exists(folder))
            return false;

        try
        {
            Directory.Delete(folder, recursive: true);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HudProfileStore] Delete failed: {ex.Message}");
            return false;
        }
    }
}