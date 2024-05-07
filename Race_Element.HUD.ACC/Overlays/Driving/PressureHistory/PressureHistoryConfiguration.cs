using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.PressureHistory;

internal sealed class PressureHistoryConfiguration : OverlayConfiguration
{
    public PressureHistoryConfiguration() => GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Table", "Adjust settings for the sector data table")]
    public TableGrouping Table { get; init; } = new();
    public sealed class TableGrouping
    {
        [ToolTip("Changed the amount of corner roundering for each cell in the table.")]
        [IntRange(0, 12, 2)]
        public int Roundness { get; init; } = 2;
    }
}

