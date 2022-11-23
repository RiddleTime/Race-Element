using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.OverlayUtil.ProgressBars;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ACCManager.HUD.ACC.Overlays.OverlayInputBars
{
    [Overlay(Name = "Input Bars", Version = 1.00, OverlayType = OverlayType.Release,
      Description = "Live input bars of throttle and brake.")]
    internal sealed class InputBarsOverlay : AbstractOverlay
    {
        private readonly InputBarsConfiguration _config = new InputBarsConfiguration();
        private class InputBarsConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Shape", "The shape of the bars")]
            public ShapeGrouping Shape { get; set; } = new ShapeGrouping();
            public class ShapeGrouping
            {
                [ToolTip("Enables horizontal input bars.")]
                public bool Horizontal { get; set; } = false;

                [ToolTip("Defines the transparency of the bars.")]
                [ByteRange(40, 255, 1)]
                public byte Transparency { get; set; } = 185;

                [ToolTip("Length of the input bars.")]
                [IntRange(100, 250, 1)]
                public int Length { get; set; } = 200;

                [ToolTip("Changes the thickness of each input bar.")]
                [IntRange(10, 40, 1)]
                public int Thickness { get; set; } = 20;

                [ToolTip("Changes the spacing between the input bars")]
                [IntRange(2, 15, 1)]
                public int Spacing { get; set; } = 5;
            }

            [ToolTip("Displays a color change on the input bars when either abs or traction control is activated.")]
            public bool ShowElectronics { get; set; } = true;

            public InputBarsConfiguration()
            {
                AllowRescale = true;
            }
        }

        private CachedBitmap _cachedBackground;

        private HorizontalProgressBar _horizontalGasBar;
        private HorizontalProgressBar _horizontalBrakeBar;
        private VerticalProgressBar _verticalGasBar;
        private VerticalProgressBar _verticalBrakeBar;

        public InputBarsOverlay(Rectangle rectangle) : base(rectangle, "Input Bars Overlay")
        {
            if (_config.Shape.Horizontal)
            {
                this.Width = _config.Shape.Length + 1;
                this.Height = _config.Shape.Thickness * 2 + _config.Shape.Spacing + 1;
            }
            else
            {
                this.Width = _config.Shape.Thickness * 2 + _config.Shape.Spacing + 1;
                this.Height = _config.Shape.Length + 1;
            }
        }

        public override bool ShouldRender() => DefaultShouldRender();

        public override void BeforeStart()
        {
            int width = _config.Shape.Thickness * 2 + _config.Shape.Spacing;
            int height = _config.Shape.Length;

            if (_config.Shape.Horizontal)
            {
                width = _config.Shape.Length;
                height = _config.Shape.Thickness * 2 + _config.Shape.Spacing;
            }

            _cachedBackground = new CachedBitmap((int)(width * this.Scale), (int)(height * this.Scale), g =>
            {
                if (_config.Shape.Horizontal)
                {
                    using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new Rectangle(0, 0, (int)(_config.Shape.Length * this.Scale), (int)(_config.Shape.Thickness * this.Scale)), Color.FromArgb(120, Color.Black), Color.FromArgb(230, Color.Black), LinearGradientMode.Horizontal))
                    {
                        g.FillRoundedRectangle(gradientBrush, new Rectangle(0, 0, (int)(_config.Shape.Length * this.Scale), (int)(_config.Shape.Thickness * this.Scale)), (int)(5 * this.Scale));
                        g.FillRoundedRectangle(gradientBrush, new Rectangle(0, (int)((_config.Shape.Thickness + _config.Shape.Spacing) * this.Scale), (int)(_config.Shape.Length * this.Scale), (int)(_config.Shape.Thickness * this.Scale)), (int)(5 * this.Scale));
                    }
                }
                else
                {
                    using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new Rectangle(0, 0, (int)(_config.Shape.Thickness * this.Scale), (int)(height * this.Scale)), Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical))
                    {
                        g.FillRoundedRectangle(gradientBrush, new Rectangle(0, 0, (int)(_config.Shape.Thickness * this.Scale), (int)(height * this.Scale)), (int)(6 * this.Scale));
                        g.FillRoundedRectangle(gradientBrush, new Rectangle((int)((_config.Shape.Thickness + _config.Shape.Spacing) * this.Scale), 0, (int)(_config.Shape.Thickness * this.Scale), (int)(height * this.Scale)), (int)(5 * this.Scale));
                    }
                }
            });

            Brush outlineBrush = new SolidBrush(Color.FromArgb(196, Color.Black));

            if (_config.Shape.Horizontal)
            {
                _horizontalBrakeBar = new HorizontalProgressBar(_config.Shape.Length, _config.Shape.Thickness)
                {
                    Value = 0,
                    Min = 0,
                    Max = 1,
                    FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.OrangeRed)),
                    OutlineBrush = outlineBrush,
                    Rounded = true,
                    Scale = this.Scale,
                    Rounding = 5,
                };
                _horizontalGasBar = new HorizontalProgressBar(_config.Shape.Length, _config.Shape.Thickness)
                {
                    Value = 0,
                    Min = 0,
                    Max = 1,
                    FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.LimeGreen)),
                    OutlineBrush = outlineBrush,
                    Rounded = true,
                    Scale = this.Scale,
                    Rounding = 5,
                };
            }
            else
            {
                _verticalBrakeBar = new VerticalProgressBar(_config.Shape.Thickness, _config.Shape.Length)
                {
                    Value = 0,
                    Min = 0,
                    Max = 1,
                    FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.OrangeRed)),
                    OutlineBrush = outlineBrush,
                    Rounded = true,
                    Scale = this.Scale,
                    Rounding = 5,
                };
                _verticalGasBar = new VerticalProgressBar(_config.Shape.Thickness, _config.Shape.Length)
                {
                    Value = 0,
                    Min = 0,
                    Max = 1,
                    FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.LimeGreen)),
                    OutlineBrush = outlineBrush,
                    Rounded = true,
                    Scale = this.Scale,
                    Rounding = 5,
                };
            }
        }

        public override void BeforeStop()
        {
            _cachedBackground?.Dispose();
        }

        public override void Render(Graphics g)
        {

            if (_config.ShowElectronics)
                ApplyFillColor();

            if (_config.Shape.Horizontal)
            {
                _cachedBackground?.Draw(g, _config.Shape.Length, _config.Shape.Thickness * 2 + _config.Shape.Spacing);

                _horizontalGasBar.Value = pagePhysics.Gas;
                _horizontalBrakeBar.Value = pagePhysics.Brake;

                _horizontalGasBar?.Draw(g, 0, 0);
                _horizontalBrakeBar?.Draw(g, 0, _config.Shape.Thickness + _config.Shape.Spacing);
            }
            else
            {
                _cachedBackground?.Draw(g, _config.Shape.Thickness * 2 + _config.Shape.Spacing, _config.Shape.Length);

                _verticalBrakeBar.Value = pagePhysics.Brake;
                _verticalGasBar.Value = pagePhysics.Gas;

                _verticalGasBar?.Draw(g, 0, 0);
                _verticalBrakeBar?.Draw(g, _config.Shape.Thickness + _config.Shape.Spacing, 0);
            }
        }

        /// <summary>
        /// Applies a fill color to the brake and gas bar based on electronics
        /// </summary>
        private void ApplyFillColor()
        {
            if (_config.Shape.Horizontal)
            {
                if (pagePhysics.Abs > 0)
                    _horizontalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.Orange));
                else
                    _horizontalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.OrangeRed));

                if (pagePhysics.TC > 0)
                    _horizontalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.Orange));
                else
                    _horizontalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.LimeGreen));
            }
            else
            {
                if (pagePhysics.Abs > 0)
                    _verticalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.Orange));
                else
                    _verticalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.OrangeRed));

                if (pagePhysics.TC > 0)
                    _verticalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.Orange));
                else
                    _verticalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Shape.Transparency, Color.LimeGreen));
            }
        }
    }
}
