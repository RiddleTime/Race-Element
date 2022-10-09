using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayInputBars
{
    [Overlay(Name = "Input Bars", Version = 1.00, OverlayType = OverlayType.Release,
      Description = "Live input bars of throttle and brake.")]
    internal sealed class InputBarsOverlay : AbstractOverlay
    {
        private InputBarsConfiguration _config = new InputBarsConfiguration();
        private class InputBarsConfiguration : OverlayConfiguration
        {
            [IntRange(10, 40, 1)]
            public int BarWidth { get; set; } = 15;

            [IntRange(50, 200, 1)]
            public int BarHeight { get; set; } = 130;

            [IntRange(5, 15, 1)]
            public int BarSpacing { get; set; } = 5;

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

        public override void BeforeStart()
        {
            _cachedBackground = new CachedBitmap(this.Width, this.Height, g =>
            {
                Brush brush = new SolidBrush(Color.FromArgb(140, Color.Black));
                g.FillRoundedRectangle(brush, new Rectangle(0, 0, _config.BarWidth, _config.BarHeight), 3);
                g.FillRoundedRectangle(brush, new Rectangle(_config.BarWidth + _config.BarSpacing, 0, _config.BarWidth, _config.BarHeight), 3);
            });

            Brush outlineBrush = new SolidBrush(Color.FromArgb(196, Color.Black));
            _gasBar = new VerticalProgressBar(_config.BarWidth, _config.BarHeight)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = Brushes.LimeGreen,
                OutlineBrush = outlineBrush,
                Rounded = true,
            };
            _brakeBar = new VerticalProgressBar(_config.BarWidth, _config.BarHeight)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = Brushes.OrangeRed,
                OutlineBrush = outlineBrush,
                Rounded = true,
            };
        }

        public override void BeforeStop()
        {
            _cachedBackground.Dispose();
        }

        public override void Render(Graphics g)
        {
            _cachedBackground?.Draw(g);

         
            _gasBar.Value = pagePhysics.Gas;
            _gasBar.Draw(g, 0, 0);

            _brakeBar.Value = pagePhysics.Brake;
            _brakeBar.Draw(g, _config.BarWidth + _config.BarSpacing, 0);
        }

        public override bool ShouldRender() => DefaultShouldRender();
    }
}
