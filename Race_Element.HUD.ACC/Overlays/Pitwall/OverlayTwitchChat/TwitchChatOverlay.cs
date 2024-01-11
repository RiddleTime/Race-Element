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
    private readonly StringFormat _stringFormat = new() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

    private CachedBitmap _cachedBackground;

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
        _cachedBackground = new CachedBitmap(_config.Shape.Width, _config.Shape.Height, g =>
        {
            using Brush brush = new SolidBrush(Color.FromArgb(170, 0, 0, 0));
            g.FillRoundedRectangle(brush, new Rectangle(0, 0, _config.Shape.Width, _config.Shape.Height), 4);
        });

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
        _twitchClient.OnMessageReceived += (s, e) => _messages.Add(new($"{e.ChatMessage.DisplayName}", e.ChatMessage.Message));
        _twitchClient.OnConnected += (s, e) => _twitchClient.SendMessage(_twitchClient.JoinedChannels[0], "Race Element Connected to Twitch Chat!");

        _font = FontUtil.FontConthrax(8);
    }

    public override void BeforeStop()
    {
        if (_isPreviewing) return;

        if (_twitchClient.IsConnected)
            _twitchClient.Disconnect();

        _cachedBackground?.Dispose();
        _stringFormat?.Dispose();
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

        _cachedBackground?.Draw(g, 0, 0, _config.Shape.Width, _config.Shape.Height);

        if (_messages.Count > 0)
        {
            int y = 0;
            foreach ((string user, string message) in _messages.TakeLast(20).Reverse())
            {
                if (y > _config.Shape.Height) break;

                string text = $"{user}: {message}";
                var size = g.MeasureString(text, _font, _config.Shape.Width, _stringFormat);
                g.DrawStringWithShadow(text, _font, Brushes.White, new RectangleF(0, y, size.Width, size.Height), _stringFormat);

                y += (int)Math.Ceiling(size.Height);
            }
        }
    }
}
