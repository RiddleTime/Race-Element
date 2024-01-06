using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlaySpeedometerV2;

#if DEBUG
[Overlay(Name = "Speedometer V2", Description = "(WIP)")]
#endif
internal sealed class SpeedometerV2Overlay(Rectangle rectangle) : AbstractOverlay(rectangle, "Speedometer V2")
{
    private readonly SpeedometerConfiguration _config = new();
    private sealed class SpeedometerConfiguration : OverlayConfiguration
    {
        public SpeedometerConfiguration() => AllowRescale = true;

        [ConfigGrouping("Settings", "Adjust general settings for the Speedometer overlay.")]
        public SettingsGrouping Settings { get; init; } = new SettingsGrouping();
        public sealed class SettingsGrouping
        {
            [IntRange(180, 340, 1)]
            public int MaxSpeed { get; init; } = 300;
        }
    }

    public override void SetupPreviewData()
    {
        pagePhysics.SpeedKmh = 180;
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
        int padding = 25;
        DrawSpeedIndicator(g, padding, padding, 150);
    }

    private void DrawSpeedIndicator(Graphics g, int x, int y, int size)
    {
        double speedPercentage = pagePhysics.SpeedKmh / _config.Settings.MaxSpeed;
        speedPercentage.Clip(0, 1);

        Rectangle rect = new(x, y, size, size);

        DrawGauge(g, rect, 100, 240, speedPercentage);


        Font font = FontUtil.FontOrbitron(24);
        using StringFormat format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        string speedText = $"{pagePhysics.SpeedKmh:F1}";
        g.DrawStringWithShadow(speedText, font, Brushes.White, rect, format);
    }

    private static void DrawGauge(Graphics g, Rectangle rect, float startAngle, float sweepAngle, double percentage)
    {
        percentage.Clip(0, 1);
        // drawBackground
        float backgroundWidth = 16;
        using Pen backgroundPen = new(Color.Black, backgroundWidth);
        g.DrawArc(backgroundPen, rect, startAngle, sweepAngle);

        // drawIndicator
        float indicatorWidth = 10;
        using Pen indicatorPen = new(Color.White, indicatorWidth);
        g.DrawArc(indicatorPen, rect, startAngle, (float)Math.Ceiling(sweepAngle * percentage));
    }
}

