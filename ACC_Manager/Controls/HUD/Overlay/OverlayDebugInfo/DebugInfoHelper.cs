using ACCSetupApp.Controls.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayDebugInfo
{
    internal class DebugInfoHelper
    {
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
