using RaceElement.Data;
using RaceElement.Data.ACC.Cars;
using RaceElement.Data.ACC.Tracks;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util;
using System;
using System.Diagnostics;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using WebSocketSharp;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;

internal class TwitchChatCommandHandler
{
    private readonly AbstractOverlay _overlay;

    private readonly TwitchClient _client;

    public const char ChatCommandCharacter = '+';

    public readonly struct ChatResponse(string command, Func<string> result)
    {
        public readonly string Command = command;
        public readonly Func<string> Result = result;
    }
    private readonly ChatResponse[] Responses;

    public TwitchChatCommandHandler(AbstractOverlay overlay, TwitchClient client)
    {
        _overlay = overlay;
        _client = client;

        Responses = [
            new("commands", GetCommandsList),
            new("hud", () => "These are Race Element HUDs, it's free to use: https://race.elementfuture.com"),
            new("damage", () => $"{TimeSpan.FromSeconds(Damage.GetTotalRepairTime(_overlay.pagePhysics)):mm\\:ss\\.fff}"),
            new("conditions", () => $"Air {_overlay.pagePhysics.AirTemp:F2}°, Track {_overlay.pagePhysics.RoadTemp:F2}°, Wind {_overlay.pageGraphics.WindSpeed:F2}, Grip: {_overlay.pageGraphics.trackGripStatus}"),
            new("track", GetCurrentTrackResponse),
            new("car", GetCurrentCarResponse),
            new("steering", GetSteeringLockResponse),
            new("purple", GetPurpleLapResponse),
            //new("position", GetPositionResonse)
        ];
    }

    internal void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
    {
        try
        {
            string command = e.Command.CommandText.ToLower();
            string replyMessage = string.Empty;
            foreach (ChatResponse response in Responses.AsSpan())
                if (response.Command.Equals(command))
                {
                    replyMessage = response.Result();
                    break;
                }

            if (replyMessage.IsNullOrEmpty())
                return;

            _client.SendReply(_client.JoinedChannels[0], e.Command.ChatMessage.Id, replyMessage);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            LogWriter.WriteToLog(ex);
        }
    }
    private string GetCommandsList()
    {
        StringBuilder sb = new("Race Element Commands: ");
        Span<ChatResponse> responses = Responses.AsSpan();
        for (int i = 0; i < responses.Length; i++)
        {
            sb.Append(responses[i].Command);
            if (i < responses.Length - 1)
                sb.Append(", ");
        }
        sb.Append('.');
        return sb.ToString();
    }

    private string GetPositionResonse()
    {
        StringBuilder sb = new();
        sb.Append($"P:{_overlay.pageGraphics.Position}");

        return $"P:{_overlay.pageGraphics.Position}, in class: {_overlay.broadCastLocalCar.CupPosition}";
    }

    private string GetPurpleLapResponse()
    {
        var lobbyBest = _overlay.broadCastRealTime.BestSessionLap;
        if (lobbyBest == null || lobbyBest.IsInvalid)
            return "There isn't a valid lap yet in the lobby.";

        try
        {
            TimeSpan lapTime = TimeSpan.FromSeconds(lobbyBest.LaptimeMS.Value / 1000d);
            TimeSpan s1 = TimeSpan.FromSeconds(lobbyBest.Splits[0].Value / 1000d);
            TimeSpan s2 = TimeSpan.FromSeconds(lobbyBest.Splits[1].Value / 1000d);
            TimeSpan s3 = TimeSpan.FromSeconds(lobbyBest.Splits[2].Value / 1000d);
            return $"{lapTime:m\\:ss\\:fff} || {s1:m\\:ss\\:fff} | {s2:m\\:ss\\:fff} | {s3:m\\:ss\\:fff}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        return string.Empty;
    }

    private string GetSteeringLockResponse()
    {
        if (_overlay.pageStatic.CarModel.IsNullOrEmpty())
            return string.Empty;

        return $"{ConversionFactory.GetCarName(_overlay.pageStatic.CarModel)}: {SteeringLock.Get(_overlay.pageStatic.CarModel)}°";
    }

    private string GetCurrentCarResponse()
    {
        if (_overlay.pageStatic.CarModel.IsNullOrEmpty())
            return string.Empty;

        return $"{ConversionFactory.GetCarName(_overlay.pageStatic.CarModel)}";
    }

    private string GetCurrentTrackResponse()
    {
        if (_overlay.pageStatic.Track.IsNullOrEmpty())
            return string.Empty;

        var currentTrack = TrackData.GetCurrentTrack(_overlay.pageStatic.Track);
        if (currentTrack == null) return string.Empty;

        return $"{currentTrack.FullName}, Length: {currentTrack.TrackLength} meters";
    }
}
