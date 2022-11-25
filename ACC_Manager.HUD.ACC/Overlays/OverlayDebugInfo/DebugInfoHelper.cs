using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo
{
    internal class DebugInfoHelper
    {
        public class DebugConfig : OverlayConfiguration
        {
            [ConfigGrouping("Dock", "Provides settings for overlay docking.")]
            public DockConfigGrouping Dock { get; set; } = new DockConfigGrouping();
            public class DockConfigGrouping
            {
                [ToolTip("Allows you to reposition this debug panel.")]
                public bool Undock { get; set; } = false;
            }

            public DebugConfig()
            {
                this.AllowRescale = true;
            }
        }

        public static DebugInfoHelper Instance { get; } = new DebugInfoHelper();

        private List<AbstractOverlay> _infoOverlays = new List<AbstractOverlay>();

        public void AddOverlay(AbstractOverlay overlay)
        {
            _infoOverlays.Add(overlay);
            WidthChanged.Invoke(this, true);
        }

        public void RemoveOverlay(AbstractOverlay overlay)
        {
            _infoOverlays.Remove(overlay);
            if (WidthChanged != null)
                WidthChanged.Invoke(this, true);
        }

        public int GetX(AbstractOverlay overlay)
        {
            int x = 0;
            for (int i = 0; i < _infoOverlays.Count; i++)
            {
                if (_infoOverlays[i] == overlay)
                {
                    return x;
                }
                x += _infoOverlays[i].Width + 5;
            }
            return 0;
        }

        public event EventHandler<bool> WidthChanged;
    }
}
