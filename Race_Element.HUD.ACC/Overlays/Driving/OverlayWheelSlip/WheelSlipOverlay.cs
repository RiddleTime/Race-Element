using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayWheelSlip
{
    [Overlay(Name = "Wheel Slip",
        Description = "Shows wheel slip angle and amount of each tyre.",
        OverlayCategory = OverlayCategory.Physics,
        OverlayType = OverlayType.Release)]
    internal sealed class WheelSlipOverlay : AbstractOverlay
    {
        private readonly WheelSlipConfiguration _config = new WheelSlipConfiguration();
        private sealed class WheelSlipConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Data", "Adjust the data displayed.")]
            public DataGrouping Data { get; set; } = new DataGrouping();
            public sealed class DataGrouping
            {
                [ToolTip("Adjust maximum amount of wheel slip displayed.")]
                [FloatRange(0.5f, 10f, 0.1f, 1)]
                public float MaxSlipAmount { get; set; } = 2f;
            }

            [ConfigGrouping("Shape", "Adjust the shape.")]
            public ShapeGrouping Shape { get; set; } = new ShapeGrouping();
            public sealed class ShapeGrouping
            {
                [ToolTip("Adjust maximum amount of wheel slip displayed.")]
                [IntRange(40, 100, 2)]
                public int WheelSize { get; set; } = 52;
            }

            public WheelSlipConfiguration() => AllowRescale = true;
        }

        private CachedBitmap _cachedCircleBackground;
        private Brush _wheelBrush;
        private Pen _wheelPen;

        public WheelSlipOverlay(Rectangle rectangle) : base(rectangle, "Wheel Slip")
        {
            RefreshRateHz = 30;
        }

        public override void SetupPreviewData()
        {
            pagePhysics.WheelSlip = new float[] { 0.3f, 0.3f, 0.6f, 0.745f };
            pagePhysics.SlipAngle = new float[] { 0, 0, -0.45f, -0.35f };
        }

        public override void BeforeStart()
        {
            _wheelBrush = new SolidBrush(Color.FromArgb(175, Color.Red));
            _wheelPen = new Pen(Brushes.White, 4);

            int scaledRadius = (int)(_config.Shape.WheelSize * Scale);
            _cachedCircleBackground = new CachedBitmap(scaledRadius + 1, scaledRadius + 1, g =>
            {
                var wheelRect = new Rectangle(0, 0, scaledRadius, scaledRadius);

                using GraphicsPath gradientPath = new GraphicsPath();
                gradientPath.AddEllipse(wheelRect);
                using PathGradientBrush pthGrBrush = new PathGradientBrush(gradientPath);
                pthGrBrush.CenterColor = Color.FromArgb(40, 0, 0, 0);
                pthGrBrush.SurroundColors = new Color[] { Color.FromArgb(220, 0, 0, 0) };

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
            _wheelBrush?.Dispose();
            _wheelPen?.Dispose();
        }

        public override void Render(Graphics g)
        {
            int baseX = 2;
            int baseY = 2;
            int wheelSize = _config.Shape.WheelSize;
            int gap = 8;

            DrawWheelSlip(g, baseX + 0, baseY + 0, wheelSize, Wheel.FrontLeft);
            DrawWheelSlip(g, baseX + wheelSize + gap, baseY + 0, wheelSize, Wheel.FrontRight);
            DrawWheelSlip(g, baseX + 0, baseY + wheelSize + gap, wheelSize, Wheel.RearLeft);
            DrawWheelSlip(g, baseX + wheelSize + gap, baseY + wheelSize + gap, wheelSize, Wheel.RearRight);
        }

        private void DrawWheelSlip(Graphics g, int x, int y, int size, Wheel wheel)
        {
            var wheelRect = new Rectangle(x, y, size, size);

            // draw outline
            _cachedCircleBackground?.Draw(g, x, y, size, size);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // draw wheel specific slip based on outline size
            float wheelSlip = pagePhysics.WheelSlip[(int)wheel];
            wheelSlip.ClipMax(_config.Data.MaxSlipAmount);

            float percentage = (float)wheelSlip * 100 / _config.Data.MaxSlipAmount;
            percentage.ClipMax(100);
            int centerX = x + size / 2;
            int centerY = y + size / 2;

            using GraphicsPath gradientPath = new GraphicsPath();
            gradientPath.AddEllipse(wheelRect);
            using PathGradientBrush pthGrBrush = new PathGradientBrush(gradientPath);
            pthGrBrush.CenterColor = Color.FromArgb(185, 255, 0, 0);
            pthGrBrush.SurroundColors = new Color[] { Color.FromArgb(40, 0, 0, 0) };

            g.FillEllipse(pthGrBrush, centerX, centerY, size / 2 * percentage / 100);

            float slipAngle = (float)(pagePhysics.SlipAngle[(int)wheel] * 180d / Math.PI * 2) - 90;
            g.DrawArc(_wheelPen, wheelRect, slipAngle - 10, 20);
        }
    }
}
