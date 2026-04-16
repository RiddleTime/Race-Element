using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.AssettoCorsaEvo.DataMapper;
using RaceElement.Data.Games.AssettoCorsaEvo.SharedMemory;
using System.Drawing;

namespace RaceElement.Data.Games.AssettoCorsaEvo;

internal sealed class AssettoCorsaEvoDataProvider : AbstractSimDataProvider
{
    internal sealed override int PollingRate() => 250;

    private static string GameName => Game.AssettoCorsaEvo.ToShortName();

    private int lastPhysicsPacketId = -1;
    private int _pollingBuffer = 0;
    public sealed override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        AcEvoSharedMemory.SPageFilePhysicsEvo physicsPage = AcEvoSharedMemory.Instance.ReadPhysicsPageFile();
        AcEvoSharedMemory.SPageFileGraphicEvo graphicsPage = AcEvoSharedMemory.Instance.ReadGraphicsPageFile();
        AcEvoSharedMemory.SPageFileStaticEvo staticsPage = AcEvoSharedMemory.Instance.ReadStaticPageFile();

        if (lastPhysicsPacketId == physicsPage.PacketId) // no need to remap the physics page if packet is the same
        {
            _pollingBuffer++;

            if (_pollingBuffer > 150)
            {
                lastPhysicsPacketId = physicsPage.PacketId;
                SimDataProvider.GameData.IsGamePaused = true;
                return;
            }
        }
        else
        {
            lastPhysicsPacketId = physicsPage.PacketId;
            SimDataProvider.GameData.IsGamePaused = false;
            _pollingBuffer = 0;
        }

        LocalCarMapper.AddPhysics(ref physicsPage, ref localCar, ref sessionData);


        LocalCarMapper.AddGraphics(ref graphicsPage, ref localCar, ref sessionData, ref gameData);

        gameData.Name = GameName;
    }

    internal override void Start() { }

    internal override void Stop() { }


    public override Color GetColorForCategory(string category) => Color.Empty;

    public override List<string> GetCarClasses() => [];
    public override bool HasTelemetry() => false;
}
