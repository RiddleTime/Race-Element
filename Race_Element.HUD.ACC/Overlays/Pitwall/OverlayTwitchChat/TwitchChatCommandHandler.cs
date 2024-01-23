using RaceElement.Broadcast;
using RaceElement.Broadcast.Structs;
using RaceElement.Data;
using RaceElement.Data.ACC.Cars;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Tracker;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using WebSocketSharp;
using static RaceElement.Data.ACC.EntryList.EntryListTracker;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat.TwitchChatOverlay;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;

internal class TwitchChatCommandHandler
{
    private readonly TwitchChatOverlay _overlay;

    private readonly TwitchClient _client;

    public const char ChatCommandCharacter = '+';

    public readonly struct ChatResponse(string command, Func<string[], string> result)
    {
        public readonly string Command = command;
        /// <summary>
        /// in String[]: Arguments 
        /// out String: response(string.empty if no response)
        /// </summary>
        public readonly Func<string[], string> Result = result;
    }
    private readonly ChatResponse[] Responses;

    public TwitchChatCommandHandler(TwitchChatOverlay overlay, TwitchClient client)
    {
        _overlay = overlay;
        _client = client;

        Responses = [
            new("commands", GetCommandsList),
            new("bot", (args) => "Race Element, it's free to use: https://race.elementfuture.com"),
            new("damage", (args) => $"{TimeSpan.FromSeconds(Damage.GetTotalRepairTime(_overlay.pagePhysics)):mm\\:ss\\.fff}"),
            new("potential", GetPotentialBestResponse),
            new("temps", GetTemperaturesResponse),
            new("track", GetCurrentTrackResponse),
            new("car", GetCurrentCarResponse),
            new("steering", GetSteeringLockResponse),
            new("green", GetGreenLapResponse),
            new("purple", GetPurpleLapResponse),
            new("ahead", GetCarAheadResponse),
            new("behind", GetCarBehindResponse),
        ];
    }

    internal void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
    {
        if (e.Command.CommandIdentifier != ChatCommandCharacter) return;

        try
        {
            string command = e.Command.CommandText.ToLower();
            string replyMessage = string.Empty;

            var responses = Responses.AsSpan();
            for (int i = 0; i < Responses.Length; i++)
            {
                var response = responses[i];
                if (response.Command.Equals(command))
                {
                    Span<string> argsSpan = CollectionsMarshal.AsSpan(e.Command.ArgumentsAsList);
                    for (int s = 0; s < argsSpan.Length; s++) argsSpan[s] = argsSpan[s].ToLower();
                    string[] args = argsSpan.Length > 0 ? argsSpan.ToArray() : [];
                    replyMessage = response.Result(args);
                    break;
                }
            }

            if (replyMessage.IsNullOrEmpty())
                return;

            _client.SendReply(_client.JoinedChannels[0], e.Command.ChatMessage.Id, replyMessage);

            if (_overlay._config.Bot.DisplayBotResponses && !command.Equals("commands"))
                _overlay._messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm} - {replyMessage}"));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            LogWriter.WriteToLog(ex);
        }
    }
    private string GetCommandsList(string[] args)
    {
        StringBuilder sb = new("Race Element Commands: ");
        Span<ChatResponse> responses = Responses.AsSpan();
        for (int i = 1; i < responses.Length; i++) sb.Append($"{responses[i].Command}{(i < responses.Length - 1 ? ", " : string.Empty)}");
        _ = sb.Append('.');
        return sb.ToString();
    }

    private string GetPotentialBestResponse(string[] args)
    {
        StringBuilder sb = new();
        var laps = LapTracker.Instance.Laps;
        if (laps.Count == 0) return string.Empty;

        int sector1 = laps.GetFastestSector(1);
        int sector2 = laps.GetFastestSector(2);
        int sector3 = laps.GetFastestSector(3);
        if (sector1 != int.MaxValue && sector2 != int.MaxValue && sector3 != int.MaxValue)
        {
            int totalLapTime = sector1 + sector2 + sector3;
            TimeSpan lapTime = TimeSpan.FromSeconds(totalLapTime / 1000d);
            TimeSpan s1 = TimeSpan.FromSeconds(sector1 / 1000d);
            TimeSpan s2 = TimeSpan.FromSeconds(sector2 / 1000d);
            TimeSpan s3 = TimeSpan.FromSeconds(sector3 / 1000d);
            sb.Append($"Potential Best: {lapTime:m\\:ss\\:fff} || {s1:m\\:ss\\:fff} | {s2:m\\:ss\\:fff} | {s3:m\\:ss\\:fff}");
        }
        else
            sb.Append("No valid laps");

        return sb.ToString();
    }

    private string GetTemperaturesResponse(string[] args)
    {
        StringBuilder sb = new();
        if (_overlay.pagePhysics.AirTemp > 0)
        {
            sb.Append($"Air {_overlay.pagePhysics.AirTemp:F2}°, Track {_overlay.pagePhysics.RoadTemp:F2}°, Wind {_overlay.pageGraphics.WindSpeed:F1} km/h, Grip: {_overlay.pageGraphics.trackGripStatus}");
        }
        else
        {
            sb.Append($"Air {_overlay.broadCastRealTime.AmbientTemp}° ,Track {_overlay.broadCastRealTime.TrackTemp}°");
        }
        return sb.ToString();
    }
    private string GetCarAheadResponse(string[] args)
    {
        try
        {
            CarData localCar = GetLocalCar();
            if (localCar == null) return string.Empty;

            CarData carAhead = GetCarAtPosition(localCar.RealtimeCarUpdate.Position - 1);
            if (carAhead == null) return string.Empty;
            StringBuilder sb = new($"P{localCar.RealtimeCarUpdate.Position - 1} #{carAhead.CarInfo.RaceNumber} - ");

            if (carAhead.RealtimeCarUpdate.LastLap != null)
            {
                LapInfo lastLap = carAhead.RealtimeCarUpdate.LastLap;
                bool isBest = false;
                if (args.Length > 0 && args[0] == "best")
                {
                    lastLap = carAhead.RealtimeCarUpdate.BestSessionLap;
                    isBest = true;
                }

                if (!lastLap.LaptimeMS.HasValue) { sb.Append($"no {(isBest ? "best" : "last")} lap."); goto noLastLap; }

                sb.Append($"{(isBest ? "Best: " : "Last: ")}");
                TimeSpan lapTime = TimeSpan.FromSeconds(lastLap.GetLapTimeMS() / 1000d);
                TimeSpan s1 = TimeSpan.FromSeconds(lastLap.Splits[0].Value / 1000d);
                TimeSpan s2 = TimeSpan.FromSeconds(lastLap.Splits[1].Value / 1000d);
                TimeSpan s3 = TimeSpan.FromSeconds(lastLap.Splits[2].Value / 1000d);
                sb.Append($"{lapTime:m\\:ss\\:fff} || {s1:m\\:ss\\:fff} | {s2:m\\:ss\\:fff} | {s3:m\\:ss\\:fff}");
            }
        noLastLap:;

            return sb.ToString();

        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return string.Empty;
    }
    private string GetCarBehindResponse(string[] args)
    {
        try
        {
            CarData localCar = GetLocalCar();
            if (localCar == null) return string.Empty;

            CarData carBehind = GetCarAtPosition(localCar.RealtimeCarUpdate.Position + 1);
            if (carBehind == null) return string.Empty;
            StringBuilder sb = new($"P{localCar.RealtimeCarUpdate.Position + 1} #{carBehind.CarInfo.RaceNumber} - ");

            if (carBehind.RealtimeCarUpdate.LastLap != null)
            {
                LapInfo lastLap = carBehind.RealtimeCarUpdate.LastLap;
                bool isBest = false;
                if (args.Length > 0 && args[0] == "best")
                {
                    lastLap = carBehind.RealtimeCarUpdate.BestSessionLap;
                    isBest = true;
                }

                if (!lastLap.LaptimeMS.HasValue) { sb.Append($"no {(isBest ? "best" : "last")} lap."); goto noLastLap; }

                sb.Append($"{(isBest ? "Best: " : "Last: ")}");
                TimeSpan lapTime = TimeSpan.FromSeconds(lastLap.GetLapTimeMS() / 1000d);
                TimeSpan s1 = TimeSpan.FromSeconds(lastLap.Splits[0].Value / 1000d);
                TimeSpan s2 = TimeSpan.FromSeconds(lastLap.Splits[1].Value / 1000d);
                TimeSpan s3 = TimeSpan.FromSeconds(lastLap.Splits[2].Value / 1000d);
                sb.Append($"{lapTime:m\\:ss\\:fff} || {s1:m\\:ss\\:fff} | {s2:m\\:ss\\:fff} | {s3:m\\:ss\\:fff}");
            }
        noLastLap:;

            return sb.ToString();

        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return string.Empty;
    }

    private CarData GetLocalCar()
    {
        int focussedIndex = _overlay.broadCastRealTime.FocusedCarIndex;
        if (focussedIndex < 0)
            return null;

        CarData localCar = null;
        foreach (var car in EntryListTracker.Instance.Cars)
        {
            if (car.Value.CarInfo == null) continue;
            if (car.Key == focussedIndex)
            {
                localCar = car.Value;
                break;
            }
        }

        return localCar;
    }

    private static CarData GetCarAtPosition(int globalPosition)
    {
        CarData carAtPosition = null;
        foreach (var car in EntryListTracker.Instance.Cars)
        {
            if (car.Value.CarInfo == null) continue;
            if (car.Value.RealtimeCarUpdate.Position == globalPosition)
            {
                carAtPosition = car.Value;
                break;
            }
        }
        return carAtPosition;
    }

    private string GetPurpleLapResponse(string[] args)
    {
        var lobbyBest = _overlay.broadCastRealTime.BestSessionLap;
        if (lobbyBest == null || lobbyBest.IsInvalid) goto returnNoValidLaps;

        try
        {
            if (!lobbyBest.LaptimeMS.HasValue) goto returnNoValidLaps;

            TimeSpan lapTime = TimeSpan.FromSeconds(lobbyBest.GetLapTimeMS() / 1000d);
            TimeSpan s1 = TimeSpan.FromSeconds(lobbyBest.Splits[0].Value / 1000d);
            TimeSpan s2 = TimeSpan.FromSeconds(lobbyBest.Splits[1].Value / 1000d);
            TimeSpan s3 = TimeSpan.FromSeconds(lobbyBest.Splits[2].Value / 1000d);
            return $"Lobby Best Lap: {lapTime:m\\:ss\\:fff} || {s1:m\\:ss\\:fff} | {s2:m\\:ss\\:fff} | {s3:m\\:ss\\:fff}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

    returnNoValidLaps: return "No valid lap in the lobby";
    }

    public string GetGreenLapResponse(string[] args)
    {
        var personalBest = _overlay.broadCastLocalCar.BestSessionLap;
        if (personalBest == null || personalBest.IsInvalid) goto returnNoValidLaps;

        try
        {
            if (!personalBest.LaptimeMS.HasValue) goto returnNoValidLaps;

            TimeSpan lapTime = TimeSpan.FromSeconds(personalBest.GetLapTimeMS() / 1000d);
            TimeSpan s1 = TimeSpan.FromSeconds(personalBest.Splits[0].Value / 1000d);
            TimeSpan s2 = TimeSpan.FromSeconds(personalBest.Splits[1].Value / 1000d);
            TimeSpan s3 = TimeSpan.FromSeconds(personalBest.Splits[2].Value / 1000d);
            return $"{lapTime:m\\:ss\\:fff} || {s1:m\\:ss\\:fff} | {s2:m\\:ss\\:fff} | {s3:m\\:ss\\:fff}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

    returnNoValidLaps: return "No valid laps";
    }

    private string GetSteeringLockResponse(string[] args)
    {
        if (_overlay.pageStatic.CarModel.IsNullOrEmpty())
            return string.Empty;

        return $"{ConversionFactory.GetCarName(_overlay.pageStatic.CarModel)}: {SteeringLock.Get(_overlay.pageStatic.CarModel)}°";
    }

    private string GetCurrentCarResponse(string[] args)
    {
        if (_overlay.pageStatic.CarModel.IsNullOrEmpty())
            return string.Empty;

        return $"{ConversionFactory.GetCarName(_overlay.pageStatic.CarModel)}";
    }

    private string GetCurrentTrackResponse(string[] args)
    {
        if (_overlay.pageStatic.Track.IsNullOrEmpty())
            return string.Empty;

        var currentTrack = RaceElement.Data.ACC.Tracks.TrackData.GetCurrentTrack(_overlay.pageStatic.Track);
        if (currentTrack == null) return string.Empty;

        return $"{currentTrack.FullName}, Length: {currentTrack.TrackLength} meters";
    }
}
