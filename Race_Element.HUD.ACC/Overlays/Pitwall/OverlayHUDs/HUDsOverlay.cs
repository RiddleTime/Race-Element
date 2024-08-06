using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayHUDs;

[Overlay(Name = "HUDs",
    Description = "Shows info about active HUDs",
    OverlayType = OverlayType.Pitwall)]
internal sealed class HUDsOverlay : ACCOverlay
{
    private readonly HUDsConfiguration _config = new();
    private sealed class HUDsConfiguration : OverlayConfiguration
    {
        public HUDsConfiguration() => this.GenericConfiguration.AllowRescale = true;
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
        // add summary line
        long totalPixels = 0;
        long pixelsPerSecond = 0;
        OverlaysAcc.ActiveOverlays.ForEach(hud =>
        {
            long pixels = (long)(hud.Scale * hud.Width * hud.Height);
            pixelsPerSecond += (long)(pixels * hud.RefreshRateHz);
            totalPixels += pixels;
        });
        _panel.AddLine($"  - {OverlaysAcc.ActiveOverlays.Count} HUDs -  ", $"Pixels: {totalPixels}, Per Second: {pixelsPerSecond}");

        // add details lines for all active huds
        OverlaysAcc.ActiveOverlays.ForEach(hud =>
        {
            long pixels = (long)(hud.Scale * hud.Width * hud.Height);
            _panel.AddLine(hud.Name, $"({hud.X}, {hud.Y}, {hud.Width}, {hud.Height}, scale: {hud.Scale:F3}), herz: {hud.RefreshRateHz}, vis: {hud.ShouldRender()}, pix: {pixels:F0}, PPS: {hud.RefreshRateHz * pixels:F0}");
        });

        // draw the panel
        _panel.Draw(g);
    }
}
