using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChatBot
{
    internal sealed class TwitchChatBotConfiguration : OverlayConfiguration
    {
        public TwitchChatBotConfiguration() => GenericConfiguration.AllowRescale = false;

        [ConfigGrouping("Connection", "Set up the username and O Auth token")]
        public CredentialsGrouping Credentials { get; init; } = new();
        public sealed class CredentialsGrouping
        {
            [ToolTip("Your channel name")]
            public string TwitchUser { get; init; } = "";

            [ToolTip("Create an O Auth token at twitchapps.com/tmi, click connect and connect and copy -> paste the entire result in here." +
                "\n(This is required for Race Element to connect to your chat using the twitch api.)")]
            [StringOptions(isPassword: true)]
            public string OAuthToken { get; init; } = "";
        }


        //[ConfigGrouping("Tasks", "Add additional behavior that is for example periodically actived.")]
        //public TasksGrouping Tasks { get; init; } = new();
        //public class TasksGrouping
        //{
        //    [ToolTip("Automatically shout out the +commands periodically")]
        //    public bool ShoutCommands { get; init; } = true;

        //    [ToolTip("Set the amount of minutes between each shoutout of '+commands' in chat.")]
        //    [IntRange(1, 60, 1)]
        //    public int CommandsInterval { get; init; } = 20;
        //}

    }
}
