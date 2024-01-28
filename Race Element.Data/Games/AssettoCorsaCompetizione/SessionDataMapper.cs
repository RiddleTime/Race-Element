using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.AccSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione
{
    [Mapper]
    public static partial class SessionDataMapper
    {
        [MapProperty($"{nameof(PageFilePhysics.AirTemp)}", $"{nameof(SessionData.Conditions)}.{nameof(SessionConditions.AirTemperature)}")]
        [MapProperty($"{nameof(PageFilePhysics.RoadTemp)}", $"{nameof(SessionData.Conditions)}.{nameof(SessionConditions.TrackTemperature)}")]
        public static partial void WithAccPhysicsPage(PageFilePhysics pagePhysics, SessionData sessionData);
        public static partial void WithAccGraphicsPage(PageFileGraphics pageGraphics, SessionData sessionData);
    }
}
