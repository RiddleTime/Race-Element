using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerData
{
    internal sealed class CornerDataConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Table", "Adjust what is shown in the table")]
        public TableGrouping Table { get; set; } = new TableGrouping();
        public sealed class TableGrouping
        {
            [ToolTip("Adjust the amount corners shown as history.")]
            [IntRange(1, 10, 1)]
            public int CornerAmount { get; set; } = 4;

            [ToolTip("Draws the first row as a header showing labels for each column.")]
            public bool Header { get; set; } = true;
        }

        public enum DeltaSource { BestSessionLap, LastLap, Off };

        [ConfigGrouping("Data", "Show or hide extra data/columns")]
        public DataGrouping Data { get; set; } = new DataGrouping();
        public sealed class DataGrouping
        {
            [ToolTip("Adjust source of the delta for each column, if enabled it will show for both the minimum and average speed.")]
            public DeltaSource DeltaSource { get; set; } = DeltaSource.BestSessionLap;

            [ToolTip("Shows the minimum speed through each corner")]
            public bool MinimumSpeed { get; set; } = true;

            [ToolTip("Shows the average speed through each corner.")]
            public bool AverageSpeed { get; set; } = true;

            [ToolTip("Shows the maximum lateral G force for each corner.")]
            public bool MaxLatG { get; set; } = true;
        }

        public CornerDataConfiguration() => AllowRescale = true;
    }
}
