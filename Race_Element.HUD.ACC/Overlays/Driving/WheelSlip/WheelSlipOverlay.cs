using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayWheelSlip;

[Overlay(Name = "Wheel Slip",
    Description = "Shows wheel slip angle and ratio of each tyre.",
    OverlayCategory = OverlayCategory.Physics,
    OverlayType = OverlayType.Drive)]
internal sealed class WheelSlipOverlay : AbstractOverlay
{
    private readonly WheelSlipConfiguration _config = new();
    private sealed class WheelSlipConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Data", "Adjust the data displayed.")]
        public DataGrouping Data { get; init; } = new DataGrouping();
        public sealed class DataGrouping
        {
            [ToolTip("Adjust maximum ratio of wheel slip displayed over all 4 wheels.")]
            [FloatRange(0.5f, 10f, 0.1f, 2)]
            public float MaxSlipRatio { get; init; } = 2f;

            [ToolTip("Adjust the amount of slip ratio offset required to show understeer color on the front wheels.")]
            [FloatRange(0.1f, 1.5f, 0.02f, 2)]
            public float UndersteerOffset { get; init; } = 0.3f;

            [ToolTip("Adjust the amount of slip ratio offset required to show oversteer color on the rear wheels.")]
            [FloatRange(0.1f, 1.5f, 0.02f, 2)]
            public float OversteerOffset { get; init; } = 0.3f;
        }

        [ConfigGrouping("Shape", "Adjust the shape.")]
        public ShapeGrouping Shape { get; set; } = new ShapeGrouping();
        public sealed class ShapeGrouping
        {
            [ToolTip("Adjust maximum amount of wheel slip displayed.")]
            [IntRange(40, 120, 2)]
            public int WheelSize { get; set; } = 92;
        }

        public WheelSlipConfiguration() => GenericConfiguration.AllowRescale = true;
    }

    private CachedBitmap _cachedCircleBackground;
    private Pen _wheelPen;

    public WheelSlipOverlay(Rectangle rectangle) : base(rectangle, "Wheel Slip")
    {
        RefreshRateHz = 30;
    }

    public override void SetupPreviewData()
    {
        pagePhysics.WheelSlip = [0.3f, 0.3f, 0.6f, 0.745f];
        pagePhysics.SlipAngle = [0, 0, -0.45f, -0.35f];
    }

    public override void BeforeStart()
    {
        _wheelPen = new Pen(Brushes.White, 4);

        int scaledRadius = (int)(_config.Shape.WheelSize * Scale);
        _cachedCircleBackground = new CachedBitmap(scaledRadius + 1, scaledRadius + 1, g =>
        {
            var wheelRect = new Rectangle(0, 0, scaledRadius, scaledRadius);

            using GraphicsPath gradientPath = new();
            gradientPath.AddEllipse(wheelRect);
            using PathGradientBrush pthGrBrush = new(gradientPath);
            pthGrBrush.CenterColor = Color.FromArgb(40, 0, 0, 0);
            pthGrBrush.SurroundColors = [Color.FromArgb(220, 0, 0, 0)];

            g.FillEllipse(pthGrBrush, wheelRect);
            g.DrawEllipse(Pens.Black, wheelRect);
        });


        int baseX = 2;
        int wheelSize = _config.Shape.WheelSize;
        int gap = 8;
        int size = baseX * 2 + wheelSize * 2 + gap;
        Width = size;
        Height = size;
    }

    public override void BeforeStop()
    {
        _cachedCircleBackground?.Dispose();
        _wheelPen?.Dispose();
    }

    public override void Render(Graphics g)
    {
        int baseX = 2;
        int baseY = 2;
        int wheelSize = _config.Shape.WheelSize;
        int gap = 8;

        float slipRatioFront = (pagePhysics.WheelSlip[(int)Wheel.FrontLeft] + pagePhysics.WheelSlip[(int)Wheel.FrontRight]) / 2;
        float slipRatioRear = (pagePhysics.WheelSlip[(int)Wheel.RearLeft] + pagePhysics.WheelSlip[(int)Wheel.RearRight]) / 2;

        bool isUnderSteering = slipRatioFront > slipRatioRear + _config.Data.UndersteerOffset;
        bool isOverSteering = slipRatioRear > slipRatioFront + _config.Data.OversteerOffset;

        Color oversteer = Color.FromArgb(185, 255, 0, 0);
        Color understeer = Color.FromArgb(185, 0, 0, 255);
        Color neutral = Color.FromArgb(185, 255, 255, 255);

        DrawWheelSlip(g, baseX + 0, baseY + 0, wheelSize, Wheel.FrontLeft, isUnderSteering ? understeer : neutral);
        DrawWheelSlip(g, baseX + wheelSize + gap, baseY + 0, wheelSize, Wheel.FrontRight, isUnderSteering ? understeer : neutral);
        DrawWheelSlip(g, baseX + 0, baseY + wheelSize + gap, wheelSize, Wheel.RearLeft, isOverSteering ? oversteer : neutral);
        DrawWheelSlip(g, baseX + wheelSize + gap, baseY + wheelSize + gap, wheelSize, Wheel.RearRight, isOverSteering ? oversteer : neutral);
    }

    private void DrawWheelSlip(Graphics g, int x, int y, int size, Wheel wheel, Color color)
    {
        var wheelRect = new Rectangle(x, y, size, size);

        // draw outline
        _cachedCircleBackground?.Draw(g, x, y, size, size);

        g.SmoothingMode = SmoothingMode.AntiAlias;

        // draw wheel specific slip based on outline size
        float wheelSlip = pagePhysics.WheelSlip[(int)wheel];
        wheelSlip.ClipMax(_config.Data.MaxSlipRatio);

        float percentage = wheelSlip * 100 / _config.Data.MaxSlipRatio;
        percentage.ClipMax(100);
        int centerX = x + size / 2;
        int centerY = y + size / 2;

        using GraphicsPath gradientPath = new();
        gradientPath.AddEllipse(wheelRect);
        using PathGradientBrush pthGrBrush = new(gradientPath);
        pthGrBrush.CenterColor = color;
        pthGrBrush.SurroundColors = [Color.FromArgb(40, 0, 0, 0)];

        g.FillEllipse(pthGrBrush, centerX, centerY, size / 2 * percentage / 100);

        float slipAngle = (float)(pagePhysics.SlipAngle[(int)wheel] * 180d / Math.PI * 2) - 90;
        g.DrawArc(_wheelPen, wheelRect, slipAngle - 10, 20);
    }
}
