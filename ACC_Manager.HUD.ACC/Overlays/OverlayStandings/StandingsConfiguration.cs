using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.OverlayStandings
{
    internal sealed class StandingsConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Information", "Show or hide additional information in the standings.")]
        public InformationGrouping Information { get; set; } = new InformationGrouping();
        public class InformationGrouping
        {
            [ToolTip("Multiclass")]
            public bool MultiClass { get; set; } = true;

            [ToolTip("Time Delta")]
            public bool TimeDelta { get; set; } = true;

            [ToolTip("Show an indicator for invalid laps.")]
            public bool InvalidLap { get; set; } = true;
        }

        [ConfigGrouping("Layout", "Change the layout of the standings.")]
        public LayoutGrouping Layout { get; set; } = new LayoutGrouping();
        public class LayoutGrouping
        {
            [ToolTip("Additional Rows in front and behind.")]
            [IntRange(1, 5, 1)]
            public int AdditionalRows { get; set; } = 2;

            [ToolTip("Multiclass Rows")]
            [IntRange(1, 10, 1)]
            public int MulticlassRows { get; set; } = 4;
        }

        public StandingsConfiguration()
        {
            this.AllowRescale = true;
        }
    }
}
