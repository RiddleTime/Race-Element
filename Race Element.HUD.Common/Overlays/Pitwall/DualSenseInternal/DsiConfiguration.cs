using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DualSenseInternal;

internal sealed class DsiConfiguration : OverlayConfiguration
{
    public DsiConfiguration()
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
        [IntRange(5, 8, 1)]
        public int FeedbackStrength { get; init; } = 7;

        [ToolTip("Sets the min frequency of the vibration effect in the trigger.")]
        [IntRange(1, 10, 1)]
        public int MinFrequency { get; init; } = 3;

        [ToolTip("Sets the max frequency of the vibration effect in the trigger.")]
        [IntRange(20, 150, 1)]
        public int MaxFrequency { get; init; } = 85;

        [ToolTip("Change the amplitude(strength) of the vibration effect in the trigger.")]
        [IntRange(5, 8, 1)]
        public int Amplitude { get; init; } = 8;
    }

    [ConfigGrouping("Throttle Slip", "Adjust the slip effect whilst applying the throttle.\nModify the threshold to increase or decrease sensitivity in different situations.")]
    public ThrottleSlipHaptics ThrottleSlip { get; init; } = new();
    public sealed class ThrottleSlipHaptics
    {
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
        [IntRange(5, 8, 1)]
        public int FeedbackStrength { get; init; } = 8;

        [ToolTip("Sets the min frequency of the vibration effect in the trigger.")]
        [IntRange(1, 10, 1)]
        public int MinFrequency { get; init; } = 6;

        [ToolTip("Sets the max frequency of the vibration effect in the trigger.")]
        [IntRange(20, 150, 1)]
        public int MaxFrequency { get; init; } = 96;

        [ToolTip("Change the amplitude(strength) of the vibration effect in the trigger.")]
        [IntRange(5, 8, 1)]
        public int Amplitude { get; init; } = 7;
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
