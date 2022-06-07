using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayShiftIndicator
{
    internal class ShiftIndicatorOverlay : AbstractOverlay
    {
        private readonly ShiftIndicatorConfig _config = new ShiftIndicatorConfig();
        private class ShiftIndicatorConfig : OverlayConfiguration
        {

            public ShiftIndicatorConfig()
            {
                this.AllowRescale = true;
            }
        }

        private const int _barWidth = 200;
        private const int _barHeight = 35;

        public ShiftIndicatorOverlay(Rectangle rectangle) : base(rectangle, "Shift Indicator Overlay")
        {
            AllowReposition = true;
            this.Height = _barHeight + 1;
            this.Width = _barWidth + 1;
        }

        public sealed override void BeforeStart()
        {

        }

        public sealed override void BeforeStop()
        {

        }

        public sealed override void Render(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            const int cornerRadius = 10;

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), new Rectangle(0, 0, _barWidth, _barHeight), cornerRadius);

            double maxRpm = pageStatic.MaxRpm;
            double currentRpm = pagePhysics.Rpms;
            double percent = 0;

            if (maxRpm > 0 && currentRpm > 0) percent = currentRpm / maxRpm;


            if (percent > 0)
            {
                Color rpmColor = Color.FromArgb(120, 255, 255, 255);

                if (percent > 0.85)
                    rpmColor = Color.FromArgb(120, Color.OrangeRed);
                if (percent > 0.95)
                    rpmColor = Color.FromArgb(120, 255, 0, 0);

                g.FillRoundedRectangle(new SolidBrush(rpmColor), new Rectangle(0, 0, (int)(_barWidth * percent), _barHeight), cornerRadius);
            }
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
