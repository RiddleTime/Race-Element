using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsa.DataMapper;
using RaceElement.Data.Games.AssettoCorsa.DataMapper.LocalCar;
using RaceElement.Data.Games.AssettoCorsa.SharedMemory;
using System.Drawing;
using System.Drawing.Drawing2D;
using static RaceElement.Data.Games.AssettoCorsa.SharedMemory.AcSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa;

internal class AssettoCorsa1DataProvider : AbstractSimDataProvider
{
    static int lastPhysicsPacketId = -1;

    public AssettoCorsa1DataProvider()
    {
    }

    private static string GameName { get => Game.AssettoCorsa1.ToShortName(); }

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
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

    public override List<string> GetCarClasses()
    {
        throw new NotImplementedException();
    }

    public override Color GetColorForCarClass(string carClass)
    {
        throw new NotImplementedException();
    }

    public override bool HasTelemetry()
    {
        return lastPhysicsPacketId > -1;
    }

    
    internal override void Stop()
    {
        throw new NotImplementedException();
    }
}
