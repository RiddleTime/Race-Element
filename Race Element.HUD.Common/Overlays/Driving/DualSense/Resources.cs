using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.Common.Overlays.Driving.DualSense;

internal class Util
{
    public static void DebugOut(string msg)
    {
        StackTrace st = new StackTrace(false);
        string caller = st.GetFrame(1).GetMethod().Name;
        Debug.WriteLine(caller + ": " + msg);
    }
}

internal static partial class DS5W
{
    [LibraryImport("ds5w_x64.dll")]
    public static partial int ds5w_init();

    [LibraryImport("ds5w_x64.dll")]
    public static partial void ds5w_shutdown();

    [LibraryImport("ds5w_x64.dll")]
    public static partial void ds5w_batch_begin();

    [LibraryImport("ds5w_x64.dll")]
    public static partial int ds5w_batch_end();

    [LibraryImport("ds5w_x64.dll")]
    public static partial int ds5w_set_rumble(int left, int strength);

    [LibraryImport("ds5w_x64.dll")]
    public static partial int ds5w_set_trigger_effect_off(int left);

    [LibraryImport("ds5w_x64.dll")]
    public static partial int ds5w_set_trigger_effect_vibration(int left, int pos, int amp, int freq);

    [LibraryImport("ds5w_x64.dll")]
    public static partial int ds5w_set_trigger_effect_feedback(int left, int pos, int strength);

    [LibraryImport("ds5w_x64.dll")]
    public static partial int ds5w_set_trigger_effect_weapon(int left, int start, int end, int strength);
}
