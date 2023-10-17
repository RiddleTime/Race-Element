using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayDualSenseX
{
    internal sealed class DualSenseXConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Acceleration", "Adjust the haptics for the left and right trigger.")]
        public ThrottleHapticsConfig ThrottleHaptics { get; set; } = new ThrottleHapticsConfig();
        public sealed class ThrottleHapticsConfig
        {
            [ToolTip("Adds haptics when traction control is activated.")]
            public bool TcEffect { get; set; } = true;
        }


        [ConfigGrouping("Braking", "Adjust the haptics for the left and right trigger.")]
        public BrakeHapticsConfig BrakeHaptics { get; set; } = new BrakeHapticsConfig();
        public sealed class BrakeHapticsConfig
        {
            [ToolTip("Dynamically adjust load on trigger based on brake load.")]
            public bool ActiveLoad { get; set; } = true;

            [ToolTip("Adds haptics when abs is activated.")]
            public bool AbsEffect { get; set; } = true;
        }
    }
}
