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

        public event EventHandler<int> OnMultiplierChanged;

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
                RaceSessionTracker.Instance.OnSessionPhaseChanged += Instance_OnSessionPhaseChanged;

                while (_isTracking)
                {
                    Thread.Sleep(1000);

                }

                BroadcastTracker.Instance.OnRealTimeUpdate -= Instance_OnRealTimeUpdate;
                RaceSessionTracker.Instance.OnACSessionTypeChanged -= Instance_OnACSessionTypeChanged;
                RaceSessionTracker.Instance.OnSessionPhaseChanged -= Instance_OnSessionPhaseChanged;

                _instance = null;
            }).Start();
        }

        private void Instance_OnSessionPhaseChanged(object sender, Broadcast.SessionPhase e)
        {
            if (e == Broadcast.SessionPhase.NONE)
                ResetTimeTrackingData();
        }

        private void Instance_OnACSessionTypeChanged(object sender, ACCSharedMemory.AcSessionType e)
        {
            ResetTimeTrackingData();
        }

        private void ResetTimeTrackingData()
        {
            _lastServerMilliseconds = -1;
            _timeMultiplier = -1;
            OnMultiplierChanged?.Invoke(this, _timeMultiplier);
        }

        private long _lastServerMilliseconds = -1;
        private void Instance_OnRealTimeUpdate(object sender, Broadcast.Structs.RealtimeUpdate e)
        {
            long newMilliseconds = (long)(e.TimeOfDay.TotalMilliseconds);
            if (newMilliseconds != _lastServerMilliseconds)
            {
                if (newMilliseconds - _lastServerMilliseconds < 240000)
                    if (_lastServerMilliseconds != -1)
                    {
                        double serverTimeChange = newMilliseconds - _lastServerMilliseconds;
                        serverTimeChange = Math.Ceiling(serverTimeChange);
                        serverTimeChange = Math.Floor(serverTimeChange / 5) * 5;
                        if (serverTimeChange > 0)
                        {
                            double possibleMultiplier = (int)Math.Ceiling(serverTimeChange / 5);

                            if (possibleMultiplier > 0 && possibleMultiplier < 25)
                            {
                                if (_timeMultiplier != possibleMultiplier)
                                {
                                    Debug.WriteLine($"Time Multiplier: x{possibleMultiplier}");

                                    _timeMultiplier = (int)Math.Floor(possibleMultiplier);
                                    OnMultiplierChanged?.Invoke(this, (int)Math.Floor(possibleMultiplier));
                                }
                            }
                        }
                    }

                _lastServerMilliseconds = newMilliseconds;
            }
        }

        internal void Stop()
        {
            _isTracking = false;
        }
    }
}
