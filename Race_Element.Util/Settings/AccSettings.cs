using System;

namespace RaceElement.Util.Settings;

public sealed class AccSettingsJson : IGenericSettingsJson
{
    public Guid UnlistedAccServer { get; set; }
}

public sealed class AccSettings : AbstractSettingsJson<AccSettingsJson>
{
    public override string Path => FileUtil.RaceElementSettingsPath;
    public override string FileName => "ACC.json";

    public override AccSettingsJson Default() => new()
    {
        UnlistedAccServer = Guid.Empty,
    };
}
