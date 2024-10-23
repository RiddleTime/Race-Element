using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Overlay.Internal;
internal static unsafe partial class Timers
{
    [LibraryImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
    internal static partial uint TimeBeginPeriod(uint uMilliseconds);

    [LibraryImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
    internal static partial uint TimeEndPeriod(uint uMilliseconds);
}
