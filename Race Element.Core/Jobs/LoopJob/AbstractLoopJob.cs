using System;
using System.Diagnostics;
using System.Threading;

namespace RaceElement.Core.Jobs.LoopJob;

public abstract class AbstractLoopJob : IJob
{
    private bool _isCancelling = false;
    private bool _isStopped = true;

    public bool IsRunning => !_isStopped;
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
        if (_isStopped) return;
        _isCancelling = true;
        WaitEvent.Set();
    }

    public void CancelJoin()
    {
        Cancel();
        this.WaitForCompletion(50);
    }

    public void Run()
    {
        _isStopped = false;
        _isCancelling = false;

        new Thread(() =>
        {
            Stopwatch sw = Stopwatch.StartNew();

            while (!_isCancelling)
            {
                RunAction();

                long waitTime = IntervalMillis - sw.ElapsedMilliseconds;
                if (waitTime > 0) WaitEvent.WaitOne((int)waitTime);

                sw.Restart();
            }

            AfterCancel();
            _isStopped = true;
        })
        { IsBackground = true }.Start();
    }
}
