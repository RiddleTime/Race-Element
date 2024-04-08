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
using TwitchLib.Client.Events;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;

[Overlay(Name = "Twitch Chat",
Description = "Shows twitch chat, newest messages appear at the top.\nTo allow Race Element to connect to the twitch api create the O Auth token at twitchapps.com/tmi",
OverlayType = OverlayType.Pitwall,
Authors = ["Reinier Klarenberg"])]
internal sealed class TwitchChatOverlay : AbstractOverlay
{
    internal readonly TwitchChatConfiguration _config = new();

    private TwitchClient _twitchClient = null;

    internal static readonly List<(MessageType, string)> Messages = [];

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

        if (!IsPreviewing)
            Messages.Clear();
    }

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        DisconnectTwitchClient();

        _cachedBackground?.Dispose();
        _stringFormat?.Dispose();

        _textBrushChat?.Dispose();
        _textBrushBits?.Dispose();
        _textBrushRaid?.Dispose();
        _textBrushSubscription?.Dispose();
        _textBrushBot?.Dispose();

        _dividerPen?.Dispose();
    }

    private void TwitchClient_OnConnectionError(object sender, OnConnectionErrorArgs e)
    {
        LogWriter.WriteToLog($"Twitch chat bot error: {e.Error}");
    }

    public sealed override bool ShouldRender()
    {
        if (_config.Behaviour.AlwaysVisible)
            return true;

        if (_config.Behaviour.HideInQualifying && pageGraphics.SessionType == ACCSharedMemory.AcSessionType.AC_QUALIFY)
            return false;

        return base.ShouldRender();
    }
    public sealed override void Render(Graphics g)
    {
        if (IsPreviewing) return;

        if (_twitchClient == null || !_twitchClient.IsConnected)
        {
            try
            {
                InitTwitchClient();
                _twitchClient.Connect();
            }
            catch (Exception) { }
        }

        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        if (_config.Colors.BackgroundOpacity != 0)
            _cachedBackground?.Draw(g, 0, 0, _config.Shape.Width, _config.Shape.Height);

        if (Messages.Count > 0)
        {
            int y = _config.Shape.Direction == TwitchChatConfiguration.Direction.TopToBottom ? 0 : Height;

            Action<int, TwitchChatConfiguration.Direction> func = (index, direction) =>
            {
                if (!_config.Bot.DisplayBotResponses && Messages[index].Item1 == MessageType.Bot)
                    return;

                string message = Messages[index].Item2;
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

                g.DrawStringWithShadow(message, _font, GetMessageBrush(Messages[index].Item1), new RectangleF(0, actualY, size.Width, size.Height), _stringFormat);

                if (_config.Colors.BackgroundOpacity != 0) // draw divider line
                {
                    int linyY = actualY;
                    if (direction == TwitchChatConfiguration.Direction.TopToBottom) linyY += (int)size.Height;
                    g.DrawLine(_dividerPen, 0, linyY, _config.Shape.Width, linyY);
                }

                if (_config.Shape.Direction == TwitchChatConfiguration.Direction.BottomToTop) directionalHeight *= -1;
                y += directionalHeight;
            };


            for (int i = Messages.Count - 1; i >= 0 && y != -1; i--)
                func(i, _config.Shape.Direction);

            if (Messages.Count > 100)
                Messages.RemoveRange(0, 50);
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

    private void DisconnectTwitchClient()
    {
        if (_twitchClient == null)
            return;

        _twitchClient.OnMessageReceived -= TwitchClient_OnMessageReceived;

        _twitchClient.OnRaidNotification -= TwitchClient_OnRaidNotification;
        _twitchClient.OnNewSubscriber -= TwitchClient_OnNewSubscriber;
        _twitchClient.OnReSubscriber -= TwitchClient_OnReSubscriber;
        _twitchClient.OnPrimePaidSubscriber -= TwitchClient_OnPrimePaidSubscriber;
        _twitchClient.OnGiftedSubscription -= TwitchClient_OnGiftedSubscription;

        _twitchClient.OnConnectionError -= TwitchClient_OnConnectionError;
        _twitchClient.OnConnected -= TwitchClient_OnConnected;
        _twitchClient.OnDisconnected -= TwitchClient_OnDisconnected;
        _twitchClient.OnReconnected -= TwitchClient_OnReconnected;
        _twitchClient.OnConnectionError -= TwitchClient_OnConnectionError;

        if (_twitchClient.IsConnected)
            _twitchClient.Disconnect();

        _twitchClient = null;
    }

    private void InitTwitchClient()
    {
        DisconnectTwitchClient();

        var credentials = new TwitchLib.Client.Models.ConnectionCredentials(_config.Credentials.TwitchUser, _config.Credentials.OAuthToken);
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30),
            ReconnectionPolicy = new ReconnectionPolicy(5),
        };
        WebSocketClient customClient = new(clientOptions);
        _twitchClient = new(customClient);

        _twitchClient.Initialize(credentials, _config.Credentials.TwitchUser);

        _twitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;

        _twitchClient.OnRaidNotification += TwitchClient_OnRaidNotification;
        _twitchClient.OnNewSubscriber += TwitchClient_OnNewSubscriber;
        _twitchClient.OnReSubscriber += TwitchClient_OnReSubscriber;
        _twitchClient.OnPrimePaidSubscriber += TwitchClient_OnPrimePaidSubscriber;
        _twitchClient.OnGiftedSubscription += TwitchClient_OnGiftedSubscription;

        _twitchClient.OnConnected += TwitchClient_OnConnected;
        _twitchClient.OnDisconnected += TwitchClient_OnDisconnected;
        _twitchClient.OnReconnected += TwitchClient_OnReconnected;
        _twitchClient.OnConnectionError += TwitchClient_OnConnectionError;
    }

    private void TwitchClient_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.IsBroadcaster && e.ChatMessage.ChatReply != null)
            return;

        if (e.ChatMessage.Bits > 0)
            Messages.Add(new(MessageType.Bits, $"{DateTime.Now:HH:mm} {e.ChatMessage.DisplayName} cheered {e.ChatMessage.Bits} bits: {e.ChatMessage.Message}"));
        else
            Messages.Add(new(MessageType.Chat, $"{DateTime.Now:HH:mm} {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}"));
    }

    private void TwitchClient_OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
    {
        string subPlan = e.GiftedSubscription.MsgParamSubPlan == TwitchLib.Client.Enums.SubscriptionPlan.NotSet ? "" : $"({e.GiftedSubscription.MsgParamSubPlan})";
        Messages.Add(new(MessageType.Subscriber, $"{e.GiftedSubscription.DisplayName} gifted a subscription {subPlan} to {e.GiftedSubscription.MsgParamRecipientDisplayName}"));
    }

    private void TwitchClient_OnPrimePaidSubscriber(object sender, OnPrimePaidSubscriberArgs e)
    {
        Messages.Add(new(MessageType.Subscriber, $"{e.PrimePaidSubscriber.DisplayName} Subscribed with Prime!"));
    }

    private void TwitchClient_OnReSubscriber(object sender, OnReSubscriberArgs e)
    {
        Messages.Add(new(MessageType.Subscriber, $"{e.ReSubscriber.DisplayName} Resubscribed! ({e.ReSubscriber.SubscriptionPlanName})"));
    }

    private void TwitchClient_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
        Messages.Add(new(MessageType.Subscriber, $"{e.Subscriber.DisplayName} Subscribed! ({e.Subscriber.SubscriptionPlanName})"));
    }

    private void TwitchClient_OnRaidNotification(object sender, OnRaidNotificationArgs e)
    {
        Messages.Add(new(MessageType.Raided, $"{e.RaidNotification.DisplayName} has raided the channel with {e.RaidNotification.MsgParamViewerCount} viewers."));
    }

    private void TwitchClient_OnConnected(object sender, OnConnectedArgs e)
    {
        Messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm:ss} Race Element - Chat HUD - Connected"));
    }

    private void TwitchClient_OnReconnected(object sender, OnReconnectedEventArgs e)
    {
        Messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm:ss} Race Element - Chat HUD - Reconnected"));
    }

    private void TwitchClient_OnDisconnected(object sender, OnDisconnectedEventArgs e)
    {
        Messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm:ss} Race Element - Chat HUD - Disconnected"));
    }
}
