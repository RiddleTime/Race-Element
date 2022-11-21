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
            [ToolTip("Enables horizontal input bars.")]
            public bool HorizontalBars { get; set; } = false;

            [ToolTip("Displays a color change on the input bars when either abs or traction control is activated.")]
            public bool ShowElectronics { get; set; } = true;

            [ToolTip("Defines the transparency of the bars.")]
            [ByteRange(40, 255, 1)]
            public byte BarAlpha { get; set; } = 185;

            [ToolTip("Changes the thickness of each input bar.")]
            [IntRange(10, 40, 1)]
            public int BarThickness { get; set; } = 15;

            [ToolTip("Changes the length of each input bar.")]
            [IntRange(100, 200, 1)]
            public int BarLength { get; set; } = 130;

            [ToolTip("Changes the spacing between the input bars")]
            [IntRange(2, 15, 1)]
            public int BarSpacing { get; set; } = 5;

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
            if (_config.HorizontalBars)
            {
                this.Width = _config.BarLength + 1;
                this.Height = _config.BarThickness * 2 + _config.BarSpacing + 1;
            }
            else
            {
                this.Width = _config.BarThickness * 2 + _config.BarSpacing + 1;
                this.Height = _config.BarLength + 1;
            }
        }

        public override bool ShouldRender() => DefaultShouldRender();

        public override void BeforeStart()
        {
            int width = _config.BarThickness * 2 + _config.BarSpacing;
            int height = _config.BarLength;

            if (_config.HorizontalBars)
            {
                width = _config.BarLength;
                height = _config.BarThickness * 2 + _config.BarSpacing;
            }

            _cachedBackground = new CachedBitmap((int)(width * this.Scale), (int)(height * this.Scale), g =>
            {
                if (_config.HorizontalBars)
                {
                    using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new Rectangle(0, 0, (int)(_config.BarLength * this.Scale), (int)(_config.BarThickness * this.Scale)), Color.FromArgb(120, Color.Black), Color.FromArgb(230, Color.Black), LinearGradientMode.Horizontal))
                    {
                        g.FillRoundedRectangle(gradientBrush, new Rectangle(0, 0, (int)(_config.BarLength * this.Scale), (int)(_config.BarThickness * this.Scale)), (int)(5 * this.Scale));
                        g.FillRoundedRectangle(gradientBrush, new Rectangle(0, (int)((_config.BarThickness + _config.BarSpacing) * this.Scale), (int)(_config.BarLength * this.Scale), (int)(_config.BarThickness * this.Scale)), (int)(5 * this.Scale));
                    }
                }
                else
                {
                    using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new Rectangle(0, 0, (int)(_config.BarThickness * this.Scale), (int)(height * this.Scale)), Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical))
                    {
                        g.FillRoundedRectangle(gradientBrush, new Rectangle(0, 0, (int)(_config.BarThickness * this.Scale), (int)(height * this.Scale)), (int)(6 * this.Scale));
                        g.FillRoundedRectangle(gradientBrush, new Rectangle((int)((_config.BarThickness + _config.BarSpacing) * this.Scale), 0, (int)(_config.BarThickness * this.Scale), (int)(height * this.Scale)), (int)(5 * this.Scale));
                    }
                }
            });

            Brush outlineBrush = new SolidBrush(Color.FromArgb(196, Color.Black));

            if (_config.HorizontalBars)
            {
                _horizontalBrakeBar = new HorizontalProgressBar(_config.BarLength, _config.BarThickness)
                {
                    Value = 0,
                    Min = 0,
                    Max = 1,
                    FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.OrangeRed)),
                    OutlineBrush = outlineBrush,
                    Rounded = true,
                    Scale = this.Scale,
                    Rounding = 5,
                };
                _horizontalGasBar = new HorizontalProgressBar(_config.BarLength, _config.BarThickness)
                {
                    Value = 0,
                    Min = 0,
                    Max = 1,
                    FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.LimeGreen)),
                    OutlineBrush = outlineBrush,
                    Rounded = true,
                    Scale = this.Scale,
                    Rounding = 5,
                };
            }
            else
            {
                _verticalBrakeBar = new VerticalProgressBar(_config.BarThickness, _config.BarLength)
                {
                    Value = 0,
                    Min = 0,
                    Max = 1,
                    FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.OrangeRed)),
                    OutlineBrush = outlineBrush,
                    Rounded = true,
                    Scale = this.Scale,
                    Rounding = 5,
                };
                _verticalGasBar = new VerticalProgressBar(_config.BarThickness, _config.BarLength)
                {
                    Value = 0,
                    Min = 0,
                    Max = 1,
                    FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.LimeGreen)),
                    OutlineBrush = outlineBrush,
                    Rounded = true,
                    Scale = this.Scale,
                    Rounding = 5,
                };
            }
        }

        public override void BeforeStop()
        {
            _cachedBackground.Dispose();
        }

        public override void Render(Graphics g)
        {

            if (_config.ShowElectronics)
                ApplyFillColor();

            if (_config.HorizontalBars)
            {
                _cachedBackground?.Draw(g, _config.BarLength, _config.BarThickness * 2 + _config.BarSpacing);

                _horizontalGasBar.Value = pagePhysics.Gas;
                _horizontalBrakeBar.Value = pagePhysics.Brake;

                _horizontalGasBar?.Draw(g, 0, 0);
                _horizontalBrakeBar?.Draw(g, 0, _config.BarThickness + _config.BarSpacing);
            }
            else
            {
                _cachedBackground?.Draw(g, _config.BarThickness * 2 + _config.BarSpacing, _config.BarLength);

                _verticalBrakeBar.Value = pagePhysics.Brake;
                _verticalGasBar.Value = pagePhysics.Gas;

                _verticalGasBar?.Draw(g, 0, 0);
                _verticalBrakeBar?.Draw(g, _config.BarThickness + _config.BarSpacing, 0);
            }
        }

        /// <summary>
        /// Applies a fill color to the brake and gas bar based on electronics
        /// </summary>
        private void ApplyFillColor()
        {
            if (_config.HorizontalBars)
            {
                if (pagePhysics.Abs > 0)
                    _horizontalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.Orange));
                else
                    _horizontalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.OrangeRed));

                if (pagePhysics.TC > 0)
                    _horizontalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.Orange));
                else
                    _horizontalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.LimeGreen));
            }
            else
            {
                if (pagePhysics.Abs > 0)
                    _verticalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.Orange));
                else
                    _verticalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.OrangeRed));

                if (pagePhysics.TC > 0)
                    _verticalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.Orange));
                else
                    _verticalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.BarAlpha, Color.LimeGreen));
            }
        }
    }
}
