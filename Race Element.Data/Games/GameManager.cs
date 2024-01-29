using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games
{
    public static class GameManager
    {
        public static Game CurrentGame { get; private set; } = Game.AssettoCorsaCompetizione;

        public static void SetCurrentGame(Game game)
        {
            CurrentGame = game;
        }
    }
}
