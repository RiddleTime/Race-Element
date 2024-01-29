
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
        static class FriendlyNames
        {
            public const string AssettoCorsaCompetizione = "ACC";
            public const string AssettoCorsa = "AC1";
        }

        public static string ToFriendlyName(this Game game) => game switch
        {
            Game.AssettoCorsa1 => FriendlyNames.AssettoCorsa,
            Game.AssettoCorsaCompetizione => FriendlyNames.AssettoCorsaCompetizione,
            _ => string.Empty
        };

        public static Game ToGame(this string friendlyName) => friendlyName switch
        {
            FriendlyNames.AssettoCorsa => Game.AssettoCorsa1,
            FriendlyNames.AssettoCorsaCompetizione => Game.AssettoCorsaCompetizione,
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
