using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayLapDelta
{
    internal sealed class LapTimeDeltaConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Delta", "Adjust how the delta is displayed")]
        public DeltaGrouping Delta { get; set; } = new DeltaGrouping();
        public class DeltaGrouping
        {
            [ToolTip("Sets the maximum range in seconds for the delta bar.")]
            [FloatRange(0.02f, 5.00f, 0.02f, 2)]
            public float MaxDelta { get; set; } = 2;

            [ToolTip("Sets the amount of decimals.")]
            [IntRange(1, 3, 1)]
            public int Decimals { get; set; } = 3;

            [ToolTip("Sets the size of the font.")]
            [IntRange(12, 30, 2)]
            public int FontSize { get; set; } = 20;

            [ToolTip("Hide the Lap Delta HUD during a Race session.")]
            public bool HideForRace { get; set; } = false;
        }

        [ConfigGrouping("Bar", "Adjust bar behavior.")]
        public BarGrouping Bar { get; set; } = new BarGrouping();
        public class BarGrouping
        {
            [ToolTip("Sets the Width of the Delta Bar.")]
            [IntRange(180, 800, 10)]
            public int Width { get; set; } = 300;

            [ToolTip("Sets the Height of the Delta Bar.")]
            [IntRange(20, 60, 2)]
            public int Height { get; set; } = 32;

            [IntRange(1, 9, 1)]
            public int Roundness { get; set; } = 5;
        }

        [ConfigGrouping("Colors", "Adjust Colors.")]
        public ColorsGrouping Colors { get; set; } = new ColorsGrouping();
        public class ColorsGrouping
        {
            [ToolTip("Sets the color when the delta is negative (faster).")]
            public Color FasterColor { get; set; } = Color.FromArgb(255, Color.LimeGreen);
            [IntRange(75, 255, 1)]
            public int FasterOpacity { get; set; } = 255;

            [ToolTip("Sets the color when the delta is positive (slower).")]
            public Color SlowerColor { get; set; } = Color.FromArgb(255, Color.OrangeRed);
            [IntRange(75, 255, 1)]
            public int SlowerOpacity { get; set; } = 255;
        }

        public LapTimeDeltaConfiguration() => this.AllowRescale = true;
    }
}
