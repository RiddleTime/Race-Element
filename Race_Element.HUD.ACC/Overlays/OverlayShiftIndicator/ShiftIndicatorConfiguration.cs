using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.OverlayShiftIndicator
{
    internal sealed class ShiftIndicatorConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Bar", "The shape and options of the shift indicator bar.")]
        public BarsGrouping Bar { get; set; } = new BarsGrouping();
        public class BarsGrouping
        {
            [ToolTip("Sets the Width of the shift indicator bar.")]
            [IntRange(160, 800, 10)]
            public int Width { get; set; } = 300;

            [ToolTip("Sets the Height of the shift indicator bar.")]
            [IntRange(20, 45, 5)]
            public int Height { get; set; } = 30;

            [ToolTip("Displays the current RPM inside of the shift indicator bar.")]
            public bool ShowRpm { get; set; } = true;

            [ToolTip("Displays when the pit limiter is active.")]
            public bool ShowPitLimiter { get; set; } = true;

            [ToolTip("Sets the refresh rate.")]
            [IntRange(20, 70, 2)]
            public int RefreshRate { get; set; } = 50;
        }

        [ConfigGrouping("Colors", "Adjust the colors used in the shift bar")]
        public ColorsGrouping Colors { get; set; } = new ColorsGrouping();
        public class ColorsGrouping
        {
            public Color NormalColor { get; set; } = Color.FromArgb(255, 5, 255, 5);
            [IntRange(75, 255, 1)]
            public int NormalOpacity { get; set; } = 255;

            public Color EarlyColor { get; set; } = Color.FromArgb(255, 255, 255, 0);
            [IntRange(75, 255, 1)]
            public int EarlyOpacity { get; set; } = 255;

            public Color UpshiftColor { get; set; } = Color.FromArgb(255, 255, 4, 4);
            [IntRange(75, 255, 1)]
            public int UpshiftOpacity { get; set; } = 255;
        }

        public ShiftIndicatorConfiguration()
        {
            this.AllowRescale = true;
        }
    }
}
