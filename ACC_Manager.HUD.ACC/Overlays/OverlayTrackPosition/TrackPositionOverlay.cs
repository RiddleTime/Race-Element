using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayTrackPosition
{
    internal sealed class TrackPositionOverlay : AbstractOverlay
    {
        public TrackPositionOverlay(Rectangle rectangle, string Name) : base(rectangle, Name)
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

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }
    }
}
