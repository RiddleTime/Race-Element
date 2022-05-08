using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.Overlay.Internal
{
    internal class PagePhysicsTracker : IDisposable
    {
        private static PagePhysicsTracker _instance;
        internal static PagePhysicsTracker Instance
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
        private readonly ACCSharedMemory sharedMemory;

        private PagePhysicsTracker()
        {
            sharedMemory = new ACCSharedMemory();

            trackingTask = Task.Run(() =>
            {
                isTracking = true;
                while (isTracking)
                {
                    Thread.Sleep(1);
                    Tracker?.Invoke(this, sharedMemory.ReadPhysicsPageFile());
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
