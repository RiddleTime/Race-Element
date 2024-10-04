using System;
using System.Diagnostics;
using System.Threading;

namespace RaceElement.Core.Jobs.Loop;

public abstract class AbstractLoopJob : IJob
{
    /// <summary>Used by the cancel method to wait until job finish.</summary>
    private readonly ManualResetEvent _workerExitEvent = new(false);

    /// <summary>Used by worker thread to sleep until next tick or wake up on cancel.</summary>
    private readonly ManualResetEvent _jobSleepEvent = new(false);

    /// <summary>Tick interval. At what pace "RunAction" is executed.</summary>
    private int _intervalMillis = 1;

    /// <summary>Test if the job is running. A job is running when it has been started and has not been canceled yet.</summary>
    /// <returns>true if the job is active, false otherwise</returns>
    public bool IsRunning { get; private set; } = false;

    /// <summary>Set at what interval <see cref="RunAction"/> is executed. If the execution
    /// time is less than the interval it will wait until the next interval,
    /// otherwise it will be executed immediately.
    /// </summary>
    public int IntervalMillis
    {
        get { return _intervalMillis; }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            _intervalMillis = value;
        }
    }

    /// <summary>
    /// Callback method executed when calling <see cref="Run"/>. Executed ahead of job execution.
    /// </summary>
    public virtual void BeforeRun() { }

    /// <summary>
    /// Callback method executed by the job at the certain time in future. This method is executed from another thread, you may require synchronization mechanism.
    /// </summary>
    public abstract void RunAction();

    /// <summary>
    ///    Callback method used by the job to notify it has been canceled. This method is executed from another thread, you may require synchronization mechanism.
    /// </summary>
    public virtual void AfterCancel() { }

    /// <summary>Callback used to notify the client that <see cref="RunAction"/> takes longer time than expected.</summary>
    /// <param name="millis">Number of milliseconds exceeded from <see cref="IntervalMillis"/>.</param>
    protected virtual void ExecutionIntervalOverflow(int millis) { }

    /// <summary>Cancel @ execution of the job if <see cref="IsRunning"/> without waiting for finish confirmation(no synchronization).</summary>
    public void Cancel()
    {
        if (IsRunning)
        {
            IsRunning = false;
            _jobSleepEvent.Set();
        }
    }

    /// <summary>Cancel execution of the job if <see cref="IsRunning"/> and wait for the finish confirmation(synchronization method).</summary>
    public void CancelJoin()
    {
        if (!IsRunning)
            return;

        Cancel();
        _workerExitEvent.WaitOne();
    }

    /// <summary>
    ///  Causes the job to begin executing. The result is a thread calling "RunAction()" method which is implemented by the child. 
    ///  If the job is executing and the method all calls to this method will be ignored.
    /// </summary>
    public void Run()
    {
        if (IsRunning)
            return;

        _jobSleepEvent.Reset();
        _workerExitEvent.Reset();

        BeforeRun();
        IsRunning = true;
        new Thread(WorkerThread) { IsBackground = true }.Start();
    }

    /// <summary>Worker thread, loop until "Cancel()" is executed.</summary>
    private void WorkerThread()
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (IsRunning)
        {
            RunAction();
            int waitTime = (int)(IntervalMillis - sw.ElapsedMilliseconds);

            if (waitTime > 0) _jobSleepEvent.WaitOne(waitTime);
            else if (waitTime < 0) ExecutionIntervalOverflow(-waitTime);

            sw.Restart();
        }

        AfterCancel();
        _workerExitEvent.Set();
    }
}
