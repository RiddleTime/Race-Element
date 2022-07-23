using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.HUD.ACC.Overlays.OverlaySlipAngle
{
#if DEBUG
    [Overlay(Name = "Slip Angle", Version = 1.00,
        Description = "Shows Slip angle", OverlayType = OverlayType.Release)]
#endif
    internal sealed class SlipAngleOverlay : AbstractOverlay
    {
        private readonly SlipConfiguration _config = new SlipConfiguration();
        private class SlipConfiguration : OverlayConfiguration
        {
            public SlipConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        private readonly InfoPanel _panel;
        public SlipAngleOverlay(Rectangle rectangle) : base(rectangle, "Slip Angle Overlay")
        {
            const int width = 220;
            _panel = new InfoPanel(12, width) { FirstRowLine = 0 };

            this.Width = width;
            this.Height = _panel.FontHeight * 5;
        }

        public sealed override void BeforeStart()
        {
        }

        public sealed override void BeforeStop()
        {
        }

        public sealed override void Render(Graphics g)
        {
            float slipRatioFront = (pagePhysics.WheelSlip[(int)Wheel.FrontLeft] + pagePhysics.WheelSlip[(int)Wheel.FrontRight]) / 2;
            float slipRatioRear = (pagePhysics.WheelSlip[(int)Wheel.RearLeft] + pagePhysics.WheelSlip[(int)Wheel.RearRight]) / 2;

            string type = "Neutral";
            if (slipRatioRear > slipRatioFront)
            {
                float diff = slipRatioRear - slipRatioFront;

                if (diff > 0.01)
                    type = "Oversteer";
            }
            if (slipRatioRear < slipRatioFront)
            {
                float diff = slipRatioFront - slipRatioRear;
                if (diff > 0.01)
                    type = "Understeer";
            }

            _panel.AddLine("slipFront", $"{slipRatioFront:F2}");
            _panel.AddLine("slipRear", $"{slipRatioRear:F2}");
            _panel.AddLine("Oversteer", $"{(slipRatioRear - slipRatioFront):F2}");
            _panel.AddLine("Balance", type);

            _panel.Draw(g);

        }

        public sealed override bool ShouldRender() => DefaultShouldRender();
    }
}
