using ACCManager.Data.ACC.EntryList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Data.ACC.Tracker
{
    public class PageGraphicsTracker : IDisposable
    {
        private static PageGraphicsTracker _instance;
        public static PageGraphicsTracker Instance
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

            isTracking = true;

            trackingTask = Task.Run(() =>
            {
                while (isTracking)
                {
                    Thread.Sleep(1);
                    Tracker?.Invoke(this, sharedMemory.ReadGraphicsPageFile());
                }
            });

            Task.Run(() =>
            {
                while (isTracking)
                {
                    Thread.Sleep(100);

                    SPageFileGraphic sPageFileGraphic = sharedMemory.ReadGraphicsPageFile();

                    if (sPageFileGraphic.Status == AcStatus.AC_OFF && BroadcastTracker.Instance.IsConnected)
                    {
                        Debug.WriteLine("Disconnected broadcast tracker");
                        BroadcastTracker.Instance.Disconnect();
                        EntryListTracker.Instance.Stop();
                    }
                    else if (!BroadcastTracker.Instance.IsConnected && sPageFileGraphic.Status != AcStatus.AC_OFF)
                    {
                        Debug.WriteLine("Connected broadcast tracker");
                        BroadcastTracker.Instance.Connect();
                        EntryListTracker.Instance.Start();
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
