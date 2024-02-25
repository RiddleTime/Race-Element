using RaceElement.Data.Common;
using RaceElement.Data.Games.iRacing;

namespace RaceElement.Data.Games;

public static class GameManager
{
    public static Game CurrentGame { get; private set; } = Game.AssettoCorsaCompetizione;

    public static event EventHandler<Game>? OnGameChanged;
    public static void SetCurrentGame(Game game)
    {
        if (CurrentGame == Game.iRacing)
            IRacingDataProvider.Stop();

        CurrentGame = game;
        SimDataProvider.Clear();
        OnGameChanged?.Invoke(null, game);
    }
}
