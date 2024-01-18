using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;
internal class TwitchChatConfiguration : OverlayConfiguration
{
    public TwitchChatConfiguration() => AllowRescale = true;

    [ConfigGrouping("Connection", "Set up the username and O Auth token")]
    public CredentialsGrouping Credentials { get; init; } = new();
    public class CredentialsGrouping
    {
        [ToolTip("Your channel name")]
        public string TwitchUser { get; init; } = "";

        [ToolTip("Create an O Auth token at twitchapps.com/tmi, click connect and connect and copy -> paste the entire result in here." +
            "\n(This is required for Race Element to connect to your chat using the twitch api.)")]
        [StringOptions(isPassword: true)]
        public string OAuthToken { get; init; } = "";
    }

    [ConfigGrouping("Shape", "Adjust the size of the twitch chat box")]
    public ShapeGrouping Shape { get; init; } = new();
    public class ShapeGrouping
    {
        [IntRange(100, 500, 2)]
        public int Width { get; init; } = 400;

        [IntRange(100, 500, 2)]
        public int Height { get; init; } = 150;
    }

    [ConfigGrouping("Colors", "Adjust the colors of the text and the background")]
    public ColorGrouping Colors { get; init; } = new();
    public class ColorGrouping
    {
        [ToolTip("Adjust the text color for normal chat messages")]
        public Color TextColor { get; init; } = Color.FromArgb(255, 255, 255, 255);

        [ToolTip("Adjust the background color of this HUD.")]
        public Color BackgroundColor { get; init; } = Color.FromArgb(170, 0, 0, 0);

        [ToolTip("Adjust the opacity for the background color.")]
        [IntRange(0, 255, 1)]
        public int BackgroundOpacity { get; init; } = 170;

        [ToolTip("Adjust the text color when someone sends in bits.")]
        public Color BitsColor { get; init; } = Color.FromArgb(255, 255, 215, 0);

        [ToolTip("Adjust the text color when your channel is being raided.")]
        public Color RaidColor { get; init; } = Color.FromArgb(255, 255, 215, 0);

        [ToolTip("Adjust the text color when someone subscribes.")]
        public Color SubscriptionColor { get; init; } = Color.FromArgb(255, 255, 215, 0);
    }
}
