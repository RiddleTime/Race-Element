using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayTrackPosition
{
    internal sealed class TrackPositionOverlay : AbstractOverlay
    {
        public TrackPositionOverlay(Rectangle rectangle) : base(rectangle, "Overlay Track Position")
        {
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop()
        {
        }

        public override void Render(Graphics g)
        {
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
