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
            Width = 1000;
            Height = 300;
            RefreshRateHz = 10;
        }

        public sealed override void BeforeStart() => _panel = new InfoPanel(11, Width);
        public sealed override void BeforeStop() => _panel = null;

        public sealed override bool ShouldRender() => true;

        public sealed override void Render(Graphics g)
        {
            long totalPixels = 0;
            long pixelsPerSecond = 0;

            OverlaysACC.ActiveOverlays.ForEach(hud =>
            {
                long pixels = (long)(hud.Scale * hud.Width * hud.Height);
                pixelsPerSecond += (long)(pixels * hud.RefreshRateHz);
                totalPixels += pixels;
            });

            _panel.AddLine($"{OverlaysACC.ActiveOverlays.Count} activated", $"Pixels Area: {totalPixels}, Per Second: {pixelsPerSecond}");
            OverlaysACC.ActiveOverlays.ForEach(hud =>
            {
                long pixels = (long)(hud.Scale * hud.Width * hud.Height);
                _panel.AddLine(hud.Name, $"X: {hud.X}, Y:{hud.Y}, Scale: {hud.Scale:F3}, Herz: {hud.RefreshRateHz}, Visible: {hud.ShouldRender()}, Pixels: {pixels:F0}, PPS: {hud.RefreshRateHz * pixels:F0}");
            });
            _panel.Draw(g);
        }
    }
}
