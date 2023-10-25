using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerData
{
    internal sealed class CornerDataConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Table", "Adjust what is shown in the table")]
        public TableGrouping Table { get; set; } = new TableGrouping();
        public sealed class TableGrouping
        {
            [ToolTip("Draws the first row as a header showing labels for each column.")]
            public bool Header { get; set; } = true;

            [ToolTip("Adjust the amount corners shown as history.")]
            [IntRange(1, 10, 1)]
            public int CornerAmount { get; set; } = 4;
        }

        [ConfigGrouping("Data", "Show or hide extra data/columns")]
        public DataGrouping Data { get; set; } = new DataGrouping();
        public sealed class DataGrouping
        {
            [ToolTip("Shows the maximum lateral G force for each corner.")]
            public bool MaxLatG { get; set; } = true;

            [ToolTip("Shows for each column a delta compared to your best valid lap.")]
            public bool BestLapDelta { get; set; } = true;
        }

        public CornerDataConfiguration() => AllowRescale = true;
    }
}
