namespace DualSenseAPI
{
    /// <summary>
    /// Trigger effect types
    /// </summary>
    internal enum TriggerEffectType : byte
    {
        ContinuousResistance = 0x01,
        SectionResistance = 0x02,

        /// <summary>
        /// Official Feedback
        /// </summary>
        Feedback = 0x21,
        /// <summary>
        /// Official Vibration
        /// </summary>
        Vibrate = 0x26,
        Calibrate = 0xFC,
        Default = 0x00
    }

    /// <summary>
    /// Superclass for all trigger effects.
    /// </summary>
    public class TriggerEffect
    {
        internal TriggerEffectType InternalEffect { get; private set; } = TriggerEffectType.Default;
        // Used for all trigger effects that apply resistance
        internal float InternalStartPosition { get; private set; } = 0;
        // Used for section resistance
        internal float InternalEndPosition { get; private set; } = 0;
        // Below properties are for EffectEx only
        internal float InternalStartForce { get; private set; } = 0;
        internal float InternalMiddleForce { get; private set; } = 0;
        internal float InternalEndForce { get; private set; } = 0;
        internal bool InternalKeepEffect { get; private set; } = false;
        internal byte InternalVibrationFrequency { get; private set; } = 0;
        private TriggerEffect() { }

        /// <summary>
        /// Default trigger effect. No resistance.
        /// </summary>
        public static readonly TriggerEffect Default = new SimpleEffect(TriggerEffectType.Default);

        /// <summary>
        /// Calibration sequence.
        /// </summary>
        public static readonly TriggerEffect Calibrate = new SimpleEffect(TriggerEffectType.Calibrate);

        /// <summary>
        /// Simple trigger effect that only sets the mode byte.
        /// </summary>
        private sealed class SimpleEffect : TriggerEffect
        {
            public SimpleEffect(TriggerEffectType effect)
            {
                InternalEffect = effect;
            }
        }

        /// <summary>
        /// Continuous resistance effect.
        /// </summary>
        public sealed class Continuous : TriggerEffect
        {
            /// <summary>
            /// Start position of the resistance, as a percentage (from 0 to 1).
            /// </summary>
            public float StartPosition
            {
                get { return InternalStartPosition; }
            }

            /// <summary>
            /// The resistance force, as a percentage (from 0 to 1).
            /// </summary>
            public float Force
            {
                get { return InternalStartForce; }
            }

            /// <summary>
            /// Creates a continuous resistance effect
            /// </summary>
            /// <param name="startPosition">Start position of the resistance, as a percentage (from 0 to 1).</param>
            /// <param name="forcePercentage">The resistance force, as a percentage (from 0 to 1).</param>
            public Continuous(float startPosition, float forcePercentage)
            {
                InternalEffect = TriggerEffectType.ContinuousResistance;
                InternalStartPosition = startPosition;
                InternalStartForce = forcePercentage;
            }
        }

        /// <summary>
        /// Effect that applies resistance on a section of the trigger.
        /// </summary>
        public sealed class Section : TriggerEffect
        {
            /// <summary>
            /// The start position of the resistance, as a percentage (from 0 to 1).
            /// </summary>
            public float StartPosition
            {
                get { return InternalStartPosition; }
            }

            /// <summary>
            /// The end position of the resistance, as a percentage (from 0 to 1).
            /// </summary>
            public float EndPosition
            {
                get { return InternalEndPosition; }
            }

            /// <summary>
            /// Creates a section resistance effect.
            /// </summary>
            /// <param name="startPosition">The start position of the resistance, as a percentage (from 0 to 1).</param>
            /// <param name="endPosition">The end position of the resistance, as a percentage (from 0 to 1).</param>
            public Section(float startPosition, float endPosition)
            {
                InternalEffect = TriggerEffectType.SectionResistance;
                InternalStartPosition = startPosition;
                InternalEndPosition = endPosition;
            }
        }

        /// <summary>
        /// Vibration effect.
        /// </summary>
        public sealed class Vibrate : TriggerEffect
        {
            /// <summary>
            /// The force at the start of the press, as a percentage (from 0 to 1).
            /// </summary>
            /// <remarks>
            /// The start of the trigger press is roughly when the trigger value is between 0 and 0.5.
            /// However, the user-perceived end position may not be exactly 0.5 as the trigger will be vibrating.
            /// </remarks>
            public float StartForce { get { return InternalStartForce; } }

            /// <summary>
            /// The force at the middle of the press, as a percentage (from 0 to 1).
            /// </summary>
            /// <remarks>
            /// The start of the trigger press is roughly when the trigger value is between 0.5 and 1.
            /// However, the user-perceived start position may not be exactly 0.5 as the trigger will be vibrating.
            /// </remarks>
            public float MiddleForce { get { return InternalMiddleForce; } }

            /// <summary>
            /// The force at the end of the press, as a percentage (from 0 to 1). Requires <see cref="KeepEffect"/> to be set.
            /// </summary>
            /// <remarks>
            /// There is a slight gap between when the trigger value hits 1 and when this force starts. This can lead to a small
            /// region where there is no effect playing; be mindful of this when creating your effects.
            /// </remarks>
            public float EndForce { get { return InternalEndForce; } }

            /// <summary>
            /// Whether to enable to effect after the trigger is fully pressed.
            /// </summary>
            public bool KeepEffect { get { return InternalKeepEffect; } }

            /// <summary>
            /// The vibration frequency in hertz.
            /// </summary>
            public byte VibrationFrequency { get { return InternalVibrationFrequency; } }

            /// <summary>
            /// Creates a vibration trigger effect.
            /// </summary>
            /// <param name="vibrationFreqHz">The vibration frequency in hertz.</param>
            /// <param name="startForce">The force at the start of the press, as a percentage (from 0 to 1).</param>
            /// <param name="middleForce">The force at the middle of the press, as a percentage (from 0 to 1).</param>
            /// <param name="endForce">The force at the end of the press, as a percentage (from 0 to 1). 
            /// Requires <paramref name="keepEffect"/> to be set.</param>
            /// <param name="keepEffect">Whether to enable the effect after the trigger is fully pressed.</param>
            public Vibrate(byte vibrationFreqHz, float startForce, float middleForce, float endForce, bool keepEffect = true)
            {
                InternalEffect = TriggerEffectType.Vibrate;
                InternalStartForce = startForce;
                InternalMiddleForce = middleForce;
                InternalEndForce = endForce;
                InternalKeepEffect = keepEffect;
                InternalVibrationFrequency = vibrationFreqHz;
            }
        }
    }
}
