using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.Opponents;
[Overlay(
Name = "Opponents",
Description = "Shows information about the cars ahead and behind in terms of race position.")]
internal sealed class OpponentsOverlay : AbstractOverlay
{
    private readonly OpponentsConfiguration _config = new();
    private sealed class OpponentsConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Opponents", "Change how many opponents are displayed either ahead or behind")]
        public OpponentsGrouping Opponents { get; init; } = new();
        public sealed class OpponentsGrouping
        {
            [IntRange(1, 5, 1)]
            public int AheadCount { get; init; } = 1;
            [IntRange(1, 5, 1)]
            public int BehindCount { get; init; } = 1;
        }

        [ConfigGrouping("Data", "Change which opponents data is displayed.")]
        public BehaviorGrouping Data { get; init; } = new();
        public sealed class BehaviorGrouping
        {
            public bool Sectors { get; init; } = true;
            public bool Gap { get; init; } = true;
            public bool Difference { get; init; } = true;
        }
    }
    private readonly record struct OpponentsModel
    {
        public CarDataModel[] Ahead { get; init; }
        public CarDataModel[] Behind { get; init; }
    }
    private readonly record struct CarDataModel
    {
        public int CarIndex { get; init; }
        public int LaptimeMs { get; init; }
        public int[] SectorsMs { get; init; }
        public GapModel Gap { get; init; }
    }
    private readonly record struct GapModel
    {
        public float GapTime { get; init; }
        public int GapLaps { get; init; }
    }

    private InfoTable _table;
    public OpponentsOverlay(Rectangle rectangle) : base(rectangle, "Opponents")
    {
        Width = 350;
        Height = 350;
        List<int> columnSizes = [50];

        _table = new InfoTable(12, [100, 100, 50]) { DrawBackground = true, DrawRowLines = true, DrawValueBackground = true, };

    }

    public override bool ShouldRender() => true;

    public sealed override void Render(Graphics g)
    {
        OpponentsModel model = CreateOpponentsModel();

        _table.AddRow(new()
        {
            Header = "P",
            Columns = ["Lap", "Gap"],
        });

        if (model.Ahead?.Length == 0 && model.Behind?.Length == 0)
            return;

        if (model.Ahead != null)
            foreach (var item in model.Ahead)
            {
                var car = GetCarData(item.CarIndex);
                string header = $"{car.CarInfo.RaceNumber}";
                _table.AddRow(new()
                {
                    Header = header,
                    Columns = [$"P{car.RealtimeCarUpdate.Position}", $"{item.LaptimeMs}", $"{item.Gap.GapTime:F1}"],
                });
            }
        // add local car
        var localCar = GetCarData(PlayerCarID);
        if (localCar != null)
            _table.AddRow(new()
            {
                Header = $"-> {localCar.CarInfo.RaceNumber}",
                Columns = [$"P{localCar.RealtimeCarUpdate.Position}"],
            });

        if (model.Behind != null)
            foreach (var item in model.Behind)
            {
                var car = GetCarData(item.CarIndex);
                string header = $"{car.CarInfo.RaceNumber}";
                _table.AddRow(new()
                {
                    Header = header,
                    Columns = [$"P{car.RealtimeCarUpdate.Position}", $"{item.LaptimeMs}", $"{item.Gap.GapTime:F1}"],
                });
            }

        _table.Draw(g);
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
                    if (lastLap != null && lastLap.LaptimeMS.HasValue)
                    {
                        laptime = lastLap.LaptimeMS.Value;
                        for (int s = 0; s < lastLap.Splits.Count; s++)
                            sectors[s] = lastLap.Splits[s].Value;
                    }

                    float gap = GapTracker.Instance.TimeGapBetween(playerCarId, playerCar.Value.RealtimeCarUpdate.SplinePosition, ahead.Key);
                    CarDataModel model = new()
                    {
                        CarIndex = ahead.Key,
                        LaptimeMs = laptime,
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
                    if (lastLap != null && lastLap.LaptimeMS.HasValue)
                    {

                        laptime = lastLap.LaptimeMS.Value;
                        for (int s = 0; s < lastLap.Splits.Count; s++)
                            sectors[s] = lastLap.Splits[s].Value;
                    }

                    float gap = GapTracker.Instance.TimeGapBetween(behind.Key, behind.Value.RealtimeCarUpdate.SplinePosition, playerCarId);
                    CarDataModel model = new()
                    {
                        CarIndex = behind.Key,
                        LaptimeMs = laptime,
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

    private EntryListTracker.CarData GetCarData(int carId)
    {
        var allCars = EntryListTracker.Instance.Cars;
        if (allCars.Count == 0) return null;
        var car = allCars.FirstOrDefault(x => x.Key == carId);
        return car.Value;
    }
}
