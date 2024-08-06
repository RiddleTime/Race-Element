using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using RaceElement.Data.Games.iRacing;

namespace RaceElement.Data.Games;

public static class GameManager
{
    private static SimpleLoopJob _job;

    public static Game CurrentGame { get; private set; } = Game.AssettoCorsaCompetizione;

    public static event EventHandler<Game>? OnGameChanged;
    public static void SetCurrentGame(Game game)
    {
        ExitGameData(CurrentGame);

        CurrentGame = game;
        SimDataProvider.Clear();
        OnGameChanged?.Invoke(null, game);

        // start job to update SimDataProvider every 50ms
        _job = new SimpleLoopJob() { Action = () => SimDataProvider.Update(), IntervalMillis = 1000 / 50 };
        _job.Run();                 
    }


    /// <summary>
    /// Gracefully disposes and stops all mechanisms that are required for a game so they do not interfere with other games.
    /// </summary>
    /// <param name="game"></param>
    private static void ExitGameData(Game game)
    {
        switch (game)
        {
            case Game.iRacing:
                {
                    IRacingDataProvider.Stop();
                    break;
                }
        }
        if (_job != null) {
            _job.CancelJoin();
        }

    }
}
