using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RaceElement.Core.Jobs.LoopJob
{
    public abstract class AbstractLoopJob : IJob
    {
        private bool _isCancelling = false;
        private bool _isStopped = true;

        public bool IsRunning => !_isStopped;

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

        public void Cancel() => _isCancelling = true;

        public void Run()
        {
            _isStopped = false;
            _isCancelling = false;

            Task.Run(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();

                while (!_isCancelling)
                {
                    if (sw.ElapsedMilliseconds < IntervalMillis)
                    {
                        int sleepTime = (int)(IntervalMillis - sw.ElapsedMilliseconds);
                        if (sleepTime > 2)
                            Thread.Sleep(sleepTime);

                        continue;
                    }

                    sw = Stopwatch.StartNew();

                    RunAction();
                }

                sw.Reset();
                _isStopped = true;
            });
        }
    }
}
