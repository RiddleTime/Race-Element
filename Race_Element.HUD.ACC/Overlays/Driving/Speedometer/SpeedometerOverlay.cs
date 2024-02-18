using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlaySpeedometer;

[Overlay(Name = "Speedometer",
    Description = "Displays a circular gauge with the current speed in km/h.",
    OverlayCategory = OverlayCategory.Driving,
    OverlayType = OverlayType.Drive,
Authors = ["Reinier Klarenberg"])]
internal sealed class SpeedometerOverlay(Rectangle rectangle) : AbstractOverlay(rectangle, "Speedometer")
{
    private readonly SpeedometerConfiguration _config = new();
    private const int ShapeSize = 180;
    private const int Padding = 18;
    private const int BackgroundWidth = 12;
    private const int IndicatorWidth = 6;

    private CachedBitmap _background;

    private Font _font;
    private SolidBrush _textBrush;
    private Pen _indicatorPen;

    public sealed override void SetupPreviewData()
    {
        pagePhysics.SpeedKmh = 269;
        pagePhysics.Rpms = 8523;
        pageStatic.MaxRpm = 9250;
    }

    public sealed override void BeforeStart()
    {
        Width = ShapeSize;
        Height = ShapeSize;
        RefreshRateHz = _config.Settings.RefreshRate;

        int scaledSize = (int)(ShapeSize * Scale);
        int scaledPadding = (int)(Padding * Scale);
        int scaledBackgroundWidth = (int)(BackgroundWidth * Scale);
        _background = new CachedBitmap(scaledSize, scaledSize, g =>
        {
            float startAngle = _config.Shape.StartAngle;
            float sweepAngle = _config.Shape.SweepAngle;

            if (_config.Shape.Direction != SpeedometerConfiguration.GuageDirection.Clockwise)
            {
                startAngle += sweepAngle;
                sweepAngle *= -1;
            }

            Rectangle rect = new(scaledPadding, scaledPadding, scaledSize - scaledPadding * 2, scaledSize - scaledPadding * 2);

            Color bgColor = Color.FromArgb(_config.Colors.BackgroundOpacity, _config.Colors.BackgroundColor);

            using Pen backgroundPen = new(bgColor, scaledBackgroundWidth);
            g.DrawArc(backgroundPen, rect, startAngle, sweepAngle);

            using SolidBrush backgroundBrush = new(bgColor);
            rect.Inflate(-scaledPadding / 2, -scaledPadding / 2);
            g.FillPie(backgroundBrush, rect, 0, 360);
        });

        _indicatorPen = new(_config.Colors.IndicatorColor, IndicatorWidth);
        _textBrush = new(_config.Colors.TextColor);
        _font = FontUtil.FontConthrax(33);
    }

    public sealed override void BeforeStop()
    {
        _background?.Dispose();
        _indicatorPen?.Dispose();
        _textBrush?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        g.CompositingQuality = CompositingQuality.AssumeLinear;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        _background?.Draw(g, 0, 0, ShapeSize, ShapeSize);

        DrawSpeedIndicator(g, Padding, Padding, ShapeSize - Padding * 2);
    }

    private void DrawSpeedIndicator(Graphics g, int x, int y, int size)
    {
        Rectangle rect = new(x, y, size, size);

        double speedPercentage = pagePhysics.SpeedKmh / _config.Settings.MaxSpeed;
        speedPercentage.Clip(0, 1);

        bool positiveDegrees = _config.Shape.Direction == SpeedometerConfiguration.GuageDirection.Clockwise;
        DrawGauge(g, rect, _config.Shape.StartAngle, _config.Shape.SweepAngle, speedPercentage, positiveDegrees);

        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        string speedText = $"{pagePhysics.SpeedKmh:F0}";
        g.DrawStringWithShadow(speedText, _font, _textBrush, rect, format);
    }

    private void DrawGauge(Graphics g, Rectangle rect, float startAngle, float sweepAngle, double percentage, bool positiveDegrees)
    {
        percentage.Clip(0, 1);

        float calculatedAngle = (float)Math.Ceiling(sweepAngle * percentage);
        if (!positiveDegrees)
        {
            startAngle += sweepAngle;
            calculatedAngle *= -1;
        }

        g.DrawArc(_indicatorPen, rect, startAngle, calculatedAngle);
    }
}
