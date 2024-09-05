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

        localCarData.Engine.MaxRpm = (int)Utilities.RpsToRpm(sharedData.MaxEngineRps);
        localCarData.Engine.Rpm = (int)Utilities.RpsToRpm(sharedData.EngineRps);

        localCarData.Physics.Velocity = sharedData.CarSpeed * 3.6f;

        var playerDriverData = sharedData.DriverData.First(x => x.DriverInfo.Name == sharedData.PlayerName);
        if (playerDriverData.DriverInfo.Name != null)
            localCarData.CarModel.GameName = $"{playerDriverData.DriverInfo.ModelId}";
    }
}
