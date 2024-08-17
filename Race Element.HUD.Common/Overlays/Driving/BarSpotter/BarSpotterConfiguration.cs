using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.OverlayBarSpotter;

internal sealed class BarSpotterConfiguration : OverlayConfiguration
{
    [ConfigGrouping("Bar", "The shape and options of the spotter indicator bar.")]
    public BarsGrouping Bar { get; init; } = new BarsGrouping();
    public sealed class BarsGrouping
    {
        [ToolTip("Sets the Width of the spotter indicator bar.")]
        [IntRange(10, 50, 1)]
        public int Width { get; init; } = 20;

        [ToolTip("Sets the Height of the spotter indicator bar.")]
        [IntRange(200, 500, 5)]
        public int Height { get; init; } = 250;

        [ToolTip("Distance between the spotter indicator bars.")]
        [IntRange(200, 800, 5)]
        public int Distance { get; init; } = 400;




        [ToolTip("Sets the refresh rate.")]
        [IntRange(20, 70, 2)]
        public int RefreshRate { get; init; } = 50;        
    }

    

    [ConfigGrouping("Colors", "Adjust the colors used in the spotter bar")]
    public ColorsGrouping Colors { get; init; } = new ColorsGrouping();
    public sealed class ColorsGrouping
    {
        public Color NormalColor { get; init; } = Color.FromArgb(255, 5, 255, 5);
        [IntRange(75, 255, 1)]
        public int NormalOpacity { get; init; } = 255;

   }

    public BarSpotterConfiguration()
    {
        this.GenericConfiguration.AllowRescale = true;
    }
}
