using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.RainPrediction;

internal sealed class RainPredictionConfiguration : OverlayConfiguration
{
    public RainPredictionConfiguration() => this.GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Time", "Adjust settings related to the flow of time.")]
    public TimeGrouping Time { get; init; } = new();
    public sealed class TimeGrouping
    {
        [IntRange(0, 48, 1)]
        public int TimeMultiplier { get; init; } = 1;
    }
}

