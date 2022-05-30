using ACCManager.Data.ACC.Session;
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

        public sealed override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            bool shouldRender = true;
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            if (pageGraphics.GlobalRed)
                shouldRender = false;

            if (RaceSessionState.IsPreSession(pageGraphics.GlobalRed, broadCastRealTime.Phase))
                shouldRender = true;

            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE)
                shouldRender = false;

            return shouldRender;
        }
    }
}
