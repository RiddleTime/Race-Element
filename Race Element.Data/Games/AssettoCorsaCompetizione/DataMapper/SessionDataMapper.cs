using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.AccSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione.DataMapper
{
    [Mapper]
    internal static partial class SessionDataMapper
    {
        // -- Weather Conditions
        [MapProperty($"{nameof(PageFilePhysics.AirTemp)}", nameof(@SessionData.Weather.AirTemperature))]
        // -- Track Conditions
        [MapProperty($"{nameof(PageFilePhysics.RoadTemp)}", nameof(@SessionData.Track.TrackTemperature))]
        internal static partial void WithPhysicsPage(PageFilePhysics pagePhysics, SessionData sessionData);

        // -- Weather Conditions
        [MapProperty($"{nameof(PageFileGraphics.WindSpeed)}", nameof(@SessionData.Weather.AirVelocity))]
        [MapProperty($"{nameof(PageFileGraphics.WindDirection)}", nameof(@SessionData.Weather.AirDirection))]
        internal static partial void WithGraphicsPage(PageFileGraphics pageGraphics, SessionData sessionData);
    }
}
