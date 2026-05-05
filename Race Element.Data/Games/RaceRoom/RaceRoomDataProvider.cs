using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.RaceRoom.DataMapper;
using RaceElement.Data.Games.RaceRoom.SharedMemory;
using System.Diagnostics;

namespace RaceElement.Data.Games.RaceRoom;

internal sealed class RaceRoomDataProvider : AbstractSimDataProvider
{
    private bool _isGameRunning = false;
    private DateTime _lastGameRunningCheck = DateTime.MinValue;

    private GraphJob _graphjob;

    internal sealed override int PollingRate() => 300;

    internal sealed override void Start()
    {
        _graphjob = new(this) { IntervalMillis = 500 };
        _graphjob.Run();
    }

    internal sealed override void Stop()
    {
        _graphjob.CancelJoin();
    }

    public sealed override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        if (!_isGameRunning)
        {
            if (DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(2)) > _lastGameRunningCheck)
            {
                if (Utilities.IsRrreRunning())
                    _isGameRunning = true;
                else
                {
                    _isGameRunning = false;
                    _lastGameRunningCheck = DateTime.UtcNow;
                }
            }
        }
        else
        {
            try
            {
                Shared sharedMemory = R3eSharedMemory.ReadSharedMemory();


                PlayerData playerData = sharedMemory.Player;
                // Local Car Data
                R3ELocalCarMapper.AddR3SharedMemory(sharedMemory, localCar);


                int playerId = sharedMemory.Player.UserId;
                var playerCar = sharedMemory.DriverData.FirstOrDefault(x => x.DriverInfo.UserId == playerId);
                localCar.Engine.IsRunning = playerCar.EngineState >= 1; // 1 = ignition on but not running, 2 = ignition on and starter running, 3 = ignition on and running

                // Game Data
                gameData.Name = Game.RaceRoom.ToShortName();
                gameData.Version = $"{sharedMemory.VersionMajor}.{sharedMemory.VersionMinor}";
                gameData.IsGamePaused = sharedMemory.GamePaused == 1;
                gameData.IsInReplay = sharedMemory.GameInReplay == 1;
            }
            catch (Exception e)
            {
                _isGameRunning = false;
                _lastGameRunningCheck = DateTime.UtcNow;
                Debug.WriteLine(e);
            }
        }
    }

    public override List<string> GetCarClasses() => [];

    public override bool HasTelemetry() => false;

    internal sealed class GraphJob(RaceRoomDataProvider provider) : AbstractLoopJob
    {
        private readonly RaceRoomDataProvider _provider = provider;

        public override void RunAction()
        {
            if (!_provider._isGameRunning)
            {
                SimDataProvider.RacingGraph.ClearGraph();
                return;
            }

            Shared sharedMemory = R3eSharedMemory.ReadSharedMemory();
            R3EDataGraphMapper.AddSharedMemory(SimDataProvider.RacingGraph, sharedMemory);
        }

        public override void AfterCancel()
        {
            SimDataProvider.RacingGraph.ClearGraph();
        }
    }
}

