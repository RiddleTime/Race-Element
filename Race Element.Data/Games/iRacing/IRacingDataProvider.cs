using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing.DataMapper;
using RaceElement.Data.Games.iRacing.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static RaceElement.Data.Games.iRacing.SDK.IRacingSdkEnum;

// https://github.com/mherbold/IRSDKSharper
// https://sajax.github.io/irsdkdocs/telemetry/
namespace RaceElement.Data.Games.iRacing
{
    internal static class IRacingDataProvider
    {
        private static IRSDKSharper _iRacingSDK;
        internal static IRSDKSharper IRacingSDK        
        {
            get
            {
                if (_iRacingSDK == null)
                {
                    _iRacingSDK = new IRSDKSharper
                    {
                        UpdateInterval = 1000 / 50
                    };
                    _iRacingSDK.Start();
                }
                return _iRacingSDK;
            }
        }

        internal static void Stop()
        {
            _iRacingSDK?.Stop();
        }

        internal static void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
        {
            try
            {
                gameData.Name = Game.iRacing.ToShortName();

                var iRacing = IRacingSDK;
                if (iRacing.Data.SessionInfo.DriverInfo == null) return;
                int carIndex = iRacing.Data.SessionInfo.DriverInfo.DriverCarIdx;

                var driverModel = iRacing.Data.SessionInfo.DriverInfo.Drivers.First(x => x.CarIdx == carIndex);
                if (driverModel != null)
                {
                    localCar.Race.CarNumber = driverModel.CarNumberRaw;
                    localCar.Engine.MaxRpm = (int)iRacing.Data.SessionInfo.DriverInfo.DriverCarSLLastRPM;
                    localCar.Engine.Rpm = (int)iRacing.Data.GetFloat("RPM");
                    localCar.CarModel.GameName = driverModel.CarScreenNameShort;
                    // TODO: find pit limiter flag in iRacing SDK                                           
                    //if (EngineWarnings.PitSpeedLimiter.HasFlag(EngineWarnings.PitSpeedLimiter)) {
                    //    Debug.WriteLine("pit limiter on");
                    //} else {
                        //Debug.WriteLine("Engine warnings " + EngineWarnings.PitSpeedLimiter.ToString());
                    //}
                    localCar.Engine.IsPitLimiterOn = false;
                }

                localCar.Physics.Velocity = iRacing.Data.GetFloat("Speed") * 3.6f;
                localCar.Race.GlobalPosition = iRacing.Data.GetInt("PlayerCarPosition");
                
                localCar.Inputs.Gear = iRacing.Data.GetInt("Gear") + 1;
                localCar.Inputs.Brake = iRacing.Data.GetFloat("Brake");
                localCar.Inputs.Throttle = iRacing.Data.GetFloat("Throttle");
                localCar.Inputs.Steering = iRacing.Data.GetFloat("SteeringWheelAngle");

                sessionData.Weather.AirTemperature = iRacing.Data.GetFloat("AirTemp");
                sessionData.Weather.AirVelocity = iRacing.Data.GetFloat("WindVel") * 3.6f;
                sessionData.Track.Temperature = iRacing.Data.GetFloat("TrackTempCrew");
                sessionData.Track.GameName = iRacing.Data.SessionInfo.WeekendInfo.TrackName;
                
            }
            catch (Exception)
            {
                // atm not leaning on iRacing sdk exceptions
            }
        }
    }
}
