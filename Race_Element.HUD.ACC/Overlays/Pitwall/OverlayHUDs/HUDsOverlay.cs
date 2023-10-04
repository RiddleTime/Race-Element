using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayHUDs
{
    [Overlay(Name = "HUDs",
        Description = "Shows info about active HUDs",
        OverlayType = OverlayType.Debug)]
    internal sealed class HUDsOverlay : AbstractOverlay
    {
        private readonly HUDsConfiguration _config = new HUDsConfiguration();
        private sealed class HUDsConfiguration : OverlayConfiguration
        {
            public HUDsConfiguration() => this.AllowRescale = true;
        }

        private InfoPanel _panel;
        public HUDsOverlay(Rectangle rectangle) : base(rectangle, "HUDs")
        {
            Width = 700;
            Height = 170;
            RefreshRateHz = 10;
        }
        public sealed override void BeforeStart()
        {
            _panel = new InfoPanel(11, Width);
        }
        public override bool ShouldRender() => true;

        public override void Render(Graphics g)
        {
            foreach (var item in OverlaysACC.ActiveOverlays)
                _panel.AddLine(item.Name, $"X: {item.X}, Y:{item.Y}, Scale: {item.Scale:F3}, Herz: {item.RefreshRateHz}, Visible: {item.ShouldRender()}");

            _panel.Draw(g);
        }
    }
}
