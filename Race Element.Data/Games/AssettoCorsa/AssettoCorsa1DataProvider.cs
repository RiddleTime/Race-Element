using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsa.DataMapper;
using RaceElement.Data.Games.AssettoCorsa.SharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa;

internal class AssettoCorsa1DataProvider
{
    static int packetId = -1;
    internal static void Update(ref LocalCarData LocalCar, ref SessionData SessionData)
    {
        var physicsPage = AcSharedMemory.ReadPhysicsPageFile();
        if (packetId == physicsPage.PacketId)
            return;
        var graphicsPage = AcSharedMemory.ReadGraphicsPageFile();
        var staticPage = AcSharedMemory.ReadStaticPageFile();


        LocalCarMapper.AddAcGraphics(graphicsPage, LocalCar);
        LocalCarMapper.AddAcPhysics(physicsPage, LocalCar);
        LocalCarMapper.WithStaticPage(staticPage, LocalCar);

        SessionDataMapper.WithGraphicsPage(graphicsPage, SessionData);
        SessionDataMapper.WithPhysicsPage(physicsPage, SessionData);
        SessionDataMapper.WithStaticPage(staticPage, SessionData);

        packetId = physicsPage.PacketId;
    }
}
