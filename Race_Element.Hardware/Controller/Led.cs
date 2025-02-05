namespace DualSenseAPI
{
    /// <summary>
    /// Flags for the player LEDs.
    /// </summary>
    /// <seealso cref="PlayerLedBrightness"/>
    public enum PlayerLed
    {
        /// <summary>
        /// LEDs off.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Leftmost LED on.
        /// </summary>
        Left = 0x01,

        /// <summary>
        /// Middle-left LED on.
        /// </summary>
        MiddleLeft = 0x02,

        /// <summary>
        /// Middle LED on.
        /// </summary>
        Middle = 0x04,

        /// <summary>
        /// Middle-right LED on.
        /// </summary>
        MiddleRight = 0x08,

        /// <summary>
        /// Rightmost LED on.
        /// </summary>
        Right = 0x10,

        /// <summary>
        /// Standard LEDs for player 1.
        /// </summary>
        Player1 = Middle,

        /// <summary>
        /// Standard LEDs for player 2.
        /// </summary>
        Player2 = MiddleLeft | MiddleRight,

        /// <summary>
        /// Standard LEDs for player 3.
        /// </summary>
        Player3 = Left | Middle | Right,

        /// <summary>
        /// Standard LEDs for player 4.
        /// </summary>
        Player4 = Left | MiddleLeft | MiddleRight | Right,

        /// <summary>
        /// All LEDs on.
        /// </summary>
        All = Left | MiddleLeft | Middle | MiddleRight | Right
    }

    /// <summary>
    /// The brightness of the player LEDs.
    /// </summary>
    /// <seealso cref="PlayerLed"/>
    public enum PlayerLedBrightness
    {
        /// <summary>
        /// Low LED brightness.
        /// </summary>
        Low = 0x02,

        /// <summary>
        /// Medium LED brightness.
        /// </summary>
        Medium = 0x01,

        /// <summary>
        /// High LED brightness.
        /// </summary>
        High = 0x00
    }

    /// <summary>
    /// Behavior options for the controller's lightbar.
    /// </summary>
    /// <seealso cref="LightbarColor"/>
    public enum LightbarBehavior
    {
        /// <summary>
        /// Default behavior. Pulses the lightbar blue and stays on.
        /// </summary>
        PulseBlue = 0x1,
        /// <summary>
        /// Allows the lightbar to be set a custom color.
        /// </summary>
        CustomColor = 0x2
    }

    /// <summary>
    /// Color of the controller's lightbar.
    /// </summary>
    /// <seealso cref="LightbarBehavior"/>
    public struct LightbarColor
    {
        /// <summary>
        /// The red component of the color as a percentage (0 to 1).
        /// </summary>
        public float R;

        /// <summary>
        /// The green component of the color as a percentage (0 to 1).
        /// </summary>
        public float G;

        /// <summary>
        /// The blue component of the color as a percentage (0 to 1).
        /// </summary>
        public float B;

        /// <summary>
        /// Creates a LightbarColor.
        /// </summary>
        /// <param name="r">The red component of the color as a percentage (0 to 1).</param>
        /// <param name="g">The green component of the color as a percentage (0 to 1).</param>
        /// <param name="b">The blue component of the color as a percentage (0 to 1).</param>
        public LightbarColor(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    /// <summary>
    /// Behavior options for the mic mute button LED.
    /// </summary>
    public enum MicLed
    {
        /// <summary>
        /// The LED is off.
        /// </summary>
        Off = 0,

        /// <summary>
        /// The LED is solid on.
        /// </summary>
        On = 1,

        /// <summary>
        /// The LED slowly pulses between dim and bright.
        /// </summary>
        Pulse = 2
    }
}
