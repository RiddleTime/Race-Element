namespace RaceElement.Data.Games;

[Flags]
public enum Game : int
{
    Any = 0,
    AssettoCorsa1 = 1,
    AssettoCorsaCompetizione = 2,
    iRacing = 4,
    RaceRoom = 8,
    //rFactor2,
    // LMU,
    //Automobilista2
}

public static class GameExtensions
{
    private static class FriendlyNames
    {
        public const string AssettoCorsaCompetizione = "Assetto Corsa Competizione";
        public const string AssettoCorsa = "Assetto Corsa";
        public const string IRacing = "iRacing";
        public const string RaceRoom = "RaceRoom Racing Experience";
    }

    private static class ShortNames
    {
        public const string AssettoCorsaCompetizione = "ACC";
        public const string AssettoCorsa = "AC";
        public const string IRacing = "iRacing";
        public const string RaceRoom = "RaceRoom";
    }

    public static string ToFriendlyName(this Game game) => game switch
    {
        Game.AssettoCorsa1 => FriendlyNames.AssettoCorsa,
        Game.AssettoCorsaCompetizione => FriendlyNames.AssettoCorsaCompetizione,
        Game.iRacing => FriendlyNames.IRacing,
        Game.RaceRoom => FriendlyNames.RaceRoom,
        _ => string.Empty
    };

    public static string ToShortName(this Game game) => game switch
    {
        Game.AssettoCorsa1 => ShortNames.AssettoCorsa,
        Game.AssettoCorsaCompetizione => ShortNames.AssettoCorsaCompetizione,
        Game.iRacing => ShortNames.IRacing,
        Game.RaceRoom => ShortNames.RaceRoom,
        _ => string.Empty
    };

    public static Game ToGame(this string friendlyName) => friendlyName switch
    {
        FriendlyNames.AssettoCorsa => Game.AssettoCorsa1,
        FriendlyNames.AssettoCorsaCompetizione => Game.AssettoCorsaCompetizione,
        FriendlyNames.IRacing => Game.iRacing,
        FriendlyNames.RaceRoom => Game.RaceRoom,
        _ => Game.AssettoCorsaCompetizione,
    };

    public static int GetSteamID(this Game game) => game switch
    {
        Game.AssettoCorsa1 => 244210,
        Game.AssettoCorsaCompetizione => 805550,
        Game.iRacing => 266410,
        Game.RaceRoom => 211500,
        _ => -1
    };
}
