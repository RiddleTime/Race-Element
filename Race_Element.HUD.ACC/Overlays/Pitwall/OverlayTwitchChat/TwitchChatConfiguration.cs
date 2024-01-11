using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat
{
    internal class TwitchChatConfiguration : OverlayConfiguration
    {
        public TwitchChatConfiguration() { }

        [ConfigGrouping("Connection", "Set up the username and O Auth token")]
        public CredentialsGrouping Credentials { get; init; } = new CredentialsGrouping();
        public class CredentialsGrouping
        {
            public string TwitchUser { get; init; } = "";
            public string OAuthToken { get; init; } = "";

        }

    }
}
