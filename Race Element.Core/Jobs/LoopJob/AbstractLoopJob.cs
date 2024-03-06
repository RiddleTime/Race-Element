using System;
using System.Diagnostics;
using System.Threading;

namespace RaceElement.Core.Jobs.LoopJob;

public abstract class AbstractLoopJob : IJob
{
    private bool _isRunning = false;
    public bool IsRunning => _isRunning;
    private ManualResetEvent WaitEvent = new ManualResetEvent(false);

    private int _intervalMillis = 1;
    public int IntervalMillis
    {
        get { return _intervalMillis; }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            _intervalMillis = value;
        }
    }

    public abstract void RunAction();

    public virtual void AfterCancel() { }

    public void Cancel()
     {
        if (_isRunning)
        {
            _isRunning = false;
            WaitEvent.Set();
        }
    }

    public void CancelJoin()
    {
        Cancel();
        this.WaitForCompletion(50);
    }

    public void Run()
    {
        _isRunning = true;
        new Thread(WorkerThread) { IsBackground = true }.Start();
    }

    private void WorkerThread()
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (_isRunning)
        {
            RunAction();

            long waitTime = IntervalMillis - sw.ElapsedMilliseconds;
            if (waitTime > 0) WaitEvent.WaitOne((int)waitTime);

            sw.Restart();
        }
        AfterCancel();
    }
}
