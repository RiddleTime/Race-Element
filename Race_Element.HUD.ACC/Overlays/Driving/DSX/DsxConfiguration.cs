using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.DSX;

internal sealed class DsxConfiguration : OverlayConfiguration
{
    public DsxConfiguration()
    {
        GenericConfiguration.AlwaysOnTop = false;
        GenericConfiguration.Window = false;
        GenericConfiguration.Opacity = 1.0f;
        GenericConfiguration.AllowRescale = false;
    }

    [ConfigGrouping("DSX UDP", "Adjust the port DSX uses, 6969 is default.")]
    public UdpConfig UDP { get; set; } = new UdpConfig();
    public sealed class UdpConfig
    {
        [ToolTip("Adjust the port used by DSX, 6969 is default.")]
        [IntRange(0, 65535, 1)]
        public int Port { get; set; } = 6969;
    }

    [ConfigGrouping("Light Bar", "Customize the DualSense's light bar behavior while connected to DSX.")]
    public LightBarConfig LightBar { get; set; } = new LightBarConfig();
    public sealed class LightBarConfig
    {
        [ToolTip("Sets the light bar to the Color below while connected to DSX.\nWhen set to Off, the light bar is left to your DSX settings.")]
        public bool OverrideColor { get; set; } = true;

        [ToolTip("The color the light bar shows while connected to DSX.")]
        public Color Color { get; set; } = Color.FromArgb(255, 255, 69, 0);

        [ToolTip("Blinks the light bar according to the options below when connected to DSX.\nIf Override Color is set to Off, the light bar then returns to your DSX settings.")]
        public bool BlinkOnConnect { get; set; }

        [ToolTip("The color the light bar blinks with when connected to DSX.")]
        public Color BlinkColor { get; set; } = Color.FromArgb(255, 255, 69, 0);

        [ToolTip("How many times the light bar blinks when connected to DSX.")]
        [IntRange(1, 10, 1)]
        public int BlinkCount { get; set; } = 3;

        [ToolTip("How long the light bar stays on and off for each blink, in milliseconds.")]
        [IntRange(50, 250, 10)]
        public int BlinkInterval { get; set; } = 150;
    }

    [ConfigGrouping("Acceleration", "Adjust the haptics for the left and right trigger.")]
    public ThrottleHapticsConfig ThrottleHaptics { get; set; } = new ThrottleHapticsConfig();
    public sealed class ThrottleHapticsConfig
    {
        [ToolTip("Adds haptics when traction control is activated.")]
        public bool TcEffect { get; set; } = true;

        [ToolTip("Sets the frequency of the traction control haptics.")]
        [IntRange(10, 150, 1)]
        public int TcFrequency { get; set; } = 90;
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
