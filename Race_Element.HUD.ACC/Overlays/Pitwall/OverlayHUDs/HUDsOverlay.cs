using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

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

        public sealed override void BeforeStart() => _panel = new InfoPanel(11, Width);
        public sealed override void BeforeStop() => _panel = null;

        public sealed override bool ShouldRender() => true;

        public sealed override void Render(Graphics g)
        {
            OverlaysACC.ActiveOverlays.ForEach(hud => _panel.AddLine(hud.Name, $"X: {hud.X}, Y:{hud.Y}, Scale: {hud.Scale:F3}, Herz: {hud.RefreshRateHz}, Visible: {hud.ShouldRender()}"));
            _panel.Draw(g);
        }
    }
}
