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
    // Race
    [MapProperty(nameof(Shared.Position), nameof(@LocalCarData.Race.GlobalPosition))]
    [MapProperty(nameof(Shared.PositionClass), nameof(@LocalCarData.Race.ClassPosition))]
    static partial void WithR3SharedMemory(Shared sharedData, LocalCarData localCarData);

    public static void AddR3SharedMemory(Shared sharedData, LocalCarData localCarData)
    {
        WithR3SharedMemory(sharedData, localCarData);
        localCarData.Inputs.Gear = sharedData.Gear + 1;

        localCarData.Engine.MaxRpm = (int)Utilities.RpsToRpm(sharedData.MaxEngineRps);
        localCarData.Engine.Rpm = (int)Utilities.RpsToRpm(sharedData.EngineRps);

    }
}
