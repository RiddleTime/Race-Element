using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.OverlayInputBars;

internal sealed class InputBarsConfiguration : OverlayConfiguration
{
    public InputBarsConfiguration() => GenericConfiguration.AllowRescale = true;

    public enum BarOrientation { Horizontal, Vertical };

    [ConfigGrouping("Bars", "The shape and spacing of the bars")]
    public BarsGrouping Bars { get; init; } = new BarsGrouping();
    public sealed class BarsGrouping
    {
        public BarOrientation Orientation { get; init; } = BarOrientation.Vertical;

        [ToolTip("Length of the input bars.")]
        [IntRange(100, 250, 1)]
        public int Length { get; init; } = 200;

        [ToolTip("Changes the thickness of each input bar.")]
        [IntRange(10, 45, 1)]
        public int Thickness { get; init; } = 20;

        [ToolTip("Changes the spacing between the input bars.")]
        [IntRange(1, 150, 1)]
        public int Spacing { get; init; } = 5;

        [ToolTip("Refresh rate in Hz of the HUD.")]
        [IntRange(30, 70, 1)]
        public int RefreshRate { get; init; } = 30;

        [ToolTip("Changes the order of the bars, throttle first and brake second (left to right and top to bottom).")]
        internal bool ThrottleFirst { get; set; }
    }

    [ConfigGrouping("Colors", "The shape and spacing of the bars")]
    public ColorsGrouping Colors { get; init; } = new ColorsGrouping();
    public sealed class ColorsGrouping
    {
        [ToolTip("Changes the color of the throttle bar.")]
        public Color ThrottleColor { get; init; } = Color.FromArgb(255, 50, 205, 1);
        [ToolTip("Changes the opacity of the color of the throttle bar.")]
        [IntRange(75, 255, 1)]
        public int ThrottleOpacity { get; init; } = 255;

        [ToolTip("Changes the color of the brake bar.")]
        public Color BrakeColor { get; init; } = Color.FromArgb(255, 255, 0, 0);
        [ToolTip("Changes the opacity of the color of the brake bar.")]
        [IntRange(75, 255, 1)]
        public int BrakeOpacity { get; init; } = 255;

        [ToolTip("Changes the throttle bar color when TC is activated.")]
        public Color TcColor { get; init; } = Color.FromArgb(255, 112, 0, 255);
        [ToolTip("Changes the opacity of the throttle bar color when TC is activated.")]
        [IntRange(75, 255, 1)]
        public int TcOpacity { get; init; } = 255;

        [ToolTip("Changes the brake bar color when ABS is activated.")]
        public Color AbsColor { get; init; } = Color.FromArgb(255, 255, 247, 0);
        [ToolTip("Changes the opacity of the brake bar color when ABS is activated.")]
        [IntRange(75, 255, 1)]
        public int AbsOpacity { get; init; } = 255;
    }
}
