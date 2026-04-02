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
        session.Weather.AirVelocity = shared.mWindSpeed * 3.6f;

        // Track
        session.Track.Length = (int)(shared.mTrackLength + 0.5f);
        session.Track.Temperature = shared.mTrackTemperature;
        session.Track.GameName = shared.mTranslatedTrackVariation.Data;

        // Others
        session.SessionTimeLeftSecs = shared.mEventTimeRemaining / 1000.0f;
        session.IsSetupMenuVisible = shared.mGameState == (int)Constants.GameState.GAME_INGAME_INMENU_TIME_TICKING;

        // Race session and phase type
        session.SessionType = ToSessionType((Constants.RaceSession)shared.mSessionState);
        session.Phase = ToSessionPhase((Constants.RaceState)shared.mRaceState);

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

        // Flag Mapper
        session.CurrentFlag = ToFlag((Constants.RaceFlags)shared.mHighestFlagColour);
    }

    public static void ToLocalCar(Shared shared, LocalCarData local)
    {
        // Is running as long as engine RPM > 0.
        local.Engine.IsRunning = shared.mRpm > 0;

        if (shared.mViewedParticipantIndex == -1)
            return;

        // Player info
        local.Race.LapPositionPercentage = shared.mTrackLength / shared.mParticipantInfo[shared.mViewedParticipantIndex].mCurrentLapDistance;
        local.Race.GlobalPosition = (int)shared.mParticipantInfo[shared.mViewedParticipantIndex].mRacePosition;
        local.Race.ClassPosition = (int)shared.mParticipantInfo[shared.mViewedParticipantIndex].mRacePosition;

        // Car model
        local.CarModel.CarClass = shared.mCarClassName.Data;

        // Car physics
        local.Physics.Rotation = System.Numerics.Quaternion.CreateFromYawPitchRoll(shared.mOrientation.Y, shared.mOrientation.X, shared.mOrientation.Z);
        local.Physics.Location = shared.mParticipantInfo[shared.mViewedParticipantIndex].mWorldPosition;
        // Convert acceleration from m/s² to G-force (1G = 9.80665 m/s²)
        local.Physics.Acceleration = new System.Numerics.Vector3(
            shared.mLocalAcceleration.X / 9.80665f,
            shared.mLocalAcceleration.Y / 9.80665f,
            shared.mLocalAcceleration.Z / 9.80665f);
        local.Physics.Velocity = shared.mSpeed * 3.6f;

        // Car engine
        local.Engine.FuelLiters = shared.mFuelLevel * shared.mFuelCapacity;
        local.Engine.MaxFuelLiters = shared.mFuelCapacity;
        local.Engine.MaxRpm = (int)shared.mMaxRPM;
        local.Engine.Rpm = (int)shared.mRpm;

        // Car electronics
        // AMS 2 is reporting -1 if the current car has no TC
        local.Electronics.TractionControlLevel = shared.mTractionControlSetting;
        if (local.Electronics.TractionControlLevel == -1)
        {
            local.Electronics.TractionControlActivation = 0;
        }

        local.Electronics.BrakeBias = 1 - shared.mBrakeBias;

        local.Electronics.AbsActivation = shared.mAntiLockActive ? 1.0f : 0.0f;
        local.Electronics.AbsLevel = shared.mAntiLockSetting;
        // AMS 2 is reporting -1 if the current car has no ABS
        if (local.Electronics.AbsLevel == -1)
        {
            local.Electronics.AbsLevel = 0;
        }
        local.Electronics.PushToPassActive = shared.mBoostActive;
        local.Electronics.PushToPassLevel = shared.mBoostAmount;


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

        // Tyre slip ratio calculation
        local.Tyres.SlipRatio = [
            CalculateWheelSlipRatio(shared.mTyreRPS.FL, shared.mTyreY.FL, shared.mLocalVelocity.Z),
            CalculateWheelSlipRatio(shared.mTyreRPS.FR, shared.mTyreY.FR, shared.mLocalVelocity.Z),
            CalculateWheelSlipRatio(shared.mTyreRPS.RL, shared.mTyreY.RL, shared.mLocalVelocity.Z),
            CalculateWheelSlipRatio(shared.mTyreRPS.RR, shared.mTyreY.RR, shared.mLocalVelocity.Z)
        ];

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

    private static CurrentFlag ToFlag(Constants.RaceFlags flag) => flag switch
    {
        Constants.RaceFlags.FLAG_COLOUR_GREEN => CurrentFlag.Green,
        Constants.RaceFlags.FLAG_COLOUR_BLUE => CurrentFlag.Blue,
        Constants.RaceFlags.FLAG_COLOUR_YELLOW => CurrentFlag.Yellow,
        Constants.RaceFlags.FLAG_COLOUR_DOUBLE_YELLOW => CurrentFlag.Yellow,
        Constants.RaceFlags.FLAG_COLOUR_RED => CurrentFlag.Red,
        Constants.RaceFlags.FLAG_COLOUR_BLACK => CurrentFlag.Black,
        Constants.RaceFlags.FLAG_COLOUR_WHITE_FINAL_LAP => CurrentFlag.White,
        Constants.RaceFlags.FLAG_COLOUR_WHITE_SLOW_CAR => CurrentFlag.White,
        Constants.RaceFlags.FLAG_COLOUR_CHEQUERED => CurrentFlag.Checkered,
        Constants.RaceFlags.FLAG_COLOUR_BLACK_ORANGE_CIRCLE => CurrentFlag.Damage,
        Constants.RaceFlags.FLAG_COLOUR_MAX => CurrentFlag.Max,
        Constants.RaceFlags.FLAG_COLOUR_BLACK_AND_WHITE => CurrentFlag.Penalty,
        _ => CurrentFlag.None
    };

    private static RaceSessionType ToSessionType(Constants.RaceSession session) => session switch
    {
        Constants.RaceSession.SESSION_TEST => RaceSessionType.Practice,
        Constants.RaceSession.SESSION_QUALIFY => RaceSessionType.Qualifying,
        Constants.RaceSession.SESSION_PRACTICE => RaceSessionType.Practice,
        Constants.RaceSession.SESSION_RACE => RaceSessionType.Race,
        Constants.RaceSession.SESSION_TIME_ATTACK => RaceSessionType.Hotlap,
        Constants.RaceSession.SESSION_FORMATION_LAP => RaceSessionType.Race,
        Constants.RaceSession.SESSION_INVALID => RaceSessionType.Replay,
        Constants.RaceSession.SESSION_MAX => RaceSessionType.Race,
        _ => RaceSessionType.Practice,
    };

    private static SessionPhase ToSessionPhase(Constants.RaceState state) => state switch
    {
        Constants.RaceState.RACESTATE_NOT_STARTED => SessionPhase.Starting,
        Constants.RaceState.RACESTATE_RACING => SessionPhase.Session,
        Constants.RaceState.RACESTATE_FINISHED => SessionPhase.SessionOver,
        Constants.RaceState.RACESTATE_DISQUALIFIED => SessionPhase.SessionOver,
        Constants.RaceState.RACESTATE_RETIRED => SessionPhase.SessionOver,
        Constants.RaceState.RACESTATE_DNF => SessionPhase.SessionOver,
        Constants.RaceState.RACESTATE_INVALID => SessionPhase.NONE,
        Constants.RaceState.RACESTATE_MAX => SessionPhase.ResultUI,
        _ => SessionPhase.NONE
    };

    /// <summary>
    /// Calculates wheel slip ratio from tire RPS and ground speed with sign preservation.
    /// Uses tire effective radius calculated from tire Y position (height above ground).
    /// </summary>
    /// <param name="tyreRPS">Tire rotational speed (in radians per second)</param>
    /// <param name="tyreY">Tire Y position (height in local space)</param>
    /// <param name="groundSpeed">Longitudinal ground speed in m/s</param>
    /// <returns>Signed slip ratio: positive = wheel spin (throttle), negative = wheel lock (brake), scaled by 10</returns>
    private static float CalculateWheelSlipRatio(float tyreRPS, float tyreY, float groundSpeed)
    {
        // Ground speed and tire RPS might be negative depending on game coordinates or driving backward.
        // Convert to absolute scalars first so we compare pure speed magnitudes.
        float absGroundSpeed = Math.Abs(groundSpeed);
        float absTyreRPS = Math.Abs(tyreRPS);

        // Below speed threshold, no meaningful slip can be measured
        const float minSpeedThreshold = 1.0f; // 1 m/s = 3.6 km/h
        if (absGroundSpeed < minSpeedThreshold)
            return 0f;

        // Estimate tire radius from tire Y position (typical values range 0.3-0.35m)
        // Using absolute value as a safety measure
        float tireRadius = Math.Abs(tyreY);
        
        // If radius is unrealistic, use a default (0.33m is typical for most race cars)
        if (tireRadius < 0.2f || tireRadius > 0.5f)
            tireRadius = 0.33f;

        // Calculate wheel tangential velocity magnitude (Vw = ω * r)
        // mTyreRPS in Madness engine is typically radians per second, so no 2*PI is needed.
        float wheelVelocity = absTyreRPS * tireRadius;

        // Calculate SIGNED slip ratio to distinguish brake lock from wheel spin
        // Positive: wheelVelocity > groundSpeed (wheel spin / throttle slip)
        // Negative: wheelVelocity < groundSpeed (wheel lock / brake slip)
        float velocityDifference = wheelVelocity - absGroundSpeed;
        float maxVelocity = Math.Max(wheelVelocity, absGroundSpeed);
        
        // Avoid division by zero (should not happen due to speed threshold check above)
        if (maxVelocity < 0.1f)
            return 0f;

        // Calculate slip ratio: (Vw - Vg) / max(|Vw|, |Vg|)
        // Positive = wheel spin (oil/throttle), Negative = wheel lock (brake)
        // Multiply by 10 to give a healthy range for haptics (real physical slip is around 0.05-0.15)
        float slipRatio = (velocityDifference / maxVelocity) * 10f;
        
        return slipRatio;
    }

}
