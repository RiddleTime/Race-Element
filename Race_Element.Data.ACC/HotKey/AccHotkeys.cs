using System;
using System.Threading;
using System.Windows.Forms;

namespace RaceElement.Data.ACC.HotKey;

public class AccHotkeys
{
    /// <summary>
    /// Sends the M hotkey to the active application.
    /// Returns the UtcNow time w
    /// </summary>
    /// <returns></returns>
    public static DateTime SaveReplay()
    {
        Thread.Sleep(1000);
        SendKeys.SendWait("m");
        return DateTime.UtcNow;
    }
}
