using RaceElement.Broadcast;
using RaceElement.Data.ACC.Core;
using RaceElement.Data.ACC.Core.Config;
using RaceElement.Data.ACC.Database;
using RaceElement.Data.ACC.Database.GameData;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Database.RaceWeekend;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Database.Telemetry;
using RaceElement.Data.ACC.HotKey;
using RaceElement.Data.ACC.Tracker;
using RaceElement.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.Data.ACC.Session
{
    public class RaceSessionTracker
    {
        private bool _isTracking = false;

        private float _sessionTimeLeft;

        private AcSessionType _lastSessionType = AcSessionType.AC_UNKNOWN;
        public event EventHandler<AcSessionType> OnACSessionTypeChanged;

        private SessionPhase _lastSessionPhase = SessionPhase.NONE;
        public event EventHandler<SessionPhase> OnSessionPhaseChanged;

        private AcStatus _lastAcStatus = AcStatus.AC_OFF;
        public event EventHandler<AcStatus> OnACStatusChanged;

        private int _lastSessionIndex = -1;
        public event EventHandler<int> OnSessionIndexChanged;

        public event EventHandler<DbRaceSession> OnNewSessionStarted;
        public event EventHandler<DbRaceSession> OnSessionFinished;
        public event EventHandler<DbRaceWeekend> OnRaceWeekendEnded;

        internal DbRaceSession CurrentSession { get; private set; }

        private DateTime _lastReplaySave;

        private static RaceSessionTracker _instance;

        private RaceSessionTracker()
        {
            Debug.WriteLine("Started race session tracker.");

            StartTracking();
            OnSessionIndexChanged += TryCreateNewSession;
            OnACSessionTypeChanged += TryCreateNewSession;

            ReplaySettings replaySettingsJson = new ReplaySettings();
            var json = replaySettingsJson.Get(false);
            Debug.WriteLine(json);
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

                var staticPageFile = ACCSharedMemory.Instance.ReadStaticPageFile();
                DbCarData carData = CarDataCollection.GetCarData(staticPageFile.CarModel);
                DbTrackData trackData = TrackDataCollection.GetTrackData(staticPageFile.Track);
                if (carData == null || trackData == null)
                    return;

                CurrentSession = new DbRaceSession()
                {
                    UtcStart = DateTime.UtcNow,
                    RaceWeekendId = RaceWeekendCollection.Current.Id,
                    SessionIndex = _lastSessionIndex,
                    SessionType = _lastSessionType,
                    CarId = carData.Id,
                    TrackId = trackData.Id,
                    IsOnline = staticPageFile.isOnline
                };
                _sessionTimeLeft = ACCSharedMemory.Instance.ReadGraphicsPageFile().SessionTimeLeft;

                _lastReplaySave = DateTime.UtcNow;

                RaceSessionCollection.Insert(CurrentSession);
                OnNewSessionStarted?.Invoke(this, CurrentSession);

                TelemetryRecorder.Instance.Record();
            }
        }

        private void FinalizeCurrentSession()
        {
            if (CurrentSession != null)
            {
                _lastSessionIndex = -1;
                _lastSessionType = AcSessionType.AC_UNKNOWN;
                CurrentSession.UtcEnd = DateTime.UtcNow;

                var lapData = LapDataCollection.GetForSession(CurrentSession.Id);
                if (lapData.Any())
                    RaceSessionCollection.Update(CurrentSession);
                else
                    RaceSessionCollection.Delete(CurrentSession);

                OnSessionFinished?.Invoke(this, CurrentSession);
                Debug.WriteLine("RaceSessionTracker: Finalized Current Session");
            }

            CurrentSession = null;
            Debug.WriteLine("CurrentSession Reset to null");
        }

        private void FinalizeRaceWeekend()
        {
            RaceWeekendCollection.Current.UtcEnd = DateTime.UtcNow;
            OnRaceWeekendEnded?.Invoke(this, RaceWeekendCollection.Current);
            RaceWeekendCollection.End();
        }

        private void CreateNewRaceWeekend()
        {
            var pageStatic = ACCSharedMemory.Instance.ReadStaticPageFile();
            RaceWeekendDatabase.CreateDatabase(pageStatic.Track, pageStatic.CarModel, DateTime.UtcNow);
            RaceWeekendCollection.Insert(new DbRaceWeekend() { Id = Guid.NewGuid(), UtcStart = DateTime.UtcNow });
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

                    if (AccProcess.IsRunning)
                    {
                        var pageGraphics = ACCSharedMemory.Instance.ReadGraphicsPageFile();

                        if (pageGraphics.Status != _lastAcStatus)
                        {
                            Debug.WriteLine($"AcStatus: {_lastAcStatus} -> {pageGraphics.Status}");

                            if (_lastAcStatus == AcStatus.AC_OFF)
                                CreateNewRaceWeekend();

                            _lastAcStatus = pageGraphics.Status;

                            if (_lastAcStatus == AcStatus.AC_OFF)
                            {
                                FinalizeCurrentSession();
                                FinalizeRaceWeekend();
                                OnSessionIndexChanged?.Invoke(this, _lastSessionIndex);
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

                        if (pageGraphics.SessionTimeLeft > _sessionTimeLeft)
                        {
                            if (CurrentSession == null)
                                _sessionTimeLeft = pageGraphics.SessionTimeLeft;

                            Trace.WriteLine("Detected session restart.. finalizing current session");
                            FinalizeCurrentSession();
                        }

                        if (sessionIndexChanged)
                            OnSessionIndexChanged?.Invoke(this, _lastSessionIndex);
                        if (sessionTypeChanged)
                            OnACSessionTypeChanged?.Invoke(this, _lastSessionType);

                        //HandleReplaySave();

                    }
                }

                BroadcastTracker.Instance.OnRealTimeUpdate -= Instance_OnRealTimeUpdate;
            }).Start();

        }

        private void HandleReplaySave()
        {
            if (_lastReplaySave.AddHours(1) < DateTime.UtcNow)
            {
                LogWriter.WriteToLog($"Automatically Saving Replay after {_lastReplaySave.Subtract(DateTime.UtcNow)}");
                _lastReplaySave = AccHotkeys.SaveReplay();
            }
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
