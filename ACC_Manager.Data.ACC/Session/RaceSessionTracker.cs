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

        private AcSessionType lastSessionType = AcSessionType.AC_UNKNOWN;
        public event EventHandler<AcSessionType> OnACSessionTypeChanged;

        private SessionPhase lastSessionPhase = SessionPhase.NONE;
        public event EventHandler<SessionPhase> OnSessionPhaseChanged;

        public RaceSessionTracker()
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
                    if (pageGraphics.SessionType != lastSessionType)
                    {
                        Debug.WriteLine($"SessionType: {lastSessionType} -> {pageGraphics.SessionType}");
                        lastSessionType = pageGraphics.SessionType;
                        OnACSessionTypeChanged?.Invoke(this, pageGraphics.SessionType);
                    }
                }

                BroadcastTracker.Instance.OnRealTimeUpdate -= Instance_OnRealTimeUpdate;
            }).Start();

        }

        private void Instance_OnRealTimeUpdate(object sender, Broadcast.Structs.RealtimeUpdate e)
        {
            if (e.Phase != lastSessionPhase)
            {
                Debug.WriteLine($"SessionPhase: {lastSessionPhase} -> {e.Phase}");

                lastSessionPhase = e.Phase;
                OnSessionPhaseChanged?.Invoke(this, e.Phase);
            }
        }

        internal void Stop()
        {
            _isTracking = false;
        }
    }
}
