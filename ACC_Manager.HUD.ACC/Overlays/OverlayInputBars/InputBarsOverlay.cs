using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
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
            [ToolTip("Changes the width of each input bar.")]
            [IntRange(10, 40, 1)]
            public int BarWidth { get; set; } = 15;

            [ToolTip("Changes the height of each input bar.")]
            [IntRange(100, 200, 1)]
            public int BarHeight { get; set; } = 130;

            [ToolTip("Changes the spacing between the input bars")]
            [IntRange(5, 15, 1)]
            public int BarSpacing { get; set; } = 5;

            [ToolTip("Displays a color change on the input bars when either abs or traction control is activated.")]
            public bool ShowElectronics { get; set; } = true;

            public InputBarsConfiguration()
            {
                AllowRescale = true;
            }
        }

        private CachedBitmap _cachedBackground;

        private VerticalProgressBar _gasBar;
        private VerticalProgressBar _brakeBar;

        public InputBarsOverlay(Rectangle rectangle) : base(rectangle, "Input Bars Overlay")
        {
            this.Width = _config.BarWidth * 2 + _config.BarSpacing + 1;
            this.Height = _config.BarHeight + 1;
        }

        public override bool ShouldRender() => DefaultShouldRender();

        public override void BeforeStart()
        {
            int width = _config.BarWidth * 2 + _config.BarSpacing;
            int height = _config.BarHeight;
            _cachedBackground = new CachedBitmap((int)(width * this.Scale), (int)(height * this.Scale + 1), g =>
            {
                using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new Rectangle(0, 0, (int)(_config.BarWidth * this.Scale), (int)(height * this.Scale)), Color.FromArgb(230, Color.Black), Color.FromArgb(140, Color.Black), LinearGradientMode.Vertical))
                {
                    g.FillRoundedRectangle(gradientBrush, new Rectangle(0, 0, (int)(_config.BarWidth * this.Scale), (int)(height * this.Scale)), (int)(3 * this.Scale));
                    g.FillRoundedRectangle(gradientBrush, new Rectangle((int)((_config.BarWidth + _config.BarSpacing) * this.Scale), 0, (int)(_config.BarWidth * this.Scale), (int)(height * this.Scale)), (int)(5 * this.Scale));
                }
            });

            Brush outlineBrush = new SolidBrush(Color.FromArgb(196, Color.Black));
            _brakeBar = new VerticalProgressBar(_config.BarWidth, _config.BarHeight)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = Brushes.OrangeRed,
                OutlineBrush = outlineBrush,
                Rounded = true,
                Scale = this.Scale,
                Rounding = 5,
            };
            _gasBar = new VerticalProgressBar(_config.BarWidth, _config.BarHeight)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = Brushes.LimeGreen,
                OutlineBrush = outlineBrush,
                Rounded = true,
                Scale = this.Scale,
                Rounding = 5,
            };
        }

        public override void BeforeStop()
        {
            _cachedBackground.Dispose();
        }

        public override void Render(Graphics g)
        {
            _cachedBackground?.Draw(g, _config.BarWidth * 2 + _config.BarSpacing, _config.BarHeight);

            if (_config.ShowElectronics)
                ApplyElectronicsColors();

            _brakeBar.Value = pagePhysics.Brake;
            _gasBar.Value = pagePhysics.Gas;

            _brakeBar.Draw(g, 0, 0);
            _gasBar.Draw(g, _config.BarWidth + _config.BarSpacing, 0);
        }

        private void ApplyElectronicsColors()
        {
            if (pagePhysics.Abs > 0)
                _brakeBar.FillBrush = new SolidBrush(Color.FromArgb(180, Color.Orange));
            else
                _brakeBar.FillBrush = new SolidBrush(Color.FromArgb(180, Color.OrangeRed));

            if (pagePhysics.TC > 0)
                _gasBar.FillBrush = new SolidBrush(Color.FromArgb(180, Color.Orange));
            else
                _gasBar.FillBrush = new SolidBrush(Color.FromArgb(180, Color.LimeGreen));
        }
    }
}
