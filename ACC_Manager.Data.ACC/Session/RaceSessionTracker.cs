using ACCManager.Broadcast;
using ACCManager.Data.ACC.Database.GameData;
using ACCManager.Data.ACC.Database.LapDataDB;
using ACCManager.Data.ACC.Database.SessionData;
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

        public event EventHandler<DbRaceSession> OnNewSessionStarted;

        internal DbRaceSession CurrentSession { get; private set; }

        private static RaceSessionTracker _instance;

        private RaceSessionTracker()
        {
            Debug.WriteLine("Started race session tracker.");
            _sharedMemory = new ACCSharedMemory();
            StartTracking();
            OnSessionIndexChanged += TryCreateNewSession;
            OnACSessionTypeChanged += TryCreateNewSession;
        }

        public static RaceSessionTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new RaceSessionTracker();
                return _instance;
            }
        }

        private void TryCreateNewSession(object sender, AcSessionType index)
        {
            if (index != AcSessionType.AC_UNKNOWN)
                TryCreateNewSession(sender, _lastSessionIndex);
        }

        private void TryCreateNewSession(object sender, int sessionIndex)
        {
            if (_lastSessionType == AcSessionType.AC_UNKNOWN)
                return;

            if (_lastSessionIndex > -1)
            {
                if (CurrentSession != null)
                {
                    if (_lastAcStatus != AcStatus.AC_OFF)
                        if (_lastSessionIndex == CurrentSession.SessionIndex && _lastSessionType == CurrentSession.SessionType)
                            return;

                    CurrentSession.UtcEnd = DateTime.UtcNow;
                    RaceSessionCollection.Update(CurrentSession);
                }

                var staticPageFile = _sharedMemory.ReadStaticPageFile();
                DbCarData carData = CarDataCollection.GetCarData(staticPageFile.CarModel);
                DbTrackData trackData = TrackDataCollection.GetTrackData(staticPageFile.Track);
                if (carData == null || trackData == null)
                    return;

                CurrentSession = new DbRaceSession()
                {
                    UtcStart = DateTime.UtcNow,
                    SessionIndex = _lastSessionIndex,
                    SessionType = _lastSessionType,
                    CarGuid = carData._id,
                    TrackGuid = trackData._id,
                    IsOnline = staticPageFile.isOnline
                };

                RaceSessionCollection.Insert(CurrentSession);
                OnNewSessionStarted?.Invoke(this, CurrentSession);
            }
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

                    if (pageGraphics.Status != _lastAcStatus)
                    {
                        Debug.WriteLine($"AcStatus: {_lastAcStatus} -> {pageGraphics.Status}");
                        _lastAcStatus = pageGraphics.Status;

                        if (_lastAcStatus == AcStatus.AC_OFF)
                        {
                            _lastSessionIndex = -1;
                            _lastSessionType = AcSessionType.AC_UNKNOWN;

                            if (CurrentSession != null)
                            {
                                CurrentSession.UtcEnd = DateTime.UtcNow;

                                var lapData = LapDataCollection.GetForSession(CurrentSession._id);
                                if (lapData.Any())
                                    RaceSessionCollection.Update(CurrentSession);
                                else
                                    RaceSessionCollection.Delete(CurrentSession);
                            }

                            CurrentSession = null;
                            OnSessionIndexChanged?.Invoke(this, _lastSessionIndex);
                            Debug.WriteLine("CurrentSession Reset to null");
                        }

                        OnACStatusChanged?.Invoke(this, _lastAcStatus);
                    }

                    bool sessionIndexChanged = false;
                    if (pageGraphics.SessionIndex != _lastSessionIndex && _lastAcStatus != AcStatus.AC_OFF)
                    {
                        Debug.WriteLine($"SessionIndex: {_lastSessionIndex} -> {pageGraphics.SessionIndex}");
                        _lastSessionIndex = pageGraphics.SessionIndex;
                        sessionIndexChanged = true;
                    }

                    bool sessionTypeChanged = false;
                    if (pageGraphics.SessionType != _lastSessionType && _lastAcStatus != AcStatus.AC_OFF)
                    {
                        Debug.WriteLine($"SessionType: {_lastSessionType} -> {pageGraphics.SessionType}");
                        _lastSessionType = pageGraphics.SessionType;
                        sessionTypeChanged = true;
                    }

                    if (sessionIndexChanged)
                        OnSessionIndexChanged?.Invoke(this, _lastSessionIndex);
                    if (sessionTypeChanged)
                        OnACSessionTypeChanged?.Invoke(this, _lastSessionType);


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
