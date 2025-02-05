namespace DualSenseAPI
{
    /// <summary>
    /// Available I/O connectivity modes for a DualSense controller.
    /// </summary>
    public enum IoMode
    {
        /// <summary>
        /// Connected via Bluetooth.
        /// </summary>
        Bluetooth,

        /// <summary>
        /// Connected via USB.
        /// </summary>
        USB,

        /// <summary>
        /// The connection type could not be identified.
        /// </summary>
        Unknown
    }
}
