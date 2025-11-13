using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaRally.SharedMemory.AcRallySharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaRally.DataMapper;

[Mapper]
internal static partial class GameDataMapper
{
    public static void WithStaticPage(SPageFileStatic pageStatic, GameData gameData)
    {
        gameData.Version = pageStatic.AssettoCorsaVersion;
    }
}
