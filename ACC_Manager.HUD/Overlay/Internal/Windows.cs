using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.HUD.Overlay.Internal.WindowStructs;

namespace ACCManager.HUD.Overlay.Internal
{
    public class Windows
    {
        #region Members

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        private static IList<WindowAndMonitorHandle> _windowAndMonitorHandles;

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves a list of all main window handles and their associated process id's.
        /// </summary>
        /// <returns></returns>
        public static WindowAndMonitorHandle[] GetWindowAndMonitorHandles()
        {
            if (_windowAndMonitorHandles == null)
                _windowAndMonitorHandles = new List<WindowAndMonitorHandle>();

            lock (_windowAndMonitorHandles)
            {
                // Enumerate windows
                EnumWindows(EnumTheWindows, IntPtr.Zero);

                // Return list
                return _windowAndMonitorHandles.ToArray();
            }
        }

        /// <summary>
        /// Enumerates through each window.
        /// </summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="lParam">The l parameter.</param>
        /// <returns></returns>
        private static bool EnumTheWindows(IntPtr windowHandle, IntPtr lParam)
        {
            // Get window area
            var rect = new RECT();
            GetWindowRect(windowHandle, ref rect);

            // Get current monitor
            var monitorHandle = MonitorFromRect(ref rect, MONITOR_DEFAULTTONEAREST);

            // Add to enumerated windows
            lock (_windowAndMonitorHandles)
            {
                var wamh = new WindowAndMonitorHandle(windowHandle, monitorHandle);

                if (!_windowAndMonitorHandles.Contains(wamh))
                    _windowAndMonitorHandles.Add(wamh);
            }
            return true;
        }

        #endregion

        #region Native Methods

        /// <summary>
        /// EnumWindows Processor (delegate)
        /// </summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="lParam">The lparameter.</param>
        /// <returns></returns>
        public delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr lParam);

        /// <summary>
        /// Enums the windows.
        /// </summary>
        /// <param name="enumWindowsProcessorDelegate">The enum windows processor delegate.</param>
        /// <param name="lParam">The lparameter.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumWindowsProcessorDelegate, IntPtr lParam);

        /// <summary>
        /// Gets the rectangle representing the frame of a window.
        /// </summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="rectangle">The rectangle.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr windowHandle, ref RECT rectangle);

        /// <summary>
        /// Monitors from rect.
        /// </summary>
        /// <param name="rectPointer">The RECT pointer.</param>
        /// <param name="flags">The flags.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromRect([In] ref RECT rectPointer, uint flags);

        #endregion

        /// <summary>
        /// A simple class to group our handles.
        /// </summary>
        /// <returns></returns>
        public class WindowAndMonitorHandle
        {
            public IntPtr WindowHandle { get; }
            public IntPtr MonitorHandle { get; }

            public WindowAndMonitorHandle(IntPtr windowHandle, IntPtr monitorHandle)
            {
                WindowHandle = windowHandle;
                MonitorHandle = monitorHandle;
            }

            public override bool Equals(object obj)
            {
                if (obj is WindowAndMonitorHandle)
                {
                    var wamh = (WindowAndMonitorHandle)obj;
                    return this.MonitorHandle == wamh.MonitorHandle && this.WindowHandle == wamh.WindowHandle;
                }

                return false;
            }
        }
    }
}
