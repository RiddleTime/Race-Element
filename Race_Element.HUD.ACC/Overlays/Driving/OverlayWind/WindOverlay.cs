using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.OverlayWind;

[Overlay(Name = "Wind Direction", Description = "Shows wind direction relative to car heading.",
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Track,
    Version = 1.00)]
internal sealed class WindDirectionOverlay : AbstractOverlay
{
    private readonly WindDirectionConfiguration _config = new();
    private sealed class WindDirectionConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Wind", "Adjust settings related to the wind")]
        public WindGrouping Wind { get; set; } = new WindGrouping();
        public sealed class WindGrouping
        {
            [ToolTip("Below this wind speed(km/h) the hud will hide itself.")]
            [FloatRange(0f, 10f, 0.5f, 1)]
            public float ShowThreshold { get; set; } = 0.5f;
        }

        [ConfigGrouping("Shape", "Adjust the shape")]
        public ShapeGrouping Shape { get; set; } = new ShapeGrouping();
        public sealed class ShapeGrouping
        {
            [IntRange(100, 200, 1)]
            public int Size { get; set; } = 120;
        }

        public WindDirectionConfiguration() => AllowRescale = true;
    }

    private CachedBitmap _background;
    private DrawableTextCell _textCell;
    private const int padding = 50;

    public WindDirectionOverlay(Rectangle rectangle) : base(rectangle, "Wind Direction")
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
            Rectangle rect = new(scaledPadding / 2, scaledPadding / 2, scaledSize - scaledPadding, scaledSize - scaledPadding);
            using SolidBrush brush = new(Color.FromArgb(90, 0, 0, 0));
            g.FillEllipse(brush, rect);
            g.DrawEllipse(new Pen(Color.FromArgb(165, 0, 0, 0), 18 * this.Scale), new Rectangle(scaledPadding / 2, scaledPadding / 2, scaledSize - scaledPadding, scaledSize - scaledPadding));
        });

        Font font = FontUtil.FontSegoeMono(15f * Scale);
        Rectangle rect = new(0, 0, scaledSize, scaledSize);
        _textCell = new DrawableTextCell(rect, font);
    }

    public sealed override void BeforeStop()
    {
        _background?.Dispose();
        _textCell?.Dispose();
    }

    public override bool ShouldRender()
    {
        if (_config.Wind.ShowThreshold > pageGraphics.WindSpeed && !this.IsRepositioning)
            return false;

        return base.ShouldRender();
    }

    public sealed override void Render(Graphics g)
    {
        _background?.Draw(g, 0, 0, _config.Shape.Size, _config.Shape.Size);

        double vaneAngle = pageGraphics.WindDirection;
        double carDirection = 90 + (pagePhysics.Heading * -180d) / Math.PI;
        double relativeAngle = vaneAngle + carDirection;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle rect = new(padding / 2, padding / 2, _config.Shape.Size - padding, _config.Shape.Size - padding);

        // draw relative angle (blowing to)
        g.DrawArc(new Pen(Brushes.LimeGreen, 16), rect, (float)relativeAngle - 4, 8);

        // draw angle where the wind is coming from
        g.DrawArc(new Pen(Brushes.Red, 8), rect, (float)relativeAngle - 180 - 35, 70);

        _textCell?.UpdateText($"{pageGraphics.WindSpeed:F1}");
        _textCell?.Draw(g, 1f / Scale);
    }
}
