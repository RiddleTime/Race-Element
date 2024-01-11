using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
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

    private const int maxTwitchUserNameLength = 25;
    private const int maxValueLength = 25;

    public TwitchChatOverlay(Rectangle rectangle) : base(rectangle, "Twitch Chat")
    {
        Width = 450;
        Height = 160;
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
        _messages.Add(new($"{e.ChatMessage.Username}:", e.ChatMessage.Message));
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
            Font font = FontUtil.FontSegoeMono(10);

            //var stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
            foreach ((string user, string message) in _messages.TakeLast(10))
            {
                //var size = g.MeasureString(message, font, 300, stringFormat);


                if (message.Length > maxValueLength)
                {
                    _panel.AddLine(user, message[0..maxValueLength]);

                    int count = maxValueLength;
                    while (count < message.Length)
                    {
                        int maxIndex = count + maxValueLength;
                        maxIndex.ClipMax(message.Length - 1);
                        _panel.AddLine("", message[count..maxIndex]);
                        count += maxValueLength;
                    }
                }
                else
                {
                    _panel.AddLine(user, message);
                }
            }
        }
        else
        {
            string maxTwitchUserLength = string.Empty;
            for (int i = 0; i < maxTwitchUserNameLength + 2; i++)
                _ = maxTwitchUserLength.Append(' ');


            string valueTemp = string.Empty;
            for (int i = 0; i < maxValueLength + 1; i++)
                _ = valueTemp.Append(' ');

            for (int i = 0; i < 10; i++)
            {
                _panel.AddLine(maxTwitchUserLength, valueTemp);
            }
        }


        _panel.Draw(g);
    }
}
