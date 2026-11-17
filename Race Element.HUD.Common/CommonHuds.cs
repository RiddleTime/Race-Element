using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace RaceElement.HUD.Common;
public static class CommonHuds
{
    private static readonly Lock _lock = new();

    public static readonly SortedDictionary<string, Type> AbstractOverlays = [];
    public static readonly List<CommonAbstractOverlay> ActiveOverlays = [];

    public static void GenerateDictionary()
    {
        AbstractOverlays.Clear();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsDefined(typeof(OverlayAttribute))))
        {
            OverlayAttribute overlayType = type.GetCustomAttribute<OverlayAttribute>();
            if (overlayType != null && !AbstractOverlays.ContainsKey(overlayType.Name))
            {
                // extra filter for game specific overlays
                if (overlayType.SupportedGames != Game.Any)
                {
                    if (!overlayType.SupportedGames.HasFlag(GameManager.CurrentGame))
                    {
                        continue;
                    }
                }

                if (overlayType.UnsupportedGames != Game.Any)
                {
                    if (overlayType.UnsupportedGames.HasFlag(GameManager.CurrentGame))
                    {
                        continue;
                    }
                }

                AbstractOverlays.Add(overlayType.Name, type);
            }
        }
    }

    /// <summary>
    /// Stops every active common overlay on the calling thread.
    /// Previously spawned an STA thread + Join which deadlocked with the UI thread
    /// (Mouse.SetCursor / NativeWindow affinity) when HUDs were enabled.
    /// </summary>
    public static void CloseAll()
    {
        lock (_lock)
        {
            while (ActiveOverlays.Count > 0)
            {
                var overlay = ActiveOverlays[0];
                try
                {
                    overlay.EnableReposition(false);
                    overlay.Stop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                ActiveOverlays.RemoveAt(0);
            }
        }
    }
}
