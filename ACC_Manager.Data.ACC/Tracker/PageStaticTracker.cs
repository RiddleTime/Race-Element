using ACCManager.Data.ACC.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Data.ACC.Tracker
{
    public class PageStaticTracker : IDisposable
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

        public event EventHandler<SPageFileStatic> Tracker;

        private bool isTracking = false;

        private readonly Task trackingTask;

        private PageStaticTracker()
        {
            trackingTask = Task.Run(() =>
            {
                isTracking = true;
                while (isTracking)
                {
                    Thread.Sleep(2);

                    if (AccProcess.IsRunning)
                        Tracker?.Invoke(this, ACCSharedMemory.Instance.ReadStaticPageFile());
                }
            });

            _instance = this;
        }

        public void Dispose()
        {
            isTracking = false;
            trackingTask.Dispose();
        }
    }
}
