using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing.SDK;
using System.Diagnostics;
using static RaceElement.Data.Games.iRacing.SDK.IRacingSdkSessionInfo.DriverInfoModel;
using RaceElement.Data.Common;
using System.Drawing;
using static RaceElement.Data.Games.iRacing.SDK.IRacingSdkEnum;
using static RaceElement.Data.Games.iRacing.SDK.IRacingSdkSessionInfo.SessionInfoModel.SessionModel;
using static RaceElement.Data.Games.iRacing.SDK.IRacingSdkSessionInfo.SessionInfoModel;
using System.Numerics;

// https://github.com/mherbold/IRSDKSharper
// https://sajax.github.io/irsdkdocs/telemetry/
// https://members-login.iracing.com/?ref=https%3A%2F%2Fmembers-ng.iracing.com%2Fdata%2Fdoc&signout=true (access to results data from iRacing.com. needs credentials)
// https://us.v-cdn.net/6034148/uploads/8DD84H30FIC8/telemetry-11-23-15.pdf Official doc
namespace RaceElement.Data.Games.iRacing
{
    public sealed class IRacingDataProvider : AbstractSimDataProvider
    {
        internal sealed override int PollingRate() => 50;


        Dictionary<string, Color> carClassColor = [];
        HashSet<string> carClasses = new HashSet<string>();

        bool hasTelemetry = false;

        private IRSDKSharper _iRacingSDK;
        private int lastSessionNumber = -1;

        private int lastLapIndex = 0;
        private float lastLapFuelLevelLiters = 0;
        private float lastLapFuelConsumption = 0;

        private IRacingSdkDatum carIdxLapDistPctDatum;
        private IRacingSdkDatum carIdxPositionDatum;
        private IRacingSdkDatum carIdxClassPositionDatum;
        private IRacingSdkDatum carIdxTrackSurfaceDatum;
        private IRacingSdkDatum carIdxOnPitRoadDatum;
        private IRacingSdkDatum carIdxLapDatum;
        private IRacingSdkDatum carIdxEstTimeDatum;
        private IRacingSdkDatum carIdxF2TimeDatum;
        private IRacingSdkDatum playerCarPositionDatum;
        private IRacingSdkDatum fuelLevelDatum;
        private IRacingSdkDatum rPMDatum;
        private IRacingSdkDatum speedDatum;
        private IRacingSdkDatum yawNorthDatum;
        private IRacingSdkDatum pitchDatum;
        private IRacingSdkDatum rollDatum;
        private IRacingSdkDatum gearDatum;
        private IRacingSdkDatum brakeDatum;
        private IRacingSdkDatum throttleDatum;
        private IRacingSdkDatum steeringWheelAngleDatum;
        private IRacingSdkDatum lapDeltaToSessionBestLapDatum;
        private IRacingSdkDatum airTempDatum;
        private IRacingSdkDatum windVelDatum;
        private IRacingSdkDatum windDirDatum;
        private IRacingSdkDatum trackTempCrewDatum;
        private IRacingSdkDatum fuelLevelPctDatum;
        private IRacingSdkDatum sessionTimeRemainDatum;
        private IRacingSdkDatum carLeftRightDatum;
        private IRacingSdkDatum brakeABSactiveDatum;
        private IRacingSdkDatum sessionNumDatum;
        private IRacingSdkDatum sessionStateDatum;
        private bool datumsInitialized = false;

        public CarLeftRight SpotterCallout { get; private set; }

        public IRacingDataProvider()
        {
            if (_iRacingSDK == null)
            {
                _iRacingSDK = new IRSDKSharper
                {
                    UpdateInterval = 1, // update every 1/60 second
                };
                _iRacingSDK.OnTelemetryData += OnTelemetryData;
                _iRacingSDK.OnSessionInfo += OnSessionInfo;
                _iRacingSDK.OnStopped += OnStopped;
                _iRacingSDK.OnDisconnected += OnDisconnected;

                _iRacingSDK.Start();
            }
        }

        /// <summary>
        /// Datums are used for iRacing telemetry access with doing the "string telemetry name" lookup only once.
        /// </summary>
        private void InitDatums()
        {
            if (datumsInitialized) return;
            carIdxLapDistPctDatum = _iRacingSDK.Data.TelemetryDataProperties["CarIdxLapDistPct"];
            carIdxPositionDatum = _iRacingSDK.Data.TelemetryDataProperties["CarIdxPosition"];
            carIdxClassPositionDatum = _iRacingSDK.Data.TelemetryDataProperties["CarIdxClassPosition"];
            carIdxTrackSurfaceDatum = _iRacingSDK.Data.TelemetryDataProperties["CarIdxTrackSurface"];
            carIdxOnPitRoadDatum = _iRacingSDK.Data.TelemetryDataProperties["CarIdxOnPitRoad"];
            carIdxLapDatum = _iRacingSDK.Data.TelemetryDataProperties["CarIdxLap"];
            carIdxEstTimeDatum = _iRacingSDK.Data.TelemetryDataProperties["CarIdxEstTime"];
            carIdxF2TimeDatum = _iRacingSDK.Data.TelemetryDataProperties["CarIdxF2Time"];
            playerCarPositionDatum = _iRacingSDK.Data.TelemetryDataProperties["PlayerCarPosition"];
            fuelLevelDatum = _iRacingSDK.Data.TelemetryDataProperties["FuelLevel"];
            rPMDatum = _iRacingSDK.Data.TelemetryDataProperties["RPM"];
            speedDatum = _iRacingSDK.Data.TelemetryDataProperties["Speed"];
            yawNorthDatum = _iRacingSDK.Data.TelemetryDataProperties["YawNorth"];
            pitchDatum = _iRacingSDK.Data.TelemetryDataProperties["Pitch"];
            rollDatum = _iRacingSDK.Data.TelemetryDataProperties["Roll"];
            gearDatum = _iRacingSDK.Data.TelemetryDataProperties["Gear"];
            brakeDatum = _iRacingSDK.Data.TelemetryDataProperties["Brake"];
            throttleDatum = _iRacingSDK.Data.TelemetryDataProperties["Throttle"];
            steeringWheelAngleDatum = _iRacingSDK.Data.TelemetryDataProperties["SteeringWheelAngle"];
            lapDeltaToSessionBestLapDatum = _iRacingSDK.Data.TelemetryDataProperties["LapDeltaToSessionBestLap"];
            airTempDatum = _iRacingSDK.Data.TelemetryDataProperties["AirTemp"];
            windVelDatum = _iRacingSDK.Data.TelemetryDataProperties["WindVel"];
            windDirDatum = _iRacingSDK.Data.TelemetryDataProperties["WindDir"];
            trackTempCrewDatum = _iRacingSDK.Data.TelemetryDataProperties["TrackTempCrew"];
            fuelLevelPctDatum = _iRacingSDK.Data.TelemetryDataProperties["FuelLevelPct"];
            sessionTimeRemainDatum = _iRacingSDK.Data.TelemetryDataProperties["SessionTimeRemain"];
            carLeftRightDatum = _iRacingSDK.Data.TelemetryDataProperties["CarLeftRight"];
            brakeABSactiveDatum = _iRacingSDK.Data.TelemetryDataProperties["BrakeABSactive"];
            sessionNumDatum = _iRacingSDK.Data.TelemetryDataProperties["SessionNum"];
            sessionStateDatum = _iRacingSDK.Data.TelemetryDataProperties["SessionState"];

            datumsInitialized = true;
        }

        private void OnDisconnected()
        {
            hasTelemetry = false;
        }

        private void OnStopped()
        {
            hasTelemetry = false;
        }

        /// <summary>
        /// Handle update of telemetry. That means update the data that can be retrieved with calls to _iRacingSDK.Data.GetXXX 
        /// (telemetry as opposed to the session data updated below in OnSessionInfo)
        /// </summary>
        /// The telemetry variables are documented here: https://sajax.github.io/irsdkdocs/telemetry/
        private void OnTelemetryData()
        {
            if (!_iRacingSDK.IsConnected && _iRacingSDK.IsStarted)
            {
                return;
            }
            if (_iRacingSDK.Data.SessionInfo == null)
            {
                Debug.WriteLine("No session info");
                return;
            }

            if (SessionData.Instance.Cars.Count == 0 || _iRacingSDK.Data.SessionInfo.DriverInfo == null)
            {
                Debug.WriteLine("No SessionData.Instance.Cars or DriverInfo");
                return;
            };

            InitDatums();

            try
            {
                // for each class, the time to get to the track position for the leader in that class
                Dictionary<string, float> classLeaderTrackPositionTimeDict = new Dictionary<string, float>();

                for (var iRSDKindex = 0; iRSDKindex < _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers.Count; iRSDKindex++)
                {
                    DriverModel driverModel = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[iRSDKindex];
                    if (driverModel.CarIsPaceCar > 0) continue;

                    CarInfo carInfo = GetCarInfo(driverModel.CarIdx);
                    if (carInfo == null)
                    {
                        Debug.WriteLine("Car not found with CarIdx {0}", driverModel.CarIdx);
                        continue;
                    }
                    carInfo.Position = _iRacingSDK.Data.GetInt(carIdxPositionDatum, iRSDKindex);
                    carInfo.CupPosition = _iRacingSDK.Data.GetInt(carIdxClassPositionDatum, iRSDKindex);

                    carInfo.TrackPercentCompleted = _iRacingSDK.Data.GetFloat(carIdxLapDistPctDatum, iRSDKindex);

                    // Track surface: NotInWorld, OffTrack, InPitStall, AproachingPits, OnTrack
                    // TODO: we can still distinguish between CarLocationEnum.Pitlane/PitEntry/Exit
                    TrkLoc trackSurface = (TrkLoc)_iRacingSDK.Data.GetInt(carIdxTrackSurfaceDatum, iRSDKindex);
                    if (_iRacingSDK.Data.GetBool(carIdxOnPitRoadDatum, iRSDKindex))
                    {
                        carInfo.CarLocation = CarInfo.CarLocationEnum.Pitlane;
                    }
                    else if (trackSurface == TrkLoc.NotInWorld)
                    {
                        carInfo.CarLocation = CarInfo.CarLocationEnum.NONE;
                    }
                    else
                    {
                        carInfo.CarLocation = CarInfo.CarLocationEnum.Track;
                    }

                    carInfo.CurrentDriverIndex = 0;
                    carInfo.LapIndex = _iRacingSDK.Data.GetInt(carIdxLapDatum, iRSDKindex);


                    LapInfo lapInfo = new LapInfo();
                    carInfo.CurrentLap = lapInfo;
                    if (trackSurface == TrkLoc.OffTrack)
                    {
                        // TODO: we need to reset this for other cars. Right now we only do so for player's car
                        carInfo.CurrentLap.IsInvalid = true;
                    }

                    // "CarIdxF2Time: Race time behind leader or fastest lap time otherwise"
                    // "CarIdxEstTime":  Estimated time to reach current location on track
                    //    f2time is 0 until the driver has done a (valid?) lap. So we use CarIdxEstTime to get the time it should take a player
                    //    to get to the current position on a track
                    float trackPositionTime = _iRacingSDK.Data.GetFloat(carIdxEstTimeDatum, iRSDKindex);
                    if (!classLeaderTrackPositionTimeDict.ContainsKey(carInfo.CarClass) || classLeaderTrackPositionTimeDict[carInfo.CarClass] > trackPositionTime)
                    {
                        classLeaderTrackPositionTimeDict[carInfo.CarClass] = trackPositionTime;
                    }
                    carInfo.GapToRaceLeaderMs = (int)(trackPositionTime * 1000.0);

                    carInfo.GapToPlayerMs = GetGapToPlayerMs(iRSDKindex, SessionData.Instance.PlayerCarIndex);
                }

                // determine the gaps for each car to the class leader
                for (var index = 0; index < SessionData.Instance.Cars.Count; index++)
                {
                    CarInfo carInfo = SessionData.Instance.Cars[index].Value;
                    var position = _iRacingSDK.Data.GetInt(carIdxClassPositionDatum, carInfo.CarIndex);
                    float f2Time = _iRacingSDK.Data.GetFloat(carIdxF2TimeDatum, carInfo.CarIndex);
                    if (position <= 0) continue;

                    carInfo.GapToClassLeaderMs = 0;
                    // special case for multi-class qualifying
                    if (classLeaderTrackPositionTimeDict.ContainsKey(carInfo.CarClass))
                    {
                        carInfo.GapToClassLeaderMs = (int)((classLeaderTrackPositionTimeDict[carInfo.CarClass] - f2Time) * 1000.0);
                    }
                }

                // DEBUG PrintAllCarInfo();

                // fill player's car from telemetry
                LocalCarData localCar = SimDataProvider.LocalCar;
                int playerCarIdx = _iRacingSDK.Data.SessionInfo.DriverInfo.DriverCarIdx;
                CarInfo playerCarInfo = SessionData.Instance.Cars[playerCarIdx].Value;
                localCar.Race.GlobalPosition = _iRacingSDK.Data.GetInt(playerCarPositionDatum);

                localCar.Engine.FuelLiters = _iRacingSDK.Data.GetFloat(fuelLevelDatum);
                int lapIndex = playerCarInfo.LapIndex;
                // check if we completed a lap
                if (lapIndex > lastLapIndex)
                {
                    lastLapFuelConsumption = lastLapFuelLevelLiters - localCar.Engine.FuelLiters;
                    lastLapFuelLevelLiters = localCar.Engine.FuelLiters;
                    playerCarInfo.CurrentLap.IsInvalid = false;
                    lastLapIndex = lapIndex;
                    Debug.WriteLine("new lap lastLapFuelConsumption {0} lastLapFuelLevelLiters {1}", lastLapFuelConsumption, lastLapFuelLevelLiters);
                }

                localCar.Engine.Rpm = (int)_iRacingSDK.Data.GetFloat(rPMDatum);
                // m/s -> km/h
                localCar.Physics.Velocity = _iRacingSDK.Data.GetFloat(speedDatum) * 3.6f;
                localCar.Physics.Rotation = Quaternion.CreateFromYawPitchRoll(_iRacingSDK.Data.GetFloat(yawNorthDatum), _iRacingSDK.Data.GetFloat(pitchDatum), _iRacingSDK.Data.GetFloat(rollDatum));

                localCar.Race.GlobalPosition = _iRacingSDK.Data.GetInt(playerCarPositionDatum);

                localCar.Inputs.Gear = _iRacingSDK.Data.GetInt(gearDatum) + 1;
                localCar.Inputs.Brake = _iRacingSDK.Data.GetFloat(brakeDatum);
                localCar.Inputs.Throttle = _iRacingSDK.Data.GetFloat(throttleDatum);
                localCar.Inputs.Steering = _iRacingSDK.Data.GetFloat(steeringWheelAngleDatum);


                SessionData.Instance.LapDeltaToSessionBestLapMs = _iRacingSDK.Data.GetFloat(lapDeltaToSessionBestLapDatum);

                SessionData.Instance.Weather.AirTemperature = _iRacingSDK.Data.GetFloat(airTempDatum);
                SessionData.Instance.Weather.AirVelocity = _iRacingSDK.Data.GetFloat(windVelDatum) * 3.6f;
                SessionData.Instance.Weather.AirDirection = _iRacingSDK.Data.GetFloat(windDirDatum);

                SessionData.Instance.Track.Temperature = _iRacingSDK.Data.GetFloat(trackTempCrewDatum);

                // Fuel telemetry and fuel consumption calc. This is using the last lap
                var fuelLevelPercent = _iRacingSDK.Data.GetFloat(fuelLevelPctDatum);
                localCar.Engine.MaxFuelLiters = _iRacingSDK.Data.SessionInfo.DriverInfo.DriverCarFuelMaxLtr;

                // TODO: iRacing gives unreasonable values for fuelUseKgPerHour. At least off by a factor 10
                // We keep track of fuel usage in the last lap until this is worked out.
                /* var fuelUseKgPerHour = _iRacingSDK.Data.GetFloat("FuelUsePerHour");
                float fuelKgPerLtr = _iRacingSDK.Data.SessionInfo.DriverInfo.DriverCarFuelKgPerLtr;
                float lapsPerHour = (float)TimeSpan.FromMinutes(60).TotalMilliseconds /
                    ((float) SessionData.Instance.Cars[SessionData.Instance.PlayerCarIndex].Value.LastLap.LaptimeMS);
                float fuelKgPerLap = fuelUseKgPerHour / lapsPerHour;
                float fuelLitersXLap = fuelKgPerLap / fuelKgPerLtr;
                Debug.WriteLine("Fuel kgPerLr {0} KgPerHour {1} lapsPerHour {2} kgPerLap {3} LitersXLap {4} ", fuelKgPerLtr, fuelUseKgPerHour, lapsPerHour, fuelKgPerLap, fuelLitersXLap);
                */

                localCar.Engine.FuelLitersXLap = lastLapFuelConsumption;
                localCar.Engine.FuelEstimatedLaps = localCar.Engine.FuelLiters / localCar.Engine.FuelLitersXLap;

                // ABS. We don't seem to get any other info for localCar.Electronics such as TC and engine map.
                // There is brake bias in CarSetupModel, but it is unclear if that would be updated when the user changes it when driving.
                localCar.Electronics.AbsActivation = _iRacingSDK.Data.GetBool(brakeABSactiveDatum) ? 1.0F : 0.0F;

                // TODO : public int DriverIncidentCount { get; set; } and other incident info (CurDriverIncidentCount , TeamIncidentCount, ..)

                int sessionNumber = _iRacingSDK.Data.GetInt(sessionNumDatum);
                var sessionType = _iRacingSDK.Data.SessionInfo.SessionInfo.Sessions[sessionNumber].SessionType;
                switch (sessionType)
                {
                    case "Race":
                        SessionData.Instance.SessionType = RaceSessionType.Race; break;
                    case "Practice":
                    case "Warmup": // Warmup is sort of like practice. We only need to distinguish of we want different HUD behavior
                        SessionData.Instance.SessionType = RaceSessionType.Practice; break;
                    case "Qualify":
                    case "Lone Qualify":
                    case "Open Qualify":
                        SessionData.Instance.SessionType = RaceSessionType.Qualifying; break;
                    default:
                        Debug.WriteLine("Unknown session type " + sessionType);
                        SessionData.Instance.SessionType = RaceSessionType.Race; break;
                }

                IRacingSdkEnum.SessionState sessionState = (SessionState)_iRacingSDK.Data.GetInt(sessionStateDatum);
                switch (sessionState)
                {
                    case SessionState.GetInCar:
                    case SessionState.Warmup:
                        SessionData.Instance.Phase = SessionPhase.PreSession;
                        break;
                    case SessionState.ParadeLaps:
                        SessionData.Instance.Phase = SessionPhase.FormationLap;
                        break;
                    case SessionState.Checkered:
                    case SessionState.CoolDown:
                        SessionData.Instance.Phase = SessionPhase.SessionOver;
                        break;
                    case SessionState.Racing:
                        SessionData.Instance.Phase = SessionPhase.Session;
                        break;
                    default:
                        Debug.WriteLine("Unknow session state " + sessionState);
                        break;
                }


                if (sessionNumber != lastSessionNumber)
                {
                    Debug.WriteLine("session change. curr# {0} last# {1} new state {2} type {3}", sessionNumber, lastSessionNumber, sessionState, sessionType);
                    SimDataProvider.CallSessionTypeChanged(this, SessionData.Instance.SessionType);
                    SimDataProvider.CallSessionPhaseChanged(this, SessionData.Instance.Phase);
                    lastSessionNumber = sessionNumber;
                }
                SessionData.Instance.SessionTimeLeftSecs = _iRacingSDK.Data.GetDouble(sessionTimeRemainDatum);
                // TODO more session info SessionLapsRemain, SessionLapsRemainEx, SessionTimeTotal, SessionLapsTotal, SessionTimeOfDay

                SpotterCallout = (CarLeftRight)_iRacingSDK.Data.GetInt(carLeftRightDatum);

                hasTelemetry = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString);
            }
        }

        /// <summary>
        /// Called when a session changes.
        /// </summary>
        /// This updates all data from _iRacingSDK.Data.SessionInfo.* (as opposed to telemetry's Data.GetXXX data above).
        /// The data is coming from a YAML string described here: https://sajax.github.io/irsdkdocs/yaml/
        /// 
        /// This will be called by IRSDKSharper only if session data changed, which will be much less frequently than 
        /// the telemetry
        /// 
        /// Sets the global state about session. e.g.
        /// - SessionData.Instance.*
        /// - SessionData.Instance.*
        /// - carClasses field

        private void OnSessionInfo()
        {
            // Debug.WriteLine("OnSessionInfo\n{0}", _iRacingSDK.Data.SessionInfoYaml);

            int sessionNumber = _iRacingSDK.Data.GetInt("SessionNum");
            SessionModel session = _iRacingSDK.Data.SessionInfo.SessionInfo.Sessions[sessionNumber];
            if (session.ResultsPositions == null)
            {
                Debug.WriteLine("No session or results info");
                return;
            }

            int playerCarIdx = _iRacingSDK.Data.SessionInfo.DriverInfo.DriverCarIdx;
            SessionData.Instance.PlayerCarIndex = playerCarIdx;
            SessionData.Instance.FocusedCarIndex = playerCarIdx; // TODO: we don't have a mechanism yet to set the focussed driver. iRacing has https://sajax.github.io/irsdkdocs/telemetry/camcaridx.html
                                                                 // Does that give the spectating car? The doc seems to list available telemetry too.

            string TrackLengthText = _iRacingSDK.Data.SessionInfo.WeekendInfo.TrackLength; // e.g. "3.70 km"
            string[] parts = TrackLengthText.Split(' ');
            SessionData.Instance.Track.Length = (int)(double.Parse(parts[0]) * 1000); // convert to meters
            // TODO: we can get the sectors and their start and endpoints (in track%) from the session info. struct is  "SplitTimeInfoModel".

            LocalCarData localCar = SimDataProvider.LocalCar;
            localCar.Engine.MaxRpm = (int)_iRacingSDK.Data.SessionInfo.DriverInfo.DriverCarSLLastRPM;

            DriverModel driverModel = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[SessionData.Instance.PlayerCarIndex];
            localCar.Race.CarNumber = driverModel.CarNumberRaw;
            localCar.CarModel.CarClass = driverModel.CarClassShortName != null ? driverModel.CarClassShortName : driverModel.CarScreenNameShort;
            localCar.CarModel.GameName = driverModel.CarScreenNameShort;

            // TODO: pit limiter doesn't seem to work properly
            // EngineWarnings.PitSpeedLimiter.HasFlag(EngineWarnings.PitSpeedLimiter)
            localCar.Engine.IsPitLimiterOn = false;

            SessionData.Instance.Track.GameName = _iRacingSDK.Data.SessionInfo.WeekendInfo.TrackName;


            Dictionary<int, int> carIdToCarArrayIndex = new Dictionary<int, int>();
            for (var index = 0; index < _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers.Count; index++)
            {
                driverModel = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index];
                if (driverModel.CarIsPaceCar > 0) continue;
                carIdToCarArrayIndex[driverModel.CarIdx] = index;
                // Debug.WriteLine("Drivers carIdx {0} carNumber {1} name {2}", driverModel.CarIdx, driverModel.CarNumberRaw, driverModel.UserName);
            }

            // dictionary from carIdx to results index
            Dictionary<int, int> carIndexResults = new Dictionary<int, int>();

            for (int resultsPosIndex = 0; resultsPosIndex < session.ResultsPositions.Count; resultsPosIndex++)
            {
                PositionModel result = session.ResultsPositions[resultsPosIndex];
                carIndexResults.Add(result.CarIdx, resultsPosIndex);
                /*
                if (!carIdToCarArrayIndex.ContainsKey(result.CarIdx))
                {
                    Debug.WriteLine("DriverInfo does not contain carIdx {0} ", result.CarIdx);
                }
                Debug.WriteLine("resultinfo carIdx {0} classPosition {1}", result.CarIdx, result.ClassPosition);*/
            }
            /*
            for (var index = 0; index < _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers.Count; index++)
            {
                driverModel = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index];
                if (carIndexResults.ContainsKey(driverModel.CarIdx)) {
                    Debug.WriteLine("resultModel does not contain carIdx {0} name {1}", driverModel.CarIdx, driverModel.UserName);
                }
            }*/

            for (var index = 0; index < _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers.Count; index++)
            {
                driverModel = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index];
                if (driverModel.CarIsPaceCar > 0) continue;

                CarInfo carInfo = TryAddDriver(driverModel);
                carInfo.RaceNumber = driverModel.CarNumberRaw;

                if (carIndexResults.ContainsKey(index))
                {
                    LapInfo lapInfo = new LapInfo();
                    PositionModel postion = session.ResultsPositions[carIndexResults[index]];
                    lapInfo.LaptimeMS = (int?)(postion.FastestTime * 1000.0F);
                    carInfo.FastestLap = lapInfo;

                    lapInfo = new LapInfo();
                    lapInfo.LaptimeMS = (int?)(postion.LastTime * 1000.0F);
                    carInfo.LastLap = lapInfo;
                } /* else
                {
                    Debug.WriteLine("carIndexResults does not contain carArrayINdex {0} carIdx {1}", index, driverModel.CarIdx);
                }    */

                // For multi-make classes like GT4/GT3, we use CarClassShortName otherwise (e.g. MX5 or GR86) we use CarScreenNameShort
                carInfo.CarClass = driverModel.CarClassShortName;
                if (carInfo.CarClass == null)
                {
                    carInfo.CarClass = driverModel.CarScreenNameShort;
                }
                carInfo.IsSpectator = driverModel.IsSpectator == 1;

                AddCarClassEntry(carInfo.CarClass, driverModel.CarClassColor);

                // TODO: add qualifying time info
            }
        }

        private CarInfo GetCarInfo(int carIndex)
        {

            if (SessionData.Instance.Cars.Count > carIndex)
            {
                return SessionData.Instance.Cars[carIndex].Value;

            }
            return null;
        }


        private CarInfo TryAddDriver(DriverModel driverModel)
        {
            CarInfo carInfo;
            if (SessionData.Instance.Cars.Count > driverModel.CarIdx)
            {
                carInfo = SessionData.Instance.Cars[driverModel.CarIdx].Value;
            }
            else
            {
                carInfo = new CarInfo(driverModel.CarIdx);
                SessionData.Instance.AddOrUpdateCar(driverModel.CarIdx, carInfo);

                DriverInfo driver = new DriverInfo();
                // TODO: it looks like this might change in a team race when the driver changes. We need to test this with a team race at some point.
                // None of the currently ported HUDs do want to display the non-driving drivers in the team anyway.

                driver.Name = driverModel.UserName;
                driver.Rating = driverModel.IRating;
                // LicString is "<class> <SR>".
                driver.Category = driverModel.LicString;
                carInfo.AddDriver(driver);
            }
            return carInfo;
        }

        // for debugging
        private void PrintAllCarInfo()
        {

            for (var index = 0; index < SessionData.Instance.Cars.Count; index++)
            {
                CarInfo carInfo = SessionData.Instance.Cars[index].Value;
                Debug.WriteLine("Car " + index + " #" + carInfo.RaceNumber + " " + carInfo.Drivers[0].Name + " pos: " + carInfo.CupPosition + " GL: " + carInfo.GapToClassLeaderMs + " GP:" + carInfo.GapToPlayerMs);
            }
        }

        public override void SetupPreviewData()
        {
            SessionData.Instance.FocusedCarIndex = 1;
            SessionData.Instance.PlayerCarIndex = 1;

            SessionData.Instance.Track.GameName = "Spa";
            SessionData.Instance.Track.Length = 7004;
            SessionData.Instance.Track.Temperature = 21;

            SimDataProvider.LocalCar.CarModel.CarClass = "F1";

            var lap1 = new LapInfo();
            lap1.Splits = [10000, 22000, 11343];
            lap1.LaptimeMS = lap1.Splits.Sum();
            var lap2 = new LapInfo();
            lap2.Splits = [9000, 22000, 11343];
            lap2.LaptimeMS = lap2.Splits.Sum();

            var car1 = new CarInfo(1);
            car1.TrackPercentCompleted = 10.0f;
            car1.Position = 1;
            car1.CarLocation = CarInfo.CarLocationEnum.Track;
            car1.CurrentDriverIndex = 0;
            car1.Kmh = 140;
            car1.CupPosition = 1;
            car1.RaceNumber = 17;
            car1.LastLap = lap1;
            car1.FastestLap = lap1;
            car1.CurrentLap = lap2;
            car1.GapToClassLeaderMs = 0;
            car1.CarClass = "F1";
            car1.Laps = 11;
            SessionData.Instance.AddOrUpdateCar(1, car1);
            var car1driver0 = new DriverInfo();
            car1driver0.Name = "Max Verstappen";
            car1driver0.Rating = 7123;
            car1driver0.Category = "A 2.7";
            car1.AddDriver(car1driver0);
            SimDataProvider.LocalCar.Engine.FuelEstimatedLaps = 3;
            SimDataProvider.LocalCar.Engine.FuelLiters = 26.35f;


            CarInfo car2 = new CarInfo(2);
            // 1 meter behind car1
            car2.TrackPercentCompleted = car1.TrackPercentCompleted + (1.0F / ((float)SessionData.Instance.Track.Length));
            car2.Position = 2;
            car2.CarLocation = CarInfo.CarLocationEnum.Track;
            car2.CurrentDriverIndex = 0;
            car2.Kmh = 160;
            car2.CupPosition = 2;
            car2.RaceNumber = 5;
            car2.LastLap = lap2;
            car2.FastestLap = lap2;
            car2.CurrentLap = lap1;
            car2.GapToClassLeaderMs = 1000;
            car2.GapToPlayerMs = 100;
            car2.Laps = 10;
            car2.CarClass = "F1";
            SessionData.Instance.AddOrUpdateCar(2, car2);
            var car2driver0 = new DriverInfo();
            car2driver0.Name = "Michael Schumacher";
            car2driver0.Rating = 8123;
            car2driver0.Category = "A 2.2";
            car2.AddDriver(car2driver0);




            CarInfo car3 = new CarInfo(3);
            // 10 meter behind car1
            car3.TrackPercentCompleted = car1.TrackPercentCompleted + (-10.0F / ((float)SessionData.Instance.Track.Length));
            car3.Position = 2;
            car3.CarLocation = CarInfo.CarLocationEnum.Track;
            car3.CurrentDriverIndex = 0;
            car3.Kmh = 160;
            car3.CupPosition = 2;
            car3.RaceNumber = 7;
            car3.LastLap = lap2;
            car3.FastestLap = lap2;
            car3.CurrentLap = lap1;
            car3.GapToClassLeaderMs = 1000;
            car3.GapToPlayerMs = 200;
            car3.CarClass = "F1";
            car3.Laps = 12;
            SessionData.Instance.AddOrUpdateCar(3, car3);
            var car3driver0 = new DriverInfo();
            car3driver0.Name = "Lewis Hamilton";
            car3driver0.Rating = 6123;
            car3driver0.Category = "B 2.7";
            car3.AddDriver(car3driver0);

            AddCarClassEntry("F1", "0xffffff");

            SpotterCallout = CarLeftRight.CarLeft;

            hasTelemetry = true; // TODO: will this work when we have real telemetry? And is this sample telemetry valid for all sim providers?
        }

        // Gap calculation according to https://github.com/lespalt/iRon
        // TODO:  Gap should be showing in the shortest direction. e.g. -15sec instead of +50sec
        private int GetGapToPlayerMs(int index, int playerCarIdx)
        {
            float bestForPlayer = _iRacingSDK.Data.GetFloat("CarIdxBestLapTime", playerCarIdx); // TODO: this might not work if the driver is out of e.g. practice
            if (bestForPlayer == 0)
                bestForPlayer = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[playerCarIdx].CarClassEstLapTime;

            float C = _iRacingSDK.Data.GetFloat("CarIdxEstTime", index);
            float S = _iRacingSDK.Data.GetFloat("CarIdxEstTime", playerCarIdx);

            // Does the delta between us and the other car span across the start/finish line?
            bool wrap = Math.Abs(_iRacingSDK.Data.GetFloat("CarIdxLapDistPct", index) - _iRacingSDK.Data.GetFloat("CarIdxLapDistPct", playerCarIdx)) > 0.5f;
            float delta;
            if (wrap)
            {
                delta = S > C ? (C - S) + bestForPlayer : (C - S) - bestForPlayer;
                // lapDelta += S > C ? -1 : 1;
            }
            else
            {
                delta = C - S;
            }
            return (int)(delta * 1000);
        }

        // Gap to player ahead according to  https://github.com/LEMPLS/iracing-companion-server/blob/af4ad01325f74fc81326eaad6ae986231fecaf9e/src/index.js#L98C1-L106C61
        // This is independent of classes
        private float GetGapToPlayerAheadMs(int index, int playerCarIdx)
        {

            // use position independent of classes
            int playerCarPosition = SessionData.Instance.Cars[playerCarIdx].Value.Position;
            int carAheadIdx = SessionData.Instance.Cars[playerCarPosition - 1].Value.CarIndex;

            var playerCarF2Time = _iRacingSDK.Data.GetFloat("CarIdxF2Time", playerCarIdx);
            var carAheadF2Time = _iRacingSDK.Data.GetFloat("CarIdxF2Time", (int)carAheadIdx);

            return playerCarF2Time - carAheadF2Time;
        }


        internal override void Stop()
        {
            _iRacingSDK?.Stop();
            hasTelemetry = false;
        }


        public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
        {
            gameData.Name = Game.iRacing.ToShortName();
            // Updates for iRacing are done with event handlers and don't need to be driven by Race Element with this Update method
        }

        public override Color GetColorForCarClass(String carClass)
        {
            return carClassColor[carClass];
        }
        public override List<string> GetCarClasses() { return carClasses.ToList(); }

        public override bool HasTelemetry()
        {
            return hasTelemetry;
        }

        private void AddCarClassEntry(string carClass, string aCarClassColor)
        {
            carClasses.Add(carClass);
            carClassColor.TryAdd(carClass, MapCarClassColor(aCarClassColor));
        }

        private Color MapCarClassColor(string carClassColor)
        {
            // Remove the leading '0x' if present
            if (carClassColor.StartsWith("0x"))
            {
                carClassColor = carClassColor.Substring(2);
            }

            // Convert hex string to Color
            Color color = ColorTranslator.FromHtml("#" + carClassColor);
            if (color == null) return Color.White;
            return color;
        }

        public CarLeftRight GetSpotterCallout()
        {
            return SpotterCallout;
        }

        override public bool IsSpectating(int playerCarIndex, int focusedIndex)
        {
            // TODO We need to test how spotting team mates works in a multi-driver team race.
            // E.g. what telemetry is available
            return false;
        }

        /// <summary>
        /// iRacing license class to color mapping.
        /// </summary>
        public override Color GetColorForCategory(string category)
        {
            if (category.StartsWith("A"))
            {
                return Color.Blue;
            }
            else if (category.StartsWith("B"))
            {
                return Color.Green;
            }
            else if (category.StartsWith("C"))
            {
                return Color.Yellow;
            }
            else if (category.StartsWith("D"))
            {
                return Color.Orange;
            }
            else if (category.StartsWith("R"))
            {
                return Color.Red;
            }
            else
            {
                Debug.WriteLine("Unknown license {0}", category);
                return Color.Gray;
            }
        }
    }


}
