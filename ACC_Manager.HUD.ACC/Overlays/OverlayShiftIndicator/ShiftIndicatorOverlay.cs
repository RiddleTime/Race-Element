﻿using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
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
            [IntRange(160, 500, 10)]
            internal int Width { get; set; } = 200;

            [ToolTip("Sets the Height of the shift indicator bar.")]
            [IntRange(20, 45, 5)]
            internal int Height { get; set; } = 35;

            [ToolTip("Displays the current RPM inside of the shift indicator bar.")]
            internal bool ShowRpm { get; set; } = false;

            [ToolTip("Displays when the pit limiter is active.")]
            internal bool ShowPitLimiter { get; set; } = false;


            public ShiftIndicatorConfig()
            {
                this.AllowRescale = true;
            }
        }

        private Font _font;
        private float _halfRpmStringWidth = -1;
        private float _halfPitLimiterStringWidth = -1;

        public ShiftIndicatorOverlay(Rectangle rectangle) : base(rectangle, "Shift Indicator Overlay")
        {
            AllowReposition = true;
            this.Height = _config.Height + 1;
            this.Width = _config.Width + 1;
        }

        public sealed override void BeforeStart()
        {
            if (_config.ShowRpm || _config.ShowPitLimiter)
                _font = FontUtil.FontUnispace(15);
        }

        public sealed override void BeforeStop() { }

        public sealed override void Render(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.TextContrast = 1;

            // draw background
            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), new Rectangle(0, 0, _config.Width, _config.Height), 10);

            if (_config.ShowPitLimiter && pagePhysics.PitLimiterOn)
            {
       
                DrawPitLimiterBar(g);

                string pitLimiter = "!Pit Limiter!";

                if (_halfPitLimiterStringWidth < 0)
                    _halfPitLimiterStringWidth = g.MeasureString(pitLimiter, _font).Width / 2;

                g.DrawStringWithShadow(pitLimiter, _font, Brushes.White, new PointF(_config.Width / 2 - _halfPitLimiterStringWidth, _config.Height / 2 - _font.Height / 2 + 1));
            }
            else
                DrawRpmBar(g);

            if (_config.ShowRpm)
                DrawRpmText(g);
        }

        private int _limiterColorSwitch = 0;
        private Pen _limiterBackground = Pens.Yellow;
        private void DrawPitLimiterBar(Graphics g)
        {
            g.DrawRoundedRectangle(_limiterBackground, new Rectangle(0, 0, _config.Width, _config.Height), 10);

            if (_limiterColorSwitch > this.RefreshRateHz / 3)
            {
                _limiterBackground = _limiterBackground == Pens.Yellow ? Pens.Transparent : Pens.Yellow;
                _limiterColorSwitch = 0;
            }

            _limiterColorSwitch++;
        }

        private void DrawRpmText(Graphics g)
        {
            if (_config.ShowPitLimiter && pagePhysics.PitLimiterOn)
                return;

            string currentRpm = $"{pagePhysics.Rpms}".FillStart(4, ' ');

            if (_halfRpmStringWidth < 0)
                _halfRpmStringWidth = g.MeasureString(currentRpm, _font).Width / 2;

            g.DrawStringWithShadow(currentRpm, _font, Brushes.White, new PointF(_config.Width / 2 - _halfRpmStringWidth, _config.Height / 2 - _font.Height / 2 + 1));
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
                if (percent > 0.975)
                    rpmColor = Color.FromArgb(195, 255, 7, 7);

                if (percent > 1)
                    rpmColor = Color.Red;

                percent.Clip(0.05, 1);

                g.FillRoundedRectangle(new SolidBrush(rpmColor), new Rectangle(0, 0, (int)(_config.Width * percent), _config.Height), 10);
            }
        }

        public sealed override bool ShouldRender()
        {
            return DefaultShouldRender();
        }
    }
}
