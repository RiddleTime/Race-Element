using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.ACC.Overlays.OverlayLapDelta
{
    [Overlay(Name = "Lap Delta", Description = "A Delta Bar", OverlayType = OverlayType.Release, Version = 1)]
    internal class LapDeltaOverlay : AbstractOverlay
    {
        private readonly LapDeltaConfiguration _config = new LapDeltaConfiguration();
        private sealed class LapDeltaConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Bar", "Adjust bar behavior.")]
            public BarGrouping Bar { get; set; } = new BarGrouping();
            public class BarGrouping
            {

                [ToolTip("Sets the Width of the Delta Bar.")]
                [IntRange(300, 800, 10)]
                public int Width { get; set; } = 300;

                [ToolTip("Sets the Height of the Delta Bar.")]
                [IntRange(80, 120, 5)]
                public int Height { get; set; } = 80;

                [ToolTip("Sets the maximum range in seconds for the delta bar.")]
                [IntRange(1, 5, 1)]
                public int MaxDelta { get; set; } = 2;

                public Color PositiveColor { get; set; } = Color.FromArgb(255, Color.LimeGreen);
                public Color NegativeColor { get; set; } = Color.FromArgb(255, Color.OrangeRed);

            }

            public LapDeltaConfiguration()
            {
                this.AllowRescale = true;
            }
        }


        private CachedBitmap _cachedBackground;

        public LapDeltaOverlay(Rectangle rectangle) : base(rectangle, "Lap Delta")
        {
            this.Width = _config.Bar.Width + 1;
            this.Height = _config.Bar.Height + 1;
        }

        public override void BeforeStart()
        {
            int cornerRadius = (int)(10 * this.Scale);

            _cachedBackground = new CachedBitmap((int)(_config.Bar.Width * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                int midHeight = (int)(_config.Bar.Height * this.Scale) / 2;
                var linerBrush = new LinearGradientBrush(new Point(0, midHeight), new Point((int)(_config.Bar.Width * this.Scale), midHeight), Color.FromArgb(160, 0, 0, 0), Color.FromArgb(230, 0, 0, 0));
                g.FillRoundedRectangle(linerBrush, new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
                g.DrawRoundedRectangle(new Pen(Color.Black, 1 * this.Scale), new Rectangle(0, 0, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
            });
        }

        public override void BeforeStop()
        {
        }

        public override void Render(Graphics g)
        {
            _cachedBackground.Draw(g, 0, 0, _config.Bar.Width, _config.Bar.Height);
        }

        public override bool ShouldRender() => DefaultShouldRender();
    }
}
