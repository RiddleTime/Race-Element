using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ACCManager.HUD.ACC
{
    public class OverlaysACC
    {
        public static SortedDictionary<string, Type> AbstractOverlays = new SortedDictionary<string, Type>();
        public static List<AbstractOverlay> ActiveOverlays = new List<AbstractOverlay>();

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
            lock (ActiveOverlays)
                while (ActiveOverlays.Count > 0)
                {
                    ActiveOverlays.ElementAt(0).EnableReposition(false);
                    ActiveOverlays.ElementAt(0).Stop();
                    ActiveOverlays.ElementAt(0).Dispose();
                    ActiveOverlays.Remove(ActiveOverlays.ElementAt(0));
                }
        }
    }
}
