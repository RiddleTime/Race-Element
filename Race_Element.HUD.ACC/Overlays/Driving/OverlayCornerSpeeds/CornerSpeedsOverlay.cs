using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerSpeeds
{
    [Overlay(Name = "Corner Speeds",
            Description = "Shows corner speeds for each corner.",
            OverlayCategory = OverlayCategory.Lap,
            OverlayType = OverlayType.Release)]
    internal class CornerSpeedsOverlay : AbstractOverlay
    {
        private readonly CornerSpeedsConfiguration _config = new CornerSpeedsConfiguration();
        private sealed class CornerSpeedsConfiguration : OverlayConfiguration
        {
            [ConfigGrouping("Table", "Adjust what is shown in the table")]
            private class TableGrouping
            {
                [ToolTip("Adjust the amount corners shown as history.")]
                [IntRange(1, 5, 1)]
                public int CornerCount { get; set; } = 3;
            }

            public CornerSpeedsConfiguration() => AllowRescale = true;
        }

        private InfoTable _table;

        public CornerSpeedsOverlay(Rectangle rectangle) : base(rectangle, "Corner Speeds")
        {
            Width = 300;
            Height = 150;
        }

        public override void BeforeStart()
        {

        }

        public override void Render(Graphics g)
        {

            // draw table of previous corners, min speed? corner g? min gear? 
        }
    }
}
