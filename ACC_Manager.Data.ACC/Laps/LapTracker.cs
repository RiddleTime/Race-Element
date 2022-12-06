using ACCManager.Broadcast.Structs;
using ACCManager.Data.ACC.Database.LapDataDB;
using ACCManager.Data.ACC.Session;
using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ACCManager.Data.ACC.Tracker.Laps
{
    public class LapTracker
    {
        private static LapTracker _instance;
        public static LapTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new LapTracker();
                return _instance;
            }
        }

        private bool IsTracking = false;

        private int CurrentSector = 0;

        public Dictionary<int, DbLapData> Laps = new Dictionary<int, DbLapData>();
        public DbLapData CurrentLap = new DbLapData();

        public event EventHandler<DbLapData> LapFinished;

        private LapTracker()
        {
            if (!IsTracking)
                this.Start();

            BroadcastTracker.Instance.OnRealTimeLocalCarUpdate += Instance_OnRealTimeCarUpdate;
            RaceSessionTracker.Instance.OnACSessionTypeChanged += Instance_OnACSessionTypeChanged;
            RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;
        }

        private void Instance_OnNewSessionStarted(object sender, Database.SessionData.DbRaceSession e)
        {
            Laps.Clear();
            CurrentLap = new DbLapData() { Index = ACCSharedMemory.Instance.ReadGraphicsPageFile(true).CompletedLaps + 1 };
        }

        private void Instance_OnACSessionTypeChanged(object sender, ACCSharedMemory.AcSessionType e)
        {
            var pageGraphics = ACCSharedMemory.Instance.ReadGraphicsPageFile(true);

            lock (Laps)
                Laps.Clear();

            lock (CurrentLap)
            {
                if (RaceSessionTracker.Instance.CurrentSession == null)
                    Debug.WriteLine("Lap Tracker cannot record a new lap on session type change, current session is null");
                else
                    CurrentLap = new DbLapData() { Index = pageGraphics.CompletedLaps + 1, RaceSessionId = RaceSessionTracker.Instance.CurrentSession.Id };
            }
            Debug.WriteLine("LapTracker: Resetted current lap and previous recorded laps.");
        }

        private LapInfo _lastLapInfo;

        private void Instance_OnRealTimeCarUpdate(object sender, RealtimeCarUpdate e)
        {
            if (e.LastLap != null)
            {
                _lastLapInfo = e.LastLap;

                FinalizeCurrentLapData();
            }
        }

        private void FinalizeCurrentLapData()
        {
            try
            {
                if (_lastLapInfo != null)
                    if (_lastLapInfo.Splits != null && _lastLapInfo.Splits.Count == 3)
                    {
                        lock (Laps)
                            if (Laps.Count != 0)
                            {
                                if (!Laps.TryGetValue(CurrentLap.Index - 1, out DbLapData lastData))
                                    return;

                                if (_lastLapInfo.LaptimeMS == lastData.Time)
                                {
                                    if (_lastLapInfo.Splits[2].HasValue)
                                    {
                                        if (Laps[CurrentLap.Index - 1].Sector3 != _lastLapInfo.Splits[2].Value)
                                        {
                                            Laps[CurrentLap.Index - 1].Sector3 = _lastLapInfo.Splits[2].Value;
                                            Laps[CurrentLap.Index - 1].IsValid = !_lastLapInfo.IsInvalid;
                                            Laps[CurrentLap.Index - 1].LapType = _lastLapInfo.Type;

                                            if (RaceSessionTracker.Instance.CurrentSession != null)
                                            {
                                                Laps[CurrentLap.Index - 1].Id = Guid.NewGuid();
                                                Laps[CurrentLap.Index - 1].RaceSessionId = RaceSessionTracker.Instance.CurrentSession.Id;
                                                Laps[CurrentLap.Index - 1].UtcCompleted = DateTime.UtcNow;
                                                Laps[CurrentLap.Index - 1].FuelInTank = ACCSharedMemory.Instance.ReadPhysicsPageFile().Fuel;
                                                Laps[CurrentLap.Index - 1].TempTrack = ACCSharedMemory.Instance.ReadPhysicsPageFile().RoadTemp;
                                                Laps[CurrentLap.Index - 1].TempAmbient = ACCSharedMemory.Instance.ReadPhysicsPageFile().AirTemp;
                                                Laps[CurrentLap.Index - 1].GripStatus = ACCSharedMemory.Instance.ReadGraphicsPageFile().trackGripStatus;
                                                LapDataCollection.Insert(Laps[CurrentLap.Index - 1]);

                                                Trace.WriteLine($"{Laps[CurrentLap.Index - 1]}");

                                                LapFinished?.Invoke(this, Laps[CurrentLap.Index - 1]);
                                            }
                                        }
                                    }
                                }
                            }
                    }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        internal void Start()
        {
            if (IsTracking)
                return;

            IsTracking = true;
            new Thread(x =>
            {
                while (IsTracking)
                {
                    try
                    {
                        Thread.Sleep(1000 / 10);

                        var pageGraphics = ACCSharedMemory.Instance.ReadGraphicsPageFile(true);

                        if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF)
                        {
                            Laps.Clear();
                            CurrentLap = new DbLapData() { Index = pageGraphics.CompletedLaps + 1 };
                        }


                        // TOdo if current lap is inlap/outlap reset the lap data.

                        // collect sector times.
                        if (CurrentSector != pageGraphics.CurrentSectorIndex)
                        {
                            if (CurrentLap.Sector1 == -1 && CurrentSector != 0)
                            {
                                // simply don't collect, we're already into a lap and passed sector 1, can't properly calculate the sector times now.
                            }
                            else
                                switch (pageGraphics.CurrentSectorIndex)
                                {
                                    case 1: CurrentLap.Sector1 = pageGraphics.LastSectorTime; break;
                                    case 2: CurrentLap.Sector2 = pageGraphics.LastSectorTime - CurrentLap.Sector1; break;

                                        // this sector time is now finalized with the FinalizeCurrentLapData() function    
                                        //case 0: CurrentLap.Sector3 = pageGraphics.LastTimeMs - CurrentLap.Sector2 - CurrentLap.Sector1; break;
                                }

                            CurrentSector = pageGraphics.CurrentSectorIndex;
                        }

                        // finalize lap time data and add it to history.
                        if (CurrentLap.Index - 1 != pageGraphics.CompletedLaps && pageGraphics.LastTimeMs != int.MaxValue)
                        {
                            CurrentLap.Time = pageGraphics.LastTimeMs;
                            CurrentLap.FuelUsage = (int)(ACCSharedMemory.Instance.ReadGraphicsPageFile().FuelXLap * 1000);

                            if (CurrentLap.Sector1 != -1)
                            {
                                lock (Laps)
                                {
                                    if (!Laps.ContainsKey(CurrentLap.Index))
                                        Laps.Add(CurrentLap.Index, CurrentLap);
                                }
                            }

                            CurrentLap = new DbLapData() { Index = pageGraphics.CompletedLaps + 1 };
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        LogWriter.WriteToLog(ex);
                    }
                }

                _instance = null;
                IsTracking = false;
            }).Start();
        }

        internal void Stop()
        {
            RaceSessionTracker.Instance.OnACSessionTypeChanged -= Instance_OnACSessionTypeChanged;
            BroadcastTracker.Instance.OnRealTimeLocalCarUpdate -= Instance_OnRealTimeCarUpdate;
            IsTracking = false;
        }
    }


}
