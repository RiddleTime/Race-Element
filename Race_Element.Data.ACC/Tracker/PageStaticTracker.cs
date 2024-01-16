using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Core;
using System;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.Data.ACC.Tracker;

public class PageStaticTracker : AbstractLoopJob
{
    private static PageStaticTracker _instance;
    public static PageStaticTracker Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PageStaticTracker();

            return _instance;
        }
    }

    public PageStaticTracker()
    {
        IntervalMillis = 100;
    }

    public static event EventHandler<SPageFileStatic> Tracker;

    public override void RunAction()
    {
        if (AccProcess.IsRunning)
            Tracker?.Invoke(null, ACCSharedMemory.Instance.ReadStaticPageFile());
    }
}
