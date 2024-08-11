using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing.SDK;
using System.Diagnostics;
using static RaceElement.Data.Games.iRacing.SDK.IRacingSdkSessionInfo.DriverInfoModel;
using RaceElement.Data.Common;
using System.Reflection;
using System.Drawing;
using System;

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
                - skip pace car from standings
                - skip re-reading stuff that's static for session (e.g. drivers).
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

                for (var index = 0; index < IRacingSdkConst.MaxNumCars; index++)
                {
                    var position = _iRacingSDK.Data.GetInt("CarIdxClassPosition", index);
                    if (position == 0) continue;
                    var carInfo = new CarInfo((uint)index);
                    carInfo.Position = position;
                    carInfo.CupPosition = _iRacingSDK.Data.GetInt("CarIdxPosition", index);
                    carInfo.RaceNumber = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index].CarNumberRaw;
                    carInfo.TrackPercentCompleted = _iRacingSDK.Data.GetFloat("CarIdxLapDistPct", index);

                    // TODO: CarIdxTrackSurface irsdk_TrkLoc allows also for offtrack, pit stall, approaching pits
                    carInfo.CarLocation = _iRacingSDK.Data.GetBool("CarIdxOnPitRoad", index) ? CarInfo.CarLocationEnum.Pitlane : CarInfo.CarLocationEnum.Track;
                    carInfo.CurrentDriverIndex = 0; // TODO
                    LapInfo lapInfo = new LapInfo();
                    lapInfo.LaptimeMS = (int)(_iRacingSDK.Data.GetFloat("CarIdxLastLapTime", index) * 1000);
                    carInfo.LastLap = lapInfo;
                    carInfo.CarClass = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index].CarScreenNameShort;
                    carInfo.IsSpectator = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index].IsSpectator == 1;

                    // TODO: Does this mean gap to class leader? "CarIdxF2Time: Race time behind leader or fastest lap time otherwise"
                    carInfo.GapToClassLeaderMs = _iRacingSDK.Data.GetFloat("CarIdxF2Time", index) * 1000;
                    // "CarIdxEstTime float Estimated time to reach current location on track" 
                    float estimateLaptime = _iRacingSDK.Data.GetFloat("CarIdxEstTime", index) * 1000;
                   

                    // TODO We need to calculate this with the estimated lap time and track percent completed. Compare each driver with the player's car
                    // carInfo.GapToPlayerMs = 
                    
                    string currCarClassColor = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index].CarClassColor;
                    AddCarClassEntry(_iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[index].CarScreenNameShort, currCarClassColor);                   

                    DriverInfo driver = new DriverInfo();
                    if (driverDict.ContainsKey(index))
                    {
                        DriverModel currDriverModel = driverDict[index];
                        driver.FirstName = currDriverModel.UserName;
                        driver.LastName = currDriverModel.UserName;
                        carInfo.AddDriver(driver);
                        driver.Rating = currDriverModel.IRating;
                        driver.Category = currDriverModel.LicLevel.ToString();
                        // TODO driver.SafetyRating = driverModel.
                        // TODO IRating, LicColor, LicLevel, LicString, LicSubLevel                        
                    }

                    SessionData.Instance.AddOrUpdateCar(index, carInfo);
                }

                // fill player's car from session info
                LocalCarData localCar = SimDataProvider.LocalCar;
                var sessionData = SimDataProvider.Session;
                DriverModel driverModel = _iRacingSDK.Data.SessionInfo.DriverInfo.Drivers[playerCarIdx];
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

                sessionData.SessionType = RaceSessionType.Race; // TODO
                sessionData.Phase = SessionPhase.Session; // TODO            

                // TODO: pit limiter doesn't seem to work properly
                // EngineWarnings.PitSpeedLimiter.HasFlag(EngineWarnings.PitSpeedLimiter)
                localCar.Engine.IsPitLimiterOn = false;

                hasTelemetry = true;
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString);
            }
        }

        internal override void Stop()
        {
            _iRacingSDK?.Stop();
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
            carClassColor.TryAdd(carClass, Color.AliceBlue); // TODO: map string->Color instead of hardcoded
        }
    }
}
