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
#if DEBUG
        _graphjob = new(this) { IntervalMillis = 500 };
        _graphjob.Run();
#endif
    }

    internal sealed override void Stop()
    {
#if DEBUG
        _graphjob.CancelJoin();
#endif
    }

    private float _lastLocationX = default;
    private int _velocityBuffer = 0;
    private const int _maxVelocityBuffer = 800;

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

                // Local Car Data
                R3ELocalCarMapper.AddR3SharedMemory(sharedMemory, localCar);


                if (_lastLocationX == localCar.Physics.Location.X && localCar.Physics.Velocity < 0.1f)
                {
                    _velocityBuffer++;

                    if (_velocityBuffer > _maxVelocityBuffer)
                        localCar.Engine.IsRunning = false;
                }
                else
                {
                    _velocityBuffer = 0;
                    localCar.Engine.IsRunning = true;
                }

                _lastLocationX = localCar.Physics.Location.X;


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

