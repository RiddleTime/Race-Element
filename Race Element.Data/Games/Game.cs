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

    private static class GameLogos
    {
        public const string AssettoCorsaCompetizione = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/805550/df4663c038048aa1507d8e026f22144fcfd64648.jpg";
        public const string AssettoCorsa = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/244210/b8852bb29c32345917e61c05f159a4196d205da2.jpg";
        public const string IRacing = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/266410/7c3c0f2b64ad34e238a42d0d48c4c5d67da02212.jpg";
        public const string RaceRoom = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/211500/5a9c89c56ea9ac16f4ecd45693a9fe532e3225cb.jpg";
        public const string Automobilista2 = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/1066890/a48329aed7df04cd2b66af2a68ae046e69548163.jpg";
    }

    private static class GameClientIcons
    {
        public const string AssettoCorsaCompetizione = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/805550/b37fb9f7534d471eb5e8e40fcd99a2a31003104f.ico";
        public const string AssettoCorsa = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/244210/2da95c73fc0c607dc9c0448f33577956112bd2d6.ico";
        public const string IRacing = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/266410/bd3b34b608b98040344ba2b51b6bd20197b177ba.ico";
        public const string RaceRoom = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/211500/da340c1f72ac289df39798033ee3253a7d5d469f.ico";
        public const string Automobilista2 = "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/1066890/619c117330af778dc9669b75e78c026048a462d5.ico";
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

    public static string GetSteamLogo(this Game game) => game switch
    {
        Game.AssettoCorsa1 => GameLogos.AssettoCorsa,
        Game.AssettoCorsaCompetizione => GameLogos.AssettoCorsaCompetizione,
        Game.iRacing => GameLogos.IRacing,
        Game.RaceRoom => GameLogos.RaceRoom,
        Game.Automobilista2 => GameLogos.Automobilista2,
        _ => string.Empty
    };

    public static string GetGameClientIcon(this Game game) => game switch
    {
        Game.AssettoCorsa1 => GameClientIcons.AssettoCorsa,
        Game.AssettoCorsaCompetizione => GameClientIcons.AssettoCorsaCompetizione,
        Game.iRacing => GameClientIcons.IRacing,
        Game.RaceRoom => GameClientIcons.RaceRoom,
        Game.Automobilista2 => GameClientIcons.Automobilista2,
        _ => string.Empty
    };
}
