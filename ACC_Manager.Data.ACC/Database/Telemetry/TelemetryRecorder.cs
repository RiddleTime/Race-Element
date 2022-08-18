using ACC_Manager.Util.Settings;
using ACCManager.Data.ACC.Session;
using ACCManager.Data.ACC.Tracker;
using ACCManager.Data.ACC.Tracker.Laps;
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
        private Dictionary<long, TelemetryPoint> _lapData = new Dictionary<long, TelemetryPoint>();

        public TelemetryRecorder(int intervalMillis = 50)
        {
            IntervalMillis = intervalMillis;

            PagePhysicsTracker.Instance.Tracker += OnPagePhysicsUpdated;
            PageGraphicsTracker.Instance.Tracker += OnPageGraphicsUpdated;
            LapTracker.Instance.LapFinished += OnLapFinished;
            RaceSessionTracker.Instance.OnSessionFinished += OnSessionFinished;
        }

        private void OnSessionFinished(object sender, SessionData.DbRaceSession e)
        {
            Stop();
        }

        private void OnLapFinished(object sender, LapDataDB.DbLapData e)
        {
            if (_isRunning)
            {
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
                    RaceWeekendDatabase.Database.BeginTrans();
                    collection.Insert(lapTelemetry);
                    RaceWeekendDatabase.Database.Commit();
                }
            }
        }

        private void OnPageGraphicsUpdated(object sender, SPageFileGraphic e) => _pageGraphics = e;
        private void OnPagePhysicsUpdated(object sender, SPageFilePhysics e) => _pagePhysics = e;

        public void Record()
        {
            if (!new AccManagerSettings().Get().TelemetryRecordDetailed)
                return;

            _isRunning = true;
            ThreadPool.QueueUserWorkItem(x =>
            {
                var interval = new TimeSpan(0, 0, 0, 0, IntervalMillis);
                var nextTick = DateTime.Now + interval;
                while (_isRunning)
                {
                    try
                    {
                        while (DateTime.Now < nextTick)
                            Thread.Sleep(nextTick - DateTime.Now);
                    }
                    catch { }
                    nextTick += interval;
                    long ticks = DateTime.Now.Ticks;

                    if (_pagePhysics.BrakeBias > 0)
                        if (!_lapData.Any() || _lapData.Last().Value.SplinePosition < _pageGraphics.NormalizedCarPosition)
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
                                }
                            });
                }
            });
        }

        public void Stop()
        {
            _isRunning = false;
            Thread.Sleep(IntervalMillis * 3);
            PagePhysicsTracker.Instance.Tracker -= OnPagePhysicsUpdated;
            PageGraphicsTracker.Instance.Tracker -= OnPageGraphicsUpdated;
        }
    }


}
