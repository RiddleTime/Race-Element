using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayLapDelta
{
    internal sealed class LapDeltaConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Delta", "Adjust how the delta is displayed")]
        public DeltaGrouping Delta { get; set; } = new DeltaGrouping();
        public class DeltaGrouping
        {
            [ToolTip("Sets the amount of decimals.")]
            [IntRange(1, 3, 1)]
            public int Decimals { get; set; } = 2;

            [ToolTip("Sets the maximum range in seconds for the delta bar.")]
            [IntRange(1, 5, 1)]
            public int MaxDelta { get; set; } = 2;

            [ToolTip("Sets the size of the font.")]
            [IntRange(12, 30, 1)]
            public int FontSize { get; set; } = 12;
        }

        [ConfigGrouping("Bar", "Adjust bar behavior.")]
        public BarGrouping Bar { get; set; } = new BarGrouping();
        public class BarGrouping
        {
            [ToolTip("Sets the Width of the Delta Bar.")]
            [IntRange(300, 800, 10)]
            public int Width { get; set; } = 300;

            [ToolTip("Sets the Height of the Delta Bar.")]
            [IntRange(20, 55, 5)]
            public int Height { get; set; } = 30;

            [ToolTip("Sets the color when the delta is positive (slower).")]
            public Color PositiveColor { get; set; } = Color.FromArgb(255, Color.OrangeRed);

            [ToolTip("Sets the color when the delta is negative (faster).")]
            public Color NegativeColor { get; set; } = Color.FromArgb(255, Color.LimeGreen);
        }

        public LapDeltaConfiguration()
        {
            this.AllowRescale = true;
        }
    }
}
