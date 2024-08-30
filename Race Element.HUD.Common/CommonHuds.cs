using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using System.Reflection;

namespace RaceElement.HUD.Common;
public static class CommonHuds
{
    private static readonly object _lock = new();

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
                if (overlayType.Game != 0)
                {
                    if (!overlayType.Game.HasFlag(GameManager.CurrentGame))
                    {
                        continue;
                    }
                }

                AbstractOverlays.Add(overlayType.Name, type);
            }
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
