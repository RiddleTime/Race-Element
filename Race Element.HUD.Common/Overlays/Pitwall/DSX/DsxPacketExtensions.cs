
using static RaceElement.HUD.Common.Overlays.Pitwall.DSX.Resources;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DSX;
internal static class DsxPacketExtensions
{

    /// <summary>
    /// Adds an adaptive trigger instruction to the packet for a specified controller index.
    /// This instruction configures the trigger mode and parameters for the adaptive trigger.
    /// </summary>
    /// <param name="packet">The packet to which the instruction will be added.</param>
    /// <param name="controllerIndex">The index of the controller to apply the trigger instruction.</param>
    /// <param name="trigger">The trigger (e.g., left or right trigger) to be configured.</param>
    /// <param name="triggerMode">The mode to set for the adaptive trigger.</param>
    /// <param name="parameters">Additional parameters required by the trigger mode.</param>
    /// <returns>Returns the packet with the adaptive trigger instruction added.</returns>
    internal static DsxPacket AddAdaptiveTriggerToPacket(this DsxPacket packet, int controllerIndex, Trigger trigger, TriggerMode triggerMode, List<int> parameters)
    {
        int instCount;

        if (packet.Instructions == null)
        {
            packet.Instructions = new Instruction[1];
            instCount = 0;
        }
        else
        {
            instCount = packet.Instructions.Length;
            Array.Resize(ref packet.Instructions, instCount + 1);
        }

        // Combine the fixed and variable parameters
        var combinedParameters = new object[3 + parameters.Count];
        combinedParameters[0] = controllerIndex;
        combinedParameters[1] = trigger;
        combinedParameters[2] = triggerMode;

        // Copy the List<int> parameters into the combinedParameters array
        for (int i = 0; i < parameters.Count; i++)
        {
            combinedParameters[3 + i] = parameters[i];
        }

        packet.Instructions[instCount] = new Instruction
        {
            Type = InstructionType.TriggerUpdate,
            Parameters = combinedParameters
        };

        return packet;
    }

    /// <summary>
    /// Adds a custom adaptive trigger instruction to the packet for a specified controller index.
    /// This allows for more complex trigger configurations, including a custom value mode and additional parameters.
    /// </summary>
    /// <param name="packet">The packet to which the instruction will be added.</param>
    /// <param name="controllerIndex">The index of the controller to apply the trigger instruction.</param>
    /// <param name="trigger">The trigger (e.g., left or right trigger) to be configured.</param>
    /// <param name="triggerMode">The mode to set for the adaptive trigger.</param>
    /// <param name="valueMode">The custom value mode for more detailed trigger control.</param>
    /// <param name="parameters">Additional parameters required by the custom trigger mode.</param>
    /// <returns>Returns the packet with the custom adaptive trigger instruction added.</returns>
    internal static DsxPacket AddCustomAdaptiveTriggerToPacket(this DsxPacket packet, int controllerIndex, Trigger trigger, TriggerMode triggerMode, CustomTriggerValueMode valueMode, List<int> parameters)
    {
        // This method is only for TriggerMode.CustomTriggerValue

        int instCount;

        if (packet.Instructions == null)
        {
            packet.Instructions = new Instruction[1];
            instCount = 0;
        }
        else
        {
            instCount = packet.Instructions.Length;
            Array.Resize(ref packet.Instructions, instCount + 1);
        }

        // Combine the fixed and variable parameters
        var combinedParameters = new object[4 + parameters.Count];
        combinedParameters[0] = controllerIndex;
        combinedParameters[1] = trigger;
        combinedParameters[2] = triggerMode;
        combinedParameters[3] = valueMode;

        // Copy the List<int> parameters into the combinedParameters array
        for (int i = 0; i < parameters.Count; i++)
        {
            combinedParameters[3 + i] = parameters[i];
        }

        packet.Instructions[instCount] = new Instruction
        {
            Type = InstructionType.TriggerUpdate,
            Parameters = combinedParameters
        };

        return packet;
    }

    /// <summary>
    /// Adds a trigger threshold instruction to the packet for a specified controller index.
    /// This instruction sets the threshold for a trigger's actuation point.
    /// </summary>
    /// <param name="packet">The packet to which the instruction will be added.</param>
    /// <param name="controllerIndex">The index of the controller to apply the threshold instruction.</param>
    /// <param name="trigger">The trigger (e.g., left or right trigger) to be configured.</param>
    /// <param name="threshold">The threshold value for the trigger.</param>
    /// <returns>Returns the packet with the trigger threshold instruction added.</returns>
    internal static DsxPacket AddTriggerThresholdToPacket(this DsxPacket packet, int controllerIndex, Trigger trigger, int threshold)
    {
        int instCount;

        if (packet.Instructions == null)
        {
            packet.Instructions = new Instruction[1];
            instCount = 0;
        }
        else
        {
            instCount = packet.Instructions.Length;
            Array.Resize(ref packet.Instructions, instCount + 1);
        }

        packet.Instructions[instCount] = new Instruction
        {
            Type = InstructionType.TriggerThreshold,
            Parameters = [controllerIndex, trigger, threshold]
        };

        return packet;
    }

    /// <summary>
    /// Adds an RGB update instruction to the packet for a specified controller index.
    /// This instruction sets the color and brightness of the RGB LEDs on the controller.
    /// </summary>
    /// <param name="packet">The packet to which the instruction will be added.</param>
    /// <param name="controllerIndex">The index of the controller to apply the RGB instruction.</param>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    /// <param name="brightness">The brightness level of the LEDs.</param>
    /// <returns>Returns the packet with the RGB update instruction added.</returns>
    internal static DsxPacket AddRGBToPacket(this DsxPacket packet, int controllerIndex, int red, int green, int blue, int brightness)
    {
        int instCount;

        if (packet.Instructions == null)
        {
            packet.Instructions = new Instruction[1];
            instCount = 0;
        }
        else
        {
            instCount = packet.Instructions.Length;
            Array.Resize(ref packet.Instructions, instCount + 1);
        }

        packet.Instructions[instCount] = new Instruction
        {
            Type = InstructionType.RGBUpdate,
            Parameters = [controllerIndex, red, green, blue, brightness]
        };

        return packet;
    }

    /// <summary>
    /// Adds a player LED update instruction to the packet for a specified controller index.
    /// This instruction configures the player indicator LEDs on the controller.
    /// </summary>
    /// <param name="packet">The packet to which the instruction will be added.</param>
    /// <param name="controllerIndex">The index of the controller to apply the player LED instruction.</param>
    /// <param name="playerLED">The player LED configuration to apply.</param>
    /// <returns>Returns the packet with the player LED update instruction added.</returns>
    internal static DsxPacket AddPlayerLEDToPacket(this DsxPacket packet, int controllerIndex, PlayerLEDNewRevision playerLED)
    {
        int instCount;

        if (packet.Instructions == null)
        {
            packet.Instructions = new Instruction[1];
            instCount = 0;
        }
        else
        {
            instCount = packet.Instructions.Length;
            Array.Resize(ref packet.Instructions, instCount + 1);
        }

        packet.Instructions[instCount] = new Instruction
        {
            Type = InstructionType.PlayerLEDNewRevision,
            Parameters = [controllerIndex, playerLED]
        };

        return packet;
    }

    /// <summary>
    /// Adds a microphone LED update instruction to the packet for a specified controller index.
    /// This instruction configures the microphone mute/unmute LED on the controller.
    /// </summary>
    /// <param name="packet">The packet to which the instruction will be added.</param>
    /// <param name="controllerIndex">The index of the controller to apply the microphone LED instruction.</param>
    /// <param name="micLED">The microphone LED configuration to apply.</param>
    /// <returns>Returns the packet with the microphone LED update instruction added.</returns>
    internal static DsxPacket AddMicLEDToPacket(this DsxPacket packet, int controllerIndex, MicLEDMode micLED)
    {
        int instCount;

        if (packet.Instructions == null)
        {
            packet.Instructions = new Instruction[1];
            instCount = 0;
        }
        else
        {
            instCount = packet.Instructions.Length;
            Array.Resize(ref packet.Instructions, instCount + 1);
        }

        packet.Instructions[instCount] = new Instruction
        {
            Type = InstructionType.MicLED,
            Parameters = [controllerIndex, micLED]
        };

        return packet;
    }

    /// <summary>
    /// Adds a reset instruction to the packet for a specified controller index.
    /// This instruction resets the controller's settings to the user's predefined settings from DSX.
    /// </summary>
    /// <param name="packet">The packet to which the instruction will be added.</param>
    /// <param name="controllerIndex">The index of the controller to reset.</param>
    /// <returns>Returns the packet with the reset instruction added.</returns>
    internal static DsxPacket AddResetToPacket(this DsxPacket packet, int controllerIndex)
    {
        int instCount;

        if (packet.Instructions == null)
        {
            packet.Instructions = new Instruction[1];
            instCount = 0;
        }
        else
        {
            instCount = packet.Instructions.Length;
            Array.Resize(ref packet.Instructions, instCount + 1);
        }

        packet.Instructions[instCount] = new Instruction
        {
            Type = InstructionType.ResetToUserSettings,
            Parameters = [controllerIndex]
        };

        return packet;
    }

    /// <summary>
    /// Adds a request to get the DSX status to the packet.
    /// This instruction requests the current status of the DSX server with the list of devices.
    /// </summary>
    /// <param name="packet">The packet to which the status request will be added.</param>
    /// <returns>Returns the packet with the status request instruction added.</returns>
    internal static DsxPacket AddGetDSXStatusToPacket(this DsxPacket packet)
    {
        int instCount;

        if (packet.Instructions == null)
        {
            packet.Instructions = new Instruction[1];
            instCount = 0;
        }
        else
        {
            instCount = packet.Instructions.Length;
            Array.Resize(ref packet.Instructions, instCount + 1);
        }

        packet.Instructions[instCount] = new Instruction
        {
            Type = InstructionType.GetDSXStatus,
            Parameters = []
        };

        return packet;
    }
}
