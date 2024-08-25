using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Common.Overlays.OverlayWind;

[Overlay(Name = "Wind Direction (BETA)", Description = "Shows wind direction relative to car heading. For iRacing the wind direction is on the pit straight - not at the player's location.",
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Track,
    Version = 1.00,
Authors = ["Reinier Klarenberg"])]
internal sealed class WindDirectionOverlay : CommonAbstractOverlay
{
    private readonly WindDirectionConfiguration _config = new();
    private sealed class WindDirectionConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Wind", "Adjust settings related to the wind")]
        public WindGrouping Wind { get; init; } = new WindGrouping();
        public sealed class WindGrouping
        {
            [ToolTip("Below this wind speed(km/h) the hud will hide itself.")]
            [FloatRange(0f, 10f, 0.5f, 1)]
            public float ShowThreshold { get; init; } = 0.5f;
        }

        [ConfigGrouping("Shape", "Adjust the shape")]
        public ShapeGrouping Shape { get; init; } = new ShapeGrouping();
        public sealed class ShapeGrouping
        {
            [IntRange(100, 200, 1)]
            public int Size { get; init; } = 120;
        }

        public WindDirectionConfiguration() => GenericConfiguration.AllowRescale = true;
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
            using Pen outlinePen = new(Color.FromArgb(165, 0, 0, 0), 18 * this.Scale);
            g.DrawEllipse(outlinePen, new Rectangle(scaledPadding / 2, scaledPadding / 2, scaledSize - scaledPadding, scaledSize - scaledPadding));
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

    public sealed override bool ShouldRender()
    {
        if (_config.Wind.ShowThreshold > SessionData.Instance.Weather.AirVelocity && !this.IsRepositioning)
            return false;

        return base.ShouldRender();
    }

    public sealed override void Render(Graphics g)
    {
        _background?.Draw(g, 0, 0, _config.Shape.Size, _config.Shape.Size);

        double vaneAngle = SessionData.Instance.Weather.AirDirection;
        double carDirection = 90 + (SimDataProvider.LocalCar.Physics.Heading * -180d) / Math.PI;
        double relativeAngle = vaneAngle + carDirection;
        
        g.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle rect = new(padding / 2, padding / 2, _config.Shape.Size - padding, _config.Shape.Size - padding);

        // draw relative angle (blowing to)
        using Pen limeGreenPen = new(Brushes.LimeGreen, 16);
        g.DrawArc(limeGreenPen, rect, (float)relativeAngle - 4, 8);

        // draw angle where the wind is coming from
        using Pen redPen = new(Brushes.Red, 8);
        g.DrawArc(redPen, rect, (float)relativeAngle - 180 - 35, 70);

        _textCell?.UpdateText($"{SessionData.Instance.Weather.AirVelocity:F1}");
        _textCell?.Draw(g, 1f / Scale);
    }
}
