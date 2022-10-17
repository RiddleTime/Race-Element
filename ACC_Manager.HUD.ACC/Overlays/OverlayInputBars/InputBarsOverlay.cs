using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using System.Drawing;

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
                Brush brush = new SolidBrush(Color.FromArgb(140, Color.Black));
                g.FillRoundedRectangle(brush, new Rectangle(0, 0, (int)(_config.BarWidth * this.Scale), (int)(height * this.Scale)), (int)(3 * this.Scale));
                g.FillRoundedRectangle(brush, new Rectangle((int)((_config.BarWidth + _config.BarSpacing) * this.Scale), 0, (int)(_config.BarWidth * this.Scale), (int)(height * this.Scale)), (int)(5 * this.Scale));
            });

            Brush outlineBrush = new SolidBrush(Color.FromArgb(196, Color.Black));
            _gasBar = new VerticalProgressBar((int)(_config.BarWidth), (int)(_config.BarHeight))
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
            _brakeBar = new VerticalProgressBar((int)(_config.BarWidth), (int)(_config.BarHeight))
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

            _gasBar.Value = pagePhysics.Gas;
            _gasBar.Draw(g, 0, 0);

            _brakeBar.Value = pagePhysics.Brake;
            _brakeBar.Draw(g, _config.BarWidth + _config.BarSpacing, 0);
        }

        private void ApplyElectronicsColors()
        {
            if (pagePhysics.TC > 0)
                _gasBar.FillBrush = Brushes.Orange;
            else
                _gasBar.FillBrush = Brushes.LimeGreen;

            if (pagePhysics.Abs > 0)
                _brakeBar.FillBrush = Brushes.Orange;
            else
                _brakeBar.FillBrush = Brushes.OrangeRed;
        }
    }
}
