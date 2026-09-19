using RaceElement.Controls.HUD;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;

namespace RaceElement.Controls.HUD.Profiles;

/// <summary>
/// Applies and captures HUD profiles. Depends only on HudProfileStore + OverlayLifecycleService.
/// No UI.
/// </summary>
internal sealed class HudProfileManager
{
    public static HudProfileManager Instance { get; } = new();

    public event Action<HudProfile> ProfileApplied;

    /// <summary>
    /// Loads a profile by name for the current (or given) game and applies it.
    /// </summary>
    public bool ApplyProfile(string profileName, Game? game = null)
    {
        var profile = HudProfileStore.Load(profileName, game);
        if (profile is null)
        {
            Debug.WriteLine($"[HudProfileManager] Profile not found: '{profileName}'");
            return false;
        }

        ApplyProfile(profile);
        return true;
    }

    /// <summary>
    /// Applies a loaded profile:
    /// 1. Stop active HUDs that are not in the profile (full layout switch).
    /// 2. ApplySettings for every HUD in the profile (Enabled drives start/stop).
    /// </summary>
    public void ApplyProfile(HudProfile profile)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));

        var lifecycle = OverlayLifecycleService.Instance;

        // Names that belong to this profile (ordinal ignore-case)
        var profileHudNames = new HashSet<string>(
            profile.Huds.Keys,
            StringComparer.OrdinalIgnoreCase);

        // 1. Stop anything active that is not part of this profile
        List<CommonAbstractOverlay> activeSnapshot = lifecycle.ActiveOverlays.ToList();
        foreach (var overlay in activeSnapshot)
        {
            if (!profileHudNames.Contains(overlay.Name))
            {
                Debug.WriteLine($"[HudProfileManager] Stopping HUD not in profile: {overlay.Name}");
                lifecycle.Stop(overlay.Name);
            }
        }

        // 2. Apply each HUD entry from the profile
        foreach (var kv in profile.Huds)
        {
            string hudName = kv.Key;
            OverlaySettingsJson settings = kv.Value;

            if (settings is null)
                continue;

            try
            {
                // ApplySettings persists and Start/Stops according to settings.Enabled
                lifecycle.ApplySettings(hudName, settings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HudProfileManager] ApplySettings failed for '{hudName}': {ex.Message}");
            }
        }

        ProfileApplied?.Invoke(profile);
        Debug.WriteLine($"[HudProfileManager] Applied profile '{profile.Name}' ({profile.Huds.Count} HUDs)");
    }

    /// <summary>
    /// Captures the current live overlay settings into a profile and saves the folder.
    /// Thin wrapper over HudProfileStore for a single entry point.
    /// </summary>
    public HudProfile SaveCurrentAs(string profileName, string? description = null, bool isDefault = false, Game? game = null)
    {
        return HudProfileStore.CaptureAndSave(profileName, description, isDefault, game);
    }

    /// <summary>
    /// Lists profile names for the current (or given) game.
    /// </summary>
    public IReadOnlyList<string> ListProfiles(Game? game = null)
        => HudProfileStore.ListProfileNames(game);

    /// <summary>
    /// Deletes a profile folder.
    /// </summary>
    public bool DeleteProfile(string profileName, Game? game = null)
        => HudProfileStore.Delete(profileName, game);
}