using RaceElement.Data.Games;
using RaceElement.HUD.ACC;
using RaceElement.HUD.Common;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;

namespace RaceElement.Controls.HUD;

/// <summary>
/// Overlay start / stop / apply and active-instance tracking
/// </summary>
internal sealed class OverlayLifecycleService
{
    public static OverlayLifecycleService Instance { get; } = new();

    private readonly Lock _lock = new();

    private static readonly object[] DefaultOverlayArgs =
    [
        new System.Drawing.Rectangle(
            (int)SystemParameters.PrimaryScreenWidth / 2,
            (int)SystemParameters.PrimaryScreenHeight / 2,
            300, 150)
    ];

    public event Action<string> OverlayStarted;
    public event Action<string> OverlayStopped;
    public event Action ActiveOverlaysChanged;

    private List<CommonAbstractOverlay> LiveList => GameManager.CurrentGame switch
    {
        Game.AssettoCorsaCompetizione => OverlaysAcc.ActiveOverlays,
        Game.Any => [],
        _ => CommonHuds.ActiveOverlays
    };

    public IReadOnlyList<CommonAbstractOverlay> ActiveOverlays
    {
        get { lock (_lock) return LiveList.ToList(); }
    }

    public bool IsActive(string overlayName)
    {
        lock (_lock)
            return LiveList.Exists(o => o.Name == overlayName);
    }

    public CommonAbstractOverlay? GetInstance(string overlayName)
    {
        lock (_lock)
            return LiveList.Find(o => o.Name == overlayName);
    }

    private static bool TryGetOverlayType(string overlayName, out Type? type)
    {
        type = null;

        if (GameManager.CurrentGame == Game.AssettoCorsaCompetizione)
        {
            if (OverlaysAcc.AbstractOverlays.TryGetValue(overlayName, out type))
                return true;
        }
        else if (GameManager.CurrentGame != Game.Any)
        {
            if (CommonHuds.AbstractOverlays.TryGetValue(overlayName, out type))
                return true;
        }

        return false;
    }

    public void Start(string overlayName)
    {
        if (string.IsNullOrWhiteSpace(overlayName))
            return;

        lock (_lock)
        {
            if (LiveList.Exists(o => o.Name == overlayName))
                return; // already running

            if (!TryGetOverlayType(overlayName, out Type? type) || type is null)
            {
                Debug.WriteLine($"[OverlayLifecycleService] Type not found for '{overlayName}'");
                return;
            }

            try
            {
                var overlay = (CommonAbstractOverlay)Activator.CreateInstance(type, DefaultOverlayArgs)!;
                overlay.Start();

                if (LiveList.FindIndex(o => o.Name == overlay.Name) == -1)
                    LiveList.Add(overlay);

                PersistEnabled(overlayName, true);

                OverlayStarted?.Invoke(overlayName);
                ActiveOverlaysChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }

    public void Stop(string overlayName)
    {
        if (string.IsNullOrWhiteSpace(overlayName))
            return;

        CommonAbstractOverlay? overlay = null;

        lock (_lock)
        {
            int index = LiveList.FindIndex(o => o.Name == overlayName);
            if (index == -1)
                return;

            overlay = LiveList[index];
            LiveList.RemoveAt(index);

            PersistEnabled(overlayName, false);
        }

        if (overlay is not null)
        {
            new Thread(() =>
            {
                try { overlay.Stop(); }
                catch (Exception ex) { Debug.WriteLine(ex); }
            })
            { IsBackground = true }.Start();
        }

        OverlayStopped?.Invoke(overlayName);
        ActiveOverlaysChanged?.Invoke();
    }

    public void Toggle(string overlayName)
    {
        if (IsActive(overlayName))
            Stop(overlayName);
        else
            Start(overlayName);
    }

    public void StopAll()
    {
        List<CommonAbstractOverlay> snapshot;

        lock (_lock)
        {
            snapshot = LiveList.ToList();
            LiveList.Clear();
        }

        foreach (var overlay in snapshot)
        {
            try
            {
                PersistEnabled(overlay.Name, false);
                overlay.Stop();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        if (snapshot.Count > 0)
            ActiveOverlaysChanged?.Invoke();
    }

    /// <summary>
    /// Applies the given settings: persists them and starts/stops the HUD
    /// according to settings.
    /// </summary>
    public void ApplySettings(string overlayName, OverlaySettingsJson settings)
    {
        if (string.IsNullOrWhiteSpace(overlayName) || settings is null)
            return;

        // 1. Persist as last-applied (root)
        OverlaySettings.SaveOverlaySettings(overlayName, settings);

        // 2. Disabled → stop
        if (!settings.Enabled)
        {
            Stop(overlayName);
            return;
        }

        // 3. Enabled → start or update live instance
        lock (_lock)
        {
            var live = LiveList.Find(o => o.Name == overlayName);
            if (live is not null)
            {
                // Already running: push position (and anything else the base supports)
                live.X = settings.X;
                live.Y = settings.Y;
                // If your base type has a reload-config helper, call it here.
                return;
            }
        }

        // Not running → Start (prefer that Start() loads X/Y from OverlaySettings)
        Start(overlayName);
    }

    public void SetRepositionMode(bool enabled)
    {
        lock (_lock)
        {
            foreach (var overlay in LiveList)
                overlay.EnableReposition(enabled);
        }
    }

    private static void PersistEnabled(string overlayName, bool enabled)
    {
        var settings = OverlaySettings.LoadOverlaySettings(overlayName)
                       ?? new OverlaySettingsJson();

        settings.Enabled = enabled;
        OverlaySettings.SaveOverlaySettings(overlayName, settings);
    }
}