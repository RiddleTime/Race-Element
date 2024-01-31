using Riok.Mapperly.Abstractions;
using RaceElement.Data.Common.SimulatorData;
using static RaceElement.Data.Games.AssettoCorsa.SharedMemory.AcSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa.DataMapper;

[Mapper]
internal static partial class SessionDataMapper
{
    // -- Weather Conditions
    [MapProperty(nameof(PageFilePhysics.AirDensity), nameof(@SessionData.Weather.AirPressure))]
    [MapProperty($"{nameof(PageFilePhysics.AirTemp)}", nameof(@SessionData.Weather.AirTemperature))]
    // -- Track Conditions
    [MapProperty($"{nameof(PageFilePhysics.RoadTemp)}", nameof(@SessionData.Track.Temperature))]
    internal static partial void WithPhysicsPage(PageFilePhysics pagePhysics, SessionData sessionData);

    //  -- Weather Conditions
    internal static partial void WithGraphicsPage(PageFileGraphics pageGraphics, SessionData sessionData);


    // -- Weather Conditions
    [MapperIgnoreSource(nameof(PageFileStatic.Track))]
    [MapProperty(nameof(PageFileStatic.Track), nameof(@SessionData.Track.GameName))]
    internal static partial void WithStaticPage(PageFileStatic pageStatic, SessionData sessionData);
}
