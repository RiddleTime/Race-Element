using RaceElement.HUD.Overlay.Internal;
using System.Reflection;

namespace RaceElement.HUD.Common;
public static class CommonHuds
{
    private static readonly object _lock = new();

    public static readonly SortedDictionary<string, Type> AbstractOverlays = [];
    public static readonly List<AbstractOverlay> ActiveOverlays = [];

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
    }
}
