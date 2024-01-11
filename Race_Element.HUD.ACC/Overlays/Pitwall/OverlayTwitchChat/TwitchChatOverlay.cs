using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;

[Overlay(Name = "Twitch Chat",
    Description = "Shows twitch chat",
    OverlayType = OverlayType.Pitwall)]
internal class TwitchChatOverlay : AbstractOverlay
{
    private readonly TwitchChatConfiguration _config = new TwitchChatConfiguration();

    private TwitchClient _twitchClient = null;
    private readonly List<(string, string)> _messages = new();
    private InfoPanel _panel;

    private bool _isPreviewing = false;

    public TwitchChatOverlay(Rectangle rectangle) : base(rectangle, "Twitch Chat")
    {
        Width = 300;
        Height = 150;
        RefreshRateHz = 1;
    }

    public override void SetupPreviewData()
    {
        _isPreviewing = true;
    }

    public override void BeforeStart()
    {
        if (_isPreviewing) return;

        var credentials = new TwitchLib.Client.Models.ConnectionCredentials(_config.Credentials.TwitchUser, _config.Credentials.OAuthToken);
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        WebSocketClient customClient = new(clientOptions);
        _twitchClient = new(customClient);

        _twitchClient.Initialize(credentials, _config.Credentials.TwitchUser);
        _twitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;
        _twitchClient.OnConnected += TwitchClient_OnConnected;
        _twitchClient.OnLog += (s, e) => { Debug.WriteLine(e.Data); };

        _panel = new InfoPanel(10, Width);
    }
    public override void BeforeStop()
    {
        if (_isPreviewing) return;

        if (_twitchClient.IsConnected)
        {
            _twitchClient.SendMessage(_twitchClient.JoinedChannels[0], "Race Element Disconnected from Twitch chat.");
        }
    }

    private void TwitchClient_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
    {
        Debug.WriteLine($"Connected to {e.AutoJoinChannel}");
        _twitchClient.SendMessage(_twitchClient.JoinedChannels[0], "Race Element Connected to Twitch Chat!");
    }

    private void TwitchClient_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        _messages.Add(new(e.ChatMessage.Username, e.ChatMessage.Message));
        Debug.WriteLine(e.ChatMessage.Message);
    }

    public override bool ShouldRender() => true;

    public override void Render(Graphics g)
    {
        if (_isPreviewing) return;

        if (!_twitchClient.IsConnected)
            _twitchClient.Connect();


        if (_messages.Count > 0)
        {
            foreach ((string user, string message) in _messages.TakeLast(10).Reverse())
            {
                _panel.AddLine(user, message);
            }
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                _panel.AddLine("", "");
            }
        }


        _panel.Draw(g);
    }
}
