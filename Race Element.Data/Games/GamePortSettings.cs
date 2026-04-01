using RaceElement.Core.Settings;

namespace RaceElement.Data.Games;

public sealed class GamePortSettingsJson : IGenericSettingsJson
{
    public Dictionary<Game, int> GamePorts { get; set; }
}

public sealed class GamePortSettings : AbstractSettingsJson<GamePortSettingsJson>
{
    public override string Path => FileUtil.RaceElementSettingsPath;
    public override string FileName => "GamePortSettings.json";
    public override GamePortSettingsJson Default() => new()
    {
        GamePorts = new Dictionary<Game, int>()
        {
            { Game.ProjectMotorRacing, 7576 },
            { Game.ForzaHorizon4, 5300 },
            { Game.ForzaHorizon5, 5300 },
            { Game.ForzaMotorsport, 5300 },
            { Game.RichardBurnsRally, 6776 },
        }
    };
}
