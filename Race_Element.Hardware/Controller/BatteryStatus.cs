namespace DualSenseAPI
{
    /// <summary>
    /// The status of a DualSense battery.
    /// </summary>
    public struct BatteryStatus
    {
        /// <summary>
        /// Whether the battery is currently charging.
        /// </summary>
        public bool IsCharging;

        /// <summary>
        /// Whether the battery is done charging.
        /// </summary>
        public bool IsFullyCharged;

        /// <summary>
        /// The level of the battery, from 1 to 10.
        /// </summary>
        /// <remarks>
        /// Typically, <see cref="IsFullyCharged"/> is set sometime when this is between 8 and 10. 
        /// Exactly when the flag is set varies and is likely due to the battery's overcharge protection.
        /// </remarks>
        public float Level;
    }
}
