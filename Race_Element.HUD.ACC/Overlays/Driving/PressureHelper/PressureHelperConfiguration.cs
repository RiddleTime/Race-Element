using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.PressureHelper
{
    internal sealed class PressureHelperConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Tyre Pressures", "Adjust tyre pressure targets to be used in the hud.")]
        public TyrePressures Pressures { get; init; } = new();
        public sealed class TyrePressures
        {

            [ToolTip("Set the amount of laps required for the tyres to normalize the pressures.\n" +
                     "After this amount of laps the HUD will start suggesting possible adjustments.")]
            [IntRange(1, 4, 1)]
            public int MinimumLaps { get; init; } = 2;

            [ToolTip("TODO")]
            [FloatRange(26f, 27.3f, 0.1f, 1)]
            public float DryTyreTarget { get; init; } = 26.9f;

            [ToolTip("TODO")]
            [FloatRange(29f, 35f, 0.1f, 1)]
            public float WetTyreTarget { get; init; } = 30f;
        }
    }
}
