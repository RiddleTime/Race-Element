using System.Diagnostics;

namespace RaceElement.Data.Games;

[Flags]
public enum Game : int
{
    Any = 1 << 0,
    AssettoCorsa1 = 1 << 1,
    AssettoCorsaCompetizione = 1 << 2,
    iRacing = 1 << 3,
    RaceRoom = 1 << 4,
    Automobilista2 = 1 << 5,
    EuroTruckSimulator2 = 1 << 6,
    AmericanTruckSimulator = 1 << 7,
    AssettoCorsaEvo = 1 << 8,
    ForzaHorizon5 = 1 << 9,
    LeMansUltimate = 1 << 10,
    rFactor2 = 1 << 11,
    WRC_Generations = 1 << 12,
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
        public const string EuroTruckSimulator2 = "Euro Truck Simulator 2";
        public const string AmericanTruckSimulator = "American Truck Simulator";
        public const string AssettoCorsaEvo = "Assetto Corsa EVO";
        public const string ForzaHorizon5 = "Forza Horizon 5";
        public const string LeMansUltimate = "Le Mans Ultimate";
        public const string RFactor2 = "rFactor 2";
        public const string WRC_Generations = "WRC Generations";
    }

    private static class ShortNames
    {
        public const string AssettoCorsaCompetizione = "ACC";
        public const string AssettoCorsa = "AC";
        public const string IRacing = "iRacing";
        public const string RaceRoom = "RaceRoom";
        public const string Automobilista2 = "AMS2";
        public const string EuroTruckSimulator2 = "ETS2";
        public const string AmericanTruckSimulator = "ATS";
        public const string AssettoCorsaEvo = "ACE";
        public const string ForzaHorizon5 = "FH5";
        public const string LeMansUltimate = "LMU";
        public const string RFactor2 = "rF2";
        public const string WRC_Generations = "WRCG";
    }

    private static class ExeNames
    {
        public static readonly string[] All =
        [
            AssettoCorsa, AssettoCorsaCompetizione, AssettoCorsaEvo, IRacing, RaceRoom, RaceRoomX64, Automobilista2,
            EuroTruckSimulator2, AmericanTruckSimulator, ForzaHorizon5, LeMansUltimate, RFactor2, WRC_Generations,
        ];

        public const string AssettoCorsaCompetizione = "AC2-Win64-Shipping";
        public const string AssettoCorsa = "acs";
        public const string IRacing = "iRacingSim64DX11";
        public const string RaceRoomX64 = "RRRE64";
        public const string RaceRoom = "RRRE";
        public const string Automobilista2 = "AMS2AVX";
        public const string EuroTruckSimulator2 = "eurotrucks2";
        public const string AmericanTruckSimulator = "amtrucks";
        public const string AssettoCorsaEvo = "AssettoCorsaEVO";
        public const string ForzaHorizon5 = "ForzaHorizon5";
        public const string LeMansUltimate = "Le Mans Ultimate";
        public const string RFactor2 = "rFactor2";
        public const string WRC_Generations = "WRCG";
    }

    public static Game GameFromProcessName(string processName) => processName switch
    {
        ExeNames.AssettoCorsa => Game.AssettoCorsa1,
        ExeNames.AssettoCorsaCompetizione => Game.AssettoCorsaCompetizione,
        ExeNames.Automobilista2 => Game.Automobilista2,
        ExeNames.IRacing => Game.iRacing,
        ExeNames.RaceRoom => Game.RaceRoom,
        ExeNames.RaceRoomX64 => Game.RaceRoom,
        ExeNames.EuroTruckSimulator2 => Game.EuroTruckSimulator2,
        ExeNames.AmericanTruckSimulator => Game.AmericanTruckSimulator,
        ExeNames.AssettoCorsaEvo => Game.AssettoCorsaEvo,
        ExeNames.ForzaHorizon5 => Game.ForzaHorizon5,
        ExeNames.LeMansUltimate => Game.LeMansUltimate,
        ExeNames.RFactor2 => Game.rFactor2,
        ExeNames.WRC_Generations => Game.WRC_Generations,
        _ => Game.Any,
    };

    public static string ToFriendlyName(this Game game) => game switch
    {
        Game.AssettoCorsa1 => FriendlyNames.AssettoCorsa,
        Game.AssettoCorsaCompetizione => FriendlyNames.AssettoCorsaCompetizione,
        Game.iRacing => FriendlyNames.IRacing,
        Game.RaceRoom => FriendlyNames.RaceRoom,
        Game.Automobilista2 => FriendlyNames.Automobilista2,
        Game.EuroTruckSimulator2 => FriendlyNames.EuroTruckSimulator2,
        Game.AmericanTruckSimulator => FriendlyNames.AmericanTruckSimulator,
        Game.AssettoCorsaEvo => FriendlyNames.AssettoCorsaEvo,
        Game.ForzaHorizon5 => FriendlyNames.ForzaHorizon5,
        Game.LeMansUltimate => FriendlyNames.LeMansUltimate,
        Game.rFactor2 => FriendlyNames.RFactor2,
        Game.WRC_Generations => FriendlyNames.WRC_Generations,
        _ => string.Empty
    };

    public static string ToShortName(this Game game) => game switch
    {
        Game.AssettoCorsa1 => ShortNames.AssettoCorsa,
        Game.AssettoCorsaCompetizione => ShortNames.AssettoCorsaCompetizione,
        Game.iRacing => ShortNames.IRacing,
        Game.RaceRoom => ShortNames.RaceRoom,
        Game.Automobilista2 => ShortNames.Automobilista2,
        Game.EuroTruckSimulator2 => ShortNames.EuroTruckSimulator2,
        Game.AmericanTruckSimulator => ShortNames.AmericanTruckSimulator,
        Game.AssettoCorsaEvo => ShortNames.AssettoCorsaEvo,
        Game.ForzaHorizon5 => ShortNames.ForzaHorizon5,
        Game.LeMansUltimate => ShortNames.LeMansUltimate,
        Game.rFactor2 => ShortNames.RFactor2,
        Game.WRC_Generations => ShortNames.WRC_Generations,
        _ => string.Empty
    };

    public static Game ToGame(this string friendlyName) => friendlyName switch
    {
        FriendlyNames.AssettoCorsa => Game.AssettoCorsa1,
        FriendlyNames.AssettoCorsaCompetizione => Game.AssettoCorsaCompetizione,
        FriendlyNames.IRacing => Game.iRacing,
        FriendlyNames.RaceRoom => Game.RaceRoom,
        FriendlyNames.Automobilista2 => Game.Automobilista2,
        FriendlyNames.EuroTruckSimulator2 => Game.EuroTruckSimulator2,
        FriendlyNames.AmericanTruckSimulator => Game.AmericanTruckSimulator,
        FriendlyNames.AssettoCorsaEvo => Game.AssettoCorsaEvo,
        FriendlyNames.ForzaHorizon5 => Game.ForzaHorizon5,
        FriendlyNames.LeMansUltimate => Game.LeMansUltimate,
        FriendlyNames.RFactor2 => Game.rFactor2,
        FriendlyNames.WRC_Generations => Game.WRC_Generations,
        _ => Game.AssettoCorsaCompetizione,
    };

    public static int GetSteamID(this Game game) => game switch
    {
        Game.AssettoCorsa1 => 244210,
        Game.AssettoCorsaCompetizione => 805550,
        Game.iRacing => 266410,
        Game.RaceRoom => 211500,
        Game.Automobilista2 => 1066890,
        Game.EuroTruckSimulator2 => 227300,
        Game.AmericanTruckSimulator => 270880,
        Game.AssettoCorsaEvo => 3058630,
        Game.ForzaHorizon5 => 1551360,
        Game.LeMansUltimate => 2399420,
        Game.rFactor2 => 365960,
        Game.WRC_Generations => 1953520,
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

    public static Game GetRunningGame()
    {
        var processes = Process.GetProcesses();

        foreach (string exeName in ExeNames.All)
        {
            using Process? process = processes.FirstOrDefault(x => x.ProcessName == exeName);
            if (process != null)
                return GameFromProcessName(process.ProcessName);
        }

        return Game.Any;
    }
}
