using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.RaceRoom.SharedMemory;
using Riok.Mapperly.Abstractions;

namespace RaceElement.Data.Games.RaceRoom.DataMapper;

[Mapper]
internal static partial class R3ELocalCarMapper
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

        // Inputs Data
        localCarData.Inputs.Gear = sharedData.Gear + 1;


        // Electronics Data
        localCarData.Electronics.AbsActivation = sharedData.AidSettings.Abs == 5 ? 1 : 0;
        localCarData.Electronics.TractionControlActivation = sharedData.AidSettings.Tc == 5 ? 1 : 0;


        // Engine Data
        localCarData.Engine.MaxRpm = (int)Utilities.RpsToRpm(sharedData.MaxEngineRps);
        localCarData.Engine.Rpm = (int)Utilities.RpsToRpm(sharedData.EngineRps);
        localCarData.Engine.IsPitLimiterOn = sharedData.PitLimiter == 1;
        localCarData.Engine.IsRunning = sharedData.EngineRps > 0;

        // Brakes Data
        localCarData.Brakes.Pressure = [sharedData.BrakePressure.FrontLeft, sharedData.BrakePressure.FrontRight, sharedData.BrakePressure.RearLeft, sharedData.BrakePressure.RearRight];
        localCarData.Brakes.DiscTemperature = [sharedData.BrakeTemp.FrontLeft.CurrentTemp, sharedData.BrakeTemp.FrontRight.CurrentTemp, sharedData.BrakeTemp.RearLeft.CurrentTemp, sharedData.BrakeTemp.RearRight.CurrentTemp];


        // Tyres Data
        localCarData.Tyres.Pressure = [sharedData.TirePressure.FrontLeft / 100, sharedData.TirePressure.FrontRight / 100, sharedData.TirePressure.RearLeft / 100, sharedData.TirePressure.RearRight / 100];
        localCarData.Tyres.Velocity = [-sharedData.TireSpeed.FrontLeft * 3.6f, -sharedData.TireSpeed.FrontRight * 3.6f, -sharedData.TireSpeed.RearLeft * 3.6f, -sharedData.TireSpeed.RearRight * 3.6f];

        localCarData.Tyres.SlipRatio = [
            CalculateSlipRatio(localCarData.Tyres.Velocity[0], localCarData.Physics.Velocity),
            CalculateSlipRatio(localCarData.Tyres.Velocity[1], localCarData.Physics.Velocity),
            CalculateSlipRatio(localCarData.Tyres.Velocity[2], localCarData.Physics.Velocity),
            CalculateSlipRatio(localCarData.Tyres.Velocity[3], localCarData.Physics.Velocity),
        ];

        // Physics Data
        localCarData.Physics.Velocity = sharedData.CarSpeed * 3.6f;

        localCarData.Physics.Acceleration = new System.Numerics.Vector3(sharedData.LocalAcceleration.X / 9.80665f, sharedData.LocalAcceleration.Y / 9.80665f, -sharedData.LocalAcceleration.Z / 9.80665f);
        localCarData.Physics.Location = new System.Numerics.Vector3(sharedData.CarCgLocation.X, sharedData.CarCgLocation.Y, sharedData.CarCgLocation.Z);
        localCarData.Physics.Rotation = System.Numerics.Quaternion.CreateFromYawPitchRoll(sharedData.CarOrientation.Yaw, sharedData.CarOrientation.Pitch, sharedData.CarOrientation.Roll);


        // Timing Data
        localCarData.Timing.LapTimeDeltaBestMS = (int)(sharedData.TimeDeltaBestSelf * 1000f);
        localCarData.Timing.CurrentLaptimeMS = (int)(sharedData.LapTimeCurrentSelf * 1000f);
        localCarData.Timing.IsLapValid = sharedData.CurrentLapValid == 1;

        int lapTimeBestSelf = (int)(sharedData.LapTimeBestSelf * 1000f);
        if (lapTimeBestSelf < -1) lapTimeBestSelf = -1;
        localCarData.Timing.LapTimeBestMs = lapTimeBestSelf;
        localCarData.Timing.HasLapTimeBest = sharedData.LapTimeBestSelf != -1;

        if ((sharedData.DriverData != null) && (sharedData.DriverData[0].Place != -1))
        {
            var driverData = sharedData.DriverData[sharedData.Position - 1];
            localCarData.CarModel.GameId = driverData.DriverInfo.ModelId;
        }
    }

    private static float CalculateSlipRatio(float tyreVelocity, float carVelocity)
    {
        if (carVelocity < 1) return 0;
        return (tyreVelocity - carVelocity) / tyreVelocity * 10;
    }
}
