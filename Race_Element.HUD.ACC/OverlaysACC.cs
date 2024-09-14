using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace RaceElement.HUD.ACC;

public sealed class OverlaysAcc
{
    private static readonly object _lock = new();

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

    public static void CloseAll()
    {
        lock (_lock)
            while (ActiveOverlays.Count > 0)
            {
                ActiveOverlays[0].EnableReposition(false);
                ActiveOverlays[0].Stop();
                ActiveOverlays.Remove(ActiveOverlays[0]);
            }

        Thread.Sleep(2000);
    }
}
