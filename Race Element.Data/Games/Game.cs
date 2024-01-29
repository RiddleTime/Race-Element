using static System.Net.WebRequestMethods;

namespace RaceElement.Data.Games
{

    public enum Game
    {
        AssettoCorsa1,
        AssettoCorsaCompetizione,
        //rFactor2,
        //iRacing,
    }

    public static class GameExtensions
    {
        public static string ToFriendlyName(this Game game) => game switch
        {
            Game.AssettoCorsa1 => "AC1",
            Game.AssettoCorsaCompetizione => "ACC",
            _ => string.Empty
        };

        public static Game ToGame(this string friendlyName) => friendlyName switch
        {
            "AC1" => Game.AssettoCorsa1,
            "ACC" => Game.AssettoCorsaCompetizione,
            _ => Game.AssettoCorsaCompetizione,
        };

        public static int GetSteamID(this Game game) => game switch
        {
            Game.AssettoCorsa1 => 244210,
            Game.AssettoCorsaCompetizione => 805550,
            _ => -1
        };
    }
}
