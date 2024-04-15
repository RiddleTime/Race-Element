using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.RainPrediction;

internal sealed class RainPredictionConfiguration : OverlayConfiguration
{
    public RainPredictionConfiguration() => this.GenericConfiguration.AllowRescale = true;
}

