using Newtonsoft.Json;
using RaceElement.Util;
using System.Collections.Generic;

namespace RaceElement.Data.ACC.Config;

public class CameraSettingService
{
    private readonly CameraSettings cameraSettings = new();

    public CameraSettings Settings() => cameraSettings;

    public void ResetTvCamSettings()
    {
        var settings = cameraSettings.Get(false);

        settings.TVCamFocusedAperture = 2.7999999523162842;
        settings.TVCamStaticAperture = 16;
        settings.TVCamStaticFocusDistance = 100000;
        settings.TVCamConstrainAspectRatio = 0;

        cameraSettings.Save(settings);
    }

    public void ResetHelicamCamera()
    {
        var settings = cameraSettings.Get(false);

        settings.HelicamDistance = 6000;
        settings.HelicamFOV = 60;
        settings.HelicamTargetMoreCars = 1;
        settings.HelicamTargetCarsMaxDist = 2000;
        settings.HelicamTargetInterpTime = 1;

        cameraSettings.Save(settings);
    }

    public class CameraSettings : AbstractSettingsJson<CameraSettingsJson>
    {
        public override string Path => FileUtil.AccConfigPath;

        public override string FileName => "cameraSettings.json";

        public override CameraSettingsJson Default() => new();
    }

    public class CameraSettingsJson : IGenericSettingsJson
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("lastUsedCamIndex")]
        public int LastUsedCamIndex { get; set; }

        [JsonProperty("lastUsedOnboardIndex")]
        public int LastUsedOnboardIndex { get; set; }

        [JsonProperty("generalMovement")]
        public int GeneralMovement { get; set; }

        [JsonProperty("onboardMovement")]
        public int OnboardMovement { get; set; }

        [JsonProperty("dashcamFactor")]
        public double DashcamFactor { get; set; }

        [JsonProperty("panniniProjection")]
        public float PanniniProjection { get; set; }

        [JsonProperty("helicamDistance")]
        public int HelicamDistance { get; set; }

        [JsonProperty("helicamFOV")]
        public int HelicamFOV { get; set; }

        [JsonProperty("helicamTargetMoreCars")]
        public int HelicamTargetMoreCars { get; set; }

        [JsonProperty("helicamTargetCarsMaxDist")]
        public int HelicamTargetCarsMaxDist { get; set; }

        [JsonProperty("helicamTargetInterpTime")]
        public int HelicamTargetInterpTime { get; set; }

        [JsonProperty("helicamDebug")]
        public int HelicamDebug { get; set; }

        [JsonProperty("cameraFOV")]
        public List<int> CameraFOV { get; set; }

        [JsonProperty("mapCarCameraData")]
        public object MapCarCameraData { get; set; }

        [JsonProperty("lookWithSteerGain")]
        public int LookWithSteerGain { get; set; }

        [JsonProperty("lookWithSteerGamma")]
        public double LookWithSteerGamma { get; set; }

        [JsonProperty("lookWithSteerSmoothing")]
        public double LookWithSteerSmoothing { get; set; }

        [JsonProperty("lookAroundSpeed")]
        public int LookAroundSpeed { get; set; }

        [JsonProperty("horizonLock")]
        public double HorizonLock { get; set; }

        [JsonProperty("enableTrackIR")]
        public int EnableTrackIR { get; set; }

        [JsonProperty("tripleWidth")]
        public int TripleWidth { get; set; }

        [JsonProperty("tripleDistance")]
        public int TripleDistance { get; set; }

        [JsonProperty("tripleAngle")]
        public int TripleAngle { get; set; }

        [JsonProperty("tripleBezel")]
        public int TripleBezel { get; set; }

        [JsonProperty("hDRExposure")]
        public double HDRExposure { get; set; }

        [JsonProperty("hDRContrast")]
        public double HDRContrast { get; set; }

        [JsonProperty("mirrorFOV")]
        public int MirrorFOV { get; set; }

        [JsonProperty("useGamepadForFreeCamera")]
        public int UseGamepadForFreeCamera { get; set; }

        [JsonProperty("virtualMirrorSize")]
        public double VirtualMirrorSize { get; set; }

        [JsonProperty("virtualMirrorVerticalOffset")]
        public int VirtualMirrorVerticalOffset { get; set; }

        [JsonProperty("virtualMirrorHorizontalOffset")]
        public int VirtualMirrorHorizontalOffset { get; set; }

        [JsonProperty("tVCamFocusedAperture")]
        public double TVCamFocusedAperture { get; set; }

        [JsonProperty("tVCamStaticAperture")]
        public int TVCamStaticAperture { get; set; }

        [JsonProperty("tVCamStaticFocusDistance")]
        public int TVCamStaticFocusDistance { get; set; }

        [JsonProperty("tVCamConstrainAspectRatio")]
        public int TVCamConstrainAspectRatio { get; set; }

        [JsonProperty("freeCamDOFEnabled")]
        public int FreeCamDOFEnabled { get; set; }
    }
}
