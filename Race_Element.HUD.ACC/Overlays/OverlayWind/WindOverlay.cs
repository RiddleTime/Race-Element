using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.OverlayWind
{
    [Overlay(Name = "Wind", Description = "Shows wind direction vs car heading.",
        OverlayType = OverlayType.Release,
        OverlayCategory = OverlayCategory.Track,
        Version = 1.00)]
    internal sealed class WindOverlay : AbstractOverlay
    {
        private readonly WindOverlayConfiguration _config = new WindOverlayConfiguration();
        private sealed class WindOverlayConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Shape", "Adjust the shape")]
            public ShapeGrouping Shape { get; set; } = new ShapeGrouping();
            public sealed class ShapeGrouping
            {
                [IntRange(100, 200, 1)]
                public int Size { get; set; } = 120;
            }

            public WindOverlayConfiguration() => AllowRescale = true;
        }

        private CachedBitmap _background;
        private const int padding = 50;

        public WindOverlay(Rectangle rectangle) : base(rectangle, "Wind")
        {
            Width = _config.Shape.Size;
            Height = _config.Shape.Size;
            RefreshRateHz = 8;
        }

        public sealed override void BeforeStart() => RenderBackground();

        private void RenderBackground()
        {
            int scaledSize = (int)(_config.Shape.Size * Scale);

            int scaledPadding = (int)(padding * this.Scale);
            _background = new CachedBitmap(scaledSize + 1, scaledSize + 1, g =>
            {
                g.DrawEllipse(new Pen(Color.FromArgb(165, 0, 0, 0), 18 * this.Scale), new Rectangle(scaledPadding / 2, scaledPadding / 2, scaledSize - scaledPadding, scaledSize - scaledPadding));
            });
        }

        public sealed override void BeforeStop() => _background?.Dispose();

        public sealed override void Render(Graphics g)
        {
            _background?.Draw(g, 0, 0, _config.Shape.Size, _config.Shape.Size);

            double vaneAngle = pageGraphics.WindDirection;
            double carDirection = 90 + (pagePhysics.Heading * -180d) / Math.PI;
            double relativeAngle = vaneAngle + carDirection;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(padding / 2, padding / 2, _config.Shape.Size - padding, _config.Shape.Size - padding);

            // draw relative angle (blowing to)
            g.DrawArc(new Pen(Brushes.LimeGreen, 16), rect, (float)relativeAngle - 4, 8);

            g.DrawArc(new Pen(Brushes.Red, 8), rect, (float)relativeAngle - 180 - 35, 70);    // (float)relativeAngle - 180 - 4, 8)

        }
    }
}
