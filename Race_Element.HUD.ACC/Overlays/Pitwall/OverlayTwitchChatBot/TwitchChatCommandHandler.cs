using Newtonsoft.Json;
using RaceElement.Broadcast;
using RaceElement.Broadcast.Structs;
using RaceElement.Data;
using RaceElement.Data.ACC.Cars;
using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat;
using RaceElement.Util;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TwitchLib.Client.Events;
using WebSocketSharp;
using static RaceElement.Data.ACC.EntryList.EntryListTracker;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChat.TwitchChatOverlay;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayTwitchChatBot;

internal sealed class TwitchChatBotCommandHandler
{
    private readonly TwitchChatBotOverlay _overlay;

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

    public TwitchChatBotCommandHandler(TwitchChatBotOverlay overlay)
    {
        _overlay = overlay;

        Responses = [
            new("commands", GetCommandsLink),
            new("help", GetCommandsLink),
            new("app", (args) => "https://race.elementfuture.com / https://discord.gg/26AAEW5mUq"),
            new("damage", (args) => $"{TimeSpan.FromSeconds(Damage.GetTotalRepairTime(_overlay.pagePhysics)):mm\\:ss\\.fff}"),
            new("potential", GetPotentialBestResponse),
            new("temps", GetTemperaturesResponse),
            new("track", GetCurrentTrackResponse),
            new("car", GetCurrentCarResponse),
            new("angle", GetSteeringLockResponse),
            new("purple", GetPurpleLapResponse),
            new("ahead", GetCarAheadResponse),
            new("gap", GetGapResponse),
            new("behind", GetCarBehindResponse),
            new("p", GetPositionLookupResponse),
            new("#", GetRaceNumberLookupResponse),
            new("session", GetSessionResponse),
            new("fuel", GetFuelResponse)
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

            if (_overlay._twitchClient.JoinedChannels.Count == 0)
                _overlay._twitchClient.JoinChannel(_overlay._config.Credentials.TwitchUser);

            _overlay._twitchClient.SendReply(_overlay._twitchClient.JoinedChannels[0], e.Command.ChatMessage.Id, replyMessage);

            if (!command.Equals("commands"))
                TwitchChatOverlay.Messages.Add(new(MessageType.Bot, $"{DateTime.Now:HH:mm} - {replyMessage}"));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            LogWriter.WriteToLog(ex);
        }
    }

    private string GetCommandsLink(string[] args) => "https://race.elementfuture.com/2024/04/22/twitch-chat-bot-commands.html";

    [Obsolete("In favor of the guide on the website, this command is disabled, may enable again if website would be down.")]
    private string GetCommandsList(string[] args)
    {
        StringBuilder sb = new("Race Element Commands: ");

        Span<ChatResponse> responses = Responses.AsSpan();
        for (int i = 1; i < responses.Length; i++)
            sb.Append($"{responses[i].Command}{(i < responses.Length - 1 ? ", " : string.Empty)}");

        return $"{sb.Append('.')}";
    }

    private string GetGapResponse(string[] args)
    {
        if (args.Length == 0 || args.Length > 2) return string.Empty;

        CarData localCar = GetLocalCar();
        if (localCar == null) return string.Empty;

        CarData otherCar = null;
        if (args.Length == 1)
        {
            switch (args[0])
            {
                case "ahead":
                    {
                        otherCar = GetCarAtPosition(localCar.RealtimeCarUpdate.Position - 1);
                        break;
                    }
                case "behind":
                    {

                        int localPosition = localCar.RealtimeCarUpdate.Position;
                        if (localPosition != 1)
                        {
                            otherCar = localCar;
                            localCar = GetCarAtPosition(localPosition + 1);
                        }
                        break;
                    }
            };
        }
        else if (args.Length == 2)
        {
            if (args[0] == "#")
            {

            }
            switch (args[0])
            {
                case "#":
                    {
                        if (int.TryParse(args[1], out int raceNumber))
                        {
                            int requestedPosition = -1;
                            foreach (var car in EntryListTracker.Instance.Cars)
                            {
                                if (car.Value.CarInfo == null) continue;
                                if (car.Value.CarInfo.RaceNumber == raceNumber)
                                {
                                    requestedPosition = car.Value.RealtimeCarUpdate.Position;
                                    break;
                                }
                            }

                            if (requestedPosition == -1)
                                return string.Empty;

                            otherCar = GetCarAtPosition(requestedPosition);
                        }
                        break;
                    }
                case "p":
                    {
                        if (int.TryParse(args[1], out int position))
                        {
                            otherCar = GetCarAtPosition(position);
                        }
                        break;
                    }
            }
        }

        if (localCar == null || otherCar == null) return string.Empty;

        StringBuilder sb = new();
        int lapsLocal = localCar.RealtimeCarUpdate.Laps;
        int lapsOther = otherCar.RealtimeCarUpdate.Laps;
        if (lapsLocal == lapsOther)
        {
            float gap = GapTracker.Instance.TimeGapBetween(localCar.CarInfo.CarIndex, localCar.RealtimeCarUpdate.SplinePosition, otherCar.CarInfo.CarIndex);
            if (gap == -1) return string.Empty;
            sb.Append($"+{gap:F3}");
        }
        else
        {
            int lapDiff = lapsOther - lapsLocal;
            sb.Append($"+{lapDiff} Lap{(lapDiff > 1 ? "s" : "")}");
        }
        sb.Append($" (beta)");
        return sb.ToString();
    }

    /// <summary>
    /// +fuel [minutes] [liters/lap] [laptime]
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private string GetFuelResponse(string[] args)
    {
        if (args.Length != 3) return string.Empty;
        if (!int.TryParse(args[0], out int totalMinutes))
            return string.Empty;
        if (!float.TryParse(args[1], out float litersPerLap))
            return string.Empty;
        if (!TimeSpan.TryParseExact(args[2], ["m\\:s", "m\\:s\\.f", "m\\:s\\.ff", "m\\:s\\.fff", "m\\:ss", "m\\:ss\\.f", "m\\:ss\\.ff", "m\\:ss\\.fff"], CultureInfo.InvariantCulture,
                out TimeSpan lapTime))
            return string.Empty;

        double laps = TimeSpan.FromMinutes(totalMinutes).Divide(lapTime);
        double fuelRequired = Math.Ceiling(laps) * litersPerLap;
        return $"{totalMinutes} minutes at {litersPerLap:F3}L with {lapTime:m\\:ss\\.fff} is in total {fuelRequired:F3}L. Estimated {laps:F3} laps.";
    }

    private string GetSessionResponse(string[] args)
    {
        RealtimeUpdate update = _overlay.broadCastRealTime;
        StringBuilder sb = new($"{update.SessionType} - {$"{update.SessionTime:hh\\:mm\\:ss}"}");
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
            sb.Append($"Air {_overlay.pagePhysics.AirTemp:F3}°, Track {_overlay.pagePhysics.RoadTemp:F3}°, Wind {_overlay.pageGraphics.WindSpeed:F3} km/h, Grip: {_overlay.pageGraphics.trackGripStatus}");

            if (_overlay.pageGraphics.rainIntensity != ACCSharedMemory.AcRainIntensity.No_Rain)
                sb.Append($", Rain: {ACCSharedMemory.AcRainIntensityToString(_overlay.pageGraphics.rainIntensity)}");
        }
        else
        {
            sb.Append($"Air {_overlay.broadCastRealTime.AmbientTemp}°, Track {_overlay.broadCastRealTime.TrackTemp}°");
        }
        return sb.ToString();
    }

    private string GetRaceNumberLookupResponse(string[] args)
    {
        if (args.Length == 0)
            return string.Empty;

        string possibleNumber = args[0];
        if (!int.TryParse(possibleNumber, out int raceNumber))
            return string.Empty;

        int requestedPosition = -1;
        foreach (var car in EntryListTracker.Instance.Cars)
        {
            if (car.Value.CarInfo == null) continue;
            if (car.Value.CarInfo.RaceNumber == raceNumber)
            {
                requestedPosition = car.Value.RealtimeCarUpdate.Position;
                break;
            }
        }

        if (requestedPosition == -1)
            return string.Empty;

        return GetPositionResponse(requestedPosition);
    }

    /// <summary>
    /// look up car data using the position in the race, for example: +pos 3.
    /// </summary>
    /// <param name="args">should be a number</param>
    /// <returns></returns>
    private string GetPositionLookupResponse(string[] args)
    {
        if (args.Length == 0)
            return string.Empty;

        string possibleNumber = args[0];
        if (!int.TryParse(possibleNumber, out int requestedPosition))
            return string.Empty;

        return GetPositionResponse(requestedPosition);
    }

    private static string GetPositionResponse(int requestedPosition)
    {
        if (requestedPosition >= 0 && requestedPosition <= 200)
        {
            CarData requestedCar = GetCarAtPosition(requestedPosition);
            if (requestedCar == null) return string.Empty;

            StringBuilder sb = new($"P{requestedCar.RealtimeCarUpdate.Position} #{requestedCar.CarInfo.RaceNumber} - ");

            sb.Append($"{requestedCar.CarInfo.Drivers[requestedCar.RealtimeCarUpdate.DriverIndex].FirstName} {requestedCar.CarInfo.Drivers[requestedCar.RealtimeCarUpdate.DriverIndex].LastName}");
            if (requestedCar.CarInfo.TeamName.Length > 0) sb.Append($" [{requestedCar.CarInfo.TeamName}]");

            LapInfo bestLap = requestedCar.RealtimeCarUpdate.BestSessionLap;
            if (bestLap.LaptimeMS.HasValue)
            {
                TimeSpan bestLapTime = TimeSpan.FromMilliseconds((double)bestLap.LaptimeMS);
                sb.Append($" - Best: {bestLapTime:m\\:ss\\:fff}");
            }

            LapInfo lastLap = requestedCar.RealtimeCarUpdate.LastLap;
            if (lastLap.LaptimeMS.HasValue)
            {
                TimeSpan lastLapTime = TimeSpan.FromSeconds(lastLap.GetLapTimeMS() / 1000d);
                TimeSpan s1 = TimeSpan.FromSeconds(lastLap.Splits[0].Value / 1000d);
                TimeSpan s2 = TimeSpan.FromSeconds(lastLap.Splits[1].Value / 1000d);
                TimeSpan s3 = TimeSpan.FromSeconds(lastLap.Splits[2].Value / 1000d);
                sb.Append($" - L{requestedCar.RealtimeCarUpdate.Laps}: {lastLapTime:m\\:ss\\:fff} || {s1:m\\:ss\\:fff} | {s2:m\\:ss\\:fff} | {s3:m\\:ss\\:fff}");
                sb.Append(requestedCar.RealtimeCarUpdate.CarLocation switch
                {
                    CarLocationEnum.Pitlane => " - Pitlane",
                    _ => string.Empty,
                });
            }

            return $"{sb}";
        }

        return string.Empty;
    }

    private string GetCarAheadResponse(string[] args)
    {
        CarData localCar = GetLocalCar();
        if (localCar == null) return string.Empty;

        return GetPositionResponse(localCar.RealtimeCarUpdate.Position - 1);
    }
    private string GetCarBehindResponse(string[] args)
    {
        CarData localCar = GetLocalCar();
        if (localCar == null) return string.Empty;
        return GetPositionResponse(localCar.RealtimeCarUpdate.Position + 1);
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

        Debug.WriteLine($"best lap index: {_overlay.broadCastRealTime.BestLapCarIndex}, best car index: {lobbyBest.CarIndex}");
        var purpleCar = EntryListTracker.Instance.Cars.Where(x => x.Key == _overlay.broadCastRealTime.BestSessionLap.CarIndex);
        CarData car = null;
        if (purpleCar.Any())
        {
            car = purpleCar.First().Value;
            var carBestLap = car.RealtimeCarUpdate.BestSessionLap;
            if (carBestLap.LaptimeMS != null && !carBestLap.IsInvalid && lobbyBest.IsValidForBest)
            {
                Debug.WriteLine(JsonConvert.SerializeObject(lobbyBest));
                Debug.WriteLine(JsonConvert.SerializeObject(carBestLap));
                //lobbyBest = carBestLap;
            }
        }

        try
        {
            if (!lobbyBest.LaptimeMS.HasValue) goto returnNoValidLaps;

            TimeSpan lapTime = TimeSpan.FromMilliseconds((double)lobbyBest.LaptimeMS);
            string carInfo = $"P{car.RealtimeCarUpdate.Position} #{(car == null ? string.Empty : car.CarInfo.RaceNumber)} {car.CarInfo.Drivers[car.CarInfo.CurrentDriverIndex].FirstName[0]}. {car.CarInfo.GetCurrentDriverName()}";
            return $"{carInfo} - {lapTime:m\\:ss\\:fff}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

    returnNoValidLaps: return "No valid lap in the lobby";
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
