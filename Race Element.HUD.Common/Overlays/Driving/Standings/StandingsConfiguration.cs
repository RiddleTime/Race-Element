using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.Common.Overlays.Driving.Standings;

internal sealed class StandingsConfiguration : OverlayConfiguration
{
    public StandingsConfiguration() => GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Information", "Show or hide additional information in the standings.")]
    public InformationGrouping Information { get; init; } = new InformationGrouping();
    public class InformationGrouping
    {
        [ToolTip("Multiclass")]
        public bool MultiClass { get; init; } = true;

        [ToolTip("Time Delta")]
        public bool TimeDelta { get; init; } = true;

        [ToolTip("Show an indicator for invalid laps.")]
        public bool InvalidLap { get; init; } = true;
    }

    [ConfigGrouping("Layout", "Change the layout of the standings.")]
    public LayoutGrouping Layout { get; init; } = new LayoutGrouping();
    public sealed class LayoutGrouping
    {
        [ToolTip("Additional Rows in front and behind.")]
        [IntRange(1, 5, 1)]
        public int AdditionalRows { get; init; } = 2;

        [ToolTip("Multiclass Rows")]
        [IntRange(1, 10, 1)]
        public int MulticlassRows { get; init; } = 4;
    }
}
