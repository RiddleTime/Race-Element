using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using TwitchLib.Client;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;

[Overlay(Name = "Twitch Chat",
Description = "Shows twitch chat, newest messages appear at the top. (type +commands in chat for all available ones)\nTo allow Race Element to connect to the twitch api create the O Auth token at twitchapps.com/tmi",
OverlayType = OverlayType.Pitwall,
Authors = ["Reinier Klarenberg"])]
internal sealed class TwitchChatOverlay : AbstractOverlay
{
    internal readonly TwitchChatConfiguration _config = new();

    private TwitchClient _twitchClient = null;
    private TwitchChatCommandHandler _chatCommandHandler;

    internal List<(MessageType, string)> _messages = [];

    private bool _isPreviewing = false;

    private Font _font;
    private readonly StringFormat _stringFormat = new() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

    private CachedBitmap _cachedBackground;
    private SolidBrush _textBrushChat;
    private SolidBrush _textBrushBits;
    private SolidBrush _textBrushRaid;
    private SolidBrush _textBrushSubscription;
    private SolidBrush _textBrushBot;

    private Pen _dividerPen;

    public enum MessageType
    {
        Chat,
        Bits,
        Raided,
        Subscriber,
        Bot
    }
    public TwitchChatOverlay(Rectangle rectangle) : base(rectangle, "Twitch Chat")
    {
        Width = _config.Shape.Width;
        Height = _config.Shape.Height;
        RefreshRateHz = 0.5;
    }

    public sealed override void SetupPreviewData() => _isPreviewing = true;

    public sealed override void BeforeStart()
    {
        _cachedBackground = new CachedBitmap(_config.Shape.Width, _config.Shape.Height, g =>
        {
            using SolidBrush brush = new(Color.FromArgb(_config.Colors.BackgroundOpacity, _config.Colors.BackgroundColor));
            g.FillRoundedRectangle(brush, new Rectangle(0, 0, _config.Shape.Width, _config.Shape.Height), 4);
        });
        _textBrushChat = new SolidBrush(Color.FromArgb(255, _config.Colors.TextColor));
        _textBrushRaid = new SolidBrush(Color.FromArgb(255, _config.Colors.RaidColor));
        _textBrushBits = new SolidBrush(Color.FromArgb(255, _config.Colors.BitsColor));
        _textBrushSubscription = new SolidBrush(Color.FromArgb(255, _config.Colors.SubscriptionColor));
        _textBrushBot = new SolidBrush(Color.FromArgb(255, _config.Colors.BotColor));

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
        _twitchClient.OnRaidNotification += (s, e) => _messages.Add(new(MessageType.Raided, $"{e.RaidNotification.DisplayName} has raided the channel with {e.RaidNotification.MsgParamViewerCount} viewers."));
        _twitchClient.OnNewSubscriber += (s, e) => _messages.Add(new(MessageType.Subscriber, $"{e.Subscriber.DisplayName} Subscribed! ({e.Subscriber.SubscriptionPlanName})"));
        _twitchClient.OnReSubscriber += (s, e) => _messages.Add(new(MessageType.Subscriber, $"{e.ReSubscriber.DisplayName} Resubscribed! ({e.ReSubscriber.SubscriptionPlanName})"));
        _twitchClient.OnPrimePaidSubscriber += (s, e) => _messages.Add(new(MessageType.Subscriber, $"{e.PrimePaidSubscriber.DisplayName} Subscribed with Prime!"));
        _twitchClient.OnGiftedSubscription += (s, e) => _messages.Add(new(MessageType.Subscriber, $"{e.GiftedSubscription.DisplayName} gifted a subscription ({e.GiftedSubscription.MsgParamSubPlanName}) to {e.GiftedSubscription.MsgParamRecipientDisplayName}"));

        if (_config.Bot.IsEnabled)
        {
            _chatCommandHandler = new(this, _twitchClient);
            _twitchClient.AddChatCommandIdentifier(TwitchChatCommandHandler.ChatCommandCharacter);
            _twitchClient.OnChatCommandReceived += _chatCommandHandler.OnChatCommandReceived;
        }

        _twitchClient.OnConnected += (s, e) => _messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm} Race Element - Connected"));
        _twitchClient.OnConnectionError += TwitchClient_OnConnectionError;

        if (!_config.Shape.AlwaysVisible)
        {
            if (!_twitchClient.IsConnected)
                _twitchClient.Connect();
        }
    }

    public sealed override void BeforeStop()
    {
        if (_isPreviewing) return;


        if (_config.Bot.IsEnabled)
        {
            _twitchClient.RemoveChatCommandIdentifier(TwitchChatCommandHandler.ChatCommandCharacter);
            _twitchClient.OnChatCommandReceived -= _chatCommandHandler.OnChatCommandReceived;
            _twitchClient.OnConnectionError -= TwitchClient_OnConnectionError;
        }

        _twitchClient.Disconnect();

        _cachedBackground?.Dispose();
        _stringFormat?.Dispose();

        _textBrushChat?.Dispose();
        _textBrushBits?.Dispose();
        _textBrushRaid?.Dispose();
        _textBrushSubscription?.Dispose();
        _textBrushBot?.Dispose();

        _dividerPen?.Dispose();


    }

    private void TwitchClient_OnConnectionError(object sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
    {
        LogWriter.WriteToLog($"Twitch chat bot error: {e.Error}");
    }

    public sealed override bool ShouldRender()
    {
        if (_config.Shape.AlwaysVisible)
            return true;

        return base.ShouldRender();
    }
    public sealed override void Render(Graphics g)
    {
        if (_isPreviewing) return;

        if (!_twitchClient.IsConnected)
        {
            _twitchClient.Reconnect();
            _messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm} chat hud Reconnecting..."));
        }

        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        if (_config.Colors.BackgroundOpacity != 0)
            _cachedBackground?.Draw(g, 0, 0, _config.Shape.Width, _config.Shape.Height);

        if (_messages.Count > 0)
        {
            int y = _config.Shape.Direction == TwitchChatConfiguration.Direction.TopToBottom ? 0 : Height;

            Action<int, TwitchChatConfiguration.Direction> func = (index, direction) =>
            {
                string message = _messages[index].Item2;
                var size = g.MeasureString(message, _font, _config.Shape.Width, _stringFormat);
                int directionalHeight = (int)Math.Ceiling(size.Height);

                if (direction == TwitchChatConfiguration.Direction.TopToBottom)
                {
                    if (y > _config.Shape.Height) { y = -1; return; }
                }
                else
                {
                    if (y < 0) { y = -1; return; }
                }

                int actualY = y;
                if (direction == TwitchChatConfiguration.Direction.BottomToTop)
                    actualY -= directionalHeight;

                g.DrawStringWithShadow(message, _font, GetMessageBrush(_messages[index].Item1), new RectangleF(0, actualY, size.Width, size.Height), _stringFormat);

                if (_config.Colors.BackgroundOpacity != 0) // draw divider line
                {
                    int linyY = actualY;
                    if (direction == TwitchChatConfiguration.Direction.TopToBottom) linyY += (int)size.Height;
                    g.DrawLine(_dividerPen, 0, linyY, _config.Shape.Width, linyY);
                }

                if (_config.Shape.Direction == TwitchChatConfiguration.Direction.BottomToTop) directionalHeight *= -1;
                y += directionalHeight;
            };


            for (int i = _messages.Count - 1; i >= 0 && y != -1; i--)
                func(i, _config.Shape.Direction);

            if (_messages.Count > 100)
                _messages.RemoveRange(0, 50);
        }
    }

    private SolidBrush GetMessageBrush(MessageType type) => type switch
    {
        MessageType.Chat => _textBrushChat,
        MessageType.Bits => _textBrushBits,
        MessageType.Raided => _textBrushRaid,
        MessageType.Subscriber => _textBrushSubscription,
        MessageType.Bot => _textBrushBot,
        _ => _textBrushChat,
    };
}
