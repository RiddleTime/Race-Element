using ACCManager.Broadcast;
using ACCManager.Data.ACC.Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Data.ACC.Session
{
    public class RaceSessionTracker
    {

        private static RaceSessionTracker _instance;
        public static RaceSessionTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new RaceSessionTracker();
                return _instance;
            }
        }
        private bool _isTracking = false;
        private ACCSharedMemory _sharedMemory;

        private AcSessionType _lastSessionType = AcSessionType.AC_UNKNOWN;
        public event EventHandler<AcSessionType> OnACSessionTypeChanged;

        private SessionPhase _lastSessionPhase = SessionPhase.NONE;
        public event EventHandler<SessionPhase> OnSessionPhaseChanged;

        private AcStatus _lastAcStatus = AcStatus.AC_OFF;
        public event EventHandler<AcStatus> OnACStatusChanged;

        private int _lastSessionIndex = -1;
        public event EventHandler<int> OnSessionIndexChanged;

        private RaceSessionTracker()
        {
            _sharedMemory = new ACCSharedMemory();
            StartTracking();
        }

        private void StartTracking()
        {
            _isTracking = true;

            new Thread(() =>
            {
                BroadcastTracker.Instance.OnRealTimeUpdate += Instance_OnRealTimeUpdate;

                while (_isTracking)
                {
                    Thread.Sleep(100);

                    var pageGraphics = _sharedMemory.ReadGraphicsPageFile();

                    if (pageGraphics.SessionType != _lastSessionType)
                    {
                        Debug.WriteLine($"SessionType: {_lastSessionType} -> {pageGraphics.SessionType}");
                        _lastSessionType = pageGraphics.SessionType;
                        OnACSessionTypeChanged?.Invoke(this, _lastSessionType);
                    }

                    if (pageGraphics.SessionIndex != _lastSessionIndex)
                    {
                        Debug.WriteLine($"SessionIndex: {_lastSessionIndex} -> {pageGraphics.SessionIndex}");
                        _lastSessionIndex = pageGraphics.SessionIndex;
                        OnSessionIndexChanged?.Invoke(this, _lastSessionIndex);
                    }

                    if (pageGraphics.Status != _lastAcStatus)
                    {
                        Debug.WriteLine($"AcStatus: {_lastAcStatus} -> {pageGraphics.Status}");
                        _lastAcStatus = pageGraphics.Status;
                        OnACStatusChanged?.Invoke(this, _lastAcStatus);
                    }
                }

                BroadcastTracker.Instance.OnRealTimeUpdate -= Instance_OnRealTimeUpdate;
            }).Start();

        }

        private void Instance_OnRealTimeUpdate(object sender, Broadcast.Structs.RealtimeUpdate e)
        {
            if (e.Phase != _lastSessionPhase)
            {
                Debug.WriteLine($"SessionPhase: {_lastSessionPhase} -> {e.Phase}");

                _lastSessionPhase = e.Phase;
                OnSessionPhaseChanged?.Invoke(this, e.Phase);
            }
        }

        internal void Stop()
        {
            _isTracking = false;
        }
    }
}
