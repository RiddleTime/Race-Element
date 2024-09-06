using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.Common.Overlays.Driving.Relative
{
    internal class RelativeConfiguration : OverlayConfiguration 
    {
        /* TODO This needs some work.
        [ConfigGrouping("Information", "Show or hide additional information in the Relative.")]
        public InformationGrouping Information { get; init; } = new InformationGrouping();
        public class InformationGrouping
        {            
            [ToolTip("Interval")]
            public bool Interval { get; init; } = true;

            [ToolTip("Last lap")]
            public bool LastLap { get; init; } = true;

            [ToolTip("License and Ratings")]
            public bool LicenseRatings { get; init; } = true;
        } */

        [ConfigGrouping("Layout", "Change the layout of the Relative.")]
        public LayoutGrouping Layout { get; init; } = new LayoutGrouping();
        public sealed class LayoutGrouping
        {
            [ToolTip("Additional Rows in front and behind.")]
            [IntRange(1, 5, 1)]
            public int AdditionalRows { get; init; } = 2;
        }

    }
}