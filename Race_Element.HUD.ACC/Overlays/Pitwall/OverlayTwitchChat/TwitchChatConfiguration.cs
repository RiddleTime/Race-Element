using RaceElement.HUD.Overlay.Configuration;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;

internal class TwitchChatConfiguration : OverlayConfiguration
{
    public TwitchChatConfiguration() { }

    [ConfigGrouping("Connection", "Set up the username and O Auth token")]
    public CredentialsGrouping Credentials { get; init; } = new CredentialsGrouping();
    public class CredentialsGrouping
    {
        public string TwitchUser { get; init; } = "";

        [ToolTip("Create an O Auth token at https://twitchapps.com/tmi/ and copy/paste the entire result in here.")]
        [StringOptions(isPassword: true)]
        public string OAuthToken { get; init; } = "";
    }
}
