using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayLapDelta
{
    internal sealed class LapDeltaConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Bar", "Adjust bar behavior.")]
        public BarGrouping Bar { get; set; } = new BarGrouping();
        public class BarGrouping
        {
            [ToolTip("Sets the Width of the Delta Bar.")]
            [IntRange(250, 800, 10)]
            public int Width { get; set; } = 300;

            [ToolTip("Sets the Height of the Delta Bar.")]
            [IntRange(20, 60, 2)]
            public int Height { get; set; } = 30;

            [IntRange(1, 10, 1)]
            public int Roundness { get; set; } = 10;
        }

        [ConfigGrouping("Delta", "Adjust how the delta is displayed")]
        public DeltaGrouping Delta { get; set; } = new DeltaGrouping();
        public class DeltaGrouping
        {
            [ToolTip("Sets the size of the font.")]
            [IntRange(12, 30, 2)]
            public int FontSize { get; set; } = 12;

            [ToolTip("Sets the amount of decimals.")]
            [IntRange(1, 3, 1)]
            public int Decimals { get; set; } = 2;

            [ToolTip("Sets the maximum range in seconds for the delta bar.")]
            [IntRange(1, 5, 1)]
            public int MaxDelta { get; set; } = 2;
        }

        [ConfigGrouping("Colors", "Adjust Colors.")]
        public ColorsGrouping Colors { get; set; } = new ColorsGrouping();
        public class ColorsGrouping
        {
            [ToolTip("Sets the color when the delta is negative (faster).")]
            public Color FasterColor { get; set; } = Color.FromArgb(255, Color.LimeGreen);

            [ToolTip("Sets the color when the delta is positive (slower).")]
            public Color SlowerColor { get; set; } = Color.FromArgb(255, Color.OrangeRed);
        }

        public LapDeltaConfiguration()
        {
            this.AllowRescale = true;
        }
    }
}
