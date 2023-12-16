using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayShiftIndicator
{
    internal sealed class ShiftIndicatorConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Bar", "The shape and options of the shift indicator bar.")]
        public BarsGrouping Bar { get; set; } = new BarsGrouping();
        public class BarsGrouping
        {
            [ToolTip("Sets the Width of the shift indicator bar.")]
            [IntRange(430, 800, 10)]
            public int Width { get; set; } = 530;

            [ToolTip("Sets the Height of the shift indicator bar.")]
            [IntRange(25, 50, 5)]
            public int Height { get; set; } = 40;

            [ToolTip("Hide Rpms in the bar, starting from 0.")]
            [IntRange(0, 3000, 100)]
            public int HideRpm { get; set; } = 3000;

            [ToolTip("Displays the current RPM inside of the shift indicator bar.")]
            public bool ShowRpmText { get; set; } = true;

            [ToolTip("Displays when the pit limiter is active.")]
            public bool ShowPitLimiter { get; set; } = true;

            [ToolTip("Sets the refresh rate.")]
            [IntRange(20, 70, 2)]
            public int RefreshRate { get; set; } = 50;

            [ToolTip("Sets the frequency of the upshift flash.")]
            [FloatRange(0.5f, 10f, 0.5f, 1)]
            public float FlashFrequency { get; set; } = 5f;
        }

        [ConfigGrouping("Upshift Percentages", "Adjust the Early and Upshift percentages.\n" +
            "The displayed early and upshift RPM texts only show in the GUI.\n" +
            "These RPMs will update if you are currently in a lobby, once you adjust any of the settings.")]
        public UpshiftGrouping Upshift { get; set; } = new UpshiftGrouping();
        public class UpshiftGrouping
        {
            [ToolTip("Sets the percentage of max rpm required to activate the early upshift color")]
            [FloatRange(80.0f, 96.8f, 0.02f, 2)]
            public float Early { get; set; } = 94.0f;

            [ToolTip("Sets the percentage of max rpm required to activate the upshift color")]
            [FloatRange(97f, 99.98f, 0.02f, 2)]
            public float Upshift { get; set; } = 97.3f;
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

            public Color FlashColor { get; set; } = Color.FromArgb(255, 0, 131, 255);
            [IntRange(75, 255, 1)]
            public int FlashOpacity { get; set; } = 255;
        }

        public ShiftIndicatorConfiguration()
        {
            this.AllowRescale = true;
        }
    }
}
