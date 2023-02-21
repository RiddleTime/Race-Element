using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayWind
{
    [Overlay(Name = "Wind", Description = "Shows wind direction vs heading",
        OverlayType = OverlayType.Release,
        OverlayCategory = OverlayCategory.Weather,
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
                public int Size { get; set; } = 150;
            }

            public WindOverlayConfiguration() => AllowRescale = true;
        }

        private CachedBitmap _background;

        public WindOverlay(Rectangle rectangle) : base(rectangle, "Wind")
        {
            Width = _config.Shape.Size;
            Height = _config.Shape.Size;
            RefreshRateHz = 5;
        }

        public sealed override void BeforeStart() => RenderBackground();

        private void RenderBackground()
        {
            int scaledSize = (int)(_config.Shape.Size * Scale);

            _background = new CachedBitmap(scaledSize, scaledSize, g =>
            {
                g.DrawEllipse(new Pen(Color.FromArgb(195, 0, 0, 0), 10), new Rectangle(5, 5, scaledSize - 10, scaledSize - 10));
            });
        }

        public sealed override void BeforeStop() => _background?.Dispose();

        public sealed override void Render(Graphics g)
        {
            _background?.Draw(g, 0, 0, _config.Shape.Size, _config.Shape.Size);

            double vaneAngle = pageGraphics.WindDirection * -1;
            double carDirection = (pagePhysics.Heading * -180d) / Math.PI;
            double relativeAngle = vaneAngle + carDirection;

            // draw Blown To Direction of Wind in pen color
            g.DrawArc(new Pen(Brushes.LimeGreen, 6), new Rectangle(5, 5, _config.Shape.Size - 10, _config.Shape.Size - 10), (float)relativeAngle - 35, 20);
        }
    }
}
