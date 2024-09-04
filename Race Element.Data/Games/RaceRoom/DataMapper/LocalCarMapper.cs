using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.RaceRoom.SharedMemory;
using Riok.Mapperly.Abstractions;

namespace RaceElement.Data.Games.RaceRoom.DataMapper;

[Mapper]
internal static partial class LocalCarMapper
{
    // Inputs
    [MapProperty(nameof(Shared.Throttle), nameof(@LocalCarData.Inputs.Throttle))]
    [MapProperty(nameof(Shared.Brake), nameof(@LocalCarData.Inputs.Brake))]
    [MapProperty(nameof(Shared.Clutch), nameof(@LocalCarData.Inputs.Clutch))]
    [MapProperty(nameof(Shared.SteerInputRaw), nameof(@LocalCarData.Inputs.Steering))]
    // 
    [MapProperty(nameof(Shared.Position), nameof(@LocalCarData.Race.GlobalPosition))]
    [MapProperty(nameof(Shared.PositionClass), nameof(@LocalCarData.Race.ClassPosition))]
    internal static partial void WithR3SharedMemory(Shared pageGraphics, LocalCarData commonData);
}
