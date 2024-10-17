namespace RaceElement.Data.Games;

[Flags]
public enum Game : int
{
    Any = (1 << 0),
    AssettoCorsa1 = (1 << 1),
    AssettoCorsaCompetizione = (1 << 2),
    iRacing = (1 << 3),
    RaceRoom = (1 << 4),
    Automobilista2 = (1 << 5),
    //rFactor2,
    // LMU,
}

public static class GameExtensions
{
    private static class FriendlyNames
    {
        public const string AssettoCorsaCompetizione = "Assetto Corsa Competizione";
        public const string AssettoCorsa = "Assetto Corsa";
        public const string IRacing = "iRacing";
        public const string RaceRoom = "RaceRoom Racing Experience";
        public const string Automobilista2 = "Automobilista 2";
    }

    private static class ShortNames
    {
        public const string AssettoCorsaCompetizione = "ACC";
        public const string AssettoCorsa = "AC";
        public const string IRacing = "iRacing";
        public const string RaceRoom = "RaceRoom";
        public const string Automobilista2 = "AMS2";
    }

    public static string ToFriendlyName(this Game game) => game switch
    {
        Game.AssettoCorsa1 => FriendlyNames.AssettoCorsa,
        Game.AssettoCorsaCompetizione => FriendlyNames.AssettoCorsaCompetizione,
        Game.iRacing => FriendlyNames.IRacing,
        Game.RaceRoom => FriendlyNames.RaceRoom,
        Game.Automobilista2 => FriendlyNames.Automobilista2,
        _ => string.Empty
    };

    public static string ToShortName(this Game game) => game switch
    {
        Game.AssettoCorsa1 => ShortNames.AssettoCorsa,
        Game.AssettoCorsaCompetizione => ShortNames.AssettoCorsaCompetizione,
        Game.iRacing => ShortNames.IRacing,
        Game.RaceRoom => ShortNames.RaceRoom,
        Game.Automobilista2 => ShortNames.Automobilista2,
        _ => string.Empty
    };

    public static Game ToGame(this string friendlyName) => friendlyName switch
    {
        FriendlyNames.AssettoCorsa => Game.AssettoCorsa1,
        FriendlyNames.AssettoCorsaCompetizione => Game.AssettoCorsaCompetizione,
        FriendlyNames.IRacing => Game.iRacing,
        FriendlyNames.RaceRoom => Game.RaceRoom,
        FriendlyNames.Automobilista2 => Game.Automobilista2,
        _ => Game.AssettoCorsaCompetizione,
    };

    public static int GetSteamID(this Game game) => game switch
    {
        Game.AssettoCorsa1 => 244210,
        Game.AssettoCorsaCompetizione => 805550,
        Game.iRacing => 266410,
        Game.RaceRoom => 211500,
        Game.Automobilista2 => 1066890,
        _ => -1
    };

    public static Stream? GetSteamLogo(this Game game)
    {
        var enumAssembly = typeof(Game).Assembly;

        var resourceNames = enumAssembly.GetManifestResourceNames();
        var found = resourceNames.FirstOrDefault(x => x.EndsWith($"Logos.{game.ToShortName()}.jpg"));
        if (found == null) return null;

        return enumAssembly.GetManifestResourceStream(found);
    }

    public static Stream? GetGameClientIcon(this Game game)
    {
        var enumAssembly = typeof(Game).Assembly;

        var resourceNames = enumAssembly.GetManifestResourceNames();
        var found = resourceNames.FirstOrDefault(x => x.EndsWith($"Icons.{game.ToShortName()}.ico"));
        if (found == null) return null;

        return enumAssembly.GetManifestResourceStream(found);
    }
}
