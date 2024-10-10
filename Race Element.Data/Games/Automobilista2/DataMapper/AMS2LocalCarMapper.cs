using System.Runtime.CompilerServices;
using RaceElement.Data.Games.Automobilista2.SharedMemory;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using Riok.Mapperly.Abstractions;

namespace RaceElement.Data.Games.Automobilista2.DataMapper;

[Mapper]
internal static partial class Ams2LocalCarMapper
{
    public static void ToLocalCar(Shared sharedData, LocalCarData localCarData)
    {
        // Is running as long as engine RPM > 0.
        localCarData.Engine.IsRunning = sharedData.mRpm > 0;

        // Player info
        localCarData.Race.LapPositionPercentage = sharedData.mTrackLength / sharedData.mParticipantInfo[sharedData.mViewedParticipantIndex].mCurrentLapDistance;
        localCarData.Race.GlobalPosition = (int)sharedData.mParticipantInfo[sharedData.mViewedParticipantIndex].mRacePosition;
        localCarData.Race.ClassPosition = (int)sharedData.mParticipantInfo[sharedData.mViewedParticipantIndex].mRacePosition;

        // Car model
        localCarData.CarModel.CarClass = sharedData.mCarClassName.Data;

        // Car physics
        localCarData.Physics.Rotation = System.Numerics.Quaternion.CreateFromYawPitchRoll(sharedData.mOrientation.Y, sharedData.mOrientation.X, sharedData.mOrientation.Z);
        localCarData.Physics.Location = sharedData.mParticipantInfo[sharedData.mViewedParticipantIndex].mWorldPosition.ToVector3();
        localCarData.Physics.Acceleration = sharedData.mLocalAcceleration.ToVector3();
        localCarData.Physics.Velocity = sharedData.mOdometerKM;
        localCarData.Physics.Location = sharedData.mParticipantInfo[sharedData.mViewedParticipantIndex].mWorldPosition;
        localCarData.Physics.Acceleration = sharedData.mLocalAcceleration;

        // Car engine
        localCarData.Engine.FuelLiters = sharedData.mFuelLevel * sharedData.mFuelCapacity;
        localCarData.Engine.MaxFuelLiters = sharedData.mFuelCapacity;
        localCarData.Engine.MaxRpm = (int)sharedData.mMaxRPM;
        localCarData.Engine.Rpm = (int)sharedData.mRpm;

        // Car electronics
        localCarData.Electronics.TractionControlLevel = sharedData.mTractionControlSetting;
        localCarData.Electronics.BrakeBias = sharedData.mBrakeBias * 100.0f;

        localCarData.Electronics.AbsActivation = sharedData.mAntiLockActive ? 1.0f : 0.0f;
        localCarData.Electronics.AbsLevel = sharedData.mAntiLockSetting;

        // Car inputs
        localCarData.Inputs.HandBrake = sharedData.mHandBrake * 100.0f;
        localCarData.Inputs.Steering = sharedData.mSteering;
        localCarData.Inputs.Throttle = sharedData.mThrottle;
        localCarData.Inputs.Clutch = sharedData.mClutch;
        localCarData.Inputs.Brake = sharedData.mBrake;
        localCarData.Inputs.Gear = sharedData.mGear + 1;

        // Tyres core temp
        localCarData.Tyres.CoreTemperature[0] = sharedData.mTyreTemp.FL;
        localCarData.Tyres.CoreTemperature[1] = sharedData.mTyreTemp.FR;
        localCarData.Tyres.CoreTemperature[2] = sharedData.mTyreTemp.RL;
        localCarData.Tyres.CoreTemperature[3] = sharedData.mTyreTemp.RR;

        // Tyres surface temp
        localCarData.Tyres.SurfaceTemperature[0] = sharedData.mTyreCarcassTemp.FL - 273.15f;
        localCarData.Tyres.SurfaceTemperature[1] = sharedData.mTyreCarcassTemp.FR - 273.15f;
        localCarData.Tyres.SurfaceTemperature[2] = sharedData.mTyreCarcassTemp.RL - 273.15f;
        localCarData.Tyres.SurfaceTemperature[3] = sharedData.mTyreCarcassTemp.RR - 273.15f;

        // Tyre pressure
        localCarData.Tyres.Pressure[0] = sharedData.mAirPressure.FL;
        localCarData.Tyres.Pressure[1] = sharedData.mAirPressure.FR;
        localCarData.Tyres.Pressure[2] = sharedData.mAirPressure.RL;
        localCarData.Tyres.Pressure[3] = sharedData.mAirPressure.RR;

        // Brake temp
        localCarData.Brakes.DiscTemperature[0] = sharedData.mBrakeTempCelsius.FL;
        localCarData.Brakes.DiscTemperature[1] = sharedData.mBrakeTempCelsius.FR;
        localCarData.Brakes.DiscTemperature[2] = sharedData.mBrakeTempCelsius.RL;
        localCarData.Brakes.DiscTemperature[3] = sharedData.mBrakeTempCelsius.RR;

        // Lap info
        localCarData.Timing.CurrentLaptimeMS = (int)sharedData.mCurrentTime;
        localCarData.Timing.LapTimeBestMs = (int)sharedData.mBestLapTime;
        localCarData.Timing.IsLapValid = !sharedData.mLapInvalidated;
    }
}
