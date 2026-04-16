using RaceElement.Data.Common.SimulatorData;
using static RaceElement.Data.Games.AssettoCorsaEvo.SharedMemory.AcEvoSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaEvo.DataMapper;

internal static class GameDataMapper
{
    public static void WithStaticPage(SPageFileStaticEvo pageStatic, GameData gameData)
    {
        gameData.Version = pageStatic.AcEvoVersion;
    }
}
