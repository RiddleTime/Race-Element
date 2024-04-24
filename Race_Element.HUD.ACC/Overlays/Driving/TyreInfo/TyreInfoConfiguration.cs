using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.TyreInfo;
internal sealed class TyreInfoConfiguration : OverlayConfiguration
{
    public TyreInfoConfiguration() => GenericConfiguration.AllowRescale = true;

    [ConfigGrouping("Info", "Show additional information about the condition of the tyres.")]
    public InfoGrouping Information { get; init; } = new InfoGrouping();
    public sealed class InfoGrouping
    {
        [ToolTip("Displays the percentage of brake pad life above the brake pads.")]
        public bool PadLife { get; init; } = true;

        [ToolTip("Displays the average of front and rear brake temperatures under the brake pads.")]
        public bool BrakeTemps { get; init; } = true;

        [ToolTip("Draws pressures and colored indicators on top vanilla tyre widget.")]
        public bool Pressures { get; init; } = true;

        [ToolTip("Displays the loss of pressure for each tyre.")]
        public bool LossOfPressure { get; init; } = true;

        [ToolTip("Defines the amount of decimals for the tyre pressure text.")]
        [IntRange(1, 2, 1)]
        public int Decimals { get; init; } = 2;

        [ToolTip("Refresh rate in Hz of the HUD.")]
        [IntRange(1, 8, 1)]
        public int RefreshRate { get; init; } = 8;
    }
}
