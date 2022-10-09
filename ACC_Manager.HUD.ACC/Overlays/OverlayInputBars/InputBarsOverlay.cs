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
        private readonly Brush _barOutlineBrush;

        public InputBarsOverlay(Rectangle rectangle) : base(rectangle, "Input Bars Overlay")
        {
            this.Width = _config.BarWidth * 2 + _config.BarSpacing + 1;
            this.Height = _config.BarHeight + 1;
            _barOutlineBrush = new SolidBrush(Color.FromArgb(196, Color.Black));
        }

        public override void BeforeStart()
        {
            _cachedBackground = new CachedBitmap(this.Width, this.Height, g =>
            {
                Brush brush = new SolidBrush(Color.FromArgb(140, Color.Black));
                g.FillRoundedRectangle(brush, new Rectangle(0, 0, _config.BarWidth, _config.BarHeight), 3);
                g.FillRoundedRectangle(brush, new Rectangle(_config.BarWidth + _config.BarSpacing, 0, _config.BarWidth, _config.BarHeight), 3);
            });
        }

        public override void BeforeStop()
        {
            _cachedBackground.Dispose();
        }

        public override void Render(Graphics g)
        {
            _cachedBackground?.Draw(g);

            new VerticalProgressBar(0, 1, pagePhysics.Gas).Draw(g, 0, 0, _config.BarWidth, _config.BarHeight, Brushes.LimeGreen, _barOutlineBrush, true);
            new VerticalProgressBar(0, 1, pagePhysics.Brake).Draw(g, _config.BarWidth + _config.BarSpacing, 0, _config.BarWidth, _config.BarHeight, Brushes.OrangeRed, _barOutlineBrush, true);
        }

        public override bool ShouldRender() => DefaultShouldRender();
    }
}
