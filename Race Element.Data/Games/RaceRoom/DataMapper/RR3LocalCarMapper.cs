using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.RaceRoom.SharedMemory;
using Riok.Mapperly.Abstractions;

namespace RaceElement.Data.Games.RaceRoom.DataMapper;

[Mapper]
internal static partial class RR3LocalCarMapper
{
    // Inputs
    [MapProperty(nameof(Shared.Throttle), nameof(@LocalCarData.Inputs.Throttle))]
    [MapProperty(nameof(Shared.Brake), nameof(@LocalCarData.Inputs.Brake))]
    [MapProperty(nameof(Shared.Clutch), nameof(@LocalCarData.Inputs.Clutch))]
    [MapProperty(nameof(Shared.SteerInputRaw), nameof(@LocalCarData.Inputs.Steering))]
    [MapProperty(nameof(Shared.SteerWheelRangeDegrees), nameof(@LocalCarData.Inputs.MaxSteeringAngle))]
    // Fuel
    [MapProperty(nameof(Shared.FuelLeft), nameof(@LocalCarData.Engine.FuelLiters))]
    [MapProperty(nameof(Shared.FuelPerLap), nameof(@LocalCarData.Engine.FuelLitersXLap))]
    [MapProperty(nameof(Shared.FuelCapacity), nameof(@LocalCarData.Engine.MaxFuelLiters))]
    // Race
    [MapProperty(nameof(Shared.Position), nameof(@LocalCarData.Race.GlobalPosition))]
    [MapProperty(nameof(Shared.PositionClass), nameof(@LocalCarData.Race.ClassPosition))]
    [MapProperty(nameof(Shared.LapDistanceFraction), nameof(@LocalCarData.Race.LapPositionPercentage))]
    [MapProperty(nameof(Shared.CompletedLaps), nameof(@LocalCarData.Race.LapsDriven))]
    static partial void WithR3SharedMemory(Shared sharedData, LocalCarData localCarData);

    public static void AddR3SharedMemory(Shared sharedData, LocalCarData localCarData)
    {
        WithR3SharedMemory(sharedData, localCarData);

        localCarData.Inputs.Gear = sharedData.Gear + 1;

        localCarData.Electronics.AbsActivation = sharedData.AidSettings.Abs == 5 ? 1 : 0;
        localCarData.Electronics.TractionControlActivation = sharedData.AidSettings.Tc == 5 ? 1 : 0;

        localCarData.Engine.MaxRpm = (int)Utilities.RpsToRpm(sharedData.MaxEngineRps);
        localCarData.Engine.Rpm = (int)Utilities.RpsToRpm(sharedData.EngineRps);


        localCarData.Brakes.Pressure = [sharedData.BrakePressure.FrontLeft, sharedData.BrakePressure.FrontRight, sharedData.BrakePressure.RearLeft, sharedData.BrakePressure.RearRight];
        localCarData.Brakes.DiscTemperature = [sharedData.BrakeTemp.FrontLeft.CurrentTemp, sharedData.BrakeTemp.FrontRight.CurrentTemp, sharedData.BrakeTemp.RearLeft.CurrentTemp, sharedData.BrakeTemp.RearRight.CurrentTemp];

        localCarData.Tyres.Pressure = [sharedData.TirePressure.FrontLeft, sharedData.TirePressure.FrontRight, sharedData.TirePressure.RearLeft, sharedData.TirePressure.RearRight];

        localCarData.Physics.Velocity = sharedData.CarSpeed * 3.6f;
        localCarData.Physics.Acceleration = new System.Numerics.Vector3(sharedData.LocalAcceleration.X, sharedData.LocalAcceleration.Y, sharedData.LocalAcceleration.Z);
        localCarData.Physics.Location = new System.Numerics.Vector3(sharedData.CarCgLocation.X, sharedData.CarCgLocation.Y, sharedData.CarCgLocation.Z);
        localCarData.Physics.Rotation = System.Numerics.Quaternion.CreateFromYawPitchRoll(sharedData.CarOrientation.Yaw, sharedData.CarOrientation.Pitch, sharedData.CarOrientation.Roll);

        if (sharedData.DriverData != null)
        {
            var driverData = sharedData.DriverData[sharedData.Position - 1];
            localCarData.CarModel.GameId = driverData.DriverInfo.ModelId;
        }

    }
}
