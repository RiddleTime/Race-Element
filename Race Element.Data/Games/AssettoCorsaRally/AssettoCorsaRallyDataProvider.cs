using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.AssettoCorsaRally.DataMapper;
using RaceElement.Data.Games.AssettoCorsaRally.SharedMemory;
using System.Drawing;

namespace RaceElement.Data.Games.AssettoCorsaRally;

internal sealed class AssettoCorsaRallyDataProvider : AbstractSimDataProvider
{
    static int lastPhysicsPacketId = -1;

    internal override int PollingRate() => 200;

    private static string GameName => Game.AssettoCorsaRally.ToShortName();

    public sealed override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {

        var physicsPage = AcRallySharedMemory.Instance.ReadPhysicsPageFile();
        if (lastPhysicsPacketId == physicsPage.PacketId) // no need to remap the physics page if packet is the same
        {
            lastPhysicsPacketId = physicsPage.PacketId;
            SimDataProvider.GameData.IsGamePaused = true;
            return;
        }
        else
        {
            lastPhysicsPacketId = physicsPage.PacketId;
            SimDataProvider.GameData.IsGamePaused = false;
        }

        LocalCarMapper.AddPhysics(ref physicsPage, ref localCar, ref sessionData);

        gameData.Name = GameName;
    }

    internal sealed override void Start()
    {
    }

    internal override void Stop()
    {
    }

    public override Color GetColorForCategory(string category)
    {
        return Color.White;
    }

    public override List<string> GetCarClasses()
    {
        return [];
    }

    public override bool HasTelemetry()
    {
        return lastPhysicsPacketId > 0;
    }

}
