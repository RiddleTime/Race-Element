using RaceElement.Data.ACC.Tracks;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackDistance;

[Overlay(Name = "Track Distance", Description = "Shows the total track distance and current distance on track.")]
internal sealed class TrackDistanceOverlay(Rectangle rectangle) : AbstractOverlay(rectangle, "Track Distance")
{
    private readonly TrackDistanceConfiguration _config = new();
    private sealed class TrackDistanceConfiguration : OverlayConfiguration
    {
        public TrackDistanceConfiguration() => GenericConfiguration.AllowRescale = true;
    }

    private InfoPanel _panel;

    public sealed override void SetupPreviewData()
    {
        pageGraphics.NormalizedCarPosition = 0.312f;
        pageStatic.Track = "Paul_Ricard";
    }

    public sealed override void BeforeStart()
    {
        _panel = new InfoPanel(12, 300) { FirstRowLine = 1 };
        Width = 300;
        Height = _panel.FontHeight * 2;
    }

    public sealed override void Render(Graphics g)
    {
        var currentTrack = TrackData.GetCurrentTrack(pageStatic.Track);
        if (currentTrack == null)
            _panel.AddLine($"No Track", "Waiting...");
        else
            _panel.AddProgressBarWithCenteredText($"{pageGraphics.NormalizedCarPosition * currentTrack.TrackLength:F1}M", 0, 1, pageGraphics.NormalizedCarPosition);

        _panel.AddLine("Stint Distance", $"{pageGraphics.DistanceTraveled:F1}M");

        _panel.Draw(g);
    }
}
