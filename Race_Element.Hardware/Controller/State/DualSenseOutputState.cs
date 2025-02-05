using System;
using DualSenseAPI.Util;

namespace DualSenseAPI.State
{
    /// <summary>
    /// All available output variables for a DualSense controller.
    /// </summary>
    public sealed class DualSenseOutputState
    {
        /// <summary>
        /// Left motor rumble, as a percentage (0 to 1). Defaults to 0.
        /// </summary>
        public float LeftRumble { get; set; } = 0;

        /// <summary>
        /// Right motor rumble, as a percentage (0 to 1). Defaults to 0.
        /// </summary>
        public float RightRumble { get; set; } = 0;

        /// <summary>
        /// The mic LED behavior. Defaults to <see cref="MicLed.Off"/>.
        /// </summary>
        public MicLed MicLed { get; set; } = MicLed.Off;

        /// <summary>
        /// The enabled player LEDs. Defaults to <see cref="PlayerLed.None"/>.
        /// </summary>
        public PlayerLed PlayerLed { get; set; } = PlayerLed.None;

        /// <summary>
        /// The player LED brightness. Defaults to <see cref="PlayerLedBrightness.High"/>.
        /// </summary>
        public PlayerLedBrightness PlayerLedBrightness { get; set; } = PlayerLedBrightness.High;

        /// <summary>
        /// The lightbar behavior. Defaults to <see cref="LightbarBehavior.PulseBlue"/>.
        /// </summary>
        public LightbarBehavior LightbarBehavior { get; set; } = LightbarBehavior.PulseBlue;

        /// <summary>
        /// The lightbar color. Defaults to blue. Requires <see cref="LightbarBehavior"/> to be set to <see cref="LightbarBehavior.CustomColor"/>.
        /// </summary>
        /// <seealso cref="DualSenseAPI.LightbarBehavior"/>
        public LightbarColor LightbarColor { get; set; } = new LightbarColor(0, 0, 1);

        /// <summary>
        /// R2's adaptive trigger effect. Defaults to <see cref="TriggerEffect.Default"/>.
        /// </summary>
        /// <seealso cref="TriggerEffect"/>
        public TriggerEffect R2Effect { get; set; } = TriggerEffect.Default;

        /// <summary>
        /// L2's adaptive trigger effect. Defaults to <see cref="TriggerEffect.Default"/>.
        /// </summary>
        /// <seealso cref="TriggerEffect"/>
        public TriggerEffect L2Effect { get; set; } = TriggerEffect.Default;

        // default no-arg constructor
        public DualSenseOutputState() { }

        /// <summary>
        /// Gets the bytes needed to describe an adaptive trigger effect.
        /// </summary>
        /// <param name="props">The trigger effect properties.</param>
        /// <returns>A 10 byte array describing the trigger effect, padded with extra 0s as needed.</returns>
        private static byte[] BuildTriggerReport(TriggerEffect props)
        {
            byte[] bytes = new byte[10];
            bytes[0] = (byte)props.InternalEffect;
            switch (props.InternalEffect)
            {
                case TriggerEffectType.ContinuousResistance:
                    bytes[1] = props.InternalStartPosition.UnsignedToByte();
                    bytes[2] = props.InternalStartForce.UnsignedToByte();
                    break;
                case TriggerEffectType.SectionResistance:
                    bytes[1] = props.InternalStartPosition.UnsignedToByte();
                    bytes[2] = props.InternalEndPosition.UnsignedToByte();
                    break;
                case TriggerEffectType.Vibrate:
                    bytes[1] = 0xFF;
                    if (props.InternalKeepEffect)
                    {
                        bytes[2] = 0x02;
                    }
                    bytes[4] = props.InternalStartForce.UnsignedToByte();
                    bytes[5] = props.InternalMiddleForce.UnsignedToByte();
                    bytes[6] = props.InternalEndForce.UnsignedToByte();
                    bytes[9] = props.InternalVibrationFrequency;
                    break;
                default:
                    // leave other bytes as 0. this handles Default/No-resist and calibration modes.
                    break;
            }
            return bytes;
        }

        /// <summary>
        /// Gets the bytes needed for an output report, independent of connection type.
        /// </summary>
        /// <returns>A 47 byte array for the output report to follow the necessary header byte(s).</returns>
        internal byte[] BuildHidOutputBuffer()
        {
            byte[] baseBuf = new byte[47];

            // Feature mask
            baseBuf[0x00] = 0xFF;
            baseBuf[0x01] = 0xF7;

            // L/R rumble
            baseBuf[0x02] = RightRumble.UnsignedToByte();
            baseBuf[0x03] = LeftRumble.UnsignedToByte();

            // mic led
            baseBuf[0x08] = (byte)MicLed;

            // 0x01 to allow customization, 0x02 to enable uninterruptable blue pulse
            baseBuf[0x26] = 0x03;
            // 0x01 to do a slow-fade to blue (uninterruptable) if 0x26 & 0x01 is set.
            // 0x02 to allow a slow-fade-out and set to configured color
            baseBuf[0x29] = (byte)LightbarBehavior;
            baseBuf[0x2A] = (byte)PlayerLedBrightness;
            baseBuf[0x2B] = (byte)(0x20 | (byte)PlayerLed);

            //lightbar
            baseBuf[0x2C] = LightbarColor.R.UnsignedToByte();
            baseBuf[0x2D] = LightbarColor.G.UnsignedToByte();
            baseBuf[0x2E] = LightbarColor.B.UnsignedToByte();

            //adaptive triggers
            byte[] r2Bytes = BuildTriggerReport(R2Effect);
            Array.Copy(r2Bytes, 0, baseBuf, 0x0A, 10);
            byte[] l2Bytes = BuildTriggerReport(L2Effect);
            Array.Copy(l2Bytes, 0, baseBuf, 0x15, 10);

            return baseBuf;
        }
    }
}
