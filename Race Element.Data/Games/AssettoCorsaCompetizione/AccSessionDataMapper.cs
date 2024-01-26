using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.AccSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione
{
    [Mapper]
    public static partial class AccSessionDataMapper
    {
        public static partial void WithAccPhysicsPage(PageFilePhysics pagePhysics, SessionData sessionData);
    }
}
