using RaceElement.Core.Settings;

namespace RaceElement.Data.Games.Forza;

public sealed class ForzaRevLimiterCacheJson : IGenericSettingsJson
{
    /// <summary>
    /// Detected rev limiter per engine, keyed by game, car ordinal and EngineMaxRpm.
    /// Car ordinals are shared between Forza titles while the physics are not, so the
    /// game has to be part of the key.
    /// </summary>
    public Dictionary<string, int> RevLimiters { get; set; } = [];
}

public sealed class ForzaRevLimiterCache : AbstractSettingsJson<ForzaRevLimiterCacheJson>
{
    public override string Path => FileUtil.RaceElementDataPath;
    public override string FileName => "ForzaRevLimiters.json";
    public override ForzaRevLimiterCacheJson Default() => new();

    public static string BuildKey(Game game, int carOrdinal, int engineMaxRpm) => $"{game}-{carOrdinal}-{engineMaxRpm}";
}
