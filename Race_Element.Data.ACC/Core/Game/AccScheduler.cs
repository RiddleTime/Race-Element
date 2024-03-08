using RaceElement.Data.ACC.Tracker;

namespace ACCManager.Data.ACC.Core.Game;

public static class AccScheduler
{
    public static void RegisterJobs()
    {
        PageStaticTracker.Instance.Run();
    }

    public static void UnregisterJobs()
    {
        ACCTrackerDispose.Dispose();
    }
}
