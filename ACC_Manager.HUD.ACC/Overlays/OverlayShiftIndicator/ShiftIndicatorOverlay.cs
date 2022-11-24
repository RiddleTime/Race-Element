using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ACCManager.HUD.ACC.Overlays.OverlayShiftIndicator
{
    [Overlay(Name = "Shift Indicator", Version = 1.00,
        Description = "A bar showing the current RPM, optionally showing when the pit limiter is enabled.")]
    internal class ShiftIndicatorOverlay : AbstractOverlay
    {
        private readonly ShiftIndicatorConfig _config = new ShiftIndicatorConfig();
        private class ShiftIndicatorConfig : OverlayConfiguration
        {
            [ConfigGrouping("Bar", "The shape and options of the shift indicator bar.")]
            public BarsGrouping Bar { get; set; } = new BarsGrouping();
            public class BarsGrouping
            {
                [ToolTip("Sets the Width of the shift indicator bar.")]
                [IntRange(160, 600, 10)]
                public int Width { get; set; } = 300;

                [ToolTip("Sets the Height of the shift indicator bar.")]
                [IntRange(20, 45, 5)]
                public int Height { get; set; } = 30;

                [ToolTip("Displays the current RPM inside of the shift indicator bar.")]
                public bool ShowRpm { get; set; } = true;

                [ToolTip("Displays when the pit limiter is active.")]
                public bool ShowPitLimiter { get; set; } = true;
            }

            public ShiftIndicatorConfig()
            {
                this.AllowRescale = true;
            }
        }

        private string _lastCar = string.Empty;
        private CachedBitmap _cachedBackground;
        private CachedBitmap _cachedRpmLines;
        private CachedBitmap _cachedPitLimiterOutline;

        private Font _font;
        private float _halfRpmStringWidth = -1;
        private float _halfPitLimiterStringWidth = -1;

        public ShiftIndicatorOverlay(Rectangle rectangle) : base(rectangle, "Shift Indicator Overlay")
        {
            this.RefreshRateHz = 40;
            AllowReposition = true;
            this.Height = _config.Bar.Height + 1;
            this.Width = _config.Bar.Width + 1;
        }

        public sealed override void BeforeStart()
        {
            if (_config.Bar.ShowRpm || _config.Bar.ShowPitLimiter)
                _font = FontUtil.FontUnispace(15);

            _cachedBackground = new CachedBitmap((int)(_config.Bar.Width * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), (int)(6 * Scale));
                g.DrawRoundedRectangle(Pens.DarkGray, new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), (int)(6 * Scale));
            });

            _cachedRpmLines = new CachedBitmap((int)(_config.Bar.Width * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                int lineCount = (int)Math.Floor(pageStatic.MaxRpm / 1000d);

                int leftOver = pageStatic.MaxRpm % 1000;
                if (leftOver < 70)
                    lineCount--;

                Pen linePen = new Pen(new SolidBrush(Color.FromArgb(90, Color.White)), 2);

                double thousandPercent = 1000d / pageStatic.MaxRpm * lineCount;
                double baseX = (_config.Bar.Width * this.Scale) / lineCount * thousandPercent;
                for (int i = 1; i <= lineCount; i++)
                {
                    int x = (int)(i * baseX);

                    g.DrawLine(linePen, x, 1, x, (_config.Bar.Height * this.Scale) - 1);

                    if (i == lineCount - 1)
                    {


                    }
                }

            });

            if (_config.Bar.ShowPitLimiter)
                _cachedPitLimiterOutline = new CachedBitmap((int)(_config.Bar.Width * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
                {
                    g.DrawRoundedRectangle(new Pen(Color.Yellow, 2.5f), new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), (int)(6 * Scale));
                });
        }

        public sealed override void BeforeStop()
        {
            if (_cachedBackground != null)
                _cachedBackground.Dispose();

            if (_cachedRpmLines != null)
                _cachedRpmLines.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.TextContrast = 1;

            if (_cachedBackground != null)
                _cachedBackground.Draw(g, _config.Bar.Width, _config.Bar.Height);

            if (_config.Bar.ShowPitLimiter && pagePhysics.PitLimiterOn)
            {
                DrawPitLimiterBar(g);

                string pitLimiter = "!Pit Limiter!";

                if (_halfPitLimiterStringWidth < 0)
                    _halfPitLimiterStringWidth = g.MeasureString(pitLimiter, _font).Width / 2;

                g.DrawStringWithShadow(pitLimiter, _font, Brushes.White, new PointF(_config.Bar.Width / 2 - _halfPitLimiterStringWidth, _config.Bar.Height / 2 - _font.Height / 2 + 1));
            }
            else
                DrawRpmBar(g);

            if (_config.Bar.ShowRpm)
                DrawRpmText(g);
        }

        private int _limiterColorSwitch = 0;
        private void DrawPitLimiterBar(Graphics g)
        {
            if (_limiterColorSwitch > this.RefreshRateHz / 5)
                _cachedPitLimiterOutline?.Draw(g, 0, 0, _config.Bar.Width, _config.Bar.Height);

            if (_limiterColorSwitch > this.RefreshRateHz / 2) // makes this flash 3 times a second
                _limiterColorSwitch = 0;

            _limiterColorSwitch++;
        }

        private void DrawRpmText(Graphics g)
        {
            if (_config.Bar.ShowPitLimiter && pagePhysics.PitLimiterOn)
                return;

            string currentRpm = $"{pagePhysics.Rpms}".FillStart(4, ' ');

            if (_halfRpmStringWidth < 0)
                _halfRpmStringWidth = g.MeasureString(currentRpm, _font).Width / 2;

            g.DrawStringWithShadow(currentRpm, _font, Brushes.White, new PointF(_config.Bar.Width / 2 - _halfRpmStringWidth, _config.Bar.Height / 2 - _font.Height / 2 + 1));
        }

        private void DrawRpmBar(Graphics g)
        {
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
                if (percent > 0.973)
                    rpmColor = Color.FromArgb(195, 255, 7, 7);

                if (percent > 1)
                    rpmColor = Color.Red;

                percent.Clip(0.05d, 1d);

                g.FillRoundedRectangle(new SolidBrush(rpmColor), new Rectangle(0, 0, (int)(_config.Bar.Width * percent), _config.Bar.Height), 6);
            }

            DrawRpmBar1kLines(g);
        }

        private void DrawRpmBar1kLines(Graphics g)
        {
            if (_lastCar != pageStatic.CarModel)
            {
                _cachedRpmLines.Render();
                _lastCar = pageStatic.CarModel;
            }

            _cachedRpmLines.Draw(g, _config.Bar.Width, _config.Bar.Height);
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
