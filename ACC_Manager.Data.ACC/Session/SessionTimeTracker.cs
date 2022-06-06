using ACCManager.Data.ACC.Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Session
{
    public class SessionTimeTracker
    {
        private static SessionTimeTracker _instance;
        public static SessionTimeTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new SessionTimeTracker();
                return _instance;
            }
        }

        private int _timeMultiplier = -1;
        public int TimeMultiplier { get => _timeMultiplier; }

        private bool _isTracking = false;

        private SessionTimeTracker()
        {
            StartTracking();
        }

        private void StartTracking()
        {
            new Thread(() =>
            {
                _isTracking = true;
                BroadcastTracker.Instance.OnRealTimeUpdate += Instance_OnRealTimeUpdate;
                RaceSessionTracker.Instance.OnACSessionTypeChanged += Instance_OnACSessionTypeChanged;

                while (_isTracking)
                {
                    Thread.Sleep(1000);

                }

                BroadcastTracker.Instance.OnRealTimeUpdate -= Instance_OnRealTimeUpdate;
                _instance = null;
            }).Start();
        }

        private void Instance_OnACSessionTypeChanged(object sender, ACCSharedMemory.AcSessionType e)
        {
            _lastChange = -1;
            _lastMilliseconds = -1;
        }

        private long _lastChange = -1;
        private long _lastMilliseconds = -1;
        private void Instance_OnRealTimeUpdate(object sender, Broadcast.Structs.RealtimeUpdate e)
        {
            long newMilliseconds = (long)(e.TimeOfDay.TotalMilliseconds * 1000);
            if (newMilliseconds > _lastMilliseconds)
            {
                if (_lastChange != -1 && _lastMilliseconds != -1)
                {
                    long changeTime = DateTime.Now.ToFileTime() - _lastChange;
                    _timeMultiplier = (int)(_lastMilliseconds / changeTime);
                    Debug.WriteLine("possible multi " + _lastMilliseconds / changeTime);
                }

                Debug.WriteLine(newMilliseconds - _lastMilliseconds);

                _lastMilliseconds = newMilliseconds;
                _lastChange = DateTime.Now.ToFileTime();
            }
        }

        internal void Stop()
        {
            _isTracking = false;
        }
    }
}
