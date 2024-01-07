using RaceElement.HUD.ACC.Overlays.Driving.OverlayCarDash;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlaySpeedometerV2;

[Overlay(Name = "Speedometer V2", Description = "(WIP)")]
internal sealed class SpeedometerV2Overlay(Rectangle rectangle) : AbstractOverlay(rectangle, "Speedometer V2")
{
    private readonly SpeedometerConfiguration _config = new();

    public override void SetupPreviewData()
    {
        pagePhysics.SpeedKmh = 169;
        pagePhysics.Rpms = 8523;
        pageStatic.MaxRpm = 9250;
    }

    public override void BeforeStart()
    {
        Width = 200;
        Height = 200;
    }

    public override void Render(Graphics g)
    {
        g.CompositingQuality = CompositingQuality.AssumeLinear;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        int padding = 25;
        DrawSpeedIndicator(g, padding, padding, Width - padding * 2);
    }

    private void DrawSpeedIndicator(Graphics g, int x, int y, int size)
    {
        double speedPercentage = pagePhysics.SpeedKmh / _config.Settings.MaxSpeed;
        speedPercentage.Clip(0, 1);

        Rectangle rect = new(x, y, size, size);

        bool positiveDegrees = _config.Shape.Direction == SpeedometerConfiguration.GuageDirection.Clockwise;
        DrawGauge(g, rect, _config.Shape.StartAngle, _config.Shape.SweepAngle, speedPercentage, positiveDegrees);

        Font font = FontUtil.FontConthrax(34);
        using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        string speedText = $"{pagePhysics.SpeedKmh:F0}";
        g.DrawStringWithShadow(speedText, font, Brushes.White, rect, format);
    }

    private void DrawGauge(Graphics g, Rectangle rect, float startAngle, float sweepAngle, double percentage, bool positiveDegrees)
    {
        percentage.Clip(0, 1);

        float calculatedAngle = (float)Math.Ceiling(sweepAngle * percentage);
        if (!positiveDegrees)
        {
            startAngle += sweepAngle;
            sweepAngle *= -1;
            calculatedAngle *= -1;
        }

        float backgroundWidth = 16;
        using Pen backgroundPen = new(Color.Black, backgroundWidth);
        g.DrawArc(backgroundPen, rect, startAngle, sweepAngle);

        float indicatorWidth = 10;
        using Pen indicatorPen = new(Color.White, indicatorWidth);
        g.DrawArc(indicatorPen, rect, startAngle, calculatedAngle);
    }
}
