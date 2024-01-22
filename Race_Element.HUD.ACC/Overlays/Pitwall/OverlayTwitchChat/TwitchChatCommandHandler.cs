using RaceElement.Broadcast.Structs;
using RaceElement.Data;
using RaceElement.Data.ACC.Cars;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Util;
using System;
using System.Collections.Generic;
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
            new("temps", GetTemperaturesResponse),
            new("track", GetCurrentTrackResponse),
            new("car", GetCurrentCarResponse),
            new("steering", GetSteeringLockResponse),
            new("green", GetGreenLapResponse),
            new("purple", GetPurpleLapResponse),
            new("pos", GetPositionResponse),
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
                TimeSpan lapTime = TimeSpan.FromSeconds(GetLapTimeMS(lastLap.Splits) / 1000d);
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

    private int GetLapTimeMS(List<int?> splits)
    {
        int lapTimeMs = 0;
        for (int i = 0; i < splits.Count; i++)
        {
            lapTimeMs += splits[i].GetValueOrDefault();
        }

        return lapTimeMs;
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
                TimeSpan lapTime = TimeSpan.FromSeconds(GetLapTimeMS(lastLap.Splits) / 1000d);
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

    private string GetPositionResponse(string[] args)
    {
        StringBuilder sb = new($"{_overlay.pageGraphics.Position}/{_overlay.pageGraphics.ActiveCars}");

        try
        {
            ConversionFactory.CarModels localCarModel = ConversionFactory.ParseCarName(_overlay.pageStatic.CarModel);
            if (localCarModel == ConversionFactory.CarModels.None) return "Not in a session";

            var localClass = ConversionFactory.GetConversion(localCarModel).CarClass;

            if (localCarModel != ConversionFactory.CarModels.None)
            {
                var localCar = EntryListTracker.Instance.Cars.FirstOrDefault(x => x.Value.CarInfo?.CarIndex == _overlay.pageGraphics.PlayerCarID);
                if (localCar.Value != null && localCar.Value.CarInfo != null)
                {
                    int count = 0;

                    foreach (var car in EntryListTracker.Instance.Cars)
                    {
                        if (car.Value.CarInfo == null) continue;
                        ConversionFactory.CarModels model = ConversionFactory.GetCarModels(car.Value.CarInfo.CarModelType);
                        if (model != ConversionFactory.CarModels.None && ConversionFactory.GetConversion(model).CarClass == localClass)
                            count++;
                    }

                    if (count != _overlay.pageGraphics.ActiveCars)
                        sb.Append($" | {ConversionFactory.GetConversion(localCarModel).CarClass}: {localCar.Value.RealtimeCarUpdate.CupPosition}/{count}");
                }

            }
        }
        catch (Exception)
        {
            return "Not in a session";
        }

        return sb.ToString();
    }

    private string GetPurpleLapResponse(string[] args)
    {
        var lobbyBest = _overlay.broadCastRealTime.BestSessionLap;
        if (lobbyBest == null || lobbyBest.IsInvalid) goto returnNoValidLaps;

        try
        {
            if (!lobbyBest.LaptimeMS.HasValue) goto returnNoValidLaps;

            TimeSpan lapTime = TimeSpan.FromSeconds(lobbyBest.LaptimeMS.Value / 1000d);
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

            TimeSpan lapTime = TimeSpan.FromSeconds(personalBest.LaptimeMS.Value / 1000d);
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
