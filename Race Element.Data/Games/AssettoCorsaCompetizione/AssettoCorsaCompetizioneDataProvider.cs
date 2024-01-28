using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsaCompetizione.DataMapper;
using RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione
{
    internal static class AssettoCorsaCompetizioneDataProvider
    {
        internal static void Update(ref LocalCarData LocalCar, ref SessionData SessionData)
        {
            var graphicsPage = AccSharedMemory.ReadGraphicsPageFile();
            var physicsPage = AccSharedMemory.ReadPhysicsPageFile();
            var staticPage = AccSharedMemory.ReadStaticPageFile();

            LocalCarMapper.WithGraphicsPage(graphicsPage, LocalCar);
            LocalCarMapper.WithPhysicsPage(physicsPage, LocalCar);
            LocalCarMapper.WithStaticPage(staticPage, LocalCar);

            SessionDataMapper.WithGraphicsPage(graphicsPage, SessionData);
            SessionDataMapper.WithPhysicsPage(physicsPage, SessionData);
        }

    }
}
