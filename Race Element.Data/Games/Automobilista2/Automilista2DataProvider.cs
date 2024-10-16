using System.Diagnostics;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.Automobilista2.DataMapper;
using RaceElement.Data.Games.Automobilista2.SharedMemory;

namespace RaceElement.Data.Games.Automobilista2;

internal sealed class Automobilista2DataProvider : AbstractSimDataProvider
{
    private bool _isGameRunning = false;
    private DateTime _lastGameRunningCheck = DateTime.MinValue;

    internal sealed override void Start()
    {
    }

    internal sealed override void Stop()
    {
    }

    public override List<string> GetCarClasses() => [];
    public override bool HasTelemetry() => false;
    internal override int PollingRate() => 300;

    public sealed override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        if (!_isGameRunning)
        {
            if (DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(2)) > _lastGameRunningCheck)
            {
                if (Process.GetProcessesByName("AMS2AVX").Length > 0)
                    _isGameRunning = true;
                else
                    _lastGameRunningCheck = DateTime.UtcNow;
            }
        }
        else
        {
            try
            {
                Shared sharedMemory = AMS2SharedMemory.ReadSharedMemory();
                Ams2Mapper.ToLocalSession(sharedMemory, sessionData);
                Ams2Mapper.ToLocalCar(sharedMemory, localCar);

                // Game Data
                gameData.Name = Game.Automobilista2.ToShortName();
                gameData.Version = $"{sharedMemory.mVersion} - {sharedMemory.mBuildVersionNumber}";
                gameData.IsGamePaused = sharedMemory.mGameState == (UInt32)(Constants.GameState.GAME_INGAME_PAUSED);
                gameData.IsInReplay = sharedMemory.mGameState == (UInt32)(Constants.GameState.GAME_INGAME_REPLAY);
            }
            catch (Exception e)
            {
                _isGameRunning = false;
                _lastGameRunningCheck = DateTime.UtcNow;
                Debug.WriteLine(e);
            }
        }
    }
}
