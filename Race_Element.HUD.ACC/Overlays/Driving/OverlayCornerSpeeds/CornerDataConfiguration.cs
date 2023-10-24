using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerSpeeds
{
    internal sealed class CornerDataConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Table", "Adjust what is shown in the table")]
        public TableGrouping Table { get; set; } = new TableGrouping();
        public class TableGrouping
        {
            [ToolTip("Adjust the amount corners shown as history.")]
            [IntRange(1, 8, 1)]
            public int CornerCount { get; set; } = 3;

            [ToolTip("Draws the first row as a header showing labels for each column.")]
            public bool ShowHeader { get; set; } = true;
        }

        private sealed class DataGrouping
        {
            public bool CornerG { get; set; } = false;
        }

        public CornerDataConfiguration() => AllowRescale = true;
    }
}
