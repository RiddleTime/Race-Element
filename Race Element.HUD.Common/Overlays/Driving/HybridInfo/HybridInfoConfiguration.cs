using System.Drawing;
using RaceElement.HUD.Common.Overlays.Driving.LapDeltaBar;
using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.Common.Overlays.Driving.HybridInfo;

public sealed class HybridInfoConfiguration: OverlayConfiguration
{
    public HybridInfoConfiguration() => GenericConfiguration.AllowRescale = true;
    
    [ConfigGrouping("Bar", "Adjust bar behavior.")]
    public BarGrouping Bar { get; init; } = new();
    public sealed class BarGrouping
    {
        [ToolTip("Sets the Width of the Delta Bar.")]
        [IntRange(180, 800, 10)]
        public int Width { get; init; } = 300;

        [ToolTip("Sets the Height of the Delta Bar.")]
        [IntRange(20, 60, 2)]
        public int Height { get; init; } = 32;

        [IntRange(1, 9, 1)]
        public int Roundness { get; init; } = 5;
        
        [ToolTip("Sets the size of the font.")]
        [IntRange(12, 30, 2)]
        public int FontSize { get; init; } = 20;
        
        [ToolTip("Maximum decimal position for energy value.")]
        [IntRange(0, 3, 1)]
        public int Decimals { get; init; } = 1;
    }
    
    
    [ConfigGrouping("Colors and thresholds", "Adjust Colors and thresholds.")]
    public ColorsGrouping Colors { get; init; } = new();
    public sealed class ColorsGrouping
    {
        [ToolTip("Sets the color and threshold for the high energy stage.")]
        public Color ThresholdHighColor { get; init; } = Color.FromArgb(255, Color.LimeGreen);
        [IntRange(75, 255, 1)]
        public int ThresholdHighOpacity { get; init; } = 255;
        [IntRange(1, 99, 1)]
        public int ThresholdHighLevel { get; init; } = 70;
        
        [ToolTip("Sets the color and threshold for the medium energy stage.")]
        public Color ThresholdMediumColor { get; init; } = Color.FromArgb(255, Color.Yellow);
        [IntRange(75, 255, 1)]
        public int ThresholdMediumOpacity { get; init; } = 255;
        [IntRange(1, 99, 1)]
        public int ThresholdMediumLevel { get; init; } = 40;
        
        [ToolTip("Sets the color and threshold for the low energy stage.")]
        public Color ThresholdLowColor { get; init; } = Color.FromArgb(255, Color.OrangeRed);
        [IntRange(75, 255, 1)]
        public int ThresholdLowOpacity { get; init; } = 255;
        [IntRange(1, 99, 1)]
        public int ThresholdLowLevel { get; init; } = 10;
        
    }
    
}