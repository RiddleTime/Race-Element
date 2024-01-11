using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;

internal class TwitchChatConfiguration : OverlayConfiguration
{
    public TwitchChatConfiguration() => AllowRescale = true;

    [ConfigGrouping("Connection", "Set up the username and O Auth token")]
    public CredentialsGrouping Credentials { get; init; } = new();
    public class CredentialsGrouping
    {
        public string TwitchUser { get; init; } = "";

        [ToolTip("Create an O Auth token at https://twitchapps.com/tmi/ and copy/paste the entire result in here.")]
        [StringOptions(isPassword: true)]
        public string OAuthToken { get; init; } = "";
    }

    [ConfigGrouping("Shape", "Adjust the size of the twitch chat box")]
    public ShapeGrouping Shape { get; init; } = new();
    public class ShapeGrouping
    {
        [IntRange(100, 400, 2)]
        public int Width = 400;

        [IntRange(100, 400, 2)]
        public int Height = 150;
    }
}
