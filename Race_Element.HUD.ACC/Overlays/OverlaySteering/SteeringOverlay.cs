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
    [Overlay(Name = "Steering", Version = 1.00, OverlayType = OverlayType.Release,
        Description = "Displays the Steering Input.")]
    internal sealed class SteeringOverlay : AbstractOverlay
    {
        private readonly SteeringConfig _config = new SteeringConfig();
        private sealed class SteeringConfig : OverlayConfiguration
        {
            public enum InputsText
            {
                None,
                SteeringAngle,
                Gear,
                Speed,
                Rpm,
            }

            [ConfigGrouping("Information", "Set the text that is displayed within the steering circle.")]
            public InputsGrouping Info { get; set; } = new InputsGrouping();
            public class InputsGrouping
            {
                public InputsText Text { get; set; } = InputsText.None;
            }

            [ConfigGrouping("Colors", "Adjust the colors used in the Steering HUD.")]
            public ColorGrouping Colors { get; set; } = new ColorGrouping();
            public class ColorGrouping
            {
                public Color RingColor { get; set; } = Color.FromArgb(255, 255, 0, 0);
                [IntRange(75, 255, 1)]
                public int RingOpacity { get; set; } = 80;
            }

            public SteeringConfig()
            {
                this.AllowRescale = true;
            }
        }

        private const int _size = 150;
        private const int _wheelWidth = 15;
        private const int indicatorWidth = 15;

        private CachedBitmap _cachedBackground;

        private Font _font;

        public SteeringOverlay(Rectangle rectangle) : base(rectangle, "Steering")
        {
            this.Height = _size + 1;
            this.Width = _size + 1;
        }

        public sealed override void BeforeStart()
        {
            switch (_config.Info.Text)
            {
                case SteeringConfig.InputsText.Gear: _font = FontUtil.FontConthrax(40); break;
                case SteeringConfig.InputsText.Speed: _font = FontUtil.FontConthrax(25); break;
            }

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
                g.DrawEllipse(new Pen(Color.FromArgb(_config.Colors.RingOpacity, _config.Colors.RingColor), _wheelWidth / 2 * Scale), _size / 2 * Scale, _size / 2 * Scale, _size / 2 * Scale - _wheelWidth * Scale);

            });
        }

        public sealed override void BeforeStop()
        {
            _cachedBackground?.Dispose();
        }

        public sealed override bool ShouldRender() => DefaultShouldRender();

        public sealed override void Render(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            _cachedBackground?.Draw(g, 0, 0, _size, _size);

            DrawSteeringIndicator(g);

            if (_config.Info.Text != SteeringConfig.InputsText.None)
                switch (_config.Info.Text)
                {
                    case SteeringConfig.InputsText.Gear: DrawGear(g); break;
                    case SteeringConfig.InputsText.Speed: DrawSpeed(g); break;
                }
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

        private void DrawSpeed(Graphics g)
        {
            string speed = ((int)pagePhysics.SpeedKmh).ToString();
            float stringWidth = g.MeasureString(speed, _font).Width;
            int xPos = (int)(_size / 2 - stringWidth / 2);
            int yPos = _size / 2 - _font.Height / 2;

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.DrawStringWithShadow(speed, _font, Color.White, new PointF(xPos, yPos), 2.5f);
        }

        private void DrawGear(Graphics g)
        {
            string gear;
            switch (pagePhysics.Gear)
            {
                case 0: gear = "R"; break;
                case 1: gear = "N"; break;
                default: gear = $"{pagePhysics.Gear - 1}"; break;
            }

            float gearStringWidth = g.MeasureString(gear, _font).Width;
            int xPos = (int)(_size / 2 - gearStringWidth / 2);
            int yPos = _size / 2 - _font.Height / 2;

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.DrawStringWithShadow(gear, _font, Color.White, new PointF(xPos, yPos), 2.5f);
        }
    }
}
