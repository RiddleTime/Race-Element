using RaceElement.HUD.Overlay.Configuration;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;
internal class TwitchChatConfiguration : OverlayConfiguration
{
    public enum Direction { TopToBottom, BottomToTop }

    public TwitchChatConfiguration() => GenericConfiguration.AllowRescale = true;

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
        [ToolTip("Either new text will come from the top or it will come from the bottom of the hud.")]
        public Direction Direction { get; init; } = Direction.TopToBottom;

        [IntRange(100, 500, 2)]
        public int Width { get; init; } = 400;

        [IntRange(100, 500, 2)]
        public int Height { get; init; } = 150;

        [ToolTip("When disabled the hud will only appear when the engine is running.\nWhen disabled and spectating the twitch chat HUD will dissapear.")]
        public bool AlwaysVisible { get; init; } = true;
    }

    [ConfigGrouping("Bot", "Adjust the twitch chat bot.")]
    public BotGrouping Bot { get; init; } = new();
    public class BotGrouping
    {
        [ToolTip("Enable or disable the chat bot")]
        public bool IsEnabled { get; init; } = true;

        [ToolTip("When enabled the bot responses will also be displayed in the Twitch Chat HUD.")]
        public bool DisplayBotResponses { get; init; } = true;
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

        [ToolTip("Adjust the text color when Display Bot Answers is enabled.")]
        public Color BotColor { get; init; } = Color.FromArgb(255, 0, 255, 255);
    }
}
