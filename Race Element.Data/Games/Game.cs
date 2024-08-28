
namespace RaceElement.Data.Games
{
    [Flags]
    public enum Game
    {
        None = 0,
        AssettoCorsa1 = 1,
        AssettoCorsaCompetizione = 2,
        iRacing = 4,
        //rFactor2,
        //Automobilista2
    }

    public static class GameExtensions
    {
        static class FriendlyNames
        {
            public const string AssettoCorsaCompetizione = "Assetto Corsa Competizione";
            public const string AssettoCorsa = "Assetto Corsa";
            public const string IRacing = "iRacing";
        }

        static class ShortNames
        {
            public const string AssettoCorsaCompetizione = "ACC";
            public const string AssettoCorsa = "AC";
            public const string IRacing = "iRacing";
        }

        public static string ToFriendlyName(this Game game) => game switch
        {
            Game.AssettoCorsa1 => FriendlyNames.AssettoCorsa,
            Game.AssettoCorsaCompetizione => FriendlyNames.AssettoCorsaCompetizione,
            Game.iRacing => FriendlyNames.IRacing,
            _ => string.Empty
        };

        public static string ToShortName(this Game game) => game switch
        {
            Game.AssettoCorsa1 => ShortNames.AssettoCorsa,
            Game.AssettoCorsaCompetizione => ShortNames.AssettoCorsaCompetizione,
            Game.iRacing => ShortNames.IRacing,
            _ => string.Empty
        };

        public static Game ToGame(this string friendlyName) => friendlyName switch
        {
            FriendlyNames.AssettoCorsa => Game.AssettoCorsa1,
            FriendlyNames.AssettoCorsaCompetizione => Game.AssettoCorsaCompetizione,
            FriendlyNames.IRacing => Game.iRacing,
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
