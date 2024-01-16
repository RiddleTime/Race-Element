using System;

namespace RaceElement.Util.Settings;

public class StreamingSettingsJson : IGenericSettingsJson
{
    public string StreamingSoftware { get; set; }
    public string StreamingWebSocketIP { get; set; }
    public string StreamingWebSocketPort { get; set; }
    public string StreamingWebSocketPassword { get; set; }

    public bool SetupHider { get; set; }
}

public class StreamSettings : AbstractSettingsJson<StreamingSettingsJson>
{
    public override string Path => FileUtil.RaceElementSettingsPath;
    public override string FileName => "Streaming.json";


    public override StreamingSettingsJson Default()
    {
        var settings = new StreamingSettingsJson()
        {
            StreamingSoftware = "OBS",
            StreamingWebSocketIP = "localhost",
            StreamingWebSocketPort = "4444",
            StreamingWebSocketPassword = String.Empty,
            SetupHider = false
        };
        return settings;
    }
}
