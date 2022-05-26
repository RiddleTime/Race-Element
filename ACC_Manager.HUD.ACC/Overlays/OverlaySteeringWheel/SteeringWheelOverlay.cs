using ACCManager.Data;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;
using static ACCManager.Data.ConversionFactory;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.HUD.ACC.Overlays.OverlaySteeringWheel
{
    internal sealed class SteeringWheelOverlay : AbstractOverlay
    {

        private int height = 150;
        private int width = 150;
        private int wheelWidth = 15;
        private int indicatorWidth = 15;

        private readonly SteeringWheelConfig config = new SteeringWheelConfig();

        internal class SteeringWheelConfig : OverlayConfiguration
        {
            [ToolTip("Draw steering indicator with a transparent backround.")]
            public bool TransparentBackground { get; set; } = false;

            public SteeringWheelConfig()
            {
                this.AllowRescale = true;
            }
        }

        public SteeringWheelOverlay(Rectangle rectangle) : base(rectangle, "Steering Wheel Overlay")
        {
            this.Height = height;
            this.Width = width;
        }

        public override void BeforeStart()
        {
            
        }

        public override void BeforeStop()
        {
            
        }

        public override void Render(Graphics g)
        {

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (config.TransparentBackground)
            {
                Rectangle graphRect = new Rectangle(0, 0, Width, Height);
                SolidBrush bkgrndBrush = new SolidBrush(Color.FromArgb(100, Color.Black));
                g.FillRoundedRectangle(bkgrndBrush, graphRect, 10);
            }

            Pen wheelPen = new Pen(Color.FromArgb(100, 150, 150, 150), wheelWidth);
            g.DrawCircle(wheelPen, this.Width / 2, this.Height / 2, (this.Height / 2) - wheelWidth);

            SolidBrush brush = new SolidBrush(Color.Red);
            StringFormat format = new StringFormat();
            Font font = FontUtil.FontUnispace(10);
            float accSteering = (pagePhysics.SteerAngle + 1) * 100; // map acc value to 0 - 200
            float angle = toAngle(accSteering);
#if DEBUG
            g.DrawString($"angle: {angle.ToString("000.00")}", font, brush, 10, Height-15, format);
#endif

            Rectangle rect = new Rectangle(0+wheelWidth, 0+wheelWidth, Width-(2*wheelWidth), Height-(2*wheelWidth));
            Pen indicatorPen = new Pen(Color.FromArgb(255, 255, 10, 10), wheelWidth);
            float drawAngle = angle+270-(indicatorWidth/2); // - 90 - indicatorWidth;
            g.DrawArc(indicatorPen, rect, drawAngle, indicatorWidth);
             
        }

        private float toAngle(float input)
        {
            int steeringAngle = getMaxSteeringAngle();
            float steeringAngleMax = steeringAngle*2;
            float wheelInputMax = 200;
            return ((steeringAngleMax) / (wheelInputMax) * (input))-(float)steeringAngle;

        }

        private int getMaxSteeringAngle()
        {
            //CarModels model = ConversionFactory.ParseCarName(pageStatic.CarModel);
            //ICarSetupConversion setupConversion = GetConversion(model);
            // TODO get max steering angle from acc data
            return 320;
        }

        public override bool ShouldRender()
        {
#if DEBUG
            return true;
#endif

            bool shouldRender = true;
            if (pageGraphics.Status == AcStatus.AC_OFF || pageGraphics.Status == AcStatus.AC_PAUSE || (pageGraphics.IsInPitLane == true && !pagePhysics.IgnitionOn))
                shouldRender = false;

            return shouldRender;
        }
    }
}

