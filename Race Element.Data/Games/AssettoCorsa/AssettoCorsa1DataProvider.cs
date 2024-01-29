using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsa.DataMapper;
using RaceElement.Data.Games.AssettoCorsa.SharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa;

internal static class AssettoCorsa1DataProvider
{
    static int lastPhysicsPacketId = -1;

    internal static void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        var physicsPage = AcSharedMemory.ReadPhysicsPageFile();
        if (lastPhysicsPacketId == physicsPage.PacketId) // no need to remap the physics page if packet is the same
            return;

        var graphicsPage = AcSharedMemory.ReadGraphicsPageFile();
        var staticPage = AcSharedMemory.ReadStaticPageFile();

        LocalCarMapper.AddGraphics(graphicsPage, localCar);
        LocalCarMapper.AddPhysics(physicsPage, localCar);
        LocalCarMapper.WithStaticPage(staticPage, localCar);

        SessionDataMapper.WithGraphicsPage(graphicsPage, sessionData);
        SessionDataMapper.WithPhysicsPage(physicsPage, sessionData);
        SessionDataMapper.WithStaticPage(staticPage, sessionData);

        GameDataMapper.WithStaticPage(staticPage, gameData);
        gameData.Name = GameManager.CurrentGame.ToFriendlyName();

        lastPhysicsPacketId = physicsPage.PacketId;
    }


}
