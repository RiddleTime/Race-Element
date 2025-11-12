using RaceElement.Data.Common;
using RaceElement.Util.SystemExtensions;
using static RaceElement.Util.SystemExtensions.FloatExtensions;
using static RaceElement.HUD.Common.Overlays.Driving.DualSense.DS5W;
using static RaceElement.HUD.Common.Overlays.Driving.DualSense.Util;
using static RaceElement.HUD.Common.Overlays.Driving.DualSense.DualSenseConfiguration;

namespace RaceElement.HUD.Common.Overlays.Driving.DualSense;

internal class TriggerHaptics
{
    private float mLeftAccum;
    private int mGear;
    private float[] mDamage;

    public TriggerHaptics()
    {
        mGear = 0;
        mLeftAccum = 0.0f;
        mDamage = new float[5];
    }
    private float UpdateDamage(float[] damage)
    {
        float newDamage = 0.0f;
        for (int i = 0; i < 5; ++i)
        {
            if (damage[i] > mDamage[i])
            {
                newDamage += damage[i] - mDamage[i];
            }
            mDamage[i] = damage[i];
        }
        return newDamage;
    }

    public void HandleRumble(RumbleParams config)
    {
        // Combine values for left (strong) motor rumble
        float damage = UpdateDamage(SimDataProvider.LocalCar.Physics.Damage) * config.DamageCoef;
        float gear = (SimDataProvider.LocalCar.Inputs.Gear != mGear) ? config.GearCoef : 0.0f;
        mGear = SimDataProvider.LocalCar.Inputs.Gear;
        mLeftAccum = (mLeftAccum * config.AccumDecay) + damage + gear;
        float kerb = Math.Max(0.0f, SimDataProvider.LocalCar.Physics.KerbVibration) * config.KerbCoef * 255.0f;
        float abs = Math.Abs(SimDataProvider.LocalCar.Physics.AbsVibrations) * config.ABSCoef * 255.0f;
        int lRumble = Math.Clamp((int)Math.Round(kerb + abs + mLeftAccum), 0, 255);
        // Combine values for right (weak) motor rumble
        float rpm = (float)SimDataProvider.LocalCar.Engine.Rpm / (float)SimDataProvider.LocalCar.Engine.MaxRpm;
        int rRumble = rpm > config.RPMStart ? (int)Math.Round(config.RPMCoef * 255.0f): 0;
        DebugOut("lRumble: " + lRumble + ", rRumble: " + rRumble);
        ds5w_set_rumble(1, lRumble);
        ds5w_set_rumble(0, rRumble);
    }
    public void HandleAcceleration(ThrottleSlipHaptics config)
    {
        float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;
        if (SimDataProvider.LocalCar.Inputs.Throttle <= config.ThrottleThreshold / 100f ||
            slipRatios.Length != 4)
        {
            ds5w_set_trigger_effect_off(0);
            return;
        }
        //DebugOut("Slip Ratios: " + slipRatios[0] + ", " + slipRatios[1] + ", " + slipRatios[2] + ", " + slipRatios[3]);
        float slipRatioFront = 0.5f * (slipRatios[0] + slipRatios[1]);
        float slipRatioBack = 0.5f * (slipRatios[2] + slipRatios[3]);
        float slipRatio = Lerp(config.SlipRatioBias, slipRatioFront, slipRatioBack);
        if (slipRatio < config.MinSlipRatio)
        {
            ds5w_set_trigger_effect_off(0);
            return;
        }
        // Calculate frequency from slipRatio
        float srWeight = SmoothStep(slipRatio, config.MinSlipRatio, config.MaxSlipRatio);
        float freqf = Lerp(srWeight, config.MinSlipFrequency, config.MaxSlipFrequency);
        int freq = Math.Clamp((int)Math.Round(freqf), 0, 255);
        // Calculate amplitude from slipRatio
        int amp = 1 + (int)Math.Round(Math.Sqrt(slipRatio - config.MinSlipRatio) * config.AmpGain);
        amp.ClipMax(config.MaxAmp);
        //DebugOut("slipRatio: " + slipRatio + ", freq: " + freq + ", amp: " + amp);
        ds5w_set_trigger_effect_vibration(0, 0, amp, freq);
    }
    public void HandleBraking(BrakeSlipHaptics config)
    {
        float[] slipRatios = SimDataProvider.LocalCar.Tyres.SlipRatio;
        if (SimDataProvider.LocalCar.Inputs.Brake <= config.BrakeThreshold / 100f ||
            slipRatios.Length != 4)
        {
            ds5w_set_trigger_effect_off(1);
            return;
        }
        //DebugOut("Slip Ratios: " + slipRatios[0] + ", " + slipRatios[1] + ", " + slipRatios[2] + ", " + slipRatios[3]);
        float slipRatioFront = 0.5f * (slipRatios[0] + slipRatios[1]);
        float slipRatioBack = 0.5f * (slipRatios[2] + slipRatios[3]);
        float slipRatio = Lerp(config.SlipRatioBias, slipRatioFront, slipRatioBack);
        if (slipRatio < config.MinSlipRatio)
        {
            ds5w_set_trigger_effect_off(1);
            return;
        }
        // Calculate frequency from slipRatio
        float srWeight = SmoothStep(slipRatio, config.MinSlipRatio, config.MaxSlipRatio);
        float freqf = Lerp(srWeight, config.MinSlipFrequency, config.MaxSlipFrequency);
        int freq = Math.Clamp((int)Math.Round(freqf), 0, 255);
        // Calculate amplitude from slip
        int amp = 1 + (int)Math.Round(Math.Sqrt(slipRatio - config.MinSlipRatio) * config.AmpGain);
        amp.ClipMax(config.MaxAmp);
        //DebugOut("slipRatio: " + slipRatio + ", freq: " + freq + ", amp: " + amp);
        ds5w_set_trigger_effect_vibration(1, 0, amp, freq);
    }
}
