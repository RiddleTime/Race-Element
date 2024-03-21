using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

internal sealed class GForceTraceConfiguration : OverlayConfiguration
{
    public GForceTraceConfiguration() => GenericConfiguration.AllowRescale = true;
}

