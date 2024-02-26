using RaceElement.Data.Common;
using RaceElement.Data.Games.iRacing;

namespace RaceElement.Data.Games;

public static class GameManager
{
    public static Game CurrentGame { get; private set; } = Game.AssettoCorsaCompetizione;

    public static event EventHandler<Game>? OnGameChanged;
    public static void SetCurrentGame(Game game)
    {
        ExitGameData(CurrentGame);

        CurrentGame = game;
        SimDataProvider.Clear();
        OnGameChanged?.Invoke(null, game);
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

    }
}
