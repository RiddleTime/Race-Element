using RaceElement.Broadcast;
using RaceElement.Data.ACC.Core;
using RaceElement.Data.ACC.Core.Config;
using RaceElement.Data.ACC.Database;
using RaceElement.Data.ACC.Database.GameData;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Database.RaceWeekend;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Database.Telemetry;
using RaceElement.Data.ACC.Tracker;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.Data.ACC.Session;

public sealed class RaceSessionTracker
{
    private bool _isTracking;

    private float _sessionTimeLeft;

    private int _gameTimeMultiplier = 1;
    private float _prevClockTimeDaySeconds = 0;
    public event EventHandler<int> OnMultiplierChanged;

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
    private static RaceSessionTracker _instance;
    private DateTime _lastReplaySave;

    private RaceSessionTracker()
    {
        Debug.WriteLine("Started race session tracker.");
        _prevClockTimeDaySeconds = ACCSharedMemory.Instance.ReadGraphicsPageFile().ClockTimeDaySeconds;

        _isTracking = true;
        new Thread(WorkerThread).Start();

        OnSessionIndexChanged += TryCreateNewSession;
        OnACSessionTypeChanged += TryCreateNewSession;

        ReplaySettings replaySettingsJson = new();
        var json = replaySettingsJson.Get(false);
        Debug.WriteLine(json);
    }

    public static RaceSessionTracker Instance
    {
        get
        {
            _instance ??= new RaceSessionTracker();
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
            if (lapData.Count != 0)
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

    private void WorkerThread()
    {
        BroadcastTracker.Instance.OnRealTimeUpdate += Instance_OnRealTimeUpdate;
        Stopwatch timer = Stopwatch.StartNew();

        while (_isTracking)
        {
            Thread.Sleep(100);

            if (AccProcess.IsRunning == false)
            {
                continue;
            }

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

            if (HasMultiplierChanged(pageGraphics, timer))
                OnMultiplierChanged?.Invoke(this, _gameTimeMultiplier);

            if (sessionIndexChanged)
                OnSessionIndexChanged?.Invoke(this, _lastSessionIndex);

            if (sessionTypeChanged)
                OnACSessionTypeChanged?.Invoke(this, _lastSessionType);
        }

        BroadcastTracker.Instance.OnRealTimeUpdate -= Instance_OnRealTimeUpdate;
    }

    private bool HasMultiplierChanged(SPageFileGraphic pageGraphics, Stopwatch timer)
    {
        float currentClockTimeDaySeconds = pageGraphics.ClockTimeDaySeconds;
        currentClockTimeDaySeconds += (currentClockTimeDaySeconds < _prevClockTimeDaySeconds) ? (24 * 3600) : 0;

        float inGameSeconds = currentClockTimeDaySeconds - _prevClockTimeDaySeconds;
        float worldSeconds = timer.ElapsedMilliseconds / 1000.0f;

        if (worldSeconds < 1)
        {
            return false;
        }

        _prevClockTimeDaySeconds = pageGraphics.ClockTimeDaySeconds;
        timer.Restart();

        _gameTimeMultiplier = (int)Math.Round(inGameSeconds / worldSeconds);
        return true;
    }
}
