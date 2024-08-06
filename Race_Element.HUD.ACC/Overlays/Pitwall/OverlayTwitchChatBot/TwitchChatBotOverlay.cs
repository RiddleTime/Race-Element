using RaceElement.HUD.Overlay.Internal;
using System;
using System.Drawing;
using TwitchLib.Client;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat.TwitchChatOverlay;
using TwitchLib.Communication.Models;
using RaceElement.Util;
using TwitchLib.Communication.Clients;
using System.Diagnostics;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChatBot;

[Overlay(Name = "Twitch Chat Bot",
Description = "(type +commands in chat for all available ones)\nTo allow Race Element to connect to the twitch api create the O Auth token at twitchapps.com/tmi",
OverlayType = OverlayType.Pitwall,
Authors = ["Reinier Klarenberg"])]
internal sealed class TwitchChatBotOverlay : ACCOverlay
{
    internal readonly TwitchChatBotConfiguration _config = new();

    internal TwitchClient _twitchClient = null;
    private TwitchChatBotCommandHandler _chatCommandHandler;

    public TwitchChatBotOverlay(Rectangle rectangle) : base(rectangle, "Twitch Chat Bot")
    {
        Width = 1;
        Height = 1;
        RefreshRateHz = 1 / 4f; // 1 in 4 seconds.
    }

    public sealed override bool ShouldRender() => true;

    public sealed override void BeforeStop()
    {
        if (!IsPreviewing) DisconnectTwitchClient();
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
    }

    private void DisconnectTwitchClient()
    {
        if (_twitchClient == null)
            return;

        _twitchClient.RemoveChatCommandIdentifier(TwitchChatBotCommandHandler.ChatCommandCharacter);
        _twitchClient.OnChatCommandReceived -= _chatCommandHandler.OnChatCommandReceived;
        _twitchClient.OnConnectionError -= TwitchClient_OnConnectionError;

        _twitchClient.OnConnected -= TwitchClient_OnConnected;
        _twitchClient.OnDisconnected -= TwitchClient_OnDisconnected;
        _twitchClient.OnReconnected -= TwitchClient_OnReconnected;
        _twitchClient.OnConnectionError -= TwitchClient_OnConnectionError;

        if (_twitchClient.IsConnected)
            _twitchClient.Disconnect();
        _twitchClient = null;
        _chatCommandHandler = null;
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

        _chatCommandHandler = new(this);

        _twitchClient.OnConnected += TwitchClient_OnConnected;
        _twitchClient.OnDisconnected += TwitchClient_OnDisconnected;
        _twitchClient.OnReconnected += TwitchClient_OnReconnected;
        _twitchClient.OnConnectionError += TwitchClient_OnConnectionError;
    }

    private void TwitchClient_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
    {
        _twitchClient.OnChatCommandReceived += _chatCommandHandler.OnChatCommandReceived;
        _twitchClient.AddChatCommandIdentifier(TwitchChatBotCommandHandler.ChatCommandCharacter);
        Messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm:ss} Race Element - Chat Bot - Connected to {_twitchClient.JoinedChannels[0].Channel}"));
    }

    private void TwitchClient_OnReconnected(object sender, TwitchLib.Communication.Events.OnReconnectedEventArgs e)
    {
        _twitchClient.OnChatCommandReceived += _chatCommandHandler.OnChatCommandReceived;
        _twitchClient.AddChatCommandIdentifier(TwitchChatBotCommandHandler.ChatCommandCharacter);
        Messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm:ss} Race Element - Chat Bot - Reconnected"));
    }

    private void TwitchClient_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
    {
        _twitchClient.OnChatCommandReceived -= _chatCommandHandler.OnChatCommandReceived;
        _twitchClient.RemoveChatCommandIdentifier(TwitchChatBotCommandHandler.ChatCommandCharacter);
        Messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm:ss} Race Element - Chat Bot - Disconnected"));
    }

    private void TwitchClient_OnConnectionError(object sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
    {
        Debug.WriteLine(e.Error);
        //LogWriter.WriteToLog($"Twitch chat bot error: {e.Error}");
    }
}
