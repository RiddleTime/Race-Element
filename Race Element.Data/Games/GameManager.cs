using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using RaceElement.Data.Games.iRacing;

namespace RaceElement.Data.Games;

public static class GameManager
{
    private readonly static SimpleLoopJob _dataUpdaterJob = new() { Action = () => SimDataProvider.Update(), IntervalMillis = 1000 / 50 };

    public static Game CurrentGame { get; private set; } = Game.Any;

    public static event EventHandler<Game>? OnGameChanged;
    public static void SetCurrentGame(Game game)
    {
        ExitGameData(CurrentGame);

        CurrentGame = game;
        SimDataProvider.Clear();
        OnGameChanged?.Invoke(null, game);

        _dataUpdaterJob.Run();
    }

    /// <summary>
    /// Gracefully disposes and stops all mechanisms that are required for a game so they do not interfere with other games.
    /// </summary>
    /// <param name="game"></param>
    private static void ExitGameData(Game game)
    {        
        SimDataProvider.Stop();        
        _dataUpdaterJob?.CancelJoin();
    }
}
