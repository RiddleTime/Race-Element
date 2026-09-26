using RaceElement.Controls.HUD;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;

namespace RaceElement.Controls.HUD.HudProfiles;

/// <summary>
/// Applies and captures HUD profiles. Depends only on HudProfileStore + OverlayLifecycleService.
/// No UI.
/// </summary>
internal sealed class HudProfileManager
{
    public static HudProfileManager Instance { get; } = new();

    public event Action<HudProfile>? ProfileApplied;

    public bool EnsureDefaultProfile(Game? game = null)
        => HudProfileStore.EnsureDefaultProfile(game);

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
    /// Full layout switch: disable reposition, wait for all HUDs to stop, then apply.
    /// </summary>
    public void ApplyProfile(HudProfile profile)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));

        var lifecycle = OverlayLifecycleService.Instance;

        lifecycle.SetRepositionMode(false);
        lifecycle.StopAll();

        foreach (var kv in profile.Huds)
        {
            string hudName = kv.Key;
            OverlaySettingsJson settings = kv.Value;

            if (settings is null)
                continue;

            try
            {
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

    public HudProfile SaveCurrentAs(string profileName, string? description = null, bool isDefault = false, Game? game = null)
    {
        OverlayLifecycleService.Instance.SetRepositionMode(false);
        return HudProfileStore.CaptureAndSave(profileName, description, isDefault, game);
    }

    public IReadOnlyList<string> ListProfiles(Game? game = null)
        => HudProfileStore.ListProfileNames(game);

    public bool DeleteProfile(string profileName, Game? game = null)
        => HudProfileStore.Delete(profileName, game);
}