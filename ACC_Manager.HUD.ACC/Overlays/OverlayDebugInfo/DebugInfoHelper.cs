using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo
{
    internal class DebugInfoHelper
    {
        public class DebugConfig : OverlayConfiguration
        {
            [ToolTip("Allows you to reposition this debug panel.")]
            internal bool Undock { get; set; } = false;

            public DebugConfig()
            {
                this.AllowRescale = true;
            }
        }

        private static DebugInfoHelper _instance = new DebugInfoHelper();
        public static DebugInfoHelper Instance
        {
            get { return _instance; }
        }

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
