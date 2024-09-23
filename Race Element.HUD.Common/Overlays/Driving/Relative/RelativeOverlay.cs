using System;
using System.Diagnostics;
using System.Drawing;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;

namespace RaceElement.HUD.Common.Overlays.Driving.Relative;

[Overlay(
    Name = "Relative (ALPHA)",
    Version = 1.00,
    Description = "Shows drivers next to player.",
    OverlayType = OverlayType.Drive,
    Game = Game.iRacing | Game.AssettoCorsa1,
    Authors = ["Dirk Wolf"]
)]
internal sealed class RelativeOverlay : AbstractTableOverlay
{
    private readonly RelativeConfiguration _config = new();

    private const int _height = 800;
    private const int _width = 800;

    private int playerEntryListIndex = -1;

    static Color LapDownColor = Color.Blue;
    static Color LapUpColor = Color.Red;
    static Color InPitColor = Color.Gray;
    static Color InPitLapDown = MixWithGray(LapDownColor);
    static Color InPitLapUp = MixWithGray(LapUpColor);

    // we don't list the car in the relative if the interval is larger than 40 seconds
    static int IntervalThresholdMs = 40000;

    List<List<CellValue>> relativeList = [];

    public RelativeOverlay(Rectangle rectangle) : base(rectangle, "Relative")
    {
        this.Height = _height;
        this.Width = _width;
        this.RefreshRateHz = 3;
    }


    public sealed override void Render(Graphics g)
    {
        List<KeyValuePair<int, CarInfo>> entryList = new List<KeyValuePair<int, CarInfo>>(SessionData.Instance.Cars);
        SortEntryList(entryList);

        base.Render(g);
    }

    private void SortEntryList(List<KeyValuePair<int, CarInfo>> cars)
    {
        if (cars.Count == 0) return;

        // calculate live standings based on percentage of the current lap completed
        cars.Sort((a, b) =>
        {
            if (a.Value == null)
                return -1;

            if (b.Value == null)
                return 1;

            CarInfo carCarA = a.Value;
            CarInfo carCarb = b.Value;

            if (carCarA == null) return -1;
            if (carCarb == null) return 1;

            var aSpline = carCarA.TrackPercentCompleted;
            var bSpline = carCarb.TrackPercentCompleted;

            float aPosition = aSpline / 10;
            float bPosition = bSpline / 10;
            return aPosition.CompareTo(bPosition);
        });


        int sortedListIndex = 0;
        relativeList.Clear();
        foreach (KeyValuePair<int, CarInfo> pair in cars)
        {
            if (SessionData.Instance.PlayerCarIndex == pair.Key)
            {
                playerEntryListIndex = sortedListIndex;
                break;
            }
            sortedListIndex++;
        }

        // Collect "additionalDrivers" in front of player and after. Limit to max 40secs difference.
        List<CarInfo> toAdd = new List<CarInfo>();
        var additionalDrivers = _config.Layout.AdditionalRows;
        int startIndex = (playerEntryListIndex - additionalDrivers + cars.Count) % cars.Count;
        int endIndex = (playerEntryListIndex + additionalDrivers + 1 + cars.Count) % cars.Count;

        //Debug.WriteLine("Add start {0} end {1}", startIndex, endIndex);

        for (int index = startIndex; index < endIndex; index++)
        {

            CarInfo carToAdd = cars[index].Value;

            if (Math.Abs(carToAdd.GapToPlayerMs) > IntervalThresholdMs) continue;

            if (carToAdd.CarLocation != CarInfo.CarLocationEnum.NONE || carToAdd.CarLocation != CarInfo.CarLocationEnum.Garage)
            {
                toAdd.Insert(0, carToAdd);
            }
        }

        foreach (CarInfo car in toAdd)
        {
            AddRow(car, cars[SessionData.Instance.PlayerCarIndex].Value);
        }
    }

    private void AddRow(CarInfo opponentCar, CarInfo PlayerCar)
    {
        // use colors to show whether opponent is on a different lap. For being in pit, use gray mixed color.
        Color? textColor = null;
        if (opponentCar.Laps > PlayerCar.Laps)
        {
            if (opponentCar.CarLocation != CarInfo.CarLocationEnum.Pitlane)
                textColor = LapUpColor;
            else
                textColor = InPitLapUp;
        }
        else if (opponentCar.Laps < PlayerCar.Laps)
        {
            if (opponentCar.CarLocation != CarInfo.CarLocationEnum.Pitlane)
                textColor = LapDownColor;
            else
                textColor = InPitLapDown;
        }
        else if (opponentCar.CarLocation == CarInfo.CarLocationEnum.Pitlane)
        {
            textColor = InPitColor;
        }

        relativeList.Add([
            new CellValue(opponentCar.CarIndex.ToString(), textColor , null),
                new CellValue("#" + opponentCar.RaceNumber.ToString(), SimDataProvider.Instance.GetColorForCarClass(opponentCar.CarClass), null),
                new CellValue(opponentCar.Drivers[0].Name, textColor, null),
                new CellValue(opponentCar.Drivers[0].Category, null, SimDataProvider.Instance.GetColorForCategory(opponentCar.Drivers[0].Category)),
                new CellValue(opponentCar.Drivers[0].Rating.ToString(), textColor, null),
                new CellValue(DataUtil.GetTimeDiff(opponentCar.GapToPlayerMs), textColor, null),
            ]);
    }

    public override List<List<CellValue>> GetCellRows(int section)
    {
        return relativeList;
    }

    public override List<HeaderLabel> GetSectionHeaders()
    {
        // For relative we ignore car classes
        return [];
    }

    public override List<HeaderLabel> GetOverallHeader()
    {
        // TODO: interval should be alligned with the column metadata. Maybe a helper function to determine the offset?
        return [ new HeaderLabel("Relative", 36, AbstractTableOverlay.HeaderBackground),
                new HeaderLabel("Int", 4, AbstractTableOverlay.HeaderBackground)];
    }

    public override List<ColumnMetaData> GetColumnMetaData()
    {
        return [new ColumnMetaData("Index", 2), new ColumnMetaData("CarNumber", 4), new ColumnMetaData("DriverName", 20),
            new ColumnMetaData("Category", 5), new ColumnMetaData("Rating", 4), new ColumnMetaData("Interval", 4)];
    }

    private static Color MixWithGray(Color baseColor)
    {
        Color gray = Color.Gray;
        float grayRatio = 0.5F;

        // Calculate the mixed color
        int r = (int)((baseColor.R * (1 - grayRatio)) + (gray.R * grayRatio));
        int g = (int)((baseColor.G * (1 - grayRatio)) + (gray.G * grayRatio));
        int b = (int)((baseColor.B * (1 - grayRatio)) + (gray.B * grayRatio));

        return Color.FromArgb(r, g, b);
    }
}
