using System.Collections.Generic;
using System.Threading.Tasks;

using Device.Net;
using Hid.Net.Windows;

namespace DualSenseAPI.Util
{
    /// <summary>
    /// Utilities to scann for DualSense controllers on HID.
    /// </summary>
    internal sealed class HidScanner
    {
        private readonly IDeviceFactory hidFactory;

        private static HidScanner? _instance = null;
        /// <summary>
        /// Singleton HidScanner instance.
        /// </summary>
        internal static HidScanner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HidScanner();
                }
                return _instance;
            }
        }

        private HidScanner()
        {
            hidFactory = new FilterDeviceDefinition(1356, 3302, label: "DualSense").CreateWindowsHidDeviceFactory();
        }

        /// <summary>
        /// Lists connected devices.
        /// </summary>
        /// <returns>An enumerable of connected devices.</returns>
        public IEnumerable<ConnectedDeviceDefinition> ListDevices()
        {
            Task<IEnumerable<ConnectedDeviceDefinition>> scannerTask = hidFactory.GetConnectedDeviceDefinitionsAsync();
            scannerTask.Wait();
            return scannerTask.Result;
        }

        /// <summary>
        /// Gets a device from its information.
        /// </summary>
        /// <param name="deviceDefinition">The information for the connected device.</param>
        /// <returns>The actual device.</returns>
        public IDevice GetConnectedDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            Task<IDevice> connectTask = hidFactory.GetDeviceAsync(deviceDefinition);
            connectTask.Wait();
            return connectTask.Result;
        }
    }
}
