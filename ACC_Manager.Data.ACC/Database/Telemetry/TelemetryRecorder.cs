using ACC_Manager.Util.Settings;
using ACCManager.Data.ACC.Session;
using ACCManager.Data.ACC.Tracker;
using ACCManager.Data.ACC.Tracker.Laps;
using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Data.ACC.Database.Telemetry
{
    internal class TelemetryRecorder
    {
        private readonly int IntervalMillis;
        private bool _isRunning = false;
        private ACCSharedMemory.SPageFilePhysics _pagePhysics;
        private ACCSharedMemory.SPageFileGraphic _pageGraphics;
        private readonly Dictionary<long, TelemetryPoint> _lapData = new Dictionary<long, TelemetryPoint>();

        public TelemetryRecorder()
        {
            IntervalMillis = 1000 / new AccManagerSettings().Get().TelemetryDetailedHerz;

            PagePhysicsTracker.Instance.Tracker += OnPagePhysicsUpdated;
            PageGraphicsTracker.Instance.Tracker += OnPageGraphicsUpdated;
            LapTracker.Instance.LapFinished += OnLapFinished;
            RaceSessionTracker.Instance.OnSessionFinished += OnSessionFinished;

            _pageGraphics = ACCSharedMemory.Instance.ReadGraphicsPageFile(true);
            _pagePhysics = ACCSharedMemory.Instance.ReadPhysicsPageFile(true);
        }

        private void OnSessionFinished(object sender, SessionData.DbRaceSession e)
        {
            Stop();
        }

        private void OnLapFinished(object sender, LapDataDB.DbLapData e)
        {
            if (_isRunning)
            {
                LogWriter.WriteToLog($"TelemetryRecorder: Recorded {_lapData.Count} data points.");
                DbLapTelemetry lapTelemetry = new DbLapTelemetry()
                {
                    Id = Guid.NewGuid(),
                    LapId = e.Id,
                    LapData = _lapData.SerializeLapData(),
                    Herz = 1000 / this.IntervalMillis
                };
                _lapData.Clear();
                var collection = RaceWeekendDatabase.Database.GetCollection<DbLapTelemetry>();

                var existing = collection.Find(x => x.LapId == e.Id);
                if (!existing.Any())
                {
                    LogWriter.WriteToLog("TelemetryRecorder: Inserting LapTelemetry");
                    RaceWeekendDatabase.Database.BeginTrans();
                    collection.Insert(lapTelemetry);
                    RaceWeekendDatabase.Database.Commit();
                }
                else
                {
                    LogWriter.WriteToLog($"TelemetryRecorder: LapTelemetry already exists for lap id {e.Id}");
                }
            }
        }

        private void OnPageGraphicsUpdated(object sender, SPageFileGraphic e) => _pageGraphics = e;
        private void OnPagePhysicsUpdated(object sender, SPageFilePhysics e) => _pagePhysics = e;

        public void Record()
        {
            LogWriter.WriteToLog("TelemetryRecorder: Record()");

            if (!new AccManagerSettings().Get().TelemetryRecordDetailed)
                return;

            _isRunning = true;
            new Thread(x =>
            {
                LogWriter.WriteToLog("Starting recording loop");
                var interval = new TimeSpan(0, 0, 0, 0, IntervalMillis);
                var nextTick = DateTime.UtcNow + interval;
                while (_isRunning)
                {
                    try
                    {
                        while (DateTime.UtcNow < nextTick)
                            Thread.Sleep(nextTick - DateTime.UtcNow);
                    }
                    catch { }
                    nextTick += interval;
                    long ticks = DateTime.UtcNow.Ticks;

                    try
                    {
                        bool hasLapData = _lapData.Any();
                        bool isPointFurther = false;
                        if (hasLapData)
                            isPointFurther = _lapData.Last().Value.SplinePosition < _pageGraphics.NormalizedCarPosition;

                        if (!hasLapData || isPointFurther)
                            lock (_lapData)
                            {
                                if (!_lapData.ContainsKey(ticks))
                                {
                                    int playerCarIndex = -1;
                                    for (int i = 0; i < _pageGraphics.CarIds.Length; i++)
                                        if (_pageGraphics.CarIds[i] == _pageGraphics.PlayerCarID)
                                        {
                                            playerCarIndex = i;
                                            break;
                                        }

                                    if (playerCarIndex != -1)
                                    {
                                        float xCoord = 0;
                                        float yCoord = 0;
                                        for (int i = 0; i < 4; i++)
                                        {
                                            xCoord += _pagePhysics.TyreContactPoint[i].X;
                                            yCoord += _pagePhysics.TyreContactPoint[i].Z;
                                        }
                                        xCoord /= 4;
                                        yCoord /= 4;

                                        if (xCoord == 0 || yCoord == 0)
                                            continue;

                                        _lapData.Add(ticks, new TelemetryPoint()
                                        {
                                            SplinePosition = _pageGraphics.NormalizedCarPosition,
                                            InputsData = new InputsData()
                                            {
                                                Gas = _pagePhysics.Gas,
                                                Brake = _pagePhysics.Brake,
                                                Gear = _pagePhysics.Gear,
                                                SteerAngle = _pagePhysics.SteerAngle
                                            },
                                            TyreData = new TyreData()
                                            {
                                                TyreCoreTemperature = _pagePhysics.TyreCoreTemperature,
                                                TyrePressure = _pagePhysics.WheelPressure,
                                            },
                                            BrakeData = new BrakeData()
                                            {
                                                BrakeTemperature = _pagePhysics.BrakeTemperature,
                                            },
                                            PhysicsData = new PhysicsData()
                                            {
                                                WheelSlip = _pagePhysics.WheelSlip,
                                                Speed = _pagePhysics.SpeedKmh,
                                                X = xCoord,
                                                Y = yCoord,
                                                Heading = _pagePhysics.Heading,
                                            }
                                        });
                                    }
                                }
                            }
                        else
                        {
                            if (hasLapData)
                                isPointFurther = _lapData.Last().Value.SplinePosition < _pageGraphics.NormalizedCarPosition;

                        }
                    }
                    catch (Exception ex)
                    {
                        LogWriter.WriteToLog(ex);
                    }
                }
            }).Start();
        }

        public void Stop()
        {
            LogWriter.WriteToLog("TelemetryRecorder: Stop()");
            Thread.Sleep(IntervalMillis * 3);
            _isRunning = false;

            PagePhysicsTracker.Instance.Tracker -= OnPagePhysicsUpdated;
            PageGraphicsTracker.Instance.Tracker -= OnPageGraphicsUpdated;
            LapTracker.Instance.LapFinished -= OnLapFinished;
            RaceSessionTracker.Instance.OnSessionFinished -= OnSessionFinished;
        }
    }
}
