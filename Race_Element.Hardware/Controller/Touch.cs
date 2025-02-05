namespace DualSenseAPI
{
    /// <summary>
    /// One of the DualSense's 2 touch points. The touchpad is 1920x1080, 0-indexed.
    /// </summary>
    public struct Touch
    {
        /// <summary>
        /// The X position of the touchpoint. 0 is the leftmost edge. If the touch point is currently pressed,
        /// this is the current position. If the touch point is released, it was the last position before it
        /// was released.
        /// </summary>
        public uint X;

        /// <summary>
        /// The Y position of the touchpoint. 0 is the topmost edge. If the touch point is currently pressed,
        /// this is the current position. If the touch point is released, it was the last position before it
        /// was released.
        /// </summary>
        public uint Y;

        /// <summary>
        /// Whether the touch point is currently pressed.
        /// </summary>
        public bool IsDown;

        /// <summary>
        /// The touch id. This is a counter that changes whenever a touch is pressed or released.
        /// </summary>
        public byte Id;
    }
}
