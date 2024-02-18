using RaceElement.HUD.Overlay.Internal;
using System;
using System.Drawing;
using TwitchLib.Client;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat.TwitchChatOverlay;
using TwitchLib.Communication.Models;
using RaceElement.Util;
using TwitchLib.Communication.Clients;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChatBot;

[Overlay(Name = "Twitch Chat Bot",
Description = "+commands in chat for all commands\nTo allow Race Element to connect to the twitch api create the O Auth token at twitchapps.com/tmi",
OverlayType = OverlayType.Pitwall,
Authors = ["Reinier Klarenberg"])]
internal sealed class TwitchChatBotOverlay : AbstractOverlay
{
    private TwitchChatBotConfiguration _config = new();

    private TwitchClient _twitchClient = null;
    private TwitchChatBotCommandHandler _chatCommandHandler;

    public TwitchChatBotOverlay(Rectangle rectangle) : base(rectangle, "Twitch Chat Bot")
    {
        Width = 1;
        Height = 1;
        RequestsDrawItself = true;
    }

    public sealed override void BeforeStart()
    {
        base.BeforeStart();
    }

    private void SetupTwitchClient()
    {
        if (IsPreviewing) return;

        var credentials = new TwitchLib.Client.Models.ConnectionCredentials(_config.Credentials.TwitchUser, _config.Credentials.OAuthToken);
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30),
        };
        WebSocketClient customClient = new(clientOptions);
        _twitchClient = new(customClient);

        _twitchClient.Initialize(credentials, _config.Credentials.TwitchUser);

        _chatCommandHandler = new(this, _twitchClient);
        _twitchClient.AddChatCommandIdentifier(TwitchChatBotCommandHandler.ChatCommandCharacter);


        _twitchClient.OnConnected += (s, e) => Messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm} Race Element - Connected"));
        _twitchClient.OnConnectionError += TwitchClient_OnConnectionError;


        if (!_twitchClient.IsConnected)
            _twitchClient.Connect();
    }
    private void TwitchClient_OnConnectionError(object sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
    {
        LogWriter.WriteToLog($"Twitch chat bot error: {e.Error}");
    }

    public sealed override bool ShouldRender() => true;

    public sealed override void BeforeStop()
    {
        _twitchClient.RemoveChatCommandIdentifier(TwitchChatBotCommandHandler.ChatCommandCharacter);
        _twitchClient.OnChatCommandReceived -= _chatCommandHandler.OnChatCommandReceived;
        _twitchClient.OnConnectionError -= TwitchClient_OnConnectionError;

        _twitchClient.Disconnect();
    }

    public sealed override void Render(Graphics g)
    {
        if (IsPreviewing) return;

        if (!_twitchClient.IsConnected)
        {
            _twitchClient.Reconnect();
            Messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm} chat hud Reconnecting..."));
        }
    }
}

