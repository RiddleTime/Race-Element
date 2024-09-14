using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.RaceRoom.DataMapper;
using RaceElement.Data.Games.RaceRoom.SharedMemory;
using System.Diagnostics;

namespace RaceElement.Data.Games.RaceRoom;

internal sealed class RaceRoomDataProvider : AbstractSimDataProvider
{
    internal sealed override int PollingRate() => 400;
    private bool _isGameRunning = false;
    private DateTime _lastGameRunningCheck = DateTime.MinValue;
    public sealed override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        if (!_isGameRunning)
        {
            if (DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(2)) > _lastGameRunningCheck)
            {
                if (Utilities.IsRrreRunning())
                    _isGameRunning = true;
                else
                    _lastGameRunningCheck = DateTime.UtcNow;
            }
        }
        else
        {
            try
            {
                Shared sharedMemory = R3eSharedMemory.ReadSharedMemory();
                R3ELocalCarMapper.AddR3SharedMemory(sharedMemory, localCar);
            }
            catch (Exception e)
            {
                _isGameRunning = false;
                _lastGameRunningCheck = DateTime.UtcNow;
                Debug.WriteLine(e);
            }
        }
    }

    internal override void Stop() { }

    public override List<string> GetCarClasses() => [];

    public override bool HasTelemetry() => false;
}
