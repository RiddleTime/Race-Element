using ACCManager.Data.ACC.Cars;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.ACC.Overlays.OverlayInputs
{
    internal sealed class InputsOverlay : AbstractOverlay
    {
        private readonly SteeringWheelConfig _config = new SteeringWheelConfig();
        private sealed class SteeringWheelConfig : OverlayConfiguration
        {
            [ToolTip("Show throttle input indicator.")]
            public bool ShowThrottleInput { get; set; } = true;
            [ToolTip("Show brake input indicator.")]
            public bool ShowBrakeInput { get; set; } = true;
            [ToolTip("Show selected gear.")]
            public bool ShowCurrentGear { get; set; } = true;

            public SteeringWheelConfig()
            {
                this.AllowRescale = true;
            }
        }

        private const int _size = 150;
        private const int _wheelWidth = 15;
        private const int indicatorWidth = 15;
        private const int inputCircleMinAngle = 135;
        private const int inputCircleSweepAngle = 270;

        private readonly int innerWheelWidth;

        private Font _gearIndicatorFont;

        public InputsOverlay(Rectangle rectangle) : base(rectangle, "Inputs Overlay")
        {
            this.Height = _size + 1;
            this.Width = _size + 1;

            this.innerWheelWidth = _wheelWidth / 2;
        }

        public sealed override void BeforeStart()
        {
            if (this._config.ShowCurrentGear)
                _gearIndicatorFont = FontUtil.FontOrbitron(40);
        }
        public sealed override void BeforeStop() { }

        public sealed override void Render(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillEllipse(new SolidBrush(Color.FromArgb(140, Color.Black)), new Rectangle(0, 0, _size, _size));

            DrawSteeringIndicator(g);

            if (this._config.ShowThrottleInput)
                DrawThrottleIndicator(g);

            if (this._config.ShowBrakeInput)
                DrawBrakingIndicator(g);

            if (this._config.ShowCurrentGear)
                DrawGearIndicator(g);
        }

        private void DrawGearIndicator(Graphics g)
        {
            string gear;
            switch (pagePhysics.Gear)
            {
                case 0: gear = "R"; break;
                case 1: gear = "N"; break;
                default: gear = $"{pagePhysics.Gear - 1}"; break;
            }

            float gearStringWidth = g.MeasureString(gear, _gearIndicatorFont).Width;
            int xPos = (int)(_size / 2 - gearStringWidth / 2);
            int yPos = _size / 2 - _gearIndicatorFont.Height / 2;

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.DrawStringWithShadow(gear, _gearIndicatorFont, Color.White, new PointF(xPos, yPos), 2.5f);
        }

        private void DrawBrakingIndicator(Graphics g)
        {
            var brakeColor = Color.FromArgb(255, 240, 10, 10);
            var brakingBackground = new Pen(Color.FromArgb(80, brakeColor), this.innerWheelWidth / 2);
            var brakingForeground = new Pen(brakeColor, this.innerWheelWidth);

            int x = _wheelWidth * 2 + (_config.ShowThrottleInput ? this.innerWheelWidth : 0);
            int y = _wheelWidth * 2 + (_config.ShowThrottleInput ? this.innerWheelWidth : 0);
            int width = _size - (4 * _wheelWidth) - (2 * (_config.ShowThrottleInput ? this.innerWheelWidth : 0));
            int height = _size - (4 * _wheelWidth) - (2 * (_config.ShowThrottleInput ? this.innerWheelWidth : 0));

            DrivingAssistanceIndicator((pagePhysics.Abs == 1), brakingForeground, brakeColor);

            Rectangle rect = new Rectangle(x, y, width, height);
            g.DrawArc(brakingBackground, rect, inputCircleMinAngle, inputCircleSweepAngle);
            float brakeAngle = Rescale(1, inputCircleSweepAngle, pagePhysics.Brake);
            g.DrawArc(brakingForeground, rect, inputCircleMinAngle, brakeAngle);
        }

        private void DrawThrottleIndicator(Graphics g)
        {
            var throttleColor = Color.FromArgb(255, 10, 240, 10);
            var penBackground = new Pen(Color.FromArgb(80, throttleColor), this.innerWheelWidth / 2);
            var penForeground = new Pen(throttleColor, this.innerWheelWidth);

            DrivingAssistanceIndicator((pagePhysics.TC == 1), penForeground, throttleColor);

            Rectangle rect = new Rectangle(_wheelWidth * 2, _wheelWidth * 2, _size - 4 * _wheelWidth, _size - 4 * _wheelWidth);
            g.DrawArc(penBackground, rect, inputCircleMinAngle, inputCircleSweepAngle);
            float throttleAngle = Rescale(1, inputCircleSweepAngle, pagePhysics.Gas);
            g.DrawArc(penForeground, rect, inputCircleMinAngle, throttleAngle);
        }

        private void DrawSteeringIndicator(Graphics g)
        {
            float accSteering = (pagePhysics.SteerAngle / 2 + 1) * 100; // map acc value to 0 - 200
            float angle = Rescale(200, SteeringLock.Get(pageStatic.CarModel) * 2, accSteering) - (SteeringLock.Get(pageStatic.CarModel));

            Rectangle rect = new Rectangle(0 + _wheelWidth, 0 + _wheelWidth, _size - (2 * _wheelWidth), _size - (2 * _wheelWidth));
            float drawAngle = angle + 270 - (indicatorWidth / 2);
            g.DrawEllipse(new Pen(Color.FromArgb(80, Color.White), _wheelWidth / 2), _size / 2, _size / 2, _size / 2 - _wheelWidth);
            g.DrawArc(new Pen(Color.White, _wheelWidth), rect, drawAngle, indicatorWidth);
        }

        // map value input from range 0 - fromMax into range 0 - toMax
        private float Rescale(float fromMax, float toMax, float input)
        {
            return toMax / fromMax * input;
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
            return DefaultShouldRender();
        }

    }
}
