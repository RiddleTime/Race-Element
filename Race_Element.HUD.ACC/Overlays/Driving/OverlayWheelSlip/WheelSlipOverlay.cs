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
        Description = "Shows wheel slip of each tyre",
        OverlayCategory = OverlayCategory.Physics,
        OverlayType = OverlayType.Release)]
    internal sealed class WheelSlipOverlay : AbstractOverlay
    {
        private readonly WheelSlipConfiguration _config = new WheelSlipConfiguration();
        private sealed class WheelSlipConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Slip Settings", "Adjust the configuration for the wheel slip hud")]
            public SlipGrouping Slip { get; set; } = new SlipGrouping();
            public sealed class SlipGrouping
            {
                [FloatRange(0.5f, 5f, 0.1f, 1)]
                public float MaxSlipAmount { get; set; } = 2f;
            }

            public WheelSlipConfiguration() => AllowRescale = true;
        }

        private CachedBitmap _cachedCircleBackground;
        private const int _wheelRadius = 50;
        private Brush _wheelBrush;
        private Pen _wheelPen;

        public WheelSlipOverlay(Rectangle rectangle) : base(rectangle, "Wheel Slip")
        {
            RefreshRateHz = 10;
            Width = 130;
            Height = 130;
        }

        public override void SetupPreviewData()
        {
            pagePhysics.WheelSlip = new float[] { 0.3f, 0.3f, 0.6f, 0.745f };
            pagePhysics.SlipAngle = new float[] { 0.01f, 0.02f, -0.2f, -0.3f };
        }

        public override void BeforeStart()
        {
            _wheelBrush = new SolidBrush(Color.FromArgb(183, Color.Red));
            _wheelPen = new Pen(Brushes.White, 4);

            int scaledRadius = (int)(_wheelRadius * Scale);
            _cachedCircleBackground = new CachedBitmap(scaledRadius + 1, scaledRadius + 1, g =>
            {
                var wheelRect = new Rectangle(1, 1, scaledRadius - 1, scaledRadius - 1);

                using GraphicsPath gradientPath = new GraphicsPath();
                gradientPath.AddEllipse(wheelRect);
                using PathGradientBrush pthGrBrush = new PathGradientBrush(gradientPath);
                pthGrBrush.CenterColor = Color.FromArgb(40, 0, 0, 0);
                pthGrBrush.SurroundColors = new Color[] { Color.FromArgb(220, 0, 0, 0) };

                g.FillEllipse(pthGrBrush, wheelRect);

                g.DrawEllipse(Pens.Red, wheelRect);
            });

        }

        public override void BeforeStop()
        {
            _cachedCircleBackground?.Dispose();
            _wheelBrush?.Dispose();
            _wheelPen?.Dispose();
        }

        public override void Render(Graphics g)
        {
            int baseX = 10;
            int baseY = 10;
            int wheelSize = _wheelRadius;
            int gap = 10;

            DrawWheelSlip(g, baseX + 0, baseY + 0, wheelSize, Wheel.FrontLeft);
            DrawWheelSlip(g, baseX + wheelSize + gap, baseY + 0, wheelSize, Wheel.FrontRight);
            DrawWheelSlip(g, baseX + 0, baseY + wheelSize + gap, wheelSize, Wheel.RearLeft);
            DrawWheelSlip(g, baseX + wheelSize + gap, baseY + wheelSize + gap, wheelSize, Wheel.RearRight);
        }

        private void DrawWheelSlip(Graphics g, int x, int y, int size, Wheel wheel)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            var wheelRect = new Rectangle(x, y, size, size);

            // draw outline
            _cachedCircleBackground?.Draw(g, x, y, size, size);

            // draw wheel specific slip based on outline size
            float wheelSlip = pagePhysics.WheelSlip[(int)wheel];
            wheelSlip.ClipMax(_config.Slip.MaxSlipAmount);

            float percentage = (float)wheelSlip * 100 / _config.Slip.MaxSlipAmount;
            percentage.ClipMax(100);
            int centerX = x + size / 2;
            int centerY = y + size / 2;

            g.FillEllipse(_wheelBrush, centerX, centerY, size / 2 * percentage / 100);

            float slipAngle = (float)(pagePhysics.SlipAngle[(int)wheel] * 180d / Math.PI * 2) - 90;
            g.DrawArc(_wheelPen, wheelRect, slipAngle - 10, 20);
        }
    }
}
