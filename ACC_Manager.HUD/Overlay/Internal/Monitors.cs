using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static ACCManager.HUD.Overlay.Internal.WindowStructs;

namespace ACCManager.HUD.Overlay.Internal
{
    // from https://xcalibursystems.com/accessing-monitor-information-with-c-part-2-getting-a-monitor-associated-with-a-window-handle/
    public class Monitors
    {
        private static List<MonitorInfoWithHandle> _monitorInfos;

        /// <summary>
        /// Gets the monitors.
        /// </summary>
        /// <returns></returns>
        public static MonitorInfoWithHandle[] GetMonitors()
        {
            if (_monitorInfos == null)
                // New List
                _monitorInfos = new List<MonitorInfoWithHandle>();
            else if (_monitorInfos.Count > 0)
                return _monitorInfos.ToArray();


            // Enumerate monitors
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnum, IntPtr.Zero);

#if DEBUG
            Debug.WriteLine("\nLogging Monitors");
            foreach (MonitorInfoWithHandle monitor in _monitorInfos)
            {
                Debug.WriteLine($"Handle: {monitor.MonitorHandle}");
                Debug.WriteLine($"Monitor: {monitor.MonitorInfo.monitor}");
                Debug.WriteLine($"Work: {monitor.MonitorInfo.monitor}");
                Debug.WriteLine($"Size: {monitor.MonitorInfo.size}");
            }
#endif

            // Return list
            return _monitorInfos.ToArray();
        }

        /// <summary>
        /// Monitor Enum Delegate
        /// </summary>
        /// <param name="hMonitor">A handle to the display monitor.</param>
        /// <param name="hdcMonitor">A handle to a device context.</param>
        /// <param name="lprcMonitor">A pointer to a RECT structure.</param>
        /// <param name="dwData">Application-defined data that EnumDisplayMonitors passes directly to the enumeration function.</param>
        /// <returns></returns>
        public static bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            var mi = new MONITORINFO();
            mi.size = (uint)Marshal.SizeOf(mi);
            GetMonitorInfo(hMonitor, ref mi);


            var miwh = new MonitorInfoWithHandle(hMonitor, mi);
            // Add to monitor info
            if (!_monitorInfos.Contains(miwh))
                _monitorInfos.Add(miwh);
            return true;
        }

        /// <summary>
        /// Monitor information with handle interface.
        /// </summary>
        public interface IMonitorInfoWithHandle
        {
            IntPtr MonitorHandle { get; }
            MONITORINFO MonitorInfo { get; }
        }

        /// <summary>
        /// Monitor information with handle.
        /// </summary>
        public class MonitorInfoWithHandle : IMonitorInfoWithHandle
        {
            /// <summary>
            /// Gets the monitor handle.
            /// </summary>
            /// <value>
            /// The monitor handle.
            /// </value>
            public IntPtr MonitorHandle { get; private set; }

            /// <summary>
            /// Gets the monitor information.
            /// </summary>
            /// <value>
            /// The monitor information.
            /// </value>
            public MONITORINFO MonitorInfo { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="MonitorInfoWithHandle"/> class.
            /// </summary>
            /// <param name="monitorHandle">The monitor handle.</param>
            /// <param name="monitorInfo">The monitor information.</param>
            public MonitorInfoWithHandle(IntPtr monitorHandle, MONITORINFO monitorInfo)
            {
                MonitorHandle = monitorHandle;
                MonitorInfo = monitorInfo;
            }

            public override bool Equals(object obj)
            {
                if (obj is MonitorInfoWithHandle)
                    return this.MonitorHandle == ((MonitorInfoWithHandle)obj).MonitorHandle;

                return false;
            }
        }

        /// <summary>
        /// Monitor Enum Delegate
        /// </summary>
        /// <param name="hMonitor">A handle to the display monitor.</param>
        /// <param name="hdcMonitor">A handle to a device context.</param>
        /// <param name="lprcMonitor">A pointer to a RECT structure.</param>
        /// <param name="dwData">Application-defined data that EnumDisplayMonitors passes directly to the enumeration function.</param>
        /// <returns></returns>
        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor,
            ref RECT lprcMonitor, IntPtr dwData);

        /// <summary>
        /// Enumerates through the display monitors.
        /// </summary>
        /// <param name="hdc">A handle to a display device context that defines the visible region of interest.</param>
        /// <param name="lprcClip">A pointer to a RECT structure that specifies a clipping rectangle.</param>
        /// <param name="lpfnEnum">A pointer to a MonitorEnumProc application-defined callback function.</param>
        /// <param name="dwData">Application-defined data that EnumDisplayMonitors passes directly to the MonitorEnumProc function.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
            MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        /// <summary>
        /// Gets the monitor information.
        /// </summary>
        /// <param name="hmon">A handle to the display monitor of interest.</param>
        /// <param name="mi">A pointer to a MONITORINFO instance created by this method.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hmon, ref MONITORINFO mi);

    }
}
