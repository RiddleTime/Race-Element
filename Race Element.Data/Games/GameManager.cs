using RaceElement.Data.Common;

namespace RaceElement.Data.Games;

public static class GameManager
{
    public static Game CurrentGame { get; private set; } = Game.AssettoCorsaCompetizione;

    public static void SetCurrentGame(Game game)
    {
        CurrentGame = game;
        SimDataProvider.Clear();
    }
}
