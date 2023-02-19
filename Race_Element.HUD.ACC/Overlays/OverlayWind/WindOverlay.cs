using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayWind
{
#if DEBUG
    [Overlay(Name = "Wind", Description = "Shows wind speed and direction", OverlayType = OverlayType.Release, Version = 1.00)]
#endif
    internal sealed class WindOverlay : AbstractOverlay
    {
        private WindOverlayConfiguration _config = new WindOverlayConfiguration();
        private sealed class WindOverlayConfiguration : OverlayConfiguration
        {
            public WindOverlayConfiguration() => AllowRescale = true;
        }

        // make panel
        InfoPanel _panel;

        public WindOverlay(Rectangle rectangle) : base(rectangle, "Wind")
        {
            this.Width = 300;
            this.Height = 300;
            _panel = new InfoPanel(13, 300);
        }

        public override void BeforeStart() { }

        public override void BeforeStop() { }

        public override bool ShouldRender() => DefaultShouldRender();

        public override void Render(Graphics g)
        {

            double vaneAngle = 90 + pageGraphics.WindDirection * -1;

            double carDirection = (pagePhysics.Heading * 180d) / Math.PI;
            vaneAngle -= carDirection;


            g.DrawArc(new Pen(Brushes.Red, 5), new Rectangle(60, 60, 80, 80), (float)vaneAngle - 35, 20);
            g.DrawEllipse(new Pen(Color.FromArgb(195, 255, 255, 255), 8), new Rectangle(50, 50, 100, 100));
            _panel.AddLine("Speed", $"{pageGraphics.WindSpeed:F3}");
            _panel.Draw(g);
        }
    }
}
