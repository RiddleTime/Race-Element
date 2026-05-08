using System.Collections.Immutable;
using System.Diagnostics;

namespace RaceElement.Data.Games;

[Flags]
public enum Game : long
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
    ForzaMotorsport = 1 << 13,
    AssettoCorsaRally = 1 << 14,
    ProjectMotorRacing = 1 << 15,
    DirtRally2 = 1 << 16,
    //BeamNG = 1 << 17,
    RichardBurnsRally = 1 << 18,
    ForzaHorizon4 = 1 << 19,
    MicrosoftFlightSimulator2020 = 1 << 20,
    MicrosoftFlightSimulator2024 = 1 << 21,
}

public static class GameExtensions
{
    private static class FriendlyNames
    {
        public static readonly ImmutableDictionary<Game, string> Map = new Dictionary<Game, string>
        {
            { Game.AssettoCorsa1, "Assetto Corsa" },
            { Game.AssettoCorsaCompetizione, "Assetto Corsa Competizione" },
            { Game.iRacing, "iRacing" },
            { Game.RaceRoom, "RaceRoom Racing Experience" },
            { Game.Automobilista2, "Automobilista 2" },
            { Game.EuroTruckSimulator2, "Euro Truck Simulator 2" },
            { Game.AmericanTruckSimulator, "American Truck Simulator" },
            { Game.AssettoCorsaEvo, "Assetto Corsa EVO" },
            { Game.ForzaHorizon5, "Forza Horizon 5" },
            { Game.ForzaHorizon4, "Forza Horizon 4" },
            { Game.LeMansUltimate, "Le Mans Ultimate" },
            { Game.rFactor2, "rFactor 2" },
            { Game.WRC_Generations, "WRC Generations" },
            { Game.ForzaMotorsport, "Forza Motorsport" },
            { Game.AssettoCorsaRally, "Assetto Corsa Rally" },
            { Game.ProjectMotorRacing, "Project Motor Racing" },
            { Game.DirtRally2, "DiRT Rally 2.0" },
            //{ Game.BeamNG, "BeamNG.drive" },
            { Game.RichardBurnsRally, "Richard Burns Rally" },
            { Game.MicrosoftFlightSimulator2020, "Microsoft Flight Simulator 2020" },
            { Game.MicrosoftFlightSimulator2024, "Microsoft Flight Simulator 2024" }
        }.ToImmutableDictionary();
    }

    private static class ShortNames
    {
        public static readonly ImmutableDictionary<Game, string> Map = new Dictionary<Game, string>
        {
            { Game.AssettoCorsa1, "AC" },
            { Game.AssettoCorsaCompetizione, "ACC" },
            { Game.iRacing, "iRacing" },
            { Game.RaceRoom, "RaceRoom" },
            { Game.Automobilista2, "AMS2" },
            { Game.EuroTruckSimulator2, "ETS2" },
            { Game.AmericanTruckSimulator, "ATS" },
            { Game.AssettoCorsaEvo, "ACE" },
            { Game.ForzaHorizon4, "FH4" },
            { Game.ForzaHorizon5, "FH5" },
            { Game.LeMansUltimate, "LMU" },
            { Game.rFactor2, "rF2" },
            { Game.WRC_Generations, "WRCG" },
            { Game.ForzaMotorsport, "FM" },
            { Game.AssettoCorsaRally, "ACR" },
            { Game.ProjectMotorRacing, "PMR" },
            { Game.DirtRally2, "DR2" },
            //{ Game.BeamNG, "BeamNG" },
            { Game.RichardBurnsRally, "RBR" },
            { Game.MicrosoftFlightSimulator2020, "MFS2020" },
            { Game.MicrosoftFlightSimulator2024, "MFS2024" }
        }.ToImmutableDictionary();
    }

    private static class ExeNames
    {
        public static readonly ImmutableDictionary<string, Game> ProcessMap = new Dictionary<string, Game>
        {
            { "acs", Game.AssettoCorsa1 },
            { "AC2-Win64-Shipping", Game.AssettoCorsaCompetizione },
            { "AssettoCorsaEVO", Game.AssettoCorsaEvo },
            { "iRacingSim64DX11", Game.iRacing },
            { "RRRE", Game.RaceRoom },
            { "RRRE64", Game.RaceRoom },
            { "AMS2AVX", Game.Automobilista2 },
            { "eurotrucks2", Game.EuroTruckSimulator2 },
            { "amtrucks", Game.AmericanTruckSimulator },
            { "ForzaHorizon5", Game.ForzaHorizon5 },
            { "ForzaHorizon4", Game.ForzaHorizon4 },
            { "Le Mans Ultimate", Game.LeMansUltimate },
            { "rFactor2", Game.rFactor2 },
            { "WRCG", Game.WRC_Generations },
            { "forza_steamworks_release_final", Game.ForzaMotorsport },
            { "acr", Game.AssettoCorsaRally },
            { "ProjectMotorRacingGame", Game.ProjectMotorRacing },
            { "dirtrally2", Game.DirtRally2 },
            //{ "BeamNG.drive", Game.BeamNG },
            { "RichardBurnsRally_SSE", Game.RichardBurnsRally },
            { "RichardBurnsRally", Game.RichardBurnsRally },
            { "FlightSimulator", Game.MicrosoftFlightSimulator2020  },
            { "FlightSimulator2024", Game.MicrosoftFlightSimulator2024 }
        }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

        public static readonly Lazy<ImmutableArray<string>> All = new(() => ImmutableArray.Create(ProcessMap.Select(x => x.Key).ToArray()));
    }

    private static class SteamIds
    {
        public static readonly ImmutableDictionary<Game, int> Map = new Dictionary<Game, int>
        {
            { Game.AssettoCorsa1, 244210 },
            { Game.AssettoCorsaCompetizione, 805550 },
            { Game.iRacing, 266410 },
            { Game.RaceRoom, 211500 },
            { Game.Automobilista2, 1066890 },
            { Game.EuroTruckSimulator2, 227300 },
            { Game.AmericanTruckSimulator, 270880 },
            { Game.AssettoCorsaEvo, 3058630 },
            { Game.ForzaHorizon5, 1551360 },
            { Game.ForzaHorizon4, 1293830 },
            { Game.LeMansUltimate, 2399420 },
            { Game.rFactor2, 365960 },
            { Game.WRC_Generations, 1953520 },
            { Game.ForzaMotorsport, 2440510 },
            { Game.AssettoCorsaRally, 3917090 },
            { Game.ProjectMotorRacing, 299970 },
            { Game.DirtRally2, 690790 },
            //{ Game.BeamNG, 284160 },
            { Game.RichardBurnsRally, -1 },
            { Game.MicrosoftFlightSimulator2020, 1250410 },
            { Game.MicrosoftFlightSimulator2024, 2537591 },
        }.ToImmutableDictionary();
    }

    private static readonly Lazy<string[]> ResourceNames = new(() => typeof(Game).Assembly.GetManifestResourceNames());

    public static string ToFriendlyName(this Game game) =>
        FriendlyNames.Map.TryGetValue(game, out var name) ? name : string.Empty;

    public static string ToShortName(this Game game) =>
        ShortNames.Map.TryGetValue(game, out var name) ? name : string.Empty;

    public static Game ToGame(this string? friendlyName) =>
        friendlyName != null && FriendlyNames.Map.ContainsValue(friendlyName)
            ? FriendlyNames.Map.FirstOrDefault(x => x.Value.Equals(friendlyName, StringComparison.OrdinalIgnoreCase)).Key
            : Game.AssettoCorsaCompetizione;

    public static int GetSteamID(this Game game) =>
        SteamIds.Map.TryGetValue(game, out var id) ? id : -1;

    public static Stream? GetSteamLogo(this Game game)
    {
        var shortName = game.ToShortName();
        if (string.IsNullOrEmpty(shortName)) return null;

        var resourceName = ResourceNames.Value.FirstOrDefault(x => x.EndsWith($"Logos.{shortName}.jpg", StringComparison.OrdinalIgnoreCase));
        return resourceName != null ? typeof(Game).Assembly.GetManifestResourceStream(resourceName) : null;
    }

    public static Stream? GetGameClientIcon(this Game game)
    {
        var shortName = game.ToShortName();
        if (string.IsNullOrEmpty(shortName)) return null;

        var resourceName = ResourceNames.Value.FirstOrDefault(x => x.EndsWith($"Icons.{shortName}.ico", StringComparison.OrdinalIgnoreCase));
        return resourceName != null ? typeof(Game).Assembly.GetManifestResourceStream(resourceName) : null;
    }

    public static Game GetRunningGame()
    {
        try
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (ExeNames.All.Value.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                    {
                        var game = GameFromProcessName(process.ProcessName);
                        if (game != Game.Any)
                        {
                            process?.Dispose();
                            return game;
                        }
                    }
                }
                finally
                {
                    process?.Dispose();
                }
            }
        }
        catch (Exception)
        {
        }
        return Game.Any;
    }

    public static Game GameFromProcessName(string? processName)
    {
        return (processName != null && ExeNames.ProcessMap.TryGetValue(processName, out var game))
        ? game
        : Game.Any;
    }
}