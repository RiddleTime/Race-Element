using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace RaceElement.HUD.ACC;

public sealed class OverlaysAcc
{
    private static readonly Lock _lock = new();

    public static readonly SortedDictionary<string, Type> AbstractOverlays = [];
    public static readonly List<CommonAbstractOverlay> ActiveOverlays = [];

    protected OverlaysAcc() { }

    public static void GenerateDictionary()
    {
        AbstractOverlays.Clear();

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsDefined(typeof(OverlayAttribute))))
        {
            var overlayType = type.GetCustomAttribute<OverlayAttribute>();
            if (overlayType != null && !AbstractOverlays.ContainsKey(overlayType.Name))
                AbstractOverlays.Add(overlayType.Name, type);
        }
    }

    /// <summary>
    /// Stops every active ACC overlay on the calling thread.
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
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                ActiveOverlays.RemoveAt(0);
            }
        }
    }
}
