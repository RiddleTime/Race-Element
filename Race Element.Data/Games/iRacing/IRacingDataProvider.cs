using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing.SDK;
using System.Diagnostics;
using static RaceElement.Data.Games.iRacing.SDK.IRacingSdkSessionInfo.DriverInfoModel;
using RaceElement.Data.Common;
using System.Drawing;
using static RaceElement.Data.Games.iRacing.SDK.IRacingSdkEnum;

// https://github.com/mherbold/IRSDKSharper
// https://sajax.github.io/irsdkdocs/telemetry/
// https://members-login.iracing.com/?ref=https%3A%2F%2Fmembers-ng.iracing.com%2Fdata%2Fdoc&signout=true (access to results data from iRacing.com. needs credentials)
// https://us.v-cdn.net/6034148/uploads/8DD84H30FIC8/telemetry-11-23-15.pdf Official doc
namespace RaceElement.Data.Games.iRacing
{
    internal class IRacingDataProvider : AbstractSimDataProvider
    {
        Dictionary<string, Color> carClassColor = [];
        HashSet<string> carClasses = null;

        bool hasTelemetry = false;

        private IRSDKSharper _iRacingSDK;
        public IRacingDataProvider() {                    
            if (_iRacingSDK == null)
            {
                _iRacingSDK = new IRSDKSharper
                {
                    UpdateInterval = 1000 / 50
                };
                _iRacingSDK.OnTelemetryData += OnTelemetryData;                    
                _iRacingSDK.Start();
            }                
        }    

        private void OnTelemetryData()
        {         
           if (!_iRacingSDK.IsConnected && _iRacingSDK.IsStarted) {
            return;
           }           
           if (_iRacingSDK.Data.SessionInfo.DriverInfo == null) {
            Debug.WriteLine("No driver info");
            return;
           };

            try
            {
                int playerCarIdx = _iRacingSDK.Data.SessionInfo.DriverInfo.DriverCarIdx;
                SessionData.Instance.FocusedCarIndex = playerCarIdx;
                string TrackLengthText = _iRacingSDK.Data.SessionInfo.WeekendInfo.TrackLength; // e.g. "3.70 km"
                string[] parts = TrackLengthText.Split(' ');
                SessionData.Instance.Track.Length = (int)(double.Parse(parts[0]) * 1000); // convert to meters


                /* TODO:                
                    - see if we can skip re-reading stuff that's static for session
                */
                Dictionary<int, DriverModel> driverDict = [];
                List<DriverModel> drivers = null;
                if (_iRacingSDK.Data.SessionInfo != null & _iRacingSDK.Data.SessionInfo.DriverInfo != null)
                {
                    drivers = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers;
                    if (drivers != null)
                    {
                        for (int dr = 0; dr < drivers.Count; dr++)
                        {
                            driverDict.Add(drivers[dr].CarIdx, drivers[dr]);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("No session drivers");
                    return;
                }

                // for each class, the "CarIdxF2Time" of the leader in that class
                Dictionary<string, float> classLeaderF2TimeDict = new Dictionary<string, float>();
                DriverModel driverModel;
                for (var index = 0; index < IRacingSdkConst.MaxNumCars; index++)
                {
                    var position = _iRacingSDK.Data.GetInt("CarIdxPosition", index);
                    if (position <= 0 || position > IRacingSdkConst.MaxNumCars) continue;
                    driverModel = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index];
                    var carInfo = new CarInfo((uint)index);
                    carInfo.Position = position;
                    carInfo.CupPosition = _iRacingSDK.Data.GetInt("CarIdxClassPosition", index);
                    carInfo.RaceNumber = driverModel.CarNumberRaw;
                    carInfo.TrackPercentCompleted = _iRacingSDK.Data.GetFloat("CarIdxLapDistPct", index);

                    // TODO: CarIdxTrackSurface irsdk_TrkLoc allows also for offtrack, pit stall, approaching pits
                    carInfo.CarLocation = _iRacingSDK.Data.GetBool("CarIdxOnPitRoad", index) ? CarInfo.CarLocationEnum.Pitlane : CarInfo.CarLocationEnum.Track;
                    carInfo.CurrentDriverIndex = 0; // TODO: we might remove this from carInfo if the telemetry don't give all drivers at once. Need to test with a team race. 
                    LapInfo lapInfo = new LapInfo();
                    lapInfo.LaptimeMS = (int)(_iRacingSDK.Data.GetFloat("CarIdxLastLapTime", index) * 1000.0);                    
                    carInfo.LastLap = lapInfo;
                    carInfo.CarClass = driverModel.CarScreenNameShort;
                    carInfo.IsSpectator = driverModel.IsSpectator == 1;
                    carInfo.LapIndex = _iRacingSDK.Data.GetInt("CarIdxLap", index);

                    lapInfo = new LapInfo();
                    float fl = _iRacingSDK.Data.GetFloat("CarIdxBestLapTime", index);
                    lapInfo.LaptimeMS = (int) (_iRacingSDK.Data.GetFloat("CarIdxBestLapTime", index) * 1000.0);
                    carInfo.FastestLap = lapInfo;                   

                    // "CarIdxF2Time: Race time behind leader or fastest lap time otherwise"
                    float f2Time = _iRacingSDK.Data.GetFloat("CarIdxF2Time", index);
                    if (!classLeaderF2TimeDict.ContainsKey(carInfo.CarClass) || classLeaderF2TimeDict[carInfo.CarClass] > f2Time)
                    {
                        classLeaderF2TimeDict[carInfo.CarClass] = f2Time;
                    }
                    // "CarIdxEstTime float Estimated time to reach current location on track" 
                    float estimateLaptime = _iRacingSDK.Data.GetFloat("CarIdxEstTime", index);
                    // Debug.WriteLine("position:" + position + " CarIdxF2Time:" + carInfo.GapToClassLeaderMs + " CarIdxEstTime:" + estimateLaptime);
                    
                    carInfo.GapToPlayerMs = GetGapToPlayerMs(index, playerCarIdx);

                    string currCarClassColor = driverModel.CarClassColor;
                    AddCarClassEntry(driverModel.CarScreenNameShort, currCarClassColor);                   

                    // TODO: it looks like this might change in a team race when the driver changes. We need to test this with a team race at some point.
                    DriverInfo driver = new DriverInfo();
                    if (driverDict.ContainsKey(index))
                    {
                        DriverModel currDriverModel = driverDict[index];
                        driver.Name = currDriverModel.UserName;                        
                        driver.Rating = currDriverModel.IRating;
                        // LicString is "<class> <SR>"
                        driver.Category = currDriverModel.LicString;
                        // TODO LicColor
                        carInfo.AddDriver(driver);                        
                    }

                    // TODO: add qualifying time info

                    SessionData.Instance.AddOrUpdateCar(index, carInfo);
                }

                // determine the gaps for each car to the class leader
                for (var index = 0; index < SessionData.Instance.Cars.Count; index++)
                {
                    var position = _iRacingSDK.Data.GetInt("CarIdxClassPosition", index);
                    float f2Time = _iRacingSDK.Data.GetFloat("CarIdxF2Time", index);
                    if (position <= 0) continue;
                    
                    CarInfo carInfo = SessionData.Instance.Cars[index].Value;
                    carInfo.GapToClassLeaderMs = (int)((classLeaderF2TimeDict[carInfo.CarClass] - f2Time) * 1000.0);                    
                }

                // DEBUG PrintAllCarInfo();

                // fill player's car from session info
                LocalCarData localCar = SimDataProvider.LocalCar;
                var sessionData = SimDataProvider.Session;
                driverModel = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[playerCarIdx];
                localCar.Race.CarNumber = driverModel.CarNumberRaw;
                localCar.Race.GlobalPosition = _iRacingSDK.Data.GetInt("PlayerCarPosition");
                localCar.CarModel.GameName = driverModel.CarScreenNameShort;

                localCar.Engine.MaxRpm = (int)_iRacingSDK.Data.SessionInfo.DriverInfo.DriverCarSLLastRPM;
                localCar.Engine.Rpm = (int)_iRacingSDK.Data.GetFloat("RPM");
                localCar.Physics.Velocity = _iRacingSDK.Data.GetFloat("Speed") * 3.6f;
                localCar.Race.GlobalPosition = _iRacingSDK.Data.GetInt("PlayerCarPosition");

                localCar.Inputs.Gear = _iRacingSDK.Data.GetInt("Gear") + 1;
                localCar.Inputs.Brake = _iRacingSDK.Data.GetFloat("Brake");
                localCar.Inputs.Throttle = _iRacingSDK.Data.GetFloat("Throttle");
                localCar.Inputs.Steering = _iRacingSDK.Data.GetFloat("SteeringWheelAngle");
                localCar.CarModel.CarClass = driverModel.CarScreenNameShort;

                // TODO: mark lap invalid. This seems to be only provided for player

                /* as per https://github.com/alexanderzobnin/grafana-simracing-telemetry/blob/8c008f01003502c687aa4e5278018b000b0a5eaf/pkg/iracing/sharedmemory/models.go#L193
                   we can add these to local car (not available for opponent cars)
                             Lap                             int32
                             LapCompleted                    int32
                             LapDist                         float32
                             LapDistPct                      float32
                             RaceLaps                        int32
                             LapBestLap                      int32
                             LapBestLapTime                  float32
                             LapLastLapTime                  float32
                             LapCurrentLapTime               float32
                             LapLasNLapSeq                   int32
                             LapLastNLapTime                 float32
                             LapBestNLapLap                  int32
                             LapBestNLapTime                 float32
                             LapDeltaToBestLap               float32
                             LapDeltaToBestLap_DD            float32
                             LapDeltaToBestLap_OK            bool
                             LapDeltaToOptimalLap            float32
                             LapDeltaToOptimalLap_DD         float32
                             LapDeltaToOptimalLap_OK         bool
                             LapDeltaToSessionBestLap        float32
                             LapDeltaToSessionBestLap_DD     float32
                             LapDeltaToSessionBestLap_OK     bool
                             LapDeltaToSessionOptimalLap     float32
                             LapDeltaToSessionOptimalLap_DD  float32
                             LapDeltaToSessionOptimalLap_OK  bool
                             LapDeltaToSessionLastlLap       float32
                             LapDeltaToSessionLastlLap_DD    float32
                             LapDeltaToSessionLastlLap_OK    bool
                         */

                    sessionData.Weather.AirTemperature = _iRacingSDK.Data.GetFloat("AirTemp");
                sessionData.Weather.AirVelocity = _iRacingSDK.Data.GetFloat("WindVel") * 3.6f;

                sessionData.Track.Temperature = _iRacingSDK.Data.GetFloat("TrackTempCrew");
                sessionData.Track.GameName = _iRacingSDK.Data.SessionInfo.WeekendInfo.TrackName;

                
                var sessionType = _iRacingSDK.Data.SessionInfo.SessionInfo.Sessions[0].SessionType;
                switch (sessionType)
                {
                    case "Race":
                        sessionData.SessionType = RaceSessionType.Race; break;
                    case "Practice":
                        sessionData.SessionType = RaceSessionType.Practice; break;
                    case "Qualify":
                        sessionData.SessionType = RaceSessionType.Qualifying; break;
                    default:
                        Debug.WriteLine("Uknown session type " + sessionType);
                        sessionData.SessionType = RaceSessionType.Race; break;
                }
                IRacingSdkEnum.SessionState sessionState = (SessionState)_iRacingSDK.Data.GetInt("SessionState");
                switch (sessionState)
                {
                    case SessionState.GetInCar:
                    case SessionState.Warmup:
                        sessionData.Phase = SessionPhase.PreSession;
                        break;
                    case SessionState.ParadeLaps:
                        sessionData.Phase = SessionPhase.FormationLap;
                        break;
                    case SessionState.Checkered:
                    case SessionState.CoolDown:
                        sessionData.Phase = SessionPhase.SessionOver;
                        break;
                    case SessionState.Racing:
                        sessionData.Phase = SessionPhase.Session;
                        break;
                    default:
                        Debug.WriteLine("Unknow session state " + sessionState);
                        break;
                }


                // TODO: pit limiter doesn't seem to work properly
                // EngineWarnings.PitSpeedLimiter.HasFlag(EngineWarnings.PitSpeedLimiter)
                localCar.Engine.IsPitLimiterOn = false;

                hasTelemetry = true;
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString);
            }
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

        // Gap calculation according to https://github.com/lespalt/iRon 
        private int GetGapToPlayerMs(int index, int playerCarIdx)
        {
            DriverModel driverModel = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index];
            
            float bestForPlayer = _iRacingSDK.Data.GetFloat("CarIdxBestLapTime", playerCarIdx);
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
            uint carAheadIdx = SessionData.Instance.Cars[playerCarPosition - 1].Value.CarIndex;

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
            try
            {
                gameData.Name = Game.iRacing.ToShortName();    
                if (!hasTelemetry)
                {
                    return;
                }
                OnTelemetryData(); // TODO: sometimes this is not called as the callback . So we do it explictly here for now.

            }
            catch (Exception)
            {
                // atm not leaning on iRacing sdk exceptions
            }

        }

        public override Color GetColorForCarClass(String carClass)
        {
            return carClassColor[carClass];
        }
        public override List<string> GetCarClasses() { return carClasses.ToList();}

        public override bool HasTelemetry()
        {
            return hasTelemetry;
        }        

        private void AddCarClassEntry(string carClass, string color)
        {
            if (carClasses == null)
            {
                carClasses = new HashSet<string>();
            }                        
            carClasses.Add(carClass);
            // TODO: map string->Color instead of hardcoded.  
            carClassColor.TryAdd(carClass, Color.AliceBlue); 
        }
    }
}
