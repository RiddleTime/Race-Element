using RaceElement.Data.ACC.Cars;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
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
            }

            [ConfigGrouping("Information", "Set the text that is displayed within the steering circle.")]
            public InputsGrouping Info { get; set; } = new InputsGrouping();
            public class InputsGrouping
            {
                public InputsText Text { get; set; } = InputsText.None;
            }

            [ConfigGrouping("Colors", "Adjust the colors used in the Steering HUD.")]
            public RingGrouping Ring { get; set; } = new RingGrouping();
            public class RingGrouping
            {
                [IntRange(4, 12, 1)]
                public int RingThickness { get; set; } = 5;

                public Color RingColor { get; set; } = Color.FromArgb(255, 255, 0, 0);
                [IntRange(75, 255, 1)]
                public int RingOpacity { get; set; } = 117;
            }

            public SteeringConfig() => this.AllowRescale = true;
        }

        private const int InitialSize = 140;
        private const int indicatorWidth = 15;

        private int _wheelThickness = 13;

        private CachedBitmap _cachedBackground;
        private Font _font;

        public SteeringOverlay(Rectangle rectangle) : base(rectangle, "Steering")
        {
            RefreshRateHz = 60;
        }

        public sealed override void BeforeStart()
        {
            this.Height = (int)(InitialSize + (1 * this.Scale));
            this.Width = (int)(InitialSize + (1 * this.Scale));
            _wheelThickness = _config.Ring.RingThickness;
            _font = _config.Info.Text switch
            {
                SteeringConfig.InputsText.SteeringAngle => FontUtil.FontUnispace(28),
                SteeringConfig.InputsText.Gear => FontUtil.FontConthrax(40),
                SteeringConfig.InputsText.Speed => FontUtil.FontUnispace(34),
                _ => null,
            };

            _cachedBackground = new CachedBitmap((int)(Width * this.Scale + (1 * this.Scale)), (int)(Height * this.Scale + (1 * this.Scale)), g =>
            {
                GraphicsPath gradientPath = new GraphicsPath();
                gradientPath.AddEllipse(0, 0, (int)(InitialSize * Scale), (int)(InitialSize * Scale));
                PathGradientBrush pthGrBrush = new PathGradientBrush(gradientPath);
                pthGrBrush.CenterColor = Color.FromArgb(40, 0, 0, 0);
                pthGrBrush.SurroundColors = new Color[] { Color.FromArgb(220, 0, 0, 0) };

                // draw background
                g.FillEllipse(pthGrBrush, new Rectangle(0, 0, (int)(InitialSize * Scale), (int)(InitialSize * Scale)));
                g.DrawEllipse(new Pen(Color.FromArgb(230, Color.Black)), new Rectangle(0, 0, (int)(InitialSize * Scale), (int)(InitialSize * Scale)));

                // draw steering ring
                int padding = 2;
                g.DrawEllipse(new Pen(Color.FromArgb(_config.Ring.RingOpacity, _config.Ring.RingColor),
                    _wheelThickness / 2 * Scale),
                    InitialSize / 2 * Scale,
                    InitialSize / 2 * Scale,
                    InitialSize / 2 * Scale - _wheelThickness * Scale - padding * 2);
            });
        }

        public sealed override void BeforeStop()
        {
            _cachedBackground?.Dispose();
            _font?.Dispose();
        }

        public sealed override bool ShouldRender() => DefaultShouldRender();

        public sealed override void Render(Graphics g)
        {

            _cachedBackground?.Draw(g, 0, 0, InitialSize, InitialSize);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawSteeringIndicator(g);

            if (_config.Info.Text != SteeringConfig.InputsText.None)
                switch (_config.Info.Text)
                {
                    case SteeringConfig.InputsText.SteeringAngle: DrawSteeringAngle(g); break;
                    case SteeringConfig.InputsText.Gear: DrawGear(g); break;
                    case SteeringConfig.InputsText.Speed: DrawSpeed(g); break;
                }
        }

        private void DrawSteeringIndicator(Graphics g)
        {
            float accSteering = (pagePhysics.SteerAngle / 2 + 1) * 100; // map acc value to 0 - 200
            float angle = Rescale(200, SteeringLock.Get(pageStatic.CarModel) * 2, accSteering) - (SteeringLock.Get(pageStatic.CarModel));

            int padding = 1;
            Rectangle rect = new Rectangle(0 + _wheelThickness / 2 + padding, 0 + _wheelThickness / 2 + padding, InitialSize - (2 * _wheelThickness / 2) - padding * 2, InitialSize - (2 * _wheelThickness / 2) - padding * 2);
            float drawAngle = angle + 270 - (indicatorWidth / 2);
            g.DrawArc(new Pen(Color.White, _wheelThickness), rect, drawAngle, indicatorWidth);
        }

        // map value input from range 0 - fromMax into range 0 - toMax
        private float Rescale(float fromMax, float toMax, float input)
        {
            return toMax / fromMax * input;
        }

        private void DrawSpeed(Graphics g)
        {
            string speed = ((int)pagePhysics.SpeedKmh).ToString();
            speed.FillStart(3, ' ');

            float stringWidth = g.MeasureString(speed, _font).Width;
            int xPos = (int)(InitialSize / 2 - stringWidth / 2);
            int yPos = InitialSize / 2 - _font.Height / 2;

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.DrawStringWithShadow(speed, _font, Color.White, new PointF(xPos, yPos), 2.5f);
        }

        private void DrawSteeringAngle(Graphics g)
        {
            float angle = pagePhysics.SteerAngle * SteeringLock.Get(pageStatic.CarModel) / 2;
            string speed = $"{angle:f0}";
            speed.FillStart(4, ' ');

            float stringWidth = g.MeasureString(speed, _font).Width;
            int xPos = (int)(InitialSize / 2 - stringWidth / 2);
            int yPos = InitialSize / 2 - _font.Height / 2;

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.DrawStringWithShadow(speed, _font, Color.White, new PointF(xPos, yPos), 2.5f);
        }

        private void DrawGear(Graphics g)
        {
            string gear = pagePhysics.Gear switch
            {
                0 => "R",
                1 => "N",
                _ => $"{pagePhysics.Gear - 1}",
            };
            float gearStringWidth = g.MeasureString(gear, _font).Width;
            int xPos = (int)(InitialSize / 2 - gearStringWidth / 2);
            int yPos = InitialSize / 2 - _font.Height / 2;

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.DrawStringWithShadow(gear, _font, Color.White, new PointF(xPos, yPos), 2.5f);
        }
    }
}
