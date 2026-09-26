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
/// Overlay start / stop / apply and active-instance tracking.
/// Stop / StopAll wait until overlay.Stop() has returned.
/// StopAll always drains ACC + Common package lists (CurrentGame may already have switched).
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

    public event Action<string>? OverlayStarted;
    public event Action<string>? OverlayStopped;
    public event Action? ActiveOverlaysChanged;

    private List<CommonAbstractOverlay> LiveList => ListFor(GameManager.CurrentGame);

    private static List<CommonAbstractOverlay> ListFor(Game game) => game switch
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
                return;

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

                PersistEnabled(overlayName, true, overlay.GameWhenStarted);

                OverlayStarted?.Invoke(overlayName);
                ActiveOverlaysChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }

    /// <summary>
    /// Stops one HUD and waits until overlay.Stop() has finished (Close/Dispose).
    /// </summary>
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

            PersistEnabled(overlayName, false, overlay.GameWhenStarted);
        }

        if (overlay is not null)
        {
            try { overlay.Stop(); }
            catch (Exception ex) { Debug.WriteLine(ex); }
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

    /// <summary>
    /// Turns reposition off, then stops every live HUD in <b>both</b> ACC and Common
    /// package lists and waits for each Stop().
    /// </summary>
    /// <param name="persistDisabled">
    /// True (default): write Enabled=false using each overlay's GameWhenStarted (profile apply).
    /// False: close windows only — keep previous game's overlay json (game change).
    /// </param>
    public void StopAll(bool persistDisabled = true)
    {
        List<CommonAbstractOverlay> snapshot = [];

        lock (_lock)
        {
            snapshot.AddRange(OverlaysAcc.ActiveOverlays);
            snapshot.AddRange(CommonHuds.ActiveOverlays);

            foreach (var o in snapshot)
            {
                try { o.EnableReposition(false); }
                catch (Exception ex) { Debug.WriteLine(ex); }
            }

            OverlaysAcc.ActiveOverlays.Clear();
            CommonHuds.ActiveOverlays.Clear();
        }

        foreach (var overlay in snapshot)
        {
            try
            {
                if (persistDisabled)
                    PersistEnabled(overlay.Name, false, overlay.GameWhenStarted);

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

        OverlaySettings.SaveOverlaySettings(overlayName, settings);

        if (!settings.Enabled)
        {
            Stop(overlayName);
            return;
        }

        lock (_lock)
        {
            var live = LiveList.Find(o => o.Name == overlayName);
            if (live is not null)
            {
                live.X = settings.X;
                live.Y = settings.Y;
                return;
            }
        }

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

    private static void PersistEnabled(string overlayName, bool enabled, Game gameWhenStarted = Game.Any)
    {
        var settings = OverlaySettings.LoadOverlaySettings(overlayName, gameWhenStarted)
                       ?? new OverlaySettingsJson();

        settings.Enabled = enabled;
        OverlaySettings.SaveOverlaySettings(overlayName, settings, gameWhenStarted);
    }
}