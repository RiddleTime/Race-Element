using ACCManager.Data.ACC.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Data.ACC.Tracker
{
    public class PagePhysicsTracker : IDisposable
    {
        private static PagePhysicsTracker _instance;
        public static PagePhysicsTracker Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PagePhysicsTracker();

                return _instance;
            }
        }

        public event EventHandler<SPageFilePhysics> Tracker;

        private bool isTracking = false;

        private readonly Task trackingTask;

        private PagePhysicsTracker()
        {
            trackingTask = Task.Run(() =>
            {
                isTracking = true;
                while (isTracking)
                {
                    Thread.Sleep(1);

                    if (AccProcess.IsRunning)
                        Tracker?.Invoke(this, ACCSharedMemory.Instance.ReadPhysicsPageFile());
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
