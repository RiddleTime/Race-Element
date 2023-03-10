using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System;
using System.Buffers.Text;
using System.Drawing;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayWheelSlip
{
    [Overlay(Name = "Wheel Slip",
        Description = "Shows wheel slip of each tyre",
        OverlayCategory = OverlayCategory.Physics,
        OverlayType = OverlayType.Release)]
    internal sealed class WheelSlipOverlay : AbstractOverlay
    {
        private WheelSlipConfiguration _config = new WheelSlipConfiguration();
        private class WheelSlipConfiguration : OverlayConfiguration
        {
            public WheelSlipConfiguration() => AllowRescale = true;
        }

        public WheelSlipOverlay(Rectangle rectangle) : base(rectangle, "Wheel Slip")
        {
            RefreshRateHz = 20;
            Width = 130;
            Height = 130;
        }

        public override void SetupPreviewData()
        {
            pagePhysics.WheelSlip = new float[] { 0.3f, 0.6f, 0.3f, 0.745f };
        }

        public override void Render(Graphics g)
        {
            int baseX = 10;
            int baseY = 10;
            int wheelSize = 50;
            int gap = 10;

            DrawWheelSlip(g, baseX + 0, baseY + 0, wheelSize, Wheel.FrontLeft);
            DrawWheelSlip(g, baseX + wheelSize + gap, baseY + 0, wheelSize, Wheel.FrontRight);
            DrawWheelSlip(g, baseX + 0, baseY + wheelSize + gap, wheelSize, Wheel.RearLeft);
            DrawWheelSlip(g, baseX + wheelSize + gap, baseY + wheelSize + gap, wheelSize, Wheel.RearRight);
        }

        private void DrawWheelSlip(Graphics g, int x, int y, int size, Wheel wheel)
        {
            var wheelRect = new Rectangle(x, y, size, size);
            // draw outline
            g.FillEllipse(Brushes.Black, wheelRect);
            g.DrawEllipse(Pens.Red, wheelRect);


            // draw wheel specific slip based on outline size
            float wheelSlip = pagePhysics.WheelSlip[(int)wheel];
            float maxSlip = 1.5f;

            wheelSlip.ClipMax(maxSlip);

            float percentage = (float)wheelSlip * 100 / maxSlip;
            percentage.ClipMax(100);
            int centerX = x + size / 2;
            int centerY = y + size / 2;

            g.FillEllipse(Brushes.Red, centerX, centerY, size / 2 * percentage / 100);

            float slipAngle = (float)(pagePhysics.SlipAngle[(int)wheel] * 180d / Math.PI) - 90;
            g.DrawArc(new Pen(Brushes.White, 4), wheelRect, slipAngle - 5, 10);
        }
    }
}
