using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport
{
    internal sealed class LowFuelMotorsportConfiguration : OverlayConfiguration
    {
        public enum FontFamilyConfig
        {
            SegoeMono,
            Conthrax,
            Orbitron,
            Roboto,
        }

        public LowFuelMotorsportConfiguration() => GenericConfiguration.AllowRescale = false;

        [ConfigGrouping("Connection", "LFM user information and fetch information interval")]
        public ConnectionGrouping Connection { get; init; } = new();
        public class ConnectionGrouping
        {
            [ToolTip("User identifier (https://lowfuelmotorsport.com/profile/[HERE_IS_THE_ID])")]
            public string User { get; init; } = "";

            [ToolTip("Server fetch interval in seconds")]
            [IntRange(30, 120, 10)]
            public int Interval { get; init; } = 30;
        }

        [ConfigGrouping("Font", "Font configuration")]
        public FontGrouping Font { get; init; } = new();
        public class FontGrouping
        {
            [ToolTip("Font family")]
            public FontFamilyConfig FontFamily { get; init; } = FontFamilyConfig.Roboto;

            [ToolTip("Font size")]
            [IntRange(5, 32, 1)]
            public int Size { get; init; } = 10;
        }
    }
}
