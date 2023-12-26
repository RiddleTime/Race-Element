using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerData;

internal sealed class CornerDataConfiguration : OverlayConfiguration
{
    [ConfigGrouping("Table", "Show or hide extra laps or hide the header.")]
    public TableGrouping Table { get; init; } = new TableGrouping();
    public sealed class TableGrouping
    {
        [ToolTip("Adjust the amount corners shown as history.")]
        [IntRange(1, 10, 1)]
        public int CornerAmount { get; init; } = 4;

        [ToolTip("Draws the first row as a header showing labels for each column.")]
        public bool Header { get; init; } = true;
    }

    public enum DeltaSource
    {
        BestSessionLap,
        LastLap,
        Off
    };

    [ConfigGrouping("Data", "Show or hide extra data/columns")]
    public DataGrouping Data { get; init; } = new DataGrouping();
    public sealed class DataGrouping
    {
        [ToolTip("Adjust source of the delta for each column, if enabled it will show for both the minimum and average speed.")]
        public DeltaSource DeltaSource { get; init; } = DeltaSource.BestSessionLap;

        [ToolTip("Shows the minimum speed through each corner")]
        public bool MinimumSpeed { get; init; } = true;

        [ToolTip("Shows the average speed through each corner.")]
        public bool AverageSpeed { get; init; } = true;

        [ToolTip("Shows the maximum lateral G force for each corner.")]
        public bool MaxLatG { get; init; } = true;
    }

    [ConfigGrouping("Colors", "Adjust colors used for delta")]
    public ColorGrouping Colors { get; init; } = new ColorGrouping();
    public sealed class ColorGrouping
    {
        [ToolTip("Sets the color for the delta colum when the delta is better.")]
        public Color DeltaFaster { get; init; } = Color.FromArgb(50, 205, 50);
        [ToolTip("Sets the color for the delta colum when the delta is worse.")]
        public Color DeltaSlower { get; init; } = Color.FromArgb(255, 0, 0);

        [ToolTip("Sets the color for the both the avg and min speed colum when the speed is faster.")]
        public Color SpeedFaster { get; init; } = Color.FromArgb(50, 205, 50);
        [ToolTip("Sets the color for the both the avg and min speed colum when the speed is slower.")]
        public Color SpeedSlower { get; init; } = Color.FromArgb(255, 255, 0);
    }

    public CornerDataConfiguration() => AllowRescale = true;
}
