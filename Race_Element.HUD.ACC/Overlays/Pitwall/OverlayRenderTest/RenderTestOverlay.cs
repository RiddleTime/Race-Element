using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayAbc
{
    [Overlay(Name = "Render Test",
        Description = "Heat Your Room",
        OverlayType = OverlayType.Pitwall)]
    internal sealed class RenderTestOverlay : AbstractOverlay
    {
        private readonly RenderTestConfiguration _config = new();
        private sealed class RenderTestConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Render", "Higher Is More Heat")]
            public TestGrouping Test { get; set; } = new TestGrouping();
            public class TestGrouping
            {
                internal int Herz = 100;

                [FloatRange(0.001f, 0.160f, 0.001f, 3)]
                public float TimeMultiplier { get; set; } = 0.008f;

                [IntRange(2, 40, 2)]
                public int Elements { get; set; } = 20;

                [ToolTip("Decreases performance")]
                public bool FpsCounter { get; set; } = false;
            }
        }

        private CachedBitmap _cachedPositiveImage;
        private CachedBitmap _cachedNegativeImage;

        private const int initialSize = 800;
        public RenderTestOverlay(Rectangle rectangle) : base(rectangle, "Render Test")
        {
            Width = initialSize;
            Height = Width;
            this.RefreshRateHz = _config.Test.Herz;
        }

        private Font _font;

        public override void BeforeStart()
        {
            _font = FontUtil.FontSegoeMono(16);

            int scaledWidth = (int)(Width * Scale);
            int scaledHeight = (int)(Height * Scale);
            _cachedPositiveImage = new CachedBitmap(scaledWidth, scaledHeight, g =>
            {
                using SolidBrush brushBackground = new(Color.FromArgb(14, 0, 0, 0));
                int boxSize = 16;
                Rectangle box = new(scaledWidth / 2 - boxSize / 2, scaledHeight / 2 - boxSize / 2, boxSize, boxSize);

                g.FillEllipse(brushBackground, box);

                if (_font != null)
                {
                    using StringFormat format = new()
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                    };
                    using SolidBrush brushForeground = new(Color.FromArgb(185, 255, 69, 0));
                    g.DrawStringWithShadow("+", _font, brushForeground, box, format);
                }
            });

            _cachedNegativeImage = new CachedBitmap(scaledWidth, scaledHeight, g =>
            {
                using SolidBrush brushBackground = new(Color.FromArgb(14, 0, 0, 0));
                int boxSize = 16;
                Rectangle box = new(scaledWidth / 2 - boxSize / 2, scaledHeight / 2 - boxSize / 2, boxSize, boxSize);

                g.FillEllipse(brushBackground, box);

                if (_font != null)
                {
                    using StringFormat format = new()
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                    };
                    using SolidBrush brushForeground = new(Color.FromArgb(185, 69, 255, 0));
                    g.DrawStringWithShadow("-", _font, brushForeground, box, format);
                }
            });
        }

        public override void BeforeStop()
        {
            _font?.Dispose();
            _cachedPositiveImage?.Dispose();
            _cachedNegativeImage?.Dispose();
        }

        public override bool ShouldRender() => true;

        private float multiplier = 0;

        private DateTime _lastTime;
        private int _framesRendered = 0;
        private double _fps = 0;

        public override void Render(Graphics g)
        {
            if (_config.Test.FpsCounter)
            {
                UpdateFps();
                DrawFpsCounter(g);
            }

            using (Matrix transform = g.Transform)
            {
                for (int i = 1; i <= _config.Test.Elements; i++)
                {
                    int toggle = i % 2 == 0 ? -1 : 1;
                    int translateDivider = 40;
                    float sinus = (float)(Math.Sin(i * multiplier / translateDivider)) * toggle;

                    transform.Translate((sinus * initialSize) * Scale / translateDivider, (sinus * initialSize * Scale / translateDivider) * toggle);
                    transform.RotateAt(sinus * 2 * 180, new PointF((initialSize * Scale / 2), (initialSize * Scale / 2)));
                    transform.Shear(-sinus / 50, sinus / 50);
                    g.Transform = transform;
                    _cachedPositiveImage.Draw(g, 0, 0, (int)(Width / Scale), (int)(Height / Scale));
                }
                g.ResetTransform();

                for (int i = 1; i <= _config.Test.Elements; i++)
                {
                    int toggle = i % 2 == 0 ? -1 : 1;
                    int translateDivider = 40;
                    float sinus = (float)(Math.Sin(i * multiplier * -1 / translateDivider)) * toggle;

                    transform.Translate((sinus * initialSize) * Scale / translateDivider, (sinus * initialSize * Scale / translateDivider) * toggle);
                    transform.RotateAt(sinus * 2 * 180, new PointF((initialSize * Scale / 2), (initialSize * Scale / 2)));
                    transform.Shear(-sinus / 50, sinus / 50);
                    g.Transform = transform;
                    _cachedNegativeImage.Draw(g, 0, 0, (int)(Width / Scale), (int)(Height / Scale));
                }
                g.ResetTransform();

            }

            multiplier += _config.Test.TimeMultiplier;
            _framesRendered++;
        }

        private void UpdateFps()
        {
            const double measureSeconds = 1;
            if ((DateTime.Now - _lastTime).TotalMilliseconds > measureSeconds * 1000)
            {
                _fps = _framesRendered / measureSeconds;
                _framesRendered = 0;
                _lastTime = DateTime.Now;
            }
        }

        private void DrawFpsCounter(Graphics g)
        {
            Rectangle fpsRect = new(0, 0, (int)(120 * this.Scale), _font.Height);
            g.DrawStringWithShadow($"{_fps:F0}", _font, Brushes.White, fpsRect);
        }
    }
}
