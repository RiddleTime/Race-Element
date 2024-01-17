using RaceElement.Data;
using RaceElement.Data.ACC.Cars;
using RaceElement.Data.ACC.Tracks;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Diagnostics;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using WebSocketSharp;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;

internal class TwitchChatCommandHandler(AbstractOverlay overlay, TwitchClient twitchClient)
{
    public const char ChatCommandCharacter = '+';
    private string GetCommandsList()
    {
        Span<string> commands = ["hud", "damage", "conditions", "track", "car", "steering", "commands", "lobbybestlap"];

        StringBuilder sb = new();
        sb.Append("Race Element Commands: ");
        for (int i = 0; i < commands.Length; i++)
        {
            sb.Append(commands[i]);

            if (i < commands.Length - 1)
                sb.Append(", ");
        }
        sb.Append('.');

        return sb.ToString();
    }

    internal void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
    {
        try
        {
            string result = e.Command.CommandText switch
            {
                "commands" => GetCommandsList(),
                "hud" => "These are Race Element HUDs, it's free to use: https://race.elementfuture.com",
                "damage" => $"Total damage(repair time) {TimeSpan.FromSeconds(Damage.GetTotalRepairTime(overlay.pagePhysics)):mm\\:ss\\.ff}",
                "conditions" => $"Air {overlay.pagePhysics.AirTemp:F2}, Track {overlay.pagePhysics.RoadTemp:F2}, Wind {overlay.pageGraphics.WindSpeed:F2}, Grip: {overlay.pageGraphics.trackGripStatus}",
                "track" => GetCurrentTrackResponse(),
                "car" => GetCurrentCarResponse(),
                "steering" => GetSteeringLockResponse(),
                "lobbybestlap" => GetLobbyBestLapResponse(),
                _ => string.Empty
            };

            if (result.IsNullOrEmpty())
                return;

            twitchClient.SendReply(twitchClient.JoinedChannels[0], e.Command.ChatMessage.Id, result);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private string GetLobbyBestLapResponse()
    {
        var lobbyBest = overlay.broadCastRealTime.BestSessionLap;
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
        if (overlay.pageStatic.CarModel.IsNullOrEmpty())
            return string.Empty;

        return $"{ConversionFactory.GetCarName(overlay.pageStatic.CarModel)}: {SteeringLock.Get(overlay.pageStatic.CarModel)} Degrees";
    }

    private string GetCurrentCarResponse()
    {
        if (overlay.pageStatic.CarModel.IsNullOrEmpty())
            return string.Empty;

        return $"{ConversionFactory.GetCarName(overlay.pageStatic.CarModel)}";
    }

    private string GetCurrentTrackResponse()
    {
        if (overlay.pageStatic.Track.IsNullOrEmpty())
            return string.Empty;

        var currentTrack = TrackData.GetCurrentTrack(overlay.pageStatic.Track);
        if (currentTrack == null) return string.Empty;

        return $"{currentTrack.FullName}, Length: {currentTrack.TrackLength} meters";
    }
}
