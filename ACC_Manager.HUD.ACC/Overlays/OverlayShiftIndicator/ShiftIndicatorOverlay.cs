using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
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

            [ToolTip("Displays the current RPM inside of the shift indicator bar.")]
            internal bool ShowRpm { get; set; } = false;

            public ShiftIndicatorConfig()
            {
                this.AllowRescale = true;
            }
        }

        private Font _rpmFont;
        private float _halfRpmStringWidth = -1;

        public ShiftIndicatorOverlay(Rectangle rectangle) : base(rectangle, "Shift Indicator Overlay")
        {
            AllowReposition = true;
            this.Height = _config.Height + 1;
            this.Width = _config.Width + 1;
        }

        public sealed override void BeforeStart()
        {
            if (_config.ShowRpm)
                _rpmFont = FontUtil.FontUnispace(15);
        }

        public sealed override void BeforeStop() { }

        public sealed override void Render(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawRpmBar(g);

            if (_config.ShowRpm)
            {
                string currentRpm = $"{pagePhysics.Rpms}".FillStart(4, ' ');

                if (_halfRpmStringWidth < 0)
                    _halfRpmStringWidth = g.MeasureString(currentRpm, _rpmFont).Width / 2;

                g.DrawStringWithShadow(currentRpm, _rpmFont, Brushes.White, new PointF(_config.Width / 2 - _halfRpmStringWidth, _config.Height / 2 - _rpmFont.Height / 2 + 1));
            }
        }

        private void DrawRpmBar(Graphics g)
        {
            const int cornerRadius = 10;

            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), new Rectangle(0, 0, _config.Width, _config.Height), cornerRadius);

            double maxRpm = pageStatic.MaxRpm;
            double currentRpm = pagePhysics.Rpms;
            double percent = 0;

            if (maxRpm > 0 && currentRpm > 0)
                percent = currentRpm / maxRpm;

            if (percent > 0)
            {
                Color rpmColor = Color.FromArgb(120, 255, 255, 255);

                if (percent > 0.94)
                    rpmColor = Color.FromArgb(195, 255, 120, 7);
                if (percent > 0.975)
                    rpmColor = Color.FromArgb(195, 255, 7, 7);

                if (percent > 1)
                    rpmColor = Color.Red;

                percent.Clip(0.05, 1);

                g.FillRoundedRectangle(new SolidBrush(rpmColor), new Rectangle(0, 0, (int)(_config.Width * percent), _config.Height), cornerRadius);
            }
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
