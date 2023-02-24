using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Overlay.OverlayUtil.InfoPanel
{
    public abstract class Panel
    {
        internal readonly RectangleF _rect;
        public Panel(RectangleF rect)
        {
            _rect = rect;
        }
    }
}
