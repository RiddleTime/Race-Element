using DualSenseAPI;
using RaceElement.Data.Common;
using RaceElement.Util.SystemExtensions;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DualSenseInternal;

internal static class TriggerHaptics
{
    public static void HandleBraking(DsiConfiguration config, DualSense ds)
    {
        int controllerIndex = 0;

        // TODO: add either an option to threshold it on brake input or based on some curve?
        if (SimDataProvider.LocalCar.Inputs.Brake > config.BrakeSlip.BrakeThreshold / 100f)
        {
            float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;

            if (slipRatios.Length == 0)
            {
                ds.OutputState.L2Effect = TriggerEffect.Default;
                ds.ReadWriteOnce();
                return;
            }

            if (slipRatios.Length == 4)
            {
                float slipRatioFront = Math.Max(slipRatios[0], slipRatios[1]);
                float slipRatioRear = Math.Max(slipRatios[2], slipRatios[3]);

                // TODO: add option for front and rear ratio threshold.
                if (slipRatioFront > config.BrakeSlip.FrontSlipThreshold || slipRatioRear > config.BrakeSlip.RearSlipThreshold)
                {
                    float frontslipCoefecient = slipRatioFront * 4f;
                    frontslipCoefecient.ClipMax(10);

                    float rearSlipCoefecient = slipRatioFront * 2f;
                    rearSlipCoefecient.ClipMax(7.5f);


                    float magicValue = frontslipCoefecient + rearSlipCoefecient;
                    float percentage = magicValue * 1.0f / 17.5f;

                    if (percentage >= 0.05f)
                    {
                        //ds.OutputState.L2Effect = new TriggerEffect.Continuous(0, percentage);
                        //ds.ReadWriteOnce();
                        //Thread.Sleep((int)(1000 / 200f * 4));
                    }

                    int freq = (int)(config.BrakeSlip.MaxFrequency * percentage);
                    freq.ClipMin(config.BrakeSlip.MinFrequency);
                    ds.OutputState.L2Effect = new TriggerEffect.Vibrate((byte)freq, config.BrakeSlip.Amplitude, config.BrakeSlip.Amplitude, config.BrakeSlip.Amplitude, true);
                    ds.ReadWriteOnce();
                }
            }
            else
            {
                ds.OutputState.L2Effect = TriggerEffect.Default;
                ds.ReadWriteOnce();
            }
        }
        else
        {
            ds.OutputState.L2Effect = TriggerEffect.Default;
            ds.ReadWriteOnce();
        }
    }

    public static void HandleAcceleration(DsiConfiguration config, DualSense ds)
    {
        int controllerIndex = 0;

        if (SimDataProvider.LocalCar.Inputs.Throttle > config.ThrottleSlip.ThrottleThreshold / 100f)
        {
            float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;
            if (slipRatios.Length == 0)
            {
                ds.OutputState.R2Effect = TriggerEffect.Default;
                ds.ReadWriteOnce();
                return;
            }

            if (slipRatios.Length == 4)
            {
                float slipRatioFront = Math.Max(slipRatios[0], slipRatios[1]);
                float slipRatioRear = Math.Max(slipRatios[2], slipRatios[3]);

                if (slipRatioFront > config.ThrottleSlip.FrontSlipThreshold || slipRatioRear > config.ThrottleSlip.RearSlipThreshold)
                {
                    float frontslipCoefecient = slipRatioFront * 3f;
                    frontslipCoefecient.ClipMax(5);
                    float rearSlipCoefecient = slipRatioFront * 5f;
                    rearSlipCoefecient.ClipMax(7.5f);

                    float magicValue = frontslipCoefecient + rearSlipCoefecient;
                    float percentage = magicValue * 1.0f / 12.5f;

                    if (percentage >= 0.05f)
                    {
                        //ds.OutputState.R2Effect = new TriggerEffect.Continuous(0, percentage);
                        //ds.ReadWriteOnce();
                        Thread.Sleep((int)(1000 / 200f * 4));
                    }

                    int freq = (int)(config.ThrottleSlip.MaxFrequency * percentage);
                    freq.ClipMin(config.ThrottleSlip.MinFrequency);
                    ds.OutputState.R2Effect = new TriggerEffect.Vibrate((byte)freq, config.ThrottleSlip.Amplitude, config.ThrottleSlip.Amplitude, config.ThrottleSlip.Amplitude, true);
                    ds.ReadWriteOnce();
                }
                else
                {
                    ds.OutputState.R2Effect = TriggerEffect.Default;
                    ds.ReadWriteOnce();
                }
            }
        }
        else
        {
            ds.OutputState.R2Effect = TriggerEffect.Default;
            ds.ReadWriteOnce();
        }
    }
}
