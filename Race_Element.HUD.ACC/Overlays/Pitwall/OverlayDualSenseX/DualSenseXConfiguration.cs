using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayDualSenseX
{
    internal sealed class DualSenseXConfiguration : OverlayConfiguration
    {
        public DualSenseXConfiguration()
        {
            this.GenericConfiguration.AlwaysOnTop = false;
            this.GenericConfiguration.Window = false;
            this.GenericConfiguration.Opacity = 1.0f;
            this.AllowRescale = false;
        }

        [ConfigGrouping("DSX UDP", "Adjust the port DSX uses, 6969 is default.")]
        public UdpConfig UDP { get; set; } = new UdpConfig();
        public sealed class UdpConfig
        {
            [ToolTip("Adjust the port used by DSX, 6969 is default.")]
            [IntRange(0, 65535, 1)]
            public int Port { get; set; } = 6969;
        }

        [ConfigGrouping("Acceleration", "Adjust the haptics for the left and right trigger.")]
        public ThrottleHapticsConfig ThrottleHaptics { get; set; } = new ThrottleHapticsConfig();
        public sealed class ThrottleHapticsConfig
        {
            [ToolTip("Adds haptics when traction control is activated.")]
            public bool TcEffect { get; set; } = true;

            [ToolTip("Sets the frequency of the traction control haptics.")]
            [IntRange(10, 150, 1)]
            public int TcFrequency { get; set; } = 130;
        }


        [ConfigGrouping("Braking", "Adjust the haptics for the left and right trigger.")]
        public BrakeHapticsConfig BrakeHaptics { get; set; } = new BrakeHapticsConfig();
        public sealed class BrakeHapticsConfig
        {
            [ToolTip("Dynamically adjust load on trigger based on the brake pressure.")]
            public bool ActiveLoad { get; set; } = true;

            [ToolTip("Adds haptics when abs is activated.")]
            public bool AbsEffect { get; set; } = true;

            [ToolTip("Sets the frequency of the abs haptics.")]
            [IntRange(10, 150, 1)]
            public int AbsFrequency { get; set; } = 85;
        }
    }
}
