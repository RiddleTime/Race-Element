using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            [IntRange(10, 30, 1)]
            public int BarWidth { get; set; } = 15;

            [IntRange(50, 150, 1)]
            public int BarHeight { get; set; } = 100;

            [IntRange(5, 15, 1)]
            public int BarSpacing { get; set; } = 5;

            public InputBarsConfiguration()
            {
                AllowRescale = true;
            }
        }

        private CachedBitmap _cachedBackground;

        public InputBarsOverlay(Rectangle rectangle) : base(rectangle, "Input Bars Overlay")
        {
            this.Width = _config.BarWidth * 2 + _config.BarSpacing + 1;
            this.Height = _config.BarHeight + 1;

        }

        public override void BeforeStart()
        {
            _cachedBackground = new CachedBitmap(this.Width, this.Height, g =>
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(140, Color.Black)), 0, 0, _config.BarWidth, _config.BarHeight);

                g.FillRectangle(new SolidBrush(Color.FromArgb(140, Color.Black)), _config.BarWidth + _config.BarSpacing, 0, _config.BarWidth, _config.BarHeight);
            });
        }

        public override void BeforeStop()
        {
            _cachedBackground.Dispose();
        }

        public override void Render(Graphics g)
        {
            _cachedBackground?.Draw(g);

            VerticalProgressBar throttleBar = new VerticalProgressBar(0, 1, pagePhysics.Gas);
            VerticalProgressBar brakeBar = new VerticalProgressBar(0, 1, pagePhysics.Brake);

            Brush outlineBrush = new SolidBrush(Color.FromArgb(196, Color.Black));

            throttleBar.Draw(g, 0, 0, _config.BarWidth, _config.BarHeight, Brushes.LimeGreen, outlineBrush);
            brakeBar.Draw(g, _config.BarWidth + _config.BarSpacing, 0, _config.BarWidth, _config.BarHeight, Brushes.OrangeRed, outlineBrush);
        }

        public override bool ShouldRender() => DefaultShouldRender();
    }
}
