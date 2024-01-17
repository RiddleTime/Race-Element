using RaceElement.Data.ACC.Cars;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using WebSocketSharp;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;

[Overlay(Name = "Twitch Chat",
    Description = "Shows twitch chat, newest messages appear at the top.\nTo allow Race Element to connect to the twitch api create the O Auth token at twitchapps.com/tmi",
    OverlayType = OverlayType.Pitwall)]
internal sealed class TwitchChatOverlay : AbstractOverlay
{
    private readonly TwitchChatConfiguration _config = new TwitchChatConfiguration();

    private TwitchClient _twitchClient = null;

    private readonly List<(MessageType, string)> _messages = [];

    private bool _isPreviewing = false;

    private Font _font;
    private readonly StringFormat _stringFormat = new() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

    private CachedBitmap _cachedBackground;
    private SolidBrush _textBrushChat;
    private SolidBrush _textBrushBits;
    private SolidBrush _textBrushRaid;
    private Pen _dividerPen;

    public enum MessageType
    {
        Chat,
        Bits,
        Raided,
    }

    public TwitchChatOverlay(Rectangle rectangle) : base(rectangle, "Twitch Chat")
    {
        Width = _config.Shape.Width;
        Height = _config.Shape.Height;
        RefreshRateHz = 0.5;
    }

    public sealed override void SetupPreviewData()
    {
        _isPreviewing = true;
    }

    public sealed override void BeforeStart()
    {
        _cachedBackground = new CachedBitmap(_config.Shape.Width, _config.Shape.Height, g =>
        {
            using SolidBrush brush = new SolidBrush(Color.FromArgb(_config.Colors.BackgroundOpacity, _config.Colors.BackgroundColor));
            g.FillRoundedRectangle(brush, new Rectangle(0, 0, _config.Shape.Width, _config.Shape.Height), 4);
        });
        _textBrushChat = new SolidBrush(Color.FromArgb(255, _config.Colors.TextColor));
        _textBrushRaid = new SolidBrush(Color.FromArgb(255, _config.Colors.RaidColor));
        _textBrushBits = new SolidBrush(Color.FromArgb(255, _config.Colors.BitsColor));

        _dividerPen = new(new SolidBrush(Color.FromArgb(25, _config.Colors.TextColor)), 0.5f);
        _font = FontUtil.FontRoboto(11);

        SetupTwitchClient();
    }

    private void SetupTwitchClient()
    {
        if (_isPreviewing) return;

        var credentials = new TwitchLib.Client.Models.ConnectionCredentials(_config.Credentials.TwitchUser, _config.Credentials.OAuthToken);
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30),
        };
        WebSocketClient customClient = new(clientOptions);
        _twitchClient = new(customClient);

        _twitchClient.Initialize(credentials, _config.Credentials.TwitchUser);
        _twitchClient.OnMessageReceived += (s, e) =>
        {
            if (e.ChatMessage.Bits > 0)
                _messages.Add(new(MessageType.Bits, $"{DateTime.Now:HH:mm} {e.ChatMessage.DisplayName} cheered {e.ChatMessage.Bits} bits: {e.ChatMessage.Message}"));
            else
                _messages.Add(new(MessageType.Chat, $"{DateTime.Now:HH:mm} {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}"));

        };
        _twitchClient.OnRaidNotification += (s, e) =>
        {
            _messages.Add(new(MessageType.Raided, $"{e.RaidNotification.DisplayName} has raided the channel with {e.RaidNotification.MsgParamViewerCount} viewers."));
        };

        TwitchChatCommandHandler chatCommandHandler = new(this, _twitchClient);
        _twitchClient.AddChatCommandIdentifier(TwitchChatCommandHandler.ChatCommandCharacter);
        _twitchClient.OnChatCommandReceived += chatCommandHandler.OnChatCommandReceived;

        _twitchClient.OnConnected += (s, e) =>
        {
            _twitchClient.SendMessage(_twitchClient.JoinedChannels[0], "Race Element has Connected to Twitch Chat!");
        };
    }

    public sealed override void BeforeStop()
    {
        if (_isPreviewing) return;

        if (_twitchClient.IsConnected) Task.Run(() =>
        {
            _twitchClient.RemoveChatCommandIdentifier(TwitchChatCommandHandler.ChatCommandCharacter);
            _twitchClient.Disconnect();
        });

        _cachedBackground?.Dispose();
        _stringFormat?.Dispose();
        _textBrushChat?.Dispose();
        _dividerPen?.Dispose();
    }

    public sealed override bool ShouldRender() => true;
    public sealed override void Render(Graphics g)
    {
        if (_isPreviewing) return;

        if (!_twitchClient.IsConnected)
            Task.Run(() => _twitchClient.Connect());

        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        if (_config.Colors.BackgroundOpacity != 0)
            _cachedBackground?.Draw(g, 0, 0, _config.Shape.Width, _config.Shape.Height);

        if (_messages.Count > 0)
        {
            int y = 0;

            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                if (y > _config.Shape.Height) break;

                string message = _messages[i].Item2;

                var size = g.MeasureString(message, _font, _config.Shape.Width, _stringFormat);

                SolidBrush textBrush = GetMessageBrush(_messages[i].Item1);
                g.DrawStringWithShadow(message, _font, textBrush, new RectangleF(0, y, size.Width, size.Height), _stringFormat);

                if (_config.Colors.BackgroundOpacity != 0) // draw divider line at the bottom
                    g.DrawLine(_dividerPen, 0, y + size.Height, _config.Shape.Width, y + size.Height);

                y += (int)Math.Ceiling(size.Height);
            }

            if (_messages.Count > 100)
                _messages.RemoveRange(0, 50);
        }
    }

    private SolidBrush GetMessageBrush(MessageType type) => type switch
    {
        MessageType.Chat => _textBrushChat,
        MessageType.Bits => _textBrushBits,
        MessageType.Raided => _textBrushRaid,
        _ => _textBrushChat,
    };
}
