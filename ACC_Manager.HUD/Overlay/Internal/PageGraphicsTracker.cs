using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.HUD.Overlay.Internal
{
    internal class PageGraphicsTracker : IDisposable
    {
        private static PageGraphicsTracker _instance;
        internal static PageGraphicsTracker Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PageGraphicsTracker();

                return _instance;
            }
        }

        public event EventHandler<SPageFileGraphic> Tracker;

        private bool isTracking = false;

        private readonly Task trackingTask;
        private readonly ACCSharedMemory sharedMemory;

        private PageGraphicsTracker()
        {
            sharedMemory = new ACCSharedMemory();

            trackingTask = Task.Run(() =>
            {
                isTracking = true;
                while (isTracking)
                {
                    Thread.Sleep(1);
                    Tracker?.Invoke(this, sharedMemory.ReadGraphicsPageFile());

                    if (sharedMemory.ReadGraphicsPageFile().Status == AcStatus.AC_OFF)
                    {
                        BroadcastTracker.Instance.Disconnect();
                    }
                    else
                    {
                        if (!BroadcastTracker.Instance.IsConnected)
                            BroadcastTracker.Instance.Connect();
                    }
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
