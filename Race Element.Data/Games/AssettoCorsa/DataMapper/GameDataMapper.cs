using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsa.SharedMemory.AcSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa.DataMapper
{
    [Mapper]
    internal static partial class GameDataMapper
    {
        [MapProperty(nameof(PageFileStatic.AssettoCorsaVersion), nameof(GameData.Version))]
        public static partial void WithStaticPage(PageFileStatic pageStatic, GameData gameData);
    }
}
