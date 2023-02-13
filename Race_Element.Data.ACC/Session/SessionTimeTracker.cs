using RaceElement.Data.ACC.Tracker;
using System;
using System.Diagnostics;
using System.Threading;

namespace RaceElement.Data.ACC.Session
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
                RaceSessionTracker.Instance.OnSessionIndexChanged += Instance_OnSessionIndexChanged;

                while (_isTracking)
                {
                    Thread.Sleep(1000);
                }

                BroadcastTracker.Instance.OnRealTimeUpdate -= Instance_OnRealTimeUpdate;
                RaceSessionTracker.Instance.OnACSessionTypeChanged -= Instance_OnACSessionTypeChanged;
                RaceSessionTracker.Instance.OnSessionPhaseChanged -= Instance_OnSessionPhaseChanged;
                RaceSessionTracker.Instance.OnSessionIndexChanged -= Instance_OnSessionIndexChanged;
                _instance = null;
            }).Start();
        }

        private void Instance_OnSessionIndexChanged(object sender, int e)
        {
            ResetTimeTrackingData();
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
            Debug.WriteLine("Reset time tracking data");
            _serverChanges = 0;
            _lastServerMilliseconds = -1;
            _timeMultiplier = -1;
            OnMultiplierChanged?.Invoke(_instance, _timeMultiplier);
        }

        private long _lastServerMilliseconds = -1;
        private int _serverChanges = 0;
        private void Instance_OnRealTimeUpdate(object sender, Broadcast.Structs.RealtimeUpdate e)
        {
            long newMilliseconds = (long)(e.TimeOfDay.TotalMilliseconds);
            if (newMilliseconds != _lastServerMilliseconds)
            {
                double serverTimeChange = newMilliseconds - _lastServerMilliseconds;

                if (serverTimeChange < 240000 && serverTimeChange >= 1)
                    if (_lastServerMilliseconds != -1 && _serverChanges > 3)
                    {
                        serverTimeChange = Math.Ceiling(serverTimeChange);
                        serverTimeChange = Math.Floor(serverTimeChange / 5) * 5;
                        if (serverTimeChange > 0)
                        {
                            double possibleMultiplier = (int)Math.Ceiling(serverTimeChange / 5);

                            if (possibleMultiplier > 0 && possibleMultiplier < 25)
                                if (_timeMultiplier != possibleMultiplier)
                                {
                                    _timeMultiplier = (int)Math.Floor(possibleMultiplier);
                                    OnMultiplierChanged?.Invoke(_instance, (int)Math.Floor(possibleMultiplier));
                                }
                        }
                    }

                _lastServerMilliseconds = newMilliseconds;
                _serverChanges++;
            }
        }

        internal void Stop()
        {
            _isTracking = false;
        }
    }
}
