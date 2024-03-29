﻿using Newtonsoft.Json;
using RaceElement.Util;

namespace RaceElement.Data.ACC.Config;

public class HudSettingsService
{
    private readonly AccHudSettings hudSettings = new();

    public AccHudSettings Settings() => hudSettings;

    public class AccHudSettings : AbstractSettingsJson<AccHudSettingsJson>
    {
        public override string Path => FileUtil.AccConfigPath;
        public override string FileName => "hud.json";

        public override AccHudSettingsJson Default() => new();
    }

    public class AccHudSettingsJson : IGenericSettingsJson
    {
        [JsonProperty("safezoneLeft")]
        public int SafezoneLeft { get; set; }

        [JsonProperty("safezoneTop")]
        public int SafezoneTop { get; set; }

        [JsonProperty("safezoneRight")]
        public int SafezoneRight { get; set; }

        [JsonProperty("safezoneBottom")]
        public int SafezoneBottom { get; set; }

        [JsonProperty("defaultHUDPage")]
        public int DefaultHUDPage { get; set; }

        [JsonProperty("newOverlayHUDPage")]
        public int NewOverlayHUDPage { get; set; }

        [JsonProperty("raceNotificationsVisible")]
        public int RaceNotificationsVisible { get; set; }

        [JsonProperty("ratingWidgetVisible")]
        public int RatingWidgetVisible { get; set; }

        [JsonProperty("ratingWidgetPracticeFocus")]
        public int RatingWidgetPracticeFocus { get; set; }

        [JsonProperty("ratingSensitiveWidgetVisibility")]
        public bool RatingSensitiveWidgetVisibility { get; set; }

        [JsonProperty("basicCarInfoVisible")]
        public int BasicCarInfoVisible { get; set; }

        [JsonProperty("fPSVisible")]
        public int FPSVisible { get; set; }

        [JsonProperty("electronicsVisible")]
        public int ElectronicsVisible { get; set; }

        [JsonProperty("hotlapStandingVisible")]
        public int HotlapStandingVisible { get; set; }

        [JsonProperty("laptimeInfo01Visible")]
        public int LaptimeInfo01Visible { get; set; }

        [JsonProperty("leaderboardVisible")]
        public int LeaderboardVisible { get; set; }

        [JsonProperty("trackMapVisible")]
        public int TrackMapVisible { get; set; }

        [JsonProperty("raceDirectorInvestigationVisible")]
        public int RaceDirectorInvestigationVisible { get; set; }

        [JsonProperty("raceRealtimeStandingVisible")]
        public int RaceRealtimeStandingVisible { get; set; }

        [JsonProperty("raceStandingVisible")]
        public int RaceStandingVisible { get; set; }

        [JsonProperty("radarVisible")]
        public int RadarVisible { get; set; }

        [JsonProperty("sessionInfoVisible")]
        public int SessionInfoVisible { get; set; }

        [JsonProperty("tyreTemps01Visible")]
        public int TyreTemps01Visible { get; set; }

        [JsonProperty("timeLeftWidgetVisible")]
        public int TimeLeftWidgetVisible { get; set; }

        [JsonProperty("wrongWayVisible")]
        public int WrongWayVisible { get; set; }

        [JsonProperty("virtualMirrorVisible")]
        public int VirtualMirrorVisible { get; set; }

        [JsonProperty("serverStatsVisible")]
        public int ServerStatsVisible { get; set; }

        [JsonProperty("proximityIndicatorsVisible")]
        public int ProximityIndicatorsVisible { get; set; }

        [JsonProperty("useMPH")]
        public int UseMPH { get; set; }

        [JsonProperty("greenLightsVisible")]
        public int GreenLightsVisible { get; set; }

        [JsonProperty("showDriverNamePlates")]
        public int ShowDriverNamePlates { get; set; }

        [JsonProperty("chatPopupOnMessage")]
        public int ChatPopupOnMessage { get; set; }

        [JsonProperty("chatMaxMessageCount")]
        public int ChatMaxMessageCount { get; set; }

        [JsonProperty("chatShowTimestamps")]
        public int ChatShowTimestamps { get; set; }

        [JsonProperty("chatFadeoutSeconds")]
        public int ChatFadeoutSeconds { get; set; }

        [JsonProperty("communicationPanelVisible")]
        public int CommunicationPanelVisible { get; set; }

        [JsonProperty("communicationPanelMinPriority")]
        public int CommunicationPanelMinPriority { get; set; }

        [JsonProperty("communicationPanelMaxVisibleTotal")]
        public int CommunicationPanelMaxVisibleTotal { get; set; }

        [JsonProperty("communicationPanelMaxVisibleSticky")]
        public int CommunicationPanelMaxVisibleSticky { get; set; }

        [JsonProperty("communicationPanelMaxVisibleNormal")]
        public int CommunicationPanelMaxVisibleNormal { get; set; }

        [JsonProperty("communicationPanelInCenter")]
        public int CommunicationPanelInCenter { get; set; }

        [JsonProperty("overallHUDScale")]
        public double OverallHUDScale { get; set; }

        [JsonProperty("pitInfoAlwaysVisible")]
        public int PitInfoAlwaysVisible { get; set; }

        [JsonProperty("lightIndicators")]
        public int LightIndicators { get; set; }

        [JsonProperty("cameraSetNameLabel")]
        public int CameraSetNameLabel { get; set; }

        [JsonProperty("lastMfdScreen")]
        public int LastMfdScreen { get; set; }

        [JsonProperty("autoSwitchPitMFD")]
        public bool AutoSwitchPitMFD { get; set; }

        [JsonProperty("realtimeStandingPitFilter")]
        public int RealtimeStandingPitFilter { get; set; }
    }
}
