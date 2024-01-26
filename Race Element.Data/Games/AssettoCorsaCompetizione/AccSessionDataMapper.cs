using RaceElement.Data.Common;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.ACCSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione
{
    [Mapper]
    public static partial class AccSessionDataMapper
    {
        public static partial void WithAccPhysicsPage(PageFilePhysics pagePhysics, SessionData sessionData);
    }
}
