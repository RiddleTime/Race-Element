using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.SectorData
{
    internal sealed class SectorDataConfiguration : OverlayConfiguration
    {
        public SectorDataConfiguration() => GenericConfiguration.AllowRescale = true;

        [ConfigGrouping("Table", "Adjust settings for the sector data table")]
        public TableGrouping Table { get; init; } = new();
        public sealed class TableGrouping
        {
            [ToolTip("Sets the amount of previous sectors visible, in general settings it to more than 3 will also show you the data from previous laps.")]
            [IntRange(1, 13, 1)]
            public int Rows { get; init; } = 4;

            public bool LiveCurrentSector { get; init; } = false;

            [ToolTip("Changed the amount of corner roundering for each cell in the table.")]
            [IntRange(0, 12, 2)]
            public int Roundness { get; init; } = 2;
        }
    }
}
