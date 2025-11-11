using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.Opponents;

internal sealed class OpponentsConfiguration : OverlayConfiguration
{
    [ConfigGrouping("Opponents", "Change how many opponents are displayed either ahead or behind")]
    public OpponentsGrouping Opponents { get; init; } = new();
    public sealed class OpponentsGrouping
    {
        [IntRange(1, 5, 1)]
        public int AheadCount { get; init; } = 1;
        [IntRange(1, 5, 1)]
        public int BehindCount { get; init; } = 1;
    }

    [ConfigGrouping("Data", "Change which opponents data is displayed.")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        public bool Sectors { get; init; } = true;
        public bool Difference { get; init; } = true;
    }
}
