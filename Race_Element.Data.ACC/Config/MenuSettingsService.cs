using Newtonsoft.Json;
using RaceElement.Util;
using System.Collections.Generic;

namespace RaceElement.Data.ACC.Config
{
    public class MenuSettingsService
    {
        private readonly MenuSettings cameraSettings = new();

        public MenuSettings Settings() => cameraSettings;

        public void ResetLiverySettings()
        {
            var settings = cameraSettings.Get(false);
            settings.TexDDS = 1;
            settings.TexCap = 1;
            cameraSettings.Save(settings);
        }

        public class MenuSettings : AbstractSettingsJson<MenuSettingsJson>
        {
            public override string Path => FileUtil.AccConfigPath;

            public override string FileName => "menuSettings.json";

            public override MenuSettingsJson Default() => new();
        }

        public class MenuSettingsJson : IGenericSettingsJson
        {
            [JsonProperty("singlePlayerSeason")]
            public string SinglePlayerSeason { get; set; }

            [JsonProperty("seasonGameMode")]
            public object SeasonGameMode { get; set; }

            [JsonProperty("seasonRaceEventData")]
            public object SeasonRaceEventData { get; set; }

            [JsonProperty("globalSeasonTrackName")]
            public object GlobalSeasonTrackName { get; set; }

            [JsonProperty("globalOpponentCount")]
            public int GlobalOpponentCount { get; set; }

            [JsonProperty("globalOpponentSkill")]
            public int GlobalOpponentSkill { get; set; }

            [JsonProperty("globalOpponentAggro")]
            public int GlobalOpponentAggro { get; set; }

            [JsonProperty("globalPositionOnGrid")]
            public int GlobalPositionOnGrid { get; set; }

            [JsonProperty("globalCarGroupRatios")]
            public object GlobalCarGroupRatios { get; set; }

            [JsonProperty("useNewGenGT3")]
            public int UseNewGenGT3 { get; set; }

            [JsonProperty("weatherType")]
            public string WeatherType { get; set; }

            [JsonProperty("bUseEnduranceKit")]
            public bool BUseEnduranceKit { get; set; }

            [JsonProperty("audio")]
            public object Audio { get; set; }

            [JsonProperty("currentLanguage")]
            public string CurrentLanguage { get; set; }

            [JsonProperty("multiplayerCarGroupSelection")]
            public object MultiplayerCarGroupSelection { get; set; }

            [JsonProperty("templateOverrides")]
            public object TemplateOverrides { get; set; }

            [JsonProperty("serverPasswords")]
            public object ServerPasswords { get; set; }

            [JsonProperty("specialEventPage")]
            public int SpecialEventPage { get; set; }

            [JsonProperty("isPrivacyPolicyAccepted")]
            public bool IsPrivacyPolicyAccepted { get; set; }

            [JsonProperty("lastNewsItemIdRead")]
            public string LastNewsItemIdRead { get; set; }

            [JsonProperty("useFallbackControlPage")]
            public bool UseFallbackControlPage { get; set; }

            [JsonProperty("isNativeDBoxEnabled")]
            public bool IsNativeDBoxEnabled { get; set; }

            [JsonProperty("isNativeFanatecLedsEnabled")]
            public bool IsNativeFanatecLedsEnabled { get; set; }

            [JsonProperty("isFirstLaunchDataInserted")]
            public bool IsFirstLaunchDataInserted { get; set; }

            [JsonProperty("skipIntroSequence")]
            public bool SkipIntroSequence { get; set; }

            [JsonProperty("pitcrewAnimationEnabled")]
            public bool PitcrewAnimationEnabled { get; set; }

            [JsonProperty("pitmarkersVisible")]
            public bool PitmarkersVisible { get; set; }

            [JsonProperty("graphicOptions")]
            public object GraphicOptions { get; set; }

            [JsonProperty("texCap")]
            public int TexCap { get; set; }

            [JsonProperty("texDDS")]
            public int TexDDS { get; set; }

            [JsonProperty("texMips")]
            public int TexMips { get; set; }

            [JsonProperty("customGraphicOptions")]
            public object CustomGraphicOptions { get; set; }

            [JsonProperty("customDriverInfoNames")]
            public List<object> CustomDriverInfoNames { get; set; }

            [JsonProperty("lastFilter")]
            public string LastFilter { get; set; }

            [JsonProperty("lastSeasonFilter")]
            public string LastSeasonFilter { get; set; }

            [JsonProperty("mPShowroomCarGroup")]
            public string MPShowroomCarGroup { get; set; }

            [JsonProperty("mPShowroomCarGroupFilter")]
            public string MPShowroomCarGroupFilter { get; set; }

            [JsonProperty("mPServerListCarGroupFilter")]
            public string MPServerListCarGroupFilter { get; set; }

            [JsonProperty("mpServerListFilter")]
            public object MpServerListFilter { get; set; }

            [JsonProperty("carsPreloadMode")]
            public int CarsPreloadMode { get; set; }

            [JsonProperty("carsPreloadLimitMB")]
            public int CarsPreloadLimitMB { get; set; }
        }


    }
}
