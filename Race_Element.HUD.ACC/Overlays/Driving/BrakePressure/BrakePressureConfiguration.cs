using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.BrakePressure
{
    internal sealed class BrakePressureConfiguration : OverlayConfiguration
    {
        public BrakePressureConfiguration() => GenericConfiguration.AllowRescale = true;

        [ConfigGrouping("Colors", "Adjust the colors for the brake pressure indicator.")]
        public ColorGrouping Colors { get; set; } = new ColorGrouping();
        internal sealed class ColorGrouping
        {
            public Color TextColor { get; init; } = Color.FromArgb(255, 255, 255, 255);

            [IntRange(70, 255, 1)]
            public int TextOpacity { get; init; } = 255;

            public Color FillColor { get; init; } = Color.FromArgb(255, 255, 69, 0);
            [IntRange(70, 255, 1)]
            public int FillOpacity { get; init; } = 255;

            public Color OutlineColor { get; init; } = Color.FromArgb(255, 0, 0, 0);
            [IntRange(70, 255, 1)]
            public int OutlineOpacity { get; init; } = 255;

            public Color BackgroundColor { get; init; } = Color.FromArgb(255, 0, 0, 0);
            [IntRange(70, 255, 1)]
            public int BackgroundOpacity { get; init; } = 96;
        }
    }
}
