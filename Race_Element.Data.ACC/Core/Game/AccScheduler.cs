using RaceElement.Data.ACC.Tracker;

namespace RaceElement.Data.ACC.Core.Game;

public static class AccScheduler
{
    public static void RegisterJobs()
    {
        PageStaticTracker.Instance.Run();
    }

    public static void UnregisterJobs()
    {
        PageStaticTracker.Instance.CancelJoin();
        ACCTrackerDispose.Dispose();
    }
}
