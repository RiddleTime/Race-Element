using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
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
    private readonly List<(string, string)> _messages = [];

    private bool _isPreviewing = false;

    private Font _font;

    public TwitchChatOverlay(Rectangle rectangle) : base(rectangle, "Twitch Chat")
    {
        Width = _config.Shape.Width;
        Height = _config.Shape.Height;
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

        _font = FontUtil.FontSegoeMono(9);
    }

    public override void BeforeStop()
    {
        if (_isPreviewing) return;

        if (_twitchClient.IsConnected)
        {
            _twitchClient.Disconnect();
        }
    }

    private void TwitchClient_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
    {
        Debug.WriteLine($"Connected to {e.AutoJoinChannel}");
        _twitchClient.SendMessage(_twitchClient.JoinedChannels[0], "Race Element Connected to Twitch Chat!");
    }

    private void TwitchClient_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        _messages.Add(new($"{e.ChatMessage.DisplayName}", e.ChatMessage.Message));
        Debug.WriteLine(e.ChatMessage.Message);
    }

    public override bool ShouldRender() => true;

    public override void Render(Graphics g)
    {
        if (_isPreviewing) return;

        if (!_twitchClient.IsConnected)
            _twitchClient.Connect();

        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        g.FillRoundedRectangle(Brushes.Black, new Rectangle(0, 0, _config.Shape.Width, _config.Shape.Height), 3);

        if (_messages.Count > 0)
        {
            var stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };

            int y = 0;
            foreach ((string user, string message) in _messages.TakeLast(10).Reverse())
            {
                string text = $"{user}: {message}";

                var size = g.MeasureString(text, _font, _config.Shape.Width, stringFormat);

                g.DrawStringWithShadow(text, _font, Brushes.White, new RectangleF(0, y, size.Width, size.Height), stringFormat);

                y += (int)Math.Ceiling(size.Height);
            }
        }
    }
}
