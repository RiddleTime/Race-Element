using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsaCompetizione.DataMapper;
using RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione
{
    internal static class AssettoCorsaCompetizioneDataProvider
    {
        static int packetId = -1;
        internal static void Update(ref LocalCarData LocalCar, ref SessionData SessionData)
        {
            var physicsPage = AccSharedMemory.ReadPhysicsPageFile();
            if (packetId == physicsPage.PacketId)
                return;
            var graphicsPage = AccSharedMemory.ReadGraphicsPageFile();
            var staticPage = AccSharedMemory.ReadStaticPageFile();


            LocalCarMapper.AddAccGraphics(graphicsPage, LocalCar);
            LocalCarMapper.WithPhysicsPage(physicsPage, LocalCar);
            LocalCarMapper.WithStaticPage(staticPage, LocalCar);

            SessionDataMapper.WithGraphicsPage(graphicsPage, SessionData);
            SessionDataMapper.WithPhysicsPage(physicsPage, SessionData);


            packetId = physicsPage.PacketId;
        }

    }
}
