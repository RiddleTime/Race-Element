using RaceElement.Data.Common;
using RaceElement.Util.SystemExtensions;
using static RaceElement.HUD.Common.Overlays.Driving.DSX.Resources;

namespace RaceElement.HUD.Common.Overlays.Driving.DSX;

internal static class TriggerHaptics
{
    public static DsxPacket HandleBraking(DsxConfiguration config)
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

    public static DsxPacket HandleAcceleration(DsxConfiguration config)
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
}
