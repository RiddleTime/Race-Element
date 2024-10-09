using RaceElement.Data.Games.Automobilista2.SharedMemory;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using Riok.Mapperly.Abstractions;

namespace RaceElement.Data.Games.Automobilista2.DataMapper;

[Mapper]
internal static partial class Ams2LocalCarMapper
{
    public static void ToLocalCar(Shared sharedData, LocalCarData localCarData)
    {
        // Is running as long as current session is something valid.
        localCarData.Engine.IsRunning = sharedData.mSessionState != (int)Constants.GameSession.SESSION_INVALID;

        // Player info
        localCarData.Race.GlobalPosition = (int)sharedData.mParticipantInfo[0].mRacePosition;
        localCarData.CarModel.CarClass = new string(sharedData.mCarClassName.Data);

        // Car physics
        localCarData.Physics.Location = sharedData.mParticipantInfo[0].mWorldPosition.ToVector3();
        localCarData.Physics.Acceleration = sharedData.mWorldAcceleration.ToVector3();
        localCarData.Physics.Velocity = sharedData.mOdometerKM;

        // Car engine
        localCarData.Engine.MaxFuelLiters = sharedData.mFuelCapacity;
        localCarData.Engine.FuelLiters = sharedData.mFuelLevel * sharedData.mFuelCapacity;

        // Car inputs
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

        // Brake temp
        localCarData.Brakes.DiscTemperature[0] = sharedData.mBrakeTempCelsius.FL;
        localCarData.Brakes.DiscTemperature[1] = sharedData.mBrakeTempCelsius.FR;
        localCarData.Brakes.DiscTemperature[2] = sharedData.mBrakeTempCelsius.RL;
        localCarData.Brakes.DiscTemperature[3] = sharedData.mBrakeTempCelsius.RR;
    }
}
