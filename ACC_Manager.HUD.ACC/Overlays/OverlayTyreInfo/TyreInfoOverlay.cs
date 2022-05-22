using ACCManager.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACCManager.HUD.Overlay.Util;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.ACC.Overlays.OverlayPressureTrace;
using ACCManager.HUD.Overlay.OverlayUtil;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.HUD.ACC.Overlays.OverlayTyreInfo
{
    internal sealed class TyreInfoOverlay : AbstractOverlay
    {
        private TyreInfoConfig config = new TyreInfoConfig();
        private class TyreInfoConfig : OverlayConfiguration
        {
            public TyreInfoConfig()
            {
                this.AllowRescale = true;
            }
        }

        public TyreInfoOverlay(Rectangle rectangle) : base(rectangle, "Tyre Info Overlay")
        {
            this.Width = 150;
            this.Height = 200;
            this.RefreshRateHz = 10;
        }

        public sealed override void BeforeStart()
        {
        }

        public sealed override void BeforeStop()
        {
        }

        public sealed override void Render(Graphics g)
        {
            DrawPressureBackgrounds(g);


        }

        private void DrawPressureBackgrounds(Graphics g)
        {
            TyrePressureRange range = TyrePressures.GetCurrentRange(pageGraphics.TyreCompound, pageStatic.CarModel);

            if (range != null)
            {
                DrawPressureBackground(g, 0, 10, Wheel.FrontLeft, range);
                DrawPressureBackground(g, 76, 10, Wheel.FrontRight, range);
                DrawPressureBackground(g, 0, 178, Wheel.RearLeft, range);
                DrawPressureBackground(g, 76, 178, Wheel.RearRight, range);
            }
        }

        private void DrawPressureBackground(Graphics g, int x, int y, Wheel wheel, TyrePressureRange range)
        {
            SmoothingMode previous = g.SmoothingMode;

            Color brushColor = Color.FromArgb(80, 0, 255, 0);

            if (pagePhysics.WheelPressure[(int)wheel] >= range.OptimalMaximum)
                brushColor = Color.FromArgb(80, 255, 0, 0);

            if (pagePhysics.WheelPressure[(int)wheel] <= range.OptimalMinimum)
                brushColor = Color.FromArgb(80, 0, 0, 255);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillRoundedRectangle(new SolidBrush(brushColor), new Rectangle(x, y, 60, 20), 2);

            g.SmoothingMode = previous;
        }

        private bool IsInRange(double value, double min, double max)
        {
            return value < max && value > min;
        }

        public sealed override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif
            return false;
        }
    }
}
