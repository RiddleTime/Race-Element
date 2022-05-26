using ACCManager.Data.ACC.Cars;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System.Drawing;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlaySteeringWheel
{
    internal sealed class SteeringWheelOverlay : AbstractOverlay
    {

        private const int initialHeight = 150;
        private const int initialWidth = 150;
        private const int wheelWidth = 15;
        private const int indicatorWidth = 15;
        private const int inputCircleMinAngle = 135;
        private const int inputCircleSweepAngle = 270;

        private int innerWheelWidth = 0;
        private readonly SolidBrush bkgrndBrush;
        private readonly Pen wheelPen;
        private readonly Pen indicatorPen;
        private readonly Pen thottleIndicatorBkgrnd;
        private readonly Pen thottleIndicatorFrgrnd;
        private readonly Pen brakingIndicatorBkgrnd;
        private readonly Pen brakingIndicatorFrgrnd;
        private readonly SteeringWheelConfig config = new SteeringWheelConfig();

        internal class SteeringWheelConfig : OverlayConfiguration
        {
            [ToolTip("Draw steering indicator with a transparent backround.")]
            public bool TransparentBackground { get; set; } = false;

            [ToolTip("Show throttle input indicator.")]
            public bool ThrottleIndicator { get; set; } = true;
            [ToolTip("Show brake input indicator.")]
            public bool BrakeIndicator { get; set; } = true;
            [ToolTip("Show selected gear.")]
            public bool GearIndicator { get; set; } = true;


            public SteeringWheelConfig()
            {
                this.AllowRescale = true;
            }
        }

        public SteeringWheelOverlay(Rectangle rectangle) : base(rectangle, "Steering Wheel Overlay")
        {
            this.Height = initialHeight;
            this.Width = initialWidth;

            this.innerWheelWidth = wheelWidth / 2;
            this.bkgrndBrush = new SolidBrush(Color.FromArgb(100, Color.Black));
            this.wheelPen = new Pen(Color.FromArgb(100, 150, 150, 150), wheelWidth);
            this.indicatorPen = new Pen(Color.White, wheelWidth);
            this.thottleIndicatorBkgrnd = new Pen(Color.FromArgb(50, 10, 255, 10), this.innerWheelWidth);
            this.thottleIndicatorFrgrnd = new Pen(Color.FromArgb(255, 10, 255, 10), this.innerWheelWidth);
            this.brakingIndicatorBkgrnd = new Pen(Color.FromArgb(50, 255, 10, 10), this.innerWheelWidth);
            this.brakingIndicatorFrgrnd = new Pen(Color.FromArgb(255, 255, 10, 10), this.innerWheelWidth);

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
                Rectangle graphRect = new Rectangle(0, 0, this.Width, this.Height);
                g.FillRoundedRectangle(bkgrndBrush, graphRect, 10);
            }

            drawSteeringIndicator(g);
            drawThrottleIndicator(g);
            drawBrakingIndicator(g);
            drawGearIndicator(g);

        }

        private void drawGearIndicator(Graphics g)
        {
            if (!config.GearIndicator)
            {
                return;
            }

            string gearIndicator = "?";
            int accGear = pagePhysics.Gear;
            if (accGear == 0) gearIndicator = "R";
            else if (accGear == 1) gearIndicator = "N";
            else gearIndicator = ((pagePhysics.Gear) - 1).ToString();

            SolidBrush drawBrush = new SolidBrush(Color.White);
            Font drawFont = FontUtil.FontOrbitron(30);

            SizeF gearIndicatorSize = g.MeasureString(gearIndicator, drawFont);
            int xPos = (int)((Width / 2) - (gearIndicatorSize.Width / 2));
            int yPos = (int)((Height / 2) - (gearIndicatorSize.Height / 2));
            g.DrawString(gearIndicator, drawFont, drawBrush, xPos, yPos, new StringFormat());
        }

        private void drawBrakingIndicator(Graphics g)
        {
            if (!config.BrakeIndicator)
            {
                return;
            }

            // TODO add ABS indicator

            int x = wheelWidth * 2 + (config.ThrottleIndicator ? this.innerWheelWidth : 0);
            int y = wheelWidth * 2 + (config.ThrottleIndicator ? this.innerWheelWidth : 0);
            int width = Width - (4 * wheelWidth) - (2 * (config.ThrottleIndicator ? this.innerWheelWidth : 0));
            int height = Height - (4 * wheelWidth) - (2 * (config.ThrottleIndicator ? this.innerWheelWidth : 0));

            Rectangle rect = new Rectangle(x, y, width, height);
            g.DrawArc(brakingIndicatorBkgrnd, rect, inputCircleMinAngle, inputCircleSweepAngle);
            float brakeAngle = rescale(1, inputCircleSweepAngle, pagePhysics.Brake);
            g.DrawArc(brakingIndicatorFrgrnd, rect, inputCircleMinAngle, brakeAngle);
        }

        private void drawThrottleIndicator(Graphics g)
        {
            if (!config.ThrottleIndicator)
            {
                return;
            }

            Rectangle rect = new Rectangle(0 + wheelWidth * 2, 0 + wheelWidth * 2, Width - (4 * wheelWidth), Height - (4 * wheelWidth));
            g.DrawArc(thottleIndicatorBkgrnd, rect, inputCircleMinAngle, inputCircleSweepAngle);
            float throttleAngle = rescale(1, inputCircleSweepAngle, pagePhysics.Gas);
            g.DrawArc(thottleIndicatorFrgrnd, rect, inputCircleMinAngle, throttleAngle);
        }

        private void drawSteeringIndicator(Graphics g)
        {
            g.DrawCircle(wheelPen, this.Width / 2, this.Height / 2, (this.Height / 2) - wheelWidth);

            SolidBrush brush = new SolidBrush(Color.White);
            StringFormat format = new StringFormat();
            float accSteering = (pagePhysics.SteerAngle + 1) * 100; // map acc value to 0 - 200
            float angle = rescale(200, getMaxSteeringAngle() * 2, accSteering) - (getMaxSteeringAngle());

            // steering indicator
            Rectangle rect = new Rectangle(0 + wheelWidth, 0 + wheelWidth, Width - (2 * wheelWidth), Height - (2 * wheelWidth));
            float drawAngle = angle + 270 - (indicatorWidth / 2);
            g.DrawArc(indicatorPen, rect, drawAngle, indicatorWidth);
        }

        // map value input from range 0 - fromMax into range 0 - toMax
        private float rescale(float fromMax, float toMax, float input)
        {
            return ((toMax) / (fromMax) * (input));
        }

        private int getMaxSteeringAngle()
        {
            return SteeringLock.Get(pageStatic.CarModel);
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

