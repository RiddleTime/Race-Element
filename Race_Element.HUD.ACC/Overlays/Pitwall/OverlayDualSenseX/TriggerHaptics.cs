using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using static RaceElement.ACCSharedMemory;
using static RaceElement.Data.SetupConverter;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayDualSenseX.DualSenseXConfiguration;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayDualSenseX.DualSenseXResources;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayDualSenseX;

internal static class TriggerHaptics
{
    public static Packet HandleBrakePressure(SPageFilePhysics pagePhysics)
    {
        return null;
    }

    public static Packet HandleBraking(SPageFilePhysics pagePhysics, BrakeHapticsConfig brakeConfig)
    {
        Packet p = new();
        List<Instruction> instructions = new();
        int controllerIndex = 0;

        if (pagePhysics.Abs > 0 && brakeConfig.AbsEffect)
        {
            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                /// Start: 0-9 Strength:0-8 Frequency:0-255
                //parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, 0, 6, 45 } // vibrate is not enough
                parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, brakeConfig.AbsFrequency/*85*/, 1, 0, 0, 0, 0, 0]
            });

        }
        else
        {
            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal]
            });

            if (brakeConfig.ActiveLoad)
            {
                if (pagePhysics.WheelAngularSpeed[(int)Wheel.FrontLeft] == 0 && pagePhysics.WheelAngularSpeed[(int)Wheel.FrontRight] == 0)
                {
                    // locking up the front tyres
                    instructions.Add(new Instruction()
                    {
                        type = InstructionType.TriggerUpdate,
                        parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal]
                    });
                }
                else
                {
                    float totalBrakePressure = 0;
                    Array.ForEach(pagePhysics.brakePressure, tyre => totalBrakePressure += tyre);
                    totalBrakePressure.Clip(0, 2);

                    instructions.Add(new Instruction()
                    {
                        type = InstructionType.TriggerUpdate,
                        parameters = [controllerIndex, Trigger.Left, TriggerMode.AutomaticGun, 0, 2 * totalBrakePressure, 45]
                    });
                }
            }
        }

        if (instructions.Count == 0) return null;
        p.instructions = instructions.ToArray();
        return p;
    }

    public static Packet HandleAcceleration(SPageFilePhysics pagePhysics, ThrottleHapticsConfig throttleConfig)
    {
        Packet p = new();
        List<Instruction> instructions = new();
        int controllerIndex = 0;

        float rearLeftSlip = pagePhysics.SlipRatio[(int)Wheel.RearLeft];
        float rearRightSlip = pagePhysics.SlipRatio[(int)Wheel.RearRight];
        float averageRearTyreSlip = rearLeftSlip + rearRightSlip / 2;

        if (averageRearTyreSlip > 1)
        {
            averageRearTyreSlip.ClipMax(8);
            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                //parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, 200, 1, 0, 0, 0, 0, 0 }
                /// Start: 0-9 Strength:0-8 Frequency:0-255
                parameters = [controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, 0, averageRearTyreSlip / 4, 100 + averageRearTyreSlip * 4]
            });
        }
        else
        {
            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal]
            });
        }

        if (throttleConfig.TcEffect && pagePhysics.TC > 0)
        {

            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, throttleConfig.TcFrequency /*130*/, 10, 0, 0, 0, 0, 0]
                /// Start: 0-9 Strength:0-8 Frequency:0-255
                //parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, 0, 6, 65 }
            });
        }
        else
        {

            instructions.Add(new Instruction()
            {
                type = InstructionType.TriggerUpdate,
                parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal]
            });
        }

        if (instructions.Count == 0) return null;
        p.instructions = instructions.ToArray();
        return p;
    }
}
