using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlaySlipAngle
{
#if DEBUG
    [Overlay(Name = "Slip Angle", Version = 1.00,
        Description = "Shows Slip angle", OverlayType = OverlayType.Release)]
#endif
    internal class SlipAngleOverlay : AbstractOverlay
    {
        public SlipAngleOverlay(Rectangle rectangle) : base(rectangle, "Slip Angle Overlay")
        {
        }

        public override void BeforeStart()
        {
            throw new NotImplementedException();
        }

        public override void BeforeStop()
        {
            throw new NotImplementedException();
        }

        public override void Render(Graphics g)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldRender()
        {
            throw new NotImplementedException();
        }
    }
}
