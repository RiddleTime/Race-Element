using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.DSX;

internal sealed class DsxConfiguration : OverlayConfiguration
{
    public DsxConfiguration()
    {
        GenericConfiguration.AlwaysOnTop = false;
        GenericConfiguration.Window = false;
        GenericConfiguration.Opacity = 1.0f;
        GenericConfiguration.AllowRescale = false;
    }


    [ConfigGrouping("Brake Slip", "Adjust the slip effect whilst applying the brakes.")]
    public BrakeSlipHaptics BrakeSlip { get; init; } = new();
    public sealed class BrakeSlipHaptics
    {
        [ToolTip("When set to Off, disables the brake haptics.")]
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// The brake in percentage (divide by 100f if you want 0-1 value)
        /// </summary>
        [ToolTip("The minimum brake percentage before any effects are applied. See this like a deadzone.")]
        [FloatRange(0.1f, 99f, 0.1f, 1)]
        public float BrakeThreshold { get; init; } = 3f;

        [FloatRange(0.05f, 6f, 0.002f, 3)]
        public float FrontSlipThreshold { get; init; } = 0.25f;

        [FloatRange(0.05f, 6f, 0.002f, 3)]
        public float RearSlipThreshold { get; init; } = 0.25f;

        [ToolTip("Higher is stronger dynamic feedback.")]
        [IntRange(1, 8, 1)]
        public int FeedbackStrength { get; init; } = 7;

        [ToolTip("Change the amplitude(strength) of the vibration effect in the trigger.")]
        [IntRange(1, 8, 1)]
        public int Amplitude { get; init; } = 8;

        [ToolTip("Sets the min frequency of the vibration effect in the trigger.")]
        [IntRange(1, 50, 1)]
        public int MinFrequency { get; init; } = 3;

        [ToolTip("Sets the max frequency of the vibration effect in the trigger.")]
        [IntRange(20, 150, 1)]
        public int MaxFrequency { get; init; } = 85;
    }

    [ConfigGrouping("Throttle Slip", "Adjust the slip effect whilst applying the throttle.\nModify the threshold to increase or decrease sensitivity in different situations.")]
    public ThrottleSlipHaptics ThrottleSlip { get; init; } = new();
    public sealed class ThrottleSlipHaptics
    {

        [ToolTip("When set to Off, disables the throttle haptics.")]
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// The throttle in percentage (divide by 100f if you want 0-1 value)
        /// </summary>
        [ToolTip("The minimum throttle percentage before any effects are applied. See this like a deadzone.")]
        [FloatRange(0.1f, 99f, 0.1f, 1)]
        public float ThrottleThreshold { get; init; } = 3f;

        [ToolTip("Decrease this treshold to increase the sensitivity when the front wheels slip (understeer).")]
        [FloatRange(0.05f, 6f, 0.002f, 3)]
        public float FrontSlipThreshold { get; init; } = 0.35f;

        [ToolTip("Decrease this treshold to increase the sensitivity when the rear wheels slip (oversteer).")]
        [FloatRange(0.05f, 10f, 0.002f, 3)]
        public float RearSlipThreshold { get; init; } = 0.25f;

        [ToolTip("Higher is stronger dynamic feedback.")]
        [IntRange(1, 8, 1)]
        public int FeedbackStrength { get; init; } = 8;

        [ToolTip("Change the amplitude(strength) of the vibration effect in the trigger.")]
        [IntRange(1, 8, 1)]
        public int Amplitude { get; init; } = 7;

        [ToolTip("Sets the min frequency of the vibration effect in the trigger.")]
        [IntRange(1, 50, 1)]
        public int MinFrequency { get; init; } = 6;

        [ToolTip("Sets the max frequency of the vibration effect in the trigger.")]
        [IntRange(20, 150, 1)]
        public int MaxFrequency { get; init; } = 96;
    }

    [ConfigGrouping("Light Bar", "Customize the DualSense's light bar behavior while connected to DSX.")]
    public LightBarConfig LightBar { get; init; } = new();
    public sealed class LightBarConfig
    {
        [ToolTip("Sets the light bar to the Color below while connected to DSX.\nWhen set to Off, the light bar is left to your DSX settings.")]
        public bool OverrideColor { get; init; } = true;

        [ToolTip("The color the light bar shows while connected to DSX.")]
        public Color Color { get; init; } = Color.FromArgb(255, 255, 69, 0);

        [ToolTip("Blinks the light bar according to the options below when connected to DSX.\nIf Override Color is set to Off, the light bar then returns to your DSX settings.")]
        public bool BlinkOnConnect { get; init; }

        [ToolTip("The color the light bar blinks with when connected to DSX.")]
        public Color BlinkColor { get; init; } = Color.FromArgb(255, 255, 69, 0);

        [ToolTip("How many times the light bar blinks when connected to DSX.")]
        [IntRange(1, 10, 1)]
        public int BlinkCount { get; init; } = 3;

        [ToolTip("How long the light bar stays on and off for each blink, in milliseconds.")]
        [IntRange(50, 250, 10)]
        public int BlinkInterval { get; init; } = 150;
    }

    [ConfigGrouping("DSX UDP", "Adjust the port DSX uses, 6969 is default.")]
    public UdpConfig UDP { get; init; } = new UdpConfig();
    public sealed class UdpConfig
    {
        [ToolTip("Adjust the port used by DSX, 6969 is default.")]
        [IntRange(0, 65535, 1)]
        public int Port { get; init; } = 6969;
    }
}
