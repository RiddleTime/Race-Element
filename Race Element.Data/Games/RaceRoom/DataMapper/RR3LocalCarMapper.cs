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
    // Race
    [MapProperty(nameof(Shared.Position), nameof(@LocalCarData.Race.GlobalPosition))]
    [MapProperty(nameof(Shared.PositionClass), nameof(@LocalCarData.Race.ClassPosition))]
    private static partial void WithR3SharedMemory(Shared sharedData, LocalCarData commonData);

    public static void AddR3SharedMemory(Shared sharedData, LocalCarData commonData)
    {
        WithR3SharedMemory(sharedData, commonData);
        commonData.Inputs.Gear = sharedData.Gear + 1;

        commonData.Engine.MaxRpm = (int)Utilities.RpsToRpm(sharedData.MaxEngineRps);
        commonData.Engine.Rpm = (int)Utilities.RpsToRpm(sharedData.EngineRps);
    }
}
