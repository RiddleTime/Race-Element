using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsaCompetizione.DataMapper;
using RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione
{
    internal static class AssettoCorsaCompetizioneDataProvider
    {
        static int lastPhysicsPacketId = -1;

        private static string GameName { get => Game.AssettoCorsaCompetizione.ToShortName(); }

        internal static void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
        {
            var physicsPage = AccSharedMemory.ReadPhysicsPageFile();
            if (lastPhysicsPacketId == physicsPage.PacketId)
                return;

            var graphicsPage = AccSharedMemory.ReadGraphicsPageFile();
            var staticPage = AccSharedMemory.ReadStaticPageFile();


            LocalCarMapper.AddAccGraphics(graphicsPage, localCar);
            LocalCarMapper.WithPhysicsPage(physicsPage, localCar);
            LocalCarMapper.WithStaticPage(staticPage, localCar);

            SessionDataMapper.WithGraphicsPage(graphicsPage, sessionData);
            SessionDataMapper.WithPhysicsPage(physicsPage, sessionData);
            SessionDataMapper.WithStaticPage(staticPage, sessionData);

            GameDataMapper.WithStaticPage(staticPage, gameData);
            gameData.Name = GameName;

            lastPhysicsPacketId = physicsPage.PacketId;
        }

    }
}
