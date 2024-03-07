using System;
using System.Diagnostics;
using System.Threading;

namespace RaceElement.Core.Jobs.LoopJob;

public abstract class AbstractLoopJob : IJob
{
    /** @brief Used by the cancel method to wait until job finish. */
    private ManualResetEvent _workerExitEvent = new(false);

    /** @brief Used by worker thread to sleep until next tick or wake up on cancel. */
    private ManualResetEvent _jobSleepEvent = new(false);

    /** @brief Tick interval. At what pace "RunAction" is executed.  */
    private int _intervalMillis = 1;

    /** @brief Used to keep worker thread running. */
    private bool _isRunning = false;

    /**
     * @brief Set at what interval "RunAction()" is executed.If the execution
     * time is less than the interval it will wait until the next interval,
     * otherwise it will be executed immediately.
     */
    public int IntervalMillis
    {
        get
        {
            return _intervalMillis;
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            _intervalMillis = value;
        }
    }

    /**
     * @brief Test if the job is running. A job is running when it has been
     * started and has not been canceled yet.
     *
     * @return true if the job is active, false otherwise.
     */
    public bool IsRunning => _isRunning;

    /**
     * @Callback method executed by the job at the certain time in future.
     * This method is executed from another thread, you may require
     * synchronization mechanism.
     */
    public abstract void RunAction();

    /**
     * @brief Callback method used by the job to notify it has been canceled.
     * This method is executed from another thread, you may require
     * synchronization mechanism.
     */
    public virtual void AfterCancel() { }

    /**
     * @brief Callback used to notify the client that "RunAction()" takes
     * longer time than expected.
     * @param millis Number if milliseconds exceeded from IntervalMillis.
     */
    protected virtual void ExecutionIntervalOverflow(int millis) { }

    /**
     * @brief Cancel the execution of the job without waiting for finish
     * confirmation (no synchronization).
     */
    public void Cancel()
     {
        if (_isRunning)
        {
            _isRunning = false;
            _jobSleepEvent.Set();
        }
    }

    /**
     * @brief Cancel the execution of the job and wait for the finish
     * confirmation (synchronization method).
     */
    public void CancelJoin()
    {
        Cancel();
        _workerExitEvent.WaitOne();
    }

    /**
     * @brief Causes the job to begin executing. The result is a thread calling
     * "RunAction()" method which is implemented by the child. If the job is
     * executing and the method all calls to this method will be ignored.
     */
    public void Run()
    {
        if (_isRunning)
        {
            return;
        }

        _jobSleepEvent.Reset();
        _workerExitEvent.Reset();

        _isRunning = true;
        new Thread(WorkerThread) { IsBackground = true }.Start();
    }

    /**
     * @brief Worker thread, loop until "Cancel()" is executed.
     */
    private void WorkerThread()
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (_isRunning)
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
