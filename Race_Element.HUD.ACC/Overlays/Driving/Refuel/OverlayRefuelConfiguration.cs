using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.Refuel
{
    internal sealed class OverlayRefuelConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Refuel Info", "Show or hide additional information in the panel.")]
        public InfoPanelGrouping RefuelInfoGrouping { get; init; } = new InfoPanelGrouping();
        public sealed class InfoPanelGrouping
        {
            [ToolTip("Show solid progress bar.")] 
            public bool SolidProgressBar { get; init; } = false;

            [ToolTip("Amount of extra laps for fuel calculation.")]
            [IntRange(1, 5, 1)]
            public int ExtraLaps { get; init; } = 2;

            [ToolTip("Display the race progress bar only with stint or pit stop indicator to use the overlay together with the FuelInfo Overlay.")]
            public bool ProgressBarOnly { get; init; } = true;
        }

        public OverlayRefuelConfiguration()
        {
            this.GenericConfiguration.AllowRescale = true;
        }
    }
}
