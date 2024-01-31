using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsa.DataMapper;
using RaceElement.Data.Games.AssettoCorsa.DataMapper.LocalCar;
using RaceElement.Data.Games.AssettoCorsa.SharedMemory;
using System.Drawing.Drawing2D;
using static RaceElement.Data.Games.AssettoCorsa.SharedMemory.AcSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa;

internal class AssettoCorsa1DataProvider
{
    static int lastPhysicsPacketId = -1;

    private static string GameName { get => Game.AssettoCorsa1.ToShortName(); }

    internal static void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        var physicsPage = AcSharedMemory.ReadPhysicsPageFile();
        if (lastPhysicsPacketId == physicsPage.PacketId) // no need to remap the physics page if packet is the same
            return;

        var graphicsPage = AcSharedMemory.ReadGraphicsPageFile();
        var staticPage = AcSharedMemory.ReadStaticPageFile();

        MapLocalCar(ref graphicsPage, ref physicsPage, ref localCar);
        LocalCarMapper.WithStaticPage(staticPage, localCar);

        SessionDataMapper.WithGraphicsPage(graphicsPage, sessionData);
        SessionDataMapper.WithPhysicsPage(physicsPage, sessionData);
        SessionDataMapper.WithStaticPage(staticPage, sessionData);

        GameDataMapper.WithStaticPage(staticPage, gameData);
        gameData.Name = GameName;

        lastPhysicsPacketId = physicsPage.PacketId;
    }


    private static void MapLocalCar(ref PageFileGraphics graphics, ref PageFilePhysics physics, ref LocalCarData localCar)
    {
        LocalCarMapper.AddGraphics(graphics, localCar);
        LocalCarMapper.AddPhysics(ref physics, ref localCar);

        PhysicsDataMapper.InsertPhysicsPage(ref physics, localCar.Physics);
    }

}
