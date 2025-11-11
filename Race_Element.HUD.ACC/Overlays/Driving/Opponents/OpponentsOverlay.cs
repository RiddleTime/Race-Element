using RaceElement.Broadcast.Structs;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.Opponents;

# if DEBUG
[Overlay(
    Name = "Opponents",
    Description = "Shows information about the cars ahead and behind in terms of race position."
)]
#endif
internal sealed class OpponentsOverlay : AbstractOverlay
{
    private readonly OpponentsConfiguration _config = new();

    private readonly record struct OpponentsModel
    {
        public CarDataModel[] Ahead { get; init; }
        public CarDataModel[] Behind { get; init; }
    }
    private readonly record struct CarDataModel
    {
        public int CarIndex { get; init; }
        public int LastLapMs { get; init; }
        public bool LastLapValid { get; init; }
        public int BestLapMs { get; init; }
        public int[] SectorsMs { get; init; }
        public GapModel Gap { get; init; }
    }
    private readonly record struct GapModel
    {
        public float GapTime { get; init; }
        public int GapLaps { get; init; }
    }

    private readonly InfoTable _table;
    public OpponentsOverlay(Rectangle rectangle) : base(rectangle, "Opponents")
    {
        Width = 350;
        Height = 350;
        List<int> columnSizes = [50];

        _table = new InfoTable(12, [50, 100, 100]) { DrawBackground = true, DrawRowLines = true, DrawValueBackground = true, };
    }

    public sealed override bool ShouldRender() => true;

    public sealed override void Render(Graphics g)
    {
        OpponentsModel model = CreateOpponentsModel();

        _table.AddRow(new()
        {
            Header = "P",
            Columns = ["#", "Last", "Best"],
        });

        if (model.Ahead?.Length == 0 && model.Behind?.Length == 0)
            return;

        if (model.Ahead != null)
            foreach (var item in model.Ahead)
            {
                var car = GetCarData(item.CarIndex);
                string header = $"{car.RealtimeCarUpdate.Position}";
                _table.AddRow(new()
                {
                    Header = header,
                    Columns = [$"{car.CarInfo.RaceNumber}", $"{GetLapTime(item.LastLapMs)}", $"{GetLapTime(item.BestLapMs)}"],
                    ColumnColors = [Color.White, (item.LastLapValid? Color.White : Color.Red), Color.White],
                });
            }
        // add local car
        var localCar = GetCarData(PlayerCarID);
        if (localCar != null) {
            bool lastLapInvalid = !localCar.RealtimeCarUpdate.LastLap.IsInvalid;
            _table.AddRow(new()
            {
                Header = $"{localCar.RealtimeCarUpdate.Position}",
                Columns = [$"{localCar.CarInfo.RaceNumber}", GetLapTime(localCar.RealtimeCarUpdate.LastLap), GetLapTime(localCar.RealtimeCarUpdate.BestSessionLap)],
                HeaderBackground = Color.OrangeRed,
                ColumnColors = [Color.White, (lastLapInvalid ? Color.White : Color.Red), Color.White],
            });
        }

        if (model.Behind != null)
            foreach (var item in model.Behind)
            {
                var car = GetCarData(item.CarIndex);
                string header = $"{car.RealtimeCarUpdate.Position}";
                _table.AddRow(new()
                {
                    Header = header,
                    Columns = [$"{car.CarInfo.RaceNumber}", $"{GetLapTime(item.LastLapMs)}", $"{GetLapTime(item.BestLapMs)}"],
                    ColumnColors = [Color.White, (item.LastLapValid ? Color.White : Color.Red), Color.White],
                });
            }

        _table.Draw(g);
    }

    private static string GetLapTime(LapInfo lapInfo)
    {
        if (lapInfo != null && lapInfo.LaptimeMS.HasValue && TimeSpan.FromMilliseconds(lapInfo.LaptimeMS.Value) > TimeSpan.FromSeconds(1))
            return $"{TimeSpan.FromMilliseconds(lapInfo.LaptimeMS.Value):mm\\:ss\\:fff}";
        else
            return "";
    }

    private static string GetLapTime(int lapTimeMs)
    {
        return lapTimeMs > 1000 ? $"{TimeSpan.FromMilliseconds(lapTimeMs):mm\\:ss\\:fff}" : "";
    }

    private int PlayerCarID
    {
        get
        {
            if (broadCastRealTime.FocusedCarIndex != pageGraphics.PlayerCarID) return broadCastRealTime.FocusedCarIndex;
            return pageGraphics.PlayerCarID;
        }
    }

    private OpponentsModel CreateOpponentsModel()
    {
        var allCars = EntryListTracker.Instance.Cars;
        if (allCars.Count == 0) return new();

        int playerCarId = PlayerCarID;
        var playerCar = allCars.FirstOrDefault(x => x.Key == playerCarId);
        if (playerCar.Value == null) return new();
        int playerCarPosition = playerCar.Value.RealtimeCarUpdate.Position;

        List<CarDataModel> carsAhead = [];
        if (playerCarPosition > 1)
        {
            for (int i = playerCarPosition - 1; i >= 1; i--)
            {
                if (carsAhead.Count >= _config.Opponents.AheadCount)
                    break;

                var ahead = allCars.FirstOrDefault(x => x.Value.RealtimeCarUpdate.Position == i);
                if (ahead.Value != null)
                {
                    int laptime = -1;
                    int[] sectors = [-1, -1, -1];

                    var lastLap = ahead.Value.RealtimeCarUpdate.LastLap;
                    bool lastLapValid = false;
                    if (lastLap != null && lastLap.LaptimeMS.HasValue)
                    {
                        lastLapValid = !lastLap.IsInvalid;
                        laptime = lastLap.LaptimeMS.Value;
                        for (int s = 0; s < lastLap.Splits.Count; s++)
                            sectors[s] = lastLap.Splits[s].Value;
                    }

                    float gap = GapTracker.Instance.TimeGapBetween(playerCarId, playerCar.Value.RealtimeCarUpdate.SplinePosition, ahead.Key);
                    CarDataModel model = new()
                    {
                        CarIndex = ahead.Key,
                        LastLapMs = laptime,
                        LastLapValid = lastLapValid,
                        BestLapMs = ahead.Value.RealtimeCarUpdate.BestSessionLap?.LaptimeMS ?? -1,
                        SectorsMs = sectors,
                        Gap = new GapModel()
                        {
                            GapLaps = ahead.Value.RealtimeCarUpdate.Laps - playerCar.Value.RealtimeCarUpdate.Laps,
                            GapTime = gap
                        }
                    };
                    carsAhead.Add(model);
                }
            }
            carsAhead.Reverse();
        }

        List<CarDataModel> carsBehind = [];
        if (playerCarPosition <= allCars.Count)
        {
            for (int i = playerCarPosition + 1; i <= allCars.Count; i++)
            {
                if (carsBehind.Count >= _config.Opponents.BehindCount)
                    break;

                var behind = allCars.FirstOrDefault(x => x.Value.RealtimeCarUpdate.Position == i);
                if (behind.Value != null)
                {
                    int laptime = -1;
                    int[] sectors = [-1, -1, -1];

                    var lastLap = behind.Value.RealtimeCarUpdate.LastLap;
                    bool lastLapValid = false;
                    if (lastLap != null && lastLap.LaptimeMS.HasValue)
                    {
                        lastLapValid = !lastLap.IsInvalid;
                        laptime = lastLap.LaptimeMS.Value;
                        for (int s = 0; s < lastLap.Splits.Count; s++)
                            sectors[s] = lastLap.Splits[s].Value;
                    }

                    float gap = GapTracker.Instance.TimeGapBetween(behind.Key, behind.Value.RealtimeCarUpdate.SplinePosition, playerCarId);
                    CarDataModel model = new()
                    {
                        CarIndex = behind.Key,
                        LastLapMs = laptime,
                        LastLapValid = lastLapValid,
                        BestLapMs = behind.Value.RealtimeCarUpdate.BestSessionLap?.LaptimeMS ?? -1,
                        SectorsMs = sectors,
                        Gap = new GapModel()
                        {
                            GapLaps = playerCar.Value.RealtimeCarUpdate.Laps - behind.Value.RealtimeCarUpdate.Laps,
                            GapTime = gap
                        }
                    };
                    carsBehind.Add(model);
                }
            }
        }

        return new OpponentsModel()
        {
            Ahead = [.. carsAhead],
            Behind = [.. carsBehind]
        };
    }

    private static EntryListTracker.CarData GetCarData(int carId)
    {
        var allCars = EntryListTracker.Instance.Cars;
        if (allCars.Count == 0) return null;
        var car = allCars.FirstOrDefault(x => x.Key == carId);
        return car.Value;
    }
}
