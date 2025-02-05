using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.ShiftBar;
internal sealed class ShiftBarConfiguration : OverlayConfiguration
{
    public ShiftBarConfiguration() => GenericConfiguration.AllowRescale = false;

    [ConfigGrouping("Bar", "Adjust the size of the shift bar")]
    public SizeGrouping Bar { get; init; } = new();
    public sealed class SizeGrouping
    {
        [IntRange(100, 800, 10)]
        public int Width { get; init; } = 500;

        [IntRange(12, 50, 2)]
        public int Height { get; init; } = 24;

        [ToolTip("Don't go over your monitor's refresh rate, it's useless.\nSo lower it to 60 if you got a 60 hz monitor.")]
        [IntRange(50, 240, 10, GameMaxs = [150], MaxGames = [Game.iRacing])]
        public int RefreshRate { get; init; } = 90;
    }

    [ConfigGrouping("Data", "Adjust data displayed in the shift bar")]
    public DataGrouping Data { get; init; } = new();
    public sealed class DataGrouping
    {
        /// <summary>
        /// Used for showing a certain amount of RPM, <see cref="VisibleRpmAmount"/>
        /// </summary>
        public int HideRpm = 3_000;
        public readonly int MinVisibleRpm = 500;

        [ToolTip("The amount of RPM that the bar displays, 500 at minimum, will always be limited by the max rpm of the car.\n")]
        [IntRange(500, 30_000, 100)]
        public int VisibleRpmAmount { get; init; } = 4_000;

        [ToolTip("Shows a vertical line that indicates the optimal upshift point.")]
        public bool RedlineMarker { get; init; } = true;
    }

    [ConfigGrouping("Upshift Percentages", "Adjust the Early and Upshift percentages.\n" + "The Early is always checked first, so if the Redline is lower than the early.. it won't be hit.")]
    [HideForGame(Game.RaceRoom | Game.iRacing)]
    public UpshiftGrouping Upshift { get; init; } = new();
    public sealed class UpshiftGrouping
    {
        [ToolTip("Sets the percentage of max rpm required to activate the early upshift color")]
        [FloatRange(69.0f, 99.8f, 0.001f, 3)]
        public float EarlyPercentage { get; init; } = 93.0f;

        [ToolTip("Sets the percentage of max rpm required to activate the upshift color")]
        [FloatRange(70f, 99.98f, 0.001f, 3)]
        public float RedlinePercentage { get; init; } = 97.3f;

        [ToolTip("Only enable this when configuring the Upshift Percentages below." +
                      "\nDraws the outcome of these percentages when activating this HUD. Including the Max amount of rpm." +
                      "\n")]
        public bool DrawUpshiftData { get; init; } = false;

        /// <summary>
        /// If you know the max rpm you can use this to adjust the Max Rpm in the above Preview Image.
        /// </summary>
        [IntRange(10, 17_000, 1)]
        [ToolTip("Sets the current Rpm in the above preview image.")]
        public int PreviewRpm { get; init; } = 8500;
        /// <summary>
        /// If you know the max rpm you can use this to adjust the Max Rpm in the above Preview Image.
        /// </summary>
        [IntRange(10, 17_000, 1)]
        [ToolTip("Sets the current Max Rpm in the above preview image.")]
        public int MaxPreviewRpm { get; init; } = 9250;
    }

    [ConfigGrouping("Redline Flash", "Adjust the behavior of the Flash after Redline RPM")]
    public RedlineFlashGrouping RedlineFlash { get; init; } = new();
    public sealed class RedlineFlashGrouping
    {
        [ToolTip("Actives the bar's flashing capability when the Engine Rpm is beyond redline.")]
        public bool Enabled { get; init; } = true;

        [ToolTip("The amount of milliseconds the bar uses the redline color.")]
        [IntRange(16, 700, 4)]
        public int MillisecondsRedline { get; init; } = 52;

        [ToolTip("The amount of milliseconds the bar uses the flash color.")]
        [IntRange(16, 800, 4)]
        public int MillisecondsFlash { get; init; } = 48;
    }

    [HideForGame(Game.AmericanTruckSimulator | Game.EuroTruckSimulator2)]
    [ConfigGrouping("Pit Limiter", "Adjust the behavior of the pit limiter")]
    public PitLimiterGrouping Pitlimiter { get; init; } = new();
    public sealed class PitLimiterGrouping
    {
        [ToolTip("Actives the pit limiter drawing")]
        public bool Enabled { get; init; } = true;
    }

    [ConfigGrouping("Colors", "Adjust the colors used in the shift bar")]
    public ColorsGrouping Colors { get; init; } = new();
    public sealed class ColorsGrouping
    {
        public Color NormalColor { get; init; } = Color.FromArgb(255, 255, 255, 255);

        public Color EarlyColor { get; init; } = Color.FromArgb(255, 255, 255, 0);

        public Color RedlineColor { get; init; } = Color.FromArgb(255, 255, 4, 4);

        public Color FlashColor { get; init; } = Color.FromArgb(255, 0, 131, 255);
    }
}
