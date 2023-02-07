using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using RaceElement.Util.SystemExtensions;
using RaceElement.HUD.Overlay.Util;
using System.Drawing.Text;
using System.IO;
using System.Diagnostics;

namespace RaceElement.HUD.ACC.Overlays.OverlayLapDelta
{
    [Overlay(Name = "Lap Delta", Description = "A Delta Bar", OverlayType = OverlayType.Release, Version = 1)]
    internal class LapDeltaOverlay : AbstractOverlay
    {
        private readonly LapDeltaConfiguration _config = new LapDeltaConfiguration();
        private sealed class LapDeltaConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Delta", "Adjust how the delta is displayed")]
            public DeltaGrouping Delta { get; set; } = new DeltaGrouping();
            public class DeltaGrouping
            {
                [ToolTip("Sets the amount of decimals.")]
                [IntRange(1, 3, 1)]
                public int Decimals { get; set; } = 2;

                [ToolTip("Sets the maximum range in seconds for the delta bar.")]
                [IntRange(1, 5, 1)]
                public int MaxDelta { get; set; } = 2;
            }

            [ConfigGrouping("Bar", "Adjust bar behavior.")]
            public BarGrouping Bar { get; set; } = new BarGrouping();
            public class BarGrouping
            {
                [ToolTip("Sets the Width of the Delta Bar.")]
                [IntRange(300, 800, 10)]
                public int Width { get; set; } = 300;

                [ToolTip("Sets the Height of the Delta Bar.")]
                [IntRange(25, 55, 5)]
                public int Height { get; set; } = 30;

                [ToolTip("Sets the color when the delta is positive (slower).")]
                public Color PositiveColor { get; set; } = Color.FromArgb(255, Color.OrangeRed);
                [ToolTip("Sets the color when the delta is negative (faster).")]
                public Color NegativeColor { get; set; } = Color.FromArgb(255, Color.LimeGreen);
            }

            public LapDeltaConfiguration()
            {
                this.AllowRescale = true;
            }
        }
        private float _deltaStringWidth = -1;
        private Font _font;

        private CachedBitmap _cachedBackground;
        private CachedBitmap _cachedPositiveDelta;
        private CachedBitmap _cachedNegativeDelta;

        public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta")
        {
            this.Width = _config.Bar.Width + 1;
            this.Height = _config.Bar.Height + 1;
        }

        public override void BeforeStart()
        {
            int cornerRadius = (int)(10 * this.Scale);

            float height = _config.Bar.Height - 14;
            height.Clip(13, 22);
            _font = FontUtil.FontUnispace(height);

            this.Height += (int)(height * 1.6);

            _cachedBackground = new CachedBitmap((int)(_config.Bar.Width * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                Color bgColor = Color.FromArgb(185, 0, 0, 0);
                HatchBrush hatchBrush = new HatchBrush(HatchStyle.LightDownwardDiagonal, bgColor, Color.FromArgb(bgColor.A - 50, bgColor));
                g.FillRoundedRectangle(hatchBrush, new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
                g.DrawRoundedRectangle(new Pen(Color.Black, 1 * this.Scale), new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
            });

            _cachedPositiveDelta = new CachedBitmap((int)(_config.Bar.Width / 2 * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                Rectangle rect = new Rectangle(0, 0, (int)(_config.Bar.Width / 2 * this.Scale), (int)(_config.Bar.Height * this.Scale));
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rect, cornerRadius, 0, 0, cornerRadius);
                g.FillPath(new SolidBrush(Color.FromArgb(185, _config.Bar.PositiveColor)), path);
            });

            _cachedNegativeDelta = new CachedBitmap((int)(_config.Bar.Width / 2 * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                Rectangle rect = new Rectangle(0, 0, (int)(_config.Bar.Width / 2 * this.Scale), (int)(_config.Bar.Height * this.Scale));
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rect, 0, cornerRadius, cornerRadius, 0);
                g.FillPath(new SolidBrush(Color.FromArgb(185, _config.Bar.NegativeColor)), path);
            });
        }

        public override void BeforeStop()
        {
            _cachedBackground?.Dispose();
            _cachedPositiveDelta?.Dispose();
            _cachedNegativeDelta?.Dispose();

            _font?.Dispose();
        }

        public override bool ShouldRender() => DefaultShouldRender();

        public override void Render(Graphics g)
        {
            _cachedBackground?.Draw(g, 0, 0, _config.Bar.Width, _config.Bar.Height);

            DrawDeltaBar(g);

            DrawDeltaText(g);
        }

        private void DrawDeltaBar(Graphics g)
        {
            float delta = (float)pageGraphics.DeltaLapTimeMillis / 1000;
            delta.Clip(-_config.Delta.MaxDelta, _config.Delta.MaxDelta);

            if (delta > 0)
            {
                float fillPercent = delta / _config.Delta.MaxDelta;
                float halfBarWidth = _config.Bar.Width / 2f;
                float drawWidth = halfBarWidth * fillPercent;

                g.SetClip(new Rectangle((int)(halfBarWidth - drawWidth), 0, (int)drawWidth, _config.Bar.Height));
                _cachedPositiveDelta?.Draw(g, 0, 0, (int)halfBarWidth, _config.Bar.Height);
                g.ResetClip();
            }
            if (delta < 0)
            {
                float fillPercent = delta / -_config.Delta.MaxDelta;
                float halfBarWidth = _config.Bar.Width / 2f;
                float drawWidth = halfBarWidth * fillPercent;

                g.SetClip(new Rectangle((int)(halfBarWidth), 0, (int)drawWidth, _config.Bar.Height));
                _cachedNegativeDelta?.Draw(g, (int)halfBarWidth, 0, (int)halfBarWidth, _config.Bar.Height);
                g.ResetClip();
            }
        }

        private void DrawDeltaText(Graphics g)
        {
            double delta = (double)pageGraphics.DeltaLapTimeMillis / 1000;
            delta.Clip(-9.999, 9.999);

            string currentDelta = $"{delta.ToString($"F{_config.Delta.Decimals}")}";
            if (delta > 0) currentDelta = "+" + currentDelta;
            currentDelta.FillStart(_config.Delta.Decimals + 3, ' ');

            if (_deltaStringWidth < 0)
                _deltaStringWidth = g.MeasureString(currentDelta, _font).Width;

            int x = _config.Bar.Width / 2;
            int y = _config.Bar.Height;
            DrawTextWithOutline(g, Color.White, currentDelta, x, y);
        }

        private void DrawTextWithOutline(Graphics g, Color textColor, string text, int x, int y)
        {
            Rectangle backgroundDimension = new Rectangle((int)(x - _deltaStringWidth / 2), y, (int)(_deltaStringWidth), (int)(_font.Height * 0.9));

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(185, 0, 0, 0)), backgroundDimension, (int)(2 * this.Scale));

            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.TextContrast = 1;

            g.DrawStringWithShadow(text, _font, textColor, new PointF(x - _deltaStringWidth / 2, y), 1.3f);
        }

    }
}
