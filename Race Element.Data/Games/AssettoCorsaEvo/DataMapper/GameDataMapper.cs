using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaEvo.SharedMemory.AcEvoSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaEvo.DataMapper;

[Mapper]
internal static partial class GameDataMapper
{
    public static void WithStaticPage(SPageFileStaticEvo pageStatic, GameData gameData)
    {
        gameData.Version = pageStatic.AcEvoVersion;
    }
}
