using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.RaceRoom.DataMapper;
using RaceElement.Data.Games.RaceRoom.SharedMemory;
using System.Diagnostics;

namespace RaceElement.Data.Games.RaceRoom;

internal sealed class RaceRoomDataProvider : AbstractSimDataProvider
{
    internal sealed override int PollingRate() => 200;

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        try
        {
            Shared sharedMemory = R3eSharedMemory.ReadSharedMemory();
            RR3LocalCarMapper.AddR3SharedMemory(sharedMemory, localCar);
            RR3SessionDataMapper.AddR3SharedMemory(sharedMemory, sessionData);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
        }
    }

    internal override void Stop() { }

    public override List<string> GetCarClasses() => [];

    public override bool HasTelemetry() => false;
}
