using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.ACCSharedMemory;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayDualSenseX.DualSenseXResources;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayDualSenseX
{
    internal static class TriggerHaptics
    {
        public static Packet HandleBrakePressure(SPageFilePhysics pagePhysics)
        {
            return null;
        }

        private static bool wasAbsOn = false;
        public static Packet HandleABS(SPageFilePhysics pagePhysics)
        {
            Packet p = new Packet();
            List<Instruction> instructions = new List<Instruction>();
            int controllerIndex = 0;

            if (pagePhysics.Abs > 0)
            {
                if (!wasAbsOn)
                {
                    instructions.Add(new Instruction()
                    {
                        type = InstructionType.TriggerUpdate,
                        parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, 65, 10, 0, 0, 0, 0, 0 }
                    });

                    wasAbsOn = true;
                }
            }
            else
            {
                if (wasAbsOn)
                {
                    instructions.Add(new Instruction()
                    {
                        type = InstructionType.TriggerUpdate,
                        parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Resistance, 0, 1 }
                    });
                    wasAbsOn = false;
                }
            }

            if (instructions.Count == 0) return null;
            p.instructions = instructions.ToArray();
            return p;
        }

        private static bool wasTcOn = false;
        public static Packet HandleTractionControl(SPageFilePhysics pagePhysics)
        {
            Packet p = new Packet();
            List<Instruction> instructions = new List<Instruction>();
            int controllerIndex = 0;

            if (pagePhysics.TC > 0)
            {
                if (!wasTcOn)
                {
                    instructions.Add(new Instruction()
                    {
                        type = InstructionType.TriggerUpdate,
                        //parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistanceB, 200, 1, 0, 0, 0, 0, 0 }
                        /// Start: 0-9 Strength:0-8 Frequency:0-255
                        parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.AutomaticGun, 0, 6, 65 }
                    });

                    wasTcOn = true;
                }
            }
            else
            {
                if (wasTcOn)
                {
                    instructions.Add(new Instruction()
                    {
                        type = InstructionType.TriggerUpdate,
                        parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.Normal }
                    });
                    wasTcOn = false;
                }
            }

            if (instructions.Count == 0) return null;
            p.instructions = instructions.ToArray();
            return p;
        }
    }
}
