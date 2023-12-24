using RaceElement.Util.Settings;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.Data.ACC.Database.Telemetry
{
    internal class TelemetryRecorder
    {
        private static TelemetryRecorder _instance;
        public static TelemetryRecorder Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TelemetryRecorder();

                return _instance;
            }
        }

        private readonly int IntervalMillis;
        private readonly TimeSpan _intervalTimeSpan;

        private bool _isRunning = false;
        private SPageFilePhysics _pagePhysics;
        private SPageFileGraphic _pageGraphics;
        private readonly Dictionary<long, TelemetryPoint> _lapData = new();

        private TelemetryRecorder()
        {
            IntervalMillis = 1000 / new AccManagerSettings().Get().TelemetryDetailedHerz;
            _intervalTimeSpan = new TimeSpan(0, 0, 0, IntervalMillis);

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
                lock (_lapData)
                {
                    LogWriter.WriteToLog($"TelemetryRecorder: Recorded {_lapData.Count} data points.");
                    DbLapTelemetry lapTelemetry = new()
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
        }

        private void OnPageGraphicsUpdated(object sender, SPageFileGraphic e) => _pageGraphics = e;
        private void OnPagePhysicsUpdated(object sender, SPageFilePhysics e) => _pagePhysics = e;

        public void Record()
        {
            if (_isRunning)
            {
                Debug.WriteLine("TelemetryRecorded: Already Recording");
                LogWriter.WriteToLog("TelemetryRecorded: Already Recording");
                return;
            }

            LogWriter.WriteToLog("TelemetryRecorder: Record()");

            if (!new AccManagerSettings().Get().TelemetryRecordDetailed)
                return;

            _isRunning = true;
            new Thread(x =>
            {
                LogWriter.WriteToLog("Starting recording loop");


                double tickRefreshRate = IntervalMillis;
                Stopwatch stopwatch = Stopwatch.StartNew();

                while (_isRunning)
                {
                    int millisToWait = (int)Math.Floor(tickRefreshRate - stopwatch.ElapsedMilliseconds - 0.05);
                    if (millisToWait > 0)
                        Thread.Sleep(millisToWait);

                    try
                    {
                        lock (_lapData)
                        {
                            if (_pageGraphics.GlobalRed)
                                continue;

                            float currentSplinePositio = _pageGraphics.NormalizedCarPosition;


                            bool hasLapData = _lapData.Any();
                            bool isSamePoint = false;

                            if (hasLapData)
                                isSamePoint = _lapData.Last().Value.SplinePosition == currentSplinePositio;


                            if (_pageGraphics.NormalizedCarPosition == 1)
                                continue;

                            if (!hasLapData || (hasLapData && !isSamePoint))
                            {
                                long ticks = DateTime.Now.Ticks;
                                if (!_lapData.ContainsKey(ticks))
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
                                            WheelSlipAngle = _pagePhysics.SlipAngle,
                                            Speed = _pagePhysics.SpeedKmh,
                                            Acceleration = new float[] { _pagePhysics.AccG[0], _pagePhysics.AccG[2] },
                                            X = xCoord,
                                            Y = yCoord,
                                            Heading = _pagePhysics.Heading,
                                        }
                                    });
                                }
                                else
                                {
                                    //LogWriter.WriteToLog("Telemetry Point for time tick already exists: " + ticks);
                                }
                            }
                            else
                            {
                                //if (hasLapData)
                                //{
                                //    LogWriter.WriteToLog($"lapdata.count: {_lapData.Count}, Current Spline: {_pageGraphics.NormalizedCarPosition:F4}, Last spline: {_lapData.Last().Value.SplinePosition:F4} ");
                                //}

                            }
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
            _isRunning = false;
            LogWriter.WriteToLog("TelemetryRecorder: Stop()");
            Thread.Sleep(IntervalMillis * 3);

            PagePhysicsTracker.Instance.Tracker -= OnPagePhysicsUpdated;
            PageGraphicsTracker.Instance.Tracker -= OnPageGraphicsUpdated;
            LapTracker.Instance.LapFinished -= OnLapFinished;
            RaceSessionTracker.Instance.OnSessionFinished -= OnSessionFinished;
        }
    }
}
