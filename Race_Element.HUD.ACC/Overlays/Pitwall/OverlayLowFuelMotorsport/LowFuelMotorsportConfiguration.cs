using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport
{
    internal sealed class LowFuelMotorsportConfiguration : OverlayConfiguration
    {
        public LowFuelMotorsportConfiguration() => GenericConfiguration.AllowRescale = false;

        [ConfigGrouping("Connection", "LFM user information")]
        public CredentialsGrouping Credentials { get; init; } = new();

        [ConfigGrouping("Font", "Font configuration")]
        public FontGrouping Font { get; init; } = new();

        [ConfigGrouping("Update", "Update inteval configuration")]
        public UpdateGrouping Update { get; init; } = new();

        public class CredentialsGrouping
        {
            [ToolTip("User identifier (https://lowfuelmotorsport.com/profile/[HERE_IS_THE_ID])")]
            public string User { get; init; } = "";
        }

        public class FontGrouping
        {
            [ToolTip("Font size")]
            [IntRange(1,32, 1)]
            public int Size { get; init; } = 10;
        }

        public class UpdateGrouping
        {
            [ToolTip("Update interval in seconds")]
            [IntRange(30, 3600, 10)]
            public int Interval { get; init; } = 30;
        }
    }
}
