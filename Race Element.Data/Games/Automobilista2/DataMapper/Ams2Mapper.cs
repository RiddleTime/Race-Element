using System.Diagnostics;
using RaceElement.Data.Games.Automobilista2.SharedMemory;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Common.SimulatorData;

namespace RaceElement.Data.Games.Automobilista2.DataMapper;

internal static class Ams2Mapper
{
    private static float _prevLapFuel;
    private static uint _prevLapCount;

    public static void ToLocalSession(Shared shared, SessionData session)
    {
        // Wheather
        session.Weather.AirDirection = (float)Math.Atan2(shared.mWindDirectionY, shared.mWindDirectionX);
        session.Weather.AirTemperature = shared.mAmbientTemperature;
        session.Weather.AirVelocity = shared.mWindSpeed;

        // Track
        session.Track.Length = (int)(shared.mTrackLength + 0.5f);
        session.Track.Temperature = shared.mTrackTemperature;
        session.Track.GameName = shared.mTranslatedTrackVariation.Data;

        // Others
        session.SessionTimeLeftSecs = shared.mEventTimeRemaining / 1000.0f;
        session.IsSetupMenuVisible = shared.mGameState == (int)Constants.GameState.GAME_INGAME_INMENU_TIME_TICKING;

        // Race session and phase type
        switch ((Constants.RaceSession)shared.mSessionState)
        {
            case Constants.RaceSession.SESSION_RACE:
            {
                session.SessionType = RaceSessionType.Race;
            } break;

            case Constants.RaceSession.SESSION_TEST:
            {
                session.SessionType = RaceSessionType.Practice;
            } break;

            case Constants.RaceSession.SESSION_QUALIFY:
            {
                session.SessionType = RaceSessionType.Qualifying;
            } break;

            case Constants.RaceSession.SESSION_TIME_ATTACK:
            {
                session.SessionType = RaceSessionType.Hotstint;
            } break;

            case Constants.RaceSession.SESSION_FORMATION_LAP:
            {
                session.Phase = SessionPhase.FormationLap;
            } break;

            default:
            {
                session.Phase = SessionPhase.NONE;
                session.SessionType = RaceSessionType.Practice;
            } break;
        }

        // Update drivers list
        for (int i = 0; i < shared.mNumParticipants; ++i)
        {
            var participant = shared.mParticipantInfo[i];

            var driver = new DriverInfo();
            driver.Name = participant.mName.Data;

            var carInfo = new CarInfo(i);
            carInfo.AddDriver(driver);

            carInfo.CarClass = shared.mCarNames[i].Data;

            carInfo.Position = (int)participant.mRacePosition;
            carInfo.Location = participant.mWorldPosition;

            carInfo.Kmh = (int)(shared.mSpeeds[i] * 3.6f);
            carInfo.Laps = (int)participant.mCurrentLap;

            {
                carInfo.CurrentLap = new();
                carInfo.CurrentLap.IsInvalid = shared.mLapsInvalidated[i] == 1;

                if (participant.mCurrentSector < 1 && shared.mCurrentSector1Times[i] > 0)
                {
                    carInfo.CurrentLap.Splits.Add((int)shared.mCurrentSector1Times[i]);
                }
                else if (participant.mCurrentSector < 2 && shared.mCurrentSector2Times[i] > 0)
                {
                    carInfo.CurrentLap.Splits.Add((int)shared.mCurrentSector1Times[i]);
                    carInfo.CurrentLap.Splits.Add((int)shared.mCurrentSector2Times[i]);
                }
                else if (participant.mCurrentSector < 3 && shared.mCurrentSector3Times[i] > 0)
                {
                    carInfo.CurrentLap.Splits.Add((int)shared.mCurrentSector1Times[i]);
                    carInfo.CurrentLap.Splits.Add((int)shared.mCurrentSector2Times[i]);
                    carInfo.CurrentLap.Splits.Add((int)shared.mCurrentSector3Times[i]);
                }
            }
            {
                carInfo.LastLap = new();
                carInfo.LastLap.LaptimeMS = (int)shared.mLastLapTimes[i];
            }

            session.AddOrUpdateCar(i, carInfo);
        }
    }

    public static void ToLocalCar(Shared shared, LocalCarData local)
    {
        // Is running as long as engine RPM > 0.
        local.Engine.IsRunning = shared.mRpm > 0;

        // Player info
        local.Race.LapPositionPercentage = shared.mTrackLength / shared.mParticipantInfo[shared.mViewedParticipantIndex].mCurrentLapDistance;
        local.Race.GlobalPosition = (int)shared.mParticipantInfo[shared.mViewedParticipantIndex].mRacePosition;
        local.Race.ClassPosition = (int)shared.mParticipantInfo[shared.mViewedParticipantIndex].mRacePosition;

        // Car model
        local.CarModel.CarClass = shared.mCarClassName.Data;

        // Car physics
        local.Physics.Rotation = System.Numerics.Quaternion.CreateFromYawPitchRoll(shared.mOrientation.Y, shared.mOrientation.X, shared.mOrientation.Z);
        local.Physics.Location = shared.mParticipantInfo[shared.mViewedParticipantIndex].mWorldPosition;
        local.Physics.Acceleration = shared.mLocalAcceleration;
        local.Physics.Velocity = shared.mSpeed * 3.6f;

        // Car engine
        local.Engine.FuelLiters = shared.mFuelLevel * shared.mFuelCapacity;
        local.Engine.MaxFuelLiters = shared.mFuelCapacity;
        local.Engine.MaxRpm = (int)shared.mMaxRPM;
        local.Engine.Rpm = (int)shared.mRpm;

        // Car electronics
        local.Electronics.TractionControlLevel = shared.mTractionControlSetting;
        local.Electronics.BrakeBias = shared.mBrakeBias;

        local.Electronics.AbsActivation = shared.mAntiLockActive ? 1.0f : 0.0f;
        local.Electronics.AbsLevel = shared.mAntiLockSetting;

        // Car inputs
        local.Inputs.HandBrake = shared.mHandBrake;
        local.Inputs.Steering = shared.mSteering;
        local.Inputs.Throttle = shared.mThrottle;
        local.Inputs.Clutch = shared.mClutch;
        local.Inputs.Brake = shared.mBrake;
        local.Inputs.Gear = shared.mGear + 1;

        // Tyres core temp
        local.Tyres.CoreTemperature[0] = shared.mTyreTemp.FL;
        local.Tyres.CoreTemperature[1] = shared.mTyreTemp.FR;
        local.Tyres.CoreTemperature[2] = shared.mTyreTemp.RL;
        local.Tyres.CoreTemperature[3] = shared.mTyreTemp.RR;

        // Tyres surface temp
        local.Tyres.SurfaceTemperature[0] = shared.mTyreCarcassTemp.FL - 273.15f;
        local.Tyres.SurfaceTemperature[1] = shared.mTyreCarcassTemp.FR - 273.15f;
        local.Tyres.SurfaceTemperature[2] = shared.mTyreCarcassTemp.RL - 273.15f;
        local.Tyres.SurfaceTemperature[3] = shared.mTyreCarcassTemp.RR - 273.15f;

        // Tyre pressure (from bar to psi).
        local.Tyres.Pressure[0] = (shared.mAirPressure.FL / 100.0f) * 14.504f;
        local.Tyres.Pressure[1] = (shared.mAirPressure.FR / 100.0f) * 14.504f;
        local.Tyres.Pressure[2] = (shared.mAirPressure.RL / 100.0f) * 14.504f;
        local.Tyres.Pressure[3] = (shared.mAirPressure.RR / 100.0f) * 14.504f;

        // Brake temp
        local.Brakes.DiscTemperature[0] = shared.mBrakeTempCelsius.FL;
        local.Brakes.DiscTemperature[1] = shared.mBrakeTempCelsius.FR;
        local.Brakes.DiscTemperature[2] = shared.mBrakeTempCelsius.RL;
        local.Brakes.DiscTemperature[3] = shared.mBrakeTempCelsius.RR;

        // Lap info
        local.Timing.CurrentLaptimeMS = (int)shared.mCurrentTime;
        local.Timing.LapTimeBestMs = (int)shared.mBestLapTime;
        local.Timing.IsLapValid = !shared.mLapInvalidated;

        // Compute fuel per lap and estimated laps
        if (_prevLapCount != shared.mParticipantInfo[shared.mViewedParticipantIndex].mCurrentLap)
        {
            local.Engine.FuelLitersXLap = _prevLapFuel - local.Engine.FuelLiters;
            _prevLapFuel = local.Engine.FuelLiters;

            local.Engine.FuelEstimatedLaps = local.Engine.FuelLiters / local.Engine.FuelLitersXLap;
            _prevLapCount = shared.mParticipantInfo[shared.mViewedParticipantIndex].mCurrentLap;
        }
    }
}
