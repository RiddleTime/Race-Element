using RaceElement.Data.ACC.Cars;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace RaceElement.HUD.ACC.Overlays.OverlayInputs
{
    [Overlay(Name = "Inputs", Version = 1.00, OverlayType = OverlayType.Release,
        Description = "Live inputs of steering, throttle, brake and the selected gear.")]
    internal sealed class InputsOverlay : AbstractOverlay
    {
        private readonly SteeringWheelConfig _config = new SteeringWheelConfig();
        private sealed class SteeringWheelConfig : OverlayConfiguration
        {
            [ConfigGrouping("Information", "Show or hide inputs or the current gear.")]
            public InputsGrouping Inputs { get; set; } = new InputsGrouping();
            public class InputsGrouping
            {
                [ToolTip("Displays the selected gear.")]
                public bool CurrentGear { get; set; } = true;

                [ToolTip("Displays the throttle input.")]
                public bool ThrottleInput { get; set; } = false;

                [ToolTip("Displays the brake input.")]
                public bool BrakeInput { get; set; } = false;
            }

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

        private CachedBitmap _cachedBackground;

        private Font _gearIndicatorFont;

        public InputsOverlay(Rectangle rectangle) : base(rectangle, "Inputs")
        {
            this.Height = _size + 1;
            this.Width = _size + 1;

            this.innerWheelWidth = _wheelWidth / 2;
        }

        public sealed override void BeforeStart()
        {
            if (this._config.Inputs.CurrentGear)
                _gearIndicatorFont = FontUtil.FontOrbitron(40);

            _cachedBackground = new CachedBitmap((int)(Width * this.Scale), (int)(Height * this.Scale), g =>
            {
                GraphicsPath gradientPath = new GraphicsPath();
                gradientPath.AddEllipse(0, 0, (int)(_size * Scale), (int)(_size * Scale));
                PathGradientBrush pthGrBrush = new PathGradientBrush(gradientPath);
                pthGrBrush.CenterColor = Color.FromArgb(40, 0, 0, 0);
                pthGrBrush.SurroundColors = new Color[] { Color.FromArgb(220, 0, 0, 0) };

                // background
                g.FillEllipse(pthGrBrush, new Rectangle(0, 0, (int)(_size * Scale), (int)(_size * Scale)));
                g.DrawEllipse(new Pen(Color.FromArgb(230, Color.Black)), new Rectangle(0, 0, (int)(_size * Scale), (int)(_size * Scale)));

                // steering background
                g.DrawEllipse(new Pen(Color.FromArgb(80, Color.White), _wheelWidth / 2 * Scale), _size / 2 * Scale, _size / 2 * Scale, _size / 2 * Scale - _wheelWidth * Scale);

                // braking background
                if (_config.Inputs.BrakeInput)
                {
                    var brakeColor = Color.FromArgb(255, 240, 10, 10);
                    int x = (int)((_wheelWidth * 2 + (_config.Inputs.ThrottleInput ? this.innerWheelWidth : 0)) * Scale);
                    int y = (int)((_wheelWidth * 2 + (_config.Inputs.ThrottleInput ? this.innerWheelWidth : 0)) * Scale);
                    int width = (int)((_size - (4 * _wheelWidth) - (2 * (_config.Inputs.ThrottleInput ? this.innerWheelWidth : 0))) * Scale);
                    int height = (int)((_size - (4 * _wheelWidth) - (2 * (_config.Inputs.ThrottleInput ? this.innerWheelWidth : 0))) * Scale);
                    var brakingBackground = new Pen(Color.FromArgb(80, brakeColor), this.innerWheelWidth / 2 * Scale);
                    Rectangle rect = new Rectangle(x, y, width, height);
                    g.DrawArc(brakingBackground, rect, inputCircleMinAngle, inputCircleSweepAngle);
                }

                // throttle background
                if (_config.Inputs.ThrottleInput)
                {
                    var throttleColor = Color.FromArgb(255, 10, 240, 10);
                    var penBackground = new Pen(Color.FromArgb(80, throttleColor), this.innerWheelWidth / 2 * Scale);
                    Rectangle throttleBackGroundRect = new Rectangle()
                    {
                        X = (int)(_wheelWidth * 2 * Scale),
                        Y = (int)(_wheelWidth * 2 * Scale),
                        Width = (int)((_size - 4 * _wheelWidth) * Scale),
                        Height = (int)((_size - 4 * _wheelWidth) * Scale),
                    };
                    g.DrawArc(penBackground, throttleBackGroundRect, inputCircleMinAngle, inputCircleSweepAngle);
                }
            });
        }

        public sealed override void BeforeStop()
        {
            _cachedBackground?.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            _cachedBackground?.Draw(g, 0, 0, (int)_size, (int)_size);

            DrawSteeringIndicator(g);

            if (this._config.Inputs.ThrottleInput)
                DrawThrottleIndicator(g);

            if (this._config.Inputs.BrakeInput)
                DrawBrakingIndicator(g);

            if (this._config.Inputs.CurrentGear)
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
            var brakingForeground = new Pen(brakeColor, this.innerWheelWidth);

            DrivingAssistanceIndicator((pagePhysics.Abs == 1), brakingForeground, brakeColor);

            Rectangle rect = new Rectangle()
            {
                X = _wheelWidth * 2 + (_config.Inputs.ThrottleInput ? this.innerWheelWidth : 0),
                Y = _wheelWidth * 2 + (_config.Inputs.ThrottleInput ? this.innerWheelWidth : 0),
                Width = _size - (4 * _wheelWidth) - (2 * (_config.Inputs.ThrottleInput ? this.innerWheelWidth : 0)),
                Height = _size - (4 * _wheelWidth) - (2 * (_config.Inputs.ThrottleInput ? this.innerWheelWidth : 0))
            };
            float brakeAngle = Rescale(1, inputCircleSweepAngle, pagePhysics.Brake);
            g.DrawArc(brakingForeground, rect, inputCircleMinAngle, brakeAngle);
        }

        private void DrawThrottleIndicator(Graphics g)
        {
            var throttleColor = Color.FromArgb(255, 10, 240, 10);
            var penForeground = new Pen(throttleColor, this.innerWheelWidth);

            DrivingAssistanceIndicator((pagePhysics.TC == 1), penForeground, throttleColor);

            Rectangle rect = new Rectangle(_wheelWidth * 2, _wheelWidth * 2, _size - 4 * _wheelWidth, _size - 4 * _wheelWidth);
            float throttleAngle = Rescale(1, inputCircleSweepAngle, pagePhysics.Gas);
            g.DrawArc(penForeground, rect, inputCircleMinAngle, throttleAngle);
        }

        private void DrawSteeringIndicator(Graphics g)
        {
            float accSteering = (pagePhysics.SteerAngle / 2 + 1) * 100; // map acc value to 0 - 200
            float angle = Rescale(200, SteeringLock.Get(pageStatic.CarModel) * 2, accSteering) - (SteeringLock.Get(pageStatic.CarModel));

            Rectangle rect = new Rectangle(0 + _wheelWidth, 0 + _wheelWidth, _size - (2 * _wheelWidth), _size - (2 * _wheelWidth));
            float drawAngle = angle + 270 - (indicatorWidth / 2);
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
