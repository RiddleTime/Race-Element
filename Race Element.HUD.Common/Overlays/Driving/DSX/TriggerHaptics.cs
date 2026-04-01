using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.Util.SystemExtensions;
using static RaceElement.HUD.Common.Overlays.Driving.DSX.Resources;

namespace RaceElement.HUD.Common.Overlays.Driving.DSX;

internal static class TriggerHaptics
{
    public static DsxPacket HandleBraking(DsxConfiguration config) => GameManager.CurrentGame switch
    {
        Game.RichardBurnsRally => HandleBrakingRBR(config),
        _ => HandleBrakingGeneric(config)
    };

    public static DsxPacket HandleAcceleration(DsxConfiguration config) => GameManager.CurrentGame switch
    {
        Game.RichardBurnsRally => HandleAccelerationRBR(config),
        _ => HandleAccelerationGeneric(config)
    };

    private static DsxPacket HandleBrakingGeneric(DsxConfiguration config)
    {
        DsxPacket p = new();
        int controllerIndex = 0;

        // TODO: add either an option to threshold it on brake input or based on some curve?
        if (SimDataProvider.LocalCar.Inputs.Brake > config.BrakeSlip.BrakeThreshold / 100f)
        {
            float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;

            if (slipRatios.Length == 4)
            {
                float slipRatioFront = Math.Max(slipRatios[0], slipRatios[1]);
                float slipRatioRear = Math.Max(slipRatios[2], slipRatios[3]);

                // TODO: add option for front and rear ratio threshold.
                if (slipRatioFront > config.BrakeSlip.FrontSlipThreshold || slipRatioRear > config.BrakeSlip.RearSlipThreshold)
                {
                    float frontslipCoefecient = slipRatioFront * 4f;
                    frontslipCoefecient.ClipMax(10);

                    float rearSlipCoefecient = slipRatioRear * 2f;
                    rearSlipCoefecient.ClipMax(7.5f);


                    float magicValue = frontslipCoefecient + rearSlipCoefecient;
                    float percentage = magicValue * 1.0f / 17.5f;
                    if (percentage >= 0.05f)
                        p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, [1, (int)(config.BrakeSlip.FeedbackStrength * percentage)]);

                    int freq = (int)(config.BrakeSlip.MaxFrequency * percentage);
                    freq.ClipMin(config.BrakeSlip.MinFrequency);
                    p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Left, TriggerMode.VIBRATION, [0, config.BrakeSlip.Amplitude, freq]);
                }
            }
        }

        if (p.Instructions == null) p.AddAdaptiveTriggerToPacket(0, Trigger.Left, TriggerMode.Normal, []);

        return p;
    }

    private static DsxPacket HandleAccelerationGeneric(DsxConfiguration config)
    {
        DsxPacket p = new();
        int controllerIndex = 0;

        if (SimDataProvider.LocalCar.Inputs.Throttle > config.ThrottleSlip.ThrottleThreshold / 100f)
        {
            float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;
            if (slipRatios.Length == 4)
            {
                float slipRatioFront = Math.Max(slipRatios[0], slipRatios[1]);
                float slipRatioRear = Math.Max(slipRatios[2], slipRatios[3]);

                if (slipRatioFront > config.ThrottleSlip.FrontSlipThreshold || slipRatioRear > config.ThrottleSlip.RearSlipThreshold)
                {
                    float frontslipCoefecient = slipRatioFront * 3f;
                    frontslipCoefecient.ClipMax(5);
                    float rearSlipCoefecient = slipRatioRear * 5f;
                    rearSlipCoefecient.ClipMax(7.5f);

                    float magicValue = frontslipCoefecient + rearSlipCoefecient;
                    float percentage = magicValue * 1.0f / 12.5f;

                    if (percentage >= 0.05f)
                        p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Right, TriggerMode.FEEDBACK, [1, (int)(config.ThrottleSlip.FeedbackStrength * percentage)]);

                    int freq = (int)(config.ThrottleSlip.MaxFrequency * percentage);
                    freq.ClipMin(config.ThrottleSlip.MinFrequency);
                    p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Right, TriggerMode.VIBRATION, [0, config.ThrottleSlip.Amplitude, freq]);
                }
            }
        }

        if (p.Instructions == null) p.AddAdaptiveTriggerToPacket(0, Trigger.Right, TriggerMode.Normal, []);

        return p;
    }

    /// <summary>
    /// RBR brake trigger haptics using real wheel speeds and percentage-based slip ratios.
    /// SlipRatio in RBR represents actual percentage: -10.5 = 10.5% wheel lock (brake), +10.5 = 10.5% wheel spin (throttle)
    /// Thresholds should be configured in percentage (e.g., 5.0 = 5% slip)
    /// </summary>
    private static DsxPacket HandleBrakingRBR(DsxConfiguration config)
    {
        DsxPacket p = new();
        int controllerIndex = 0;

        float brakePercentage = SimDataProvider.LocalCar.Inputs.Brake * 100f; // Convert to percentage
        float groundSpeedKmh = SimDataProvider.LocalCar.Physics.Velocity;

        // Use GUI configured brake threshold
        if (brakePercentage > config.BrakeSlip.BrakeThreshold && groundSpeedKmh > 5f)
        {
            float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;

            if (slipRatios.Length == 4)
            {
                float flSlip = slipRatios[0];
                float frSlip = slipRatios[1];

                // For RBR: threshold is in percentage (5.0 = 5% slip)
                float wheelSlipThreshold = config.BrakeSlip.FrontSlipThreshold;

                // Check if front wheels are locked (negative slip = brake lock)
                if (flSlip < -wheelSlipThreshold || frSlip < -wheelSlipThreshold)
                {
                    // Calculate max lock severity
                    float maxLock = Math.Min(flSlip, frSlip); // More negative = more lock

                    // Strength: base 2, increases with lock severity, scaled by FeedbackStrength (1-8)
                    // FeedbackStrength acts as 0-1 multiplier: 7/8 = 0.875
                    float baseStrength = Math.Min(8f, 2f + Math.Abs(maxLock) / 10f);
                    float strength = baseStrength * (config.BrakeSlip.FeedbackStrength / 8.0f);
                    int strengthInt = Math.Max(1, Math.Min(8, (int)Math.Round(strength)));

                    // Use VibrateTriggerPulse mode for direct lock feedback
                    p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Left, TriggerMode.VibrateTriggerPulse, [strengthInt, 0, 0]);
                }
                else
                {
                    // Fallback: use progressive feedback for subtle slip (not full lock yet)
                    float slipRatioFront = Math.Max(Math.Abs(slipRatios[0]), Math.Abs(slipRatios[1]));
                    float slipRatioRear = Math.Max(Math.Abs(slipRatios[2]), Math.Abs(slipRatios[3]));

                    if (slipRatioFront > config.BrakeSlip.FrontSlipThreshold || slipRatioRear > config.BrakeSlip.RearSlipThreshold)
                    {
                        float frontslipCoefecient = slipRatioFront * 4f;
                        frontslipCoefecient.ClipMax(10);

                        float rearSlipCoefecient = slipRatioRear * 2f;
                        rearSlipCoefecient.ClipMax(7.5f);

                        float magicValue = frontslipCoefecient + rearSlipCoefecient;
                        float percentage = magicValue * 1.0f / 17.5f;
                        if (percentage >= 0.05f)
                            p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, [1, (int)(config.BrakeSlip.FeedbackStrength * percentage)]);

                        int freq = (int)(config.BrakeSlip.MaxFrequency * percentage);
                        freq.ClipMin(config.BrakeSlip.MinFrequency);
                        p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Left, TriggerMode.VIBRATION, [0, config.BrakeSlip.Amplitude, freq]);
                    }
                }
            }
        }

        if (p.Instructions == null) p.AddAdaptiveTriggerToPacket(0, Trigger.Left, TriggerMode.Normal, []);

        return p;
    }

    /// <summary>
    /// RBR throttle trigger haptics for wheel spin and handbrake drift scenarios.
    /// Detects: 1) Wheel spin on any driven wheels, 2) Handbrake drift with throttle
    /// </summary>
    private static DsxPacket HandleAccelerationRBR(DsxConfiguration config)
    {
        DsxPacket p = new();
        int controllerIndex = 0;

        float throttle = SimDataProvider.LocalCar.Inputs.Throttle * 100f;
        float groundSpeedKmh = SimDataProvider.LocalCar.Physics.Velocity;

        // Scenario 1: Wheel spin detection
        if (throttle > config.ThrottleSlip.ThrottleThreshold && groundSpeedKmh > 5f)
        {
            float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;
            if (slipRatios.Length == 4)
            {
                // Use separate thresholds for front/rear (supports FWD/RWD/AWD)
                float frontSlipThreshold = config.ThrottleSlip.FrontSlipThreshold;
                float rearSlipThreshold = config.ThrottleSlip.RearSlipThreshold;

                // Find maximum wheel spin across all wheels (positive slip = spin)
                float maxSpin = Math.Max(
                    Math.Max(slipRatios[0] > frontSlipThreshold ? slipRatios[0] : 0, slipRatios[1] > frontSlipThreshold ? slipRatios[1] : 0),
                    Math.Max(slipRatios[2] > rearSlipThreshold ? slipRatios[2] : 0, slipRatios[3] > rearSlipThreshold ? slipRatios[3] : 0)
                );

                if (maxSpin > Math.Min(frontSlipThreshold, rearSlipThreshold))
                {
                    // Wheel spin feedback
                    float baseStrength = Math.Min(8f, 2f + maxSpin / 10f);
                    float strength = baseStrength * (config.ThrottleSlip.FeedbackStrength / 8.0f);
                    int strengthInt = Math.Max(1, Math.Min(8, (int)Math.Round(strength)));

                    p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Right, TriggerMode.VibrateTriggerPulse, [strengthInt, 0, 0]);
                }
                else
                {
                    // Fallback: progressive feedback for subtle slip
                    float slipRatioFront = Math.Max(slipRatios[0], slipRatios[1]);
                    float slipRatioRear = Math.Max(slipRatios[2], slipRatios[3]);

                    if (slipRatioFront > frontSlipThreshold || slipRatioRear > rearSlipThreshold)
                    {
                        float frontslipCoefecient = slipRatioFront * 3f;
                        frontslipCoefecient.ClipMax(5);
                        float rearSlipCoefecient = slipRatioRear * 5f;
                        rearSlipCoefecient.ClipMax(7.5f);

                        float magicValue = frontslipCoefecient + rearSlipCoefecient;
                        float percentage = magicValue * 1.0f / 12.5f;

                        if (percentage >= 0.05f)
                            p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Right, TriggerMode.FEEDBACK, [1, (int)(config.ThrottleSlip.FeedbackStrength * percentage)]);

                        int freq = (int)(config.ThrottleSlip.MaxFrequency * percentage);
                        freq.ClipMin(config.ThrottleSlip.MinFrequency);
                        p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Right, TriggerMode.VIBRATION, [0, config.ThrottleSlip.Amplitude, freq]);
                    }
                }
            }
        }
        // Scenario 2: Handbrake drift with throttle (rally technique)
        else if (groundSpeedKmh > 5f)
        {
            float handbrake = SimDataProvider.LocalCar.Inputs.HandBrake * 100f;
            float brake = SimDataProvider.LocalCar.Inputs.Brake * 100f;

            float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;
            if (slipRatios.Length == 4)
            {
                float rlSlip = slipRatios[2];
                float rrSlip = slipRatios[3];
                float wheelSlipThreshold = config.ThrottleSlip.RearSlipThreshold;

                // Rear wheel lock with handbrake/brake + throttle (Scandinavian flick, etc.)
                if ((rlSlip < -wheelSlipThreshold || rrSlip < -wheelSlipThreshold) &&
                    (handbrake > 30f || brake > 80f) && throttle > 30f)
                {
                    float maxLock = Math.Min(rlSlip, rrSlip);
                    float baseStrength = Math.Min(8f, 2f + Math.Abs(maxLock) / 10f);
                    float strength = baseStrength * (config.ThrottleSlip.FeedbackStrength / 8.0f);
                    int strengthInt = Math.Max(1, Math.Min(8, (int)Math.Round(strength)));

                    p.AddAdaptiveTriggerToPacket(controllerIndex, Trigger.Right, TriggerMode.VibrateTriggerPulse, [strengthInt, 0, 0]);
                }
            }
        }

        if (p.Instructions == null) p.AddAdaptiveTriggerToPacket(0, Trigger.Right, TriggerMode.Normal, []);

        return p;
    }
}
