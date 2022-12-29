using RaceElement.Util;
using RaceElement.Util;
using Newtonsoft.Json;

namespace RaceElement.Data.ACC.Core.Config
{
    public class ReplaySettingsJson : IGenericSettingsJson
    {
        [JsonProperty("replayQuality")]
        public int ReplayQuality { get; set; }

        [JsonProperty("farAIReplayQuality")]
        public int FarAIReplayQuality { get; set; }

        [JsonProperty("automaticHiglightEnabled")]
        public int AutomaticHiglightEnabled { get; set; }

        [JsonProperty("useOnlyPlayerCarHighlights")]
        public int UseOnlyPlayerCarHighlights { get; set; }

        [JsonProperty("highlightsRecTimeBefore")]
        public int HighlightsRecTimeBefore { get; set; }

        [JsonProperty("highlightsRecTimeAfter")]
        public int HighlightsRecTimeAfter { get; set; }

        [JsonProperty("maxTimeReplaySeconds")]
        public int MaxTimeReplaySeconds { get; set; }

        [JsonProperty("autoSaveEnabled")]
        public int AutoSaveEnabled { get; set; }

        [JsonProperty("autoSaveRaceNumber")]
        public int AutoSaveRaceNumber { get; set; }

        [JsonProperty("autoSaveQualifyNumber")]
        public int AutoSaveQualifyNumber { get; set; }

        [JsonProperty("autoSaveOthersNumber")]
        public int AutoSaveOthersNumber { get; set; }

        [JsonProperty("autoSaveMinTimeSeconds")]
        public int AutoSaveMinTimeSeconds { get; set; }
    }

    public class ReplaySettings : AbstractSettingsJson<ReplaySettingsJson>
    {
        public override string Path => FileUtil.AccConfigPath;

        public override string FileName => "replay.json";

        public override ReplaySettingsJson Default()
        {
            return new ReplaySettingsJson();
        }
    }
}
