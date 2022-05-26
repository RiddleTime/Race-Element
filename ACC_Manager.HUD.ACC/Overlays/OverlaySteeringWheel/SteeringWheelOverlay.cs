using ACCManager.Data.ACC.Cars;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private const int absIndicatorWidth = 60;

        private readonly int innerWheelWidth = 0;
        private readonly SolidBrush background;
        private readonly Pen wheelPen;
        private readonly Pen indicatorPen;
        private readonly Pen throttleBackground;
        private readonly Pen throttleForeground;
        private readonly Pen brakingBackground;
        private readonly Color brakeColor;
        private readonly Color throttleColor;
        private readonly SteeringWheelConfig config = new SteeringWheelConfig();

        private Pen brakingForeground;
        
        internal class SteeringWheelConfig : OverlayConfiguration
        {
            [ToolTip("Show throttle input indicator.")]
            public bool ShowThrottleInput { get; set; } = true;
            [ToolTip("Show brake input indicator.")]
            public bool ShowBrakeInput { get; set; } = true;
            [ToolTip("Show selected gear.")]
            public bool ShowGears { get; set; } = true;
            
            [ToolTip("Adds a background to the overlay")]
            public bool DrawBackground { get; set; } = false;

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
            this.background = new SolidBrush(Color.FromArgb(100, Color.Black));
            this.wheelPen = new Pen(Color.FromArgb(100, 150, 150, 150), wheelWidth);
            this.indicatorPen = new Pen(Color.White, wheelWidth);

            throttleColor = Color.FromArgb(255, 10, 255, 10);
            Color throttleBackgroundColor = Color.FromArgb(50, throttleColor);
            brakeColor = Color.FromArgb(255, 255, 10, 10);
            Color brakeBackgroundColor = Color.FromArgb(50, brakeColor);

            this.throttleBackground = new Pen(throttleBackgroundColor, this.innerWheelWidth);
            this.throttleForeground = new Pen(throttleColor, this.innerWheelWidth);
            this.brakingBackground = new Pen(brakeBackgroundColor, this.innerWheelWidth);
            this.brakingForeground = new Pen(brakeColor, this.innerWheelWidth);
        }

        public sealed override void BeforeStart()
        {

        }

        public sealed override void BeforeStop()
        {

        }

        public sealed override void Render(Graphics g)
        {
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (this.config.DrawBackground)
            {
                Rectangle graphRect = new Rectangle(0, 0, this.Width, this.Height);
                g.FillRoundedRectangle(background, graphRect, 10);
            }

            DrawSteeringIndicator(g);

            if (this.config.ShowThrottleInput)
                DrawThrottleIndicator(g);

            if (this.config.ShowBrakeInput)
                DrawBrakingIndicator(g);

            if (this.config.ShowGears)
                DrawGearIndicator(g);

            g.SmoothingMode = previous;
        }

        private void DrawGearIndicator(Graphics g)
        {
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

        private void DrawBrakingIndicator(Graphics g)
        {
            int x = wheelWidth * 2 + (config.ShowThrottleInput ? this.innerWheelWidth : 0);
            int y = wheelWidth * 2 + (config.ShowThrottleInput ? this.innerWheelWidth : 0);
            int width = Width - (4 * wheelWidth) - (2 * (config.ShowThrottleInput ? this.innerWheelWidth : 0));
            int height = Height - (4 * wheelWidth) - (2 * (config.ShowThrottleInput ? this.innerWheelWidth : 0));

            DrivingAssistanceIndicator((pagePhysics.Abs == 1), this.brakingForeground, brakeColor);

            Rectangle rect = new Rectangle(x, y, width, height);
            g.DrawArc(brakingBackground, rect, inputCircleMinAngle, inputCircleSweepAngle);
            float brakeAngle = Rescale(1, inputCircleSweepAngle, pagePhysics.Brake);
            g.DrawArc(brakingForeground, rect, inputCircleMinAngle, brakeAngle);
        }

        private void DrawThrottleIndicator(Graphics g)
        {

            DrivingAssistanceIndicator((pagePhysics.TC == 1), this.throttleForeground, throttleColor);

            Rectangle rect = new Rectangle(0 + wheelWidth * 2, 0 + wheelWidth * 2, Width - (4 * wheelWidth), Height - (4 * wheelWidth));
            g.DrawArc(throttleBackground, rect, inputCircleMinAngle, inputCircleSweepAngle);
            float throttleAngle = Rescale(1, inputCircleSweepAngle, pagePhysics.Gas);
            g.DrawArc(throttleForeground, rect, inputCircleMinAngle, throttleAngle);
        }

        private void DrawSteeringIndicator(Graphics g)
        {
            g.DrawCircle(wheelPen, this.Width / 2, this.Height / 2, (this.Height / 2) - wheelWidth);

            SolidBrush brush = new SolidBrush(Color.White);
            StringFormat format = new StringFormat();
            float accSteering = (pagePhysics.SteerAngle + 1) * 100; // map acc value to 0 - 200
            float angle = Rescale(200, SteeringLock.Get(pageStatic.CarModel) * 2, accSteering) - (SteeringLock.Get(pageStatic.CarModel));

            // steering indicator
            Rectangle rect = new Rectangle(0 + wheelWidth, 0 + wheelWidth, Width - (2 * wheelWidth), Height - (2 * wheelWidth));
            float drawAngle = angle + 270 - (indicatorWidth / 2);
            g.DrawArc(indicatorPen, rect, drawAngle, indicatorWidth);
        }

        // map value input from range 0 - fromMax into range 0 - toMax
        private float Rescale(float fromMax, float toMax, float input)
        {
            return ((toMax) / (fromMax) * (input));
        }

        private Color SetRandomTransparency(int minTransparanxy, int maxTransparancy, Color color)
        {
            Random rd = new Random();
            int randomTransparancy = rd.Next(minTransparanxy, maxTransparancy);
            return Color.FromArgb(randomTransparancy, color);
        }

        private void DrivingAssistanceIndicator(bool active, Pen pen, Color inactiveColor)
        {
            if (active)
            {
                pen.Color = SetRandomTransparency(50, 255, inactiveColor);
            }
            else
            {
                if (pen.Color.A != 255)
                    pen.Color = inactiveColor;
            }
        }



        public sealed override bool ShouldRender()
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

