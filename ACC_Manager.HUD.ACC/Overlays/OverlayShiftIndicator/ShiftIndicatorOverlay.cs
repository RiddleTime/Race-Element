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

            [ToolTip("Sets the Width of the shift indicator bar.")]
            [IntRange(100, 400, 10)]
            internal int Width { get; set; } = 200;


            [ToolTip("Sets the Height of the shift indicator bar.")]
            [IntRange(20, 50, 5)]
            internal int Height { get; set; } = 35;

            public ShiftIndicatorConfig()
            {
                this.AllowRescale = true;
            }
        }

        public ShiftIndicatorOverlay(Rectangle rectangle) : base(rectangle, "Shift Indicator Overlay")
        {
            AllowReposition = true;
            this.Height = _config.Height + 1;
            this.Width = _config.Width + 1;
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

            DrawRpmBar(g);
        }

        private void DrawRpmBar(Graphics g)
        {
            const int cornerRadius = 10;

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), new Rectangle(0, 0, _config.Width, _config.Height), cornerRadius);

            double maxRpm = pageStatic.MaxRpm;
            double currentRpm = pagePhysics.Rpms;
            double percent = 0;

            if (maxRpm > 0 && currentRpm > 0) percent = currentRpm / maxRpm;


            if (percent > 0)
            {
                Color rpmColor = Color.FromArgb(120, 255, 255, 255);

                if (percent > 0.90)
                    rpmColor = Color.FromArgb(120, Color.OrangeRed);
                if (percent > 0.97)
                    rpmColor = Color.FromArgb(120, 255, 0, 0);

                g.FillRoundedRectangle(new SolidBrush(rpmColor), new Rectangle(0, 0, (int)(_config.Width * percent), _config.Height), cornerRadius);
            }
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
