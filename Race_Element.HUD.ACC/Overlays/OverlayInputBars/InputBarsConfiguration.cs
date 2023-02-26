using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayInputBars
{
    internal sealed class InputBarsConfiguration : OverlayConfiguration
    {
        public enum BarOrientation { Horizontal, Vertical };

        [ConfigGrouping("Bars", "The shape and spacing of the bars")]
        public BarsGrouping Bars { get; set; } = new BarsGrouping();
        public class BarsGrouping
        {
            public BarOrientation Orientation { get; set; } = BarOrientation.Vertical;

            [ToolTip("Length of the input bars.")]
            [IntRange(100, 250, 1)]
            public int Length { get; set; } = 200;

            [ToolTip("Changes the thickness of each input bar.")]
            [IntRange(10, 45, 1)]
            public int Thickness { get; set; } = 20;

            [ToolTip("Changes the spacing between the input bars.")]
            [IntRange(1, 150, 1)]
            public int Spacing { get; set; } = 5;

            [ToolTip("Changes the order of the bars, throttle first and brake second (left to right and top to bottom).")]
            internal bool ThrottleFirst { get; set; }
        }

        [ConfigGrouping("Colors", "The shape and spacing of the bars")]
        public ColorsGrouping Colors { get; set; } = new ColorsGrouping();
        public class ColorsGrouping
        {
            [ToolTip("Changes the color of the throttle bar.")]
            public Color ThrottleColor { get; set; } = Color.FromArgb(255, 50, 205, 1);
            [IntRange(75, 255, 1)]
            public int ThrottleOpacity { get; set; } = 255;

            [ToolTip("Changes the color of the brake bar.")]
            public Color BrakeColor { get; set; } = Color.FromArgb(255, 255, 69, 0);
            [IntRange(75, 255, 1)]
            public int BrakeOpacity { get; set; } = 255;

            [ToolTip("Changes the color when either ABS or TC is activated.")]
            public Color AssistsColor { get; set; } = Color.FromArgb(255, 255, 247, 0);
            [IntRange(75, 255, 1)]
            public int AssistsOpacity { get; set; } = 255;
        }


        public InputBarsConfiguration()
        {
            AllowRescale = true;
        }
    }
}
