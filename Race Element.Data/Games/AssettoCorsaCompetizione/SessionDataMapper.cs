using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.AccSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione
{
    [Mapper]
    public static partial class SessionDataMapper
    {
        public static partial void WithAccPhysicsPage(PageFilePhysics pagePhysics, SessionData sessionData);
        public static partial void WithAccGraphicsPage(PageFileGraphics pageGraphics, SessionData sessionData);
    }
}
