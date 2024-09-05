using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Numerics;
using static RaceElement.Data.Common.SimulatorData.CarInfo;


namespace RaceElement.HUD.Common.Overlays.Driving.Standings;

[Overlay(
    Name = "Live Standings",
    Version = 1.00,
    Description = "Shows race standings table for different car classes. (ALPHA)",
    OverlayType = OverlayType.Drive,
    Game = Game.iRacing | Game.AssettoCorsa1,
    Authors = ["FG", "Dirk Wolf"])]
public sealed class StandingsOverlay : CommonAbstractOverlay
{
    private readonly StandingsConfiguration _config = new();
    private const int _height = 800;
    private const int _width = 800;


    private string _driverName = "";

    // the entry list splint into separate lists for every car class
    private readonly Dictionary<string, List<KeyValuePair<int, CarInfo>>> _entryListForCarClass = [];

    internal List<string> CarClasses { get; private set; } = [];

    public StandingsOverlay(Rectangle rectangle) : base(rectangle, "Live Standings")
    {
        Height = _height;
        Width = _width;
        RefreshRateHz = 1;
    }

    public sealed override void SetupPreviewData()
    {
        SimDataProvider.Instance.SetupPreviewData();
    }


    public sealed override void BeforeStart()
    {
        InitCarClassEntryLists();
        // Can be registered before SimDataProvider.Instance was created
        SimDataProvider.OnSessionTypeChanged += Instance_OnSessionTypeChanged;

        base.BeforeStart();
    }

    private void Instance_OnSessionTypeChanged(object? sender, RaceSessionType e)
    {
        ClearCarClassEntryList();
    }

    private void ClearCarClassEntryList()
    {
        if (SimDataProvider.HasTelemetry())
        {
            CarClasses = SimDataProvider.Instance.GetCarClasses();
        }
        foreach (string carClass in _entryListForCarClass.Keys)
        {
            if (!_entryListForCarClass.ContainsKey(carClass))
            {
                InitCarClassEntryLists();
            }
            _entryListForCarClass[carClass].Clear();
        }
    }

    private void InitCarClassEntryLists()
    {
        _entryListForCarClass.Clear();
        if (!SimDataProvider.HasTelemetry()) return;

        if (CarClasses == null && SimDataProvider.HasTelemetry())
        {
            CarClasses = SimDataProvider.Instance.GetCarClasses();
        }
        foreach (string carClass in CarClasses)
        {
            _entryListForCarClass[carClass] = [];
        }
    }

    private List<KeyValuePair<int, CarInfo>> GetEntryListForClass(string carClass)
    {
        if (!_entryListForCarClass.ContainsKey(carClass))
        {
            _entryListForCarClass[carClass] = [];
        }

        return _entryListForCarClass[carClass];
    }

    public sealed override void Render(Graphics g)
    {
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (!SimDataProvider.HasTelemetry()) return;

        List<KeyValuePair<int, CarInfo>> cars = SessionData.Instance.Cars;
        if (cars.Count == 0) return;

        // TODO: optimize. DetermineDriversClass and SplitEntryList should not be necessary every Render. Just Sort.
        //       Maybe we need to add cars that joined a practice session after it started.
        DetermineDriversClass(cars);
        SplitEntryList(cars);
        SortAllEntryLists();

        //int bestSessionLapMS = GetBestSessionLap();

        Dictionary<string, List<StandingsTableRow>> tableRows = [];

        if (_config.Information.MultiClass)
        {
            foreach (string carClass in CarClasses)
            {
                tableRows[carClass] = [];
                EntryListToTableRow(tableRows[carClass], GetEntryListForClass(carClass));
            }
        }
        else
        {
            tableRows[GetDriverClass()] = [];
            EntryListToTableRow(tableRows[GetDriverClass()], GetEntryListForClass(GetDriverClass()));
        }

        AddDriversRow(tableRows[GetDriverClass()], GetEntryListForClass(GetDriverClass()));

        OverlayStandingsTable ost = new(10, 10, 13);

        int height = 0;
        foreach (KeyValuePair<string, List<StandingsTableRow>> kvp in tableRows)
        {
            Color color = SimDataProvider.Instance.GetColorForCarClass(kvp.Key);
            ost.Draw(g, height, kvp.Value, _config.Layout.MulticlassRows, new SolidBrush(Color.FromArgb(150, color)), $"{kvp.Key} / {GetEntryListForClass(kvp.Key).Count()} Cars", _driverName, /* _config.Information.TimeDelta,*/ _config.Information.InvalidLap);
            height = ost.Height;
        }
    }

    // Add driver's car and "AdditionalRows" number of posisitions before and after to the standings tables in case the driver is not in the top "MulticlassRows" positions
    private void AddDriversRow(List<StandingsTableRow> standingsTableRows, List<KeyValuePair<int, CarInfo>> list)
    {
        var playersIndex = GetDriversIndex(list);
        if (playersIndex == -1 || playersIndex < _config.Layout.MulticlassRows)
        {
            return;
        }

        int startIdx = playersIndex - _config.Layout.AdditionalRows < 0 ? 0 : playersIndex + _config.Layout.AdditionalRows - _config.Layout.MulticlassRows;
        int endIdx = playersIndex + _config.Layout.AdditionalRows + 1 > list.Count() ? list.Count() : playersIndex + _config.Layout.AdditionalRows + 1;

        if (startIdx < _config.Layout.MulticlassRows) startIdx = _config.Layout.MulticlassRows;

        for (int i = startIdx; i < endIdx; i++)
        {
            CarInfo carData = list[i].Value;
            AddCarDataTableRow(carData, standingsTableRows);
        }
    }

    private int GetDriversIndex(List<KeyValuePair<int, CarInfo>> list)
    {
        for (int i = 0; i < list.Count(); i++)
        {
            CarInfo carData = list[i].Value;

            if (SessionData.Instance.FocusedCarIndex == carData.CarIndex)
            {
                return i;
            }
        }
        return -1;
    }

    private void EntryListToTableRow(List<StandingsTableRow> tableRows, List<KeyValuePair<int, CarInfo>> entryList)
    {
        for (int i = 0; i < (entryList.Count() < _config.Layout.MulticlassRows ? entryList.Count() : _config.Layout.MulticlassRows); i++)
        {
            CarInfo carData = entryList[i].Value;
            AddCarDataTableRow(carData, tableRows);
        }
    }

    private int GetBestSessionLap()
    {
        int bestSessionLapMS = -1;
        /* TODO
        if (broadCastRealTime.BestSessionLap != null)
        {
            bestSessionLapMS = broadCastRealTime.BestSessionLap.LaptimeMS.GetValueOrDefault(-1);
        } */
        return bestSessionLapMS;
    }

    private void AddCarDataTableRow(CarInfo carData, List<StandingsTableRow> standingsTableRows)
    {
        DriverInfo driverInfo = carData.Drivers[carData.CurrentDriverIndex];

        string additionInfo = AdditionalInfo(carData);

        // Interval based on the type of session:
        // - practice/qualifying: interval is gap to driver in front's BEST time (within) same class 
        // - race: interval is gap to car in front in the same class.        
        int intervalMs = 0;
        if (standingsTableRows.Count > 1)
        {
            if (SessionData.Instance.SessionType != RaceSessionType.Race)
            {
                intervalMs = (int)(carData.FastestLap.LaptimeMS - standingsTableRows[standingsTableRows.Count - 1].FastestLapTime.LaptimeMS);
            }
            else
            {
                // TODO: take into account being lap down. e.g. print "1L" for 1 lap down
                int carInFont = standingsTableRows[standingsTableRows.Count - 1].CarIdx;
                intervalMs = carData.GapToRaceLeaderMs - SessionData.Instance.Cars[carInFont].Value.GapToRaceLeaderMs;
            }
        }

        standingsTableRows.Add(new StandingsTableRow()
        {
            CarIdx = carData.CarIndex,
            Position = carData.CupPosition,
            RaceNumber = carData.RaceNumber,
            DriverName = driverInfo.Name,
            LastLapTime = carData.LastLap,

            IntervalMs = new LapInfo() { LaptimeMS = intervalMs },
            AdditionalInfo = additionInfo,
            FastestLapTime = carData.FastestLap,
            // should be something like "A3.43 4.1k" for iRacing. TODO at a later point we can add color.
            LicenseInfo = carData.Drivers[carData.CurrentDriverIndex].Category + " " + carData.Drivers[carData.CurrentDriverIndex].Rating
        });
    }

    private string AdditionalInfo(CarInfo realtimeCarUpdate)
    {
        if (SessionData.Instance.SessionType != RaceSessionType.Race
            && SessionData.Instance.SessionType != RaceSessionType.Qualifying) return "";

        switch (realtimeCarUpdate.CarLocation)
        {
            case CarLocationEnum.PitEntry:
                {
                    return "PIT Entry";
                }
            case CarLocationEnum.PitExit:
                {
                    return "PIT Exit";
                }
            case CarLocationEnum.Pitlane:
                {
                    return "Box";
                }
        }

        if (realtimeCarUpdate.CurrentLap != null && realtimeCarUpdate.CurrentLap.IsInvalid)
        {
            return "X";
        }

        return "";
    }

    public static string GetTimeDiff(LapInfo lap)
    {
        if (lap == null || !lap.LaptimeMS.HasValue)
        {
            return "--:--.---";
        }

        TimeSpan lapTime = TimeSpan.FromMilliseconds(lap.LaptimeMS.Value);
        if (lapTime.Minutes > 0)
            return $"{lapTime:m\\:s\\.f}";
        else
            return $"{lapTime:s\\.f}";
    }

    public static string GetLapTime(LapInfo lap)
    {
        if (lap == null || !lap.LaptimeMS.HasValue || lap.LaptimeMS < 0)
        {
            return "--:--.---";
        }

        TimeSpan lapTime = TimeSpan.FromMilliseconds(lap.LaptimeMS.Value);
        return $"{lapTime:mm\\:ss\\.fff}";
    }

    private void SortAllEntryLists()
    {
        foreach (string carClass in CarClasses)
        {
            SortEntryList(GetEntryListForClass(carClass));
        }
    }

    private void SortEntryList(List<KeyValuePair<int, CarInfo>> cars)
    {
        if (cars.Count == 0) return;

        switch (SessionData.Instance.SessionType)
        {

            case RaceSessionType.Race:
                {
                    switch (SessionData.Instance.Phase)
                    {
                        case SessionPhase.SessionOver:
                        case SessionPhase.PreSession:
                        case SessionPhase.PreFormation:
                            {
                                cars.Sort((a, b) =>
                                {
                                    return CompareToCupPosition(a.Value.CupPosition, b.Value.CupPosition);
                                });
                                break;
                            }
                        default:
                            {
                                // calculate live standings based on the lap cars they're in and the percentage laps completed
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

                                    var aLaps = carCarA.LapIndex;
                                    var bLaps = carCarb.LapIndex;

                                    float aPosition = aLaps + aSpline / 10;
                                    float bPosition = bLaps + bSpline / 10;
                                    return aPosition.CompareTo(bPosition);
                                });
                                cars.Reverse();
                                break;
                            };
                    }
                    break;
                }

            // For practice and qualifying we let it be ordered by cup position determined by sim
            // (which should be in order of fastest lap time)
            case RaceSessionType.Practice:
            case RaceSessionType.Qualifying:
                {
                    cars.Sort((a, b) =>
                    {
                        return CompareToCupPosition(a.Value.CupPosition, b.Value.CupPosition);
                    });
                    break;
                }

            default: break;
        }
    }

    private void DetermineDriversClass(List<KeyValuePair<int, CarInfo>> cars)
    {
        if (!SimDataProvider.HasTelemetry()) return;

        foreach (KeyValuePair<int, CarInfo> kvp in cars)
        {
            if (kvp.Key == SessionData.Instance.FocusedCarIndex)
            {
                DriverInfo driverInfo = kvp.Value.Drivers[kvp.Value.CurrentDriverIndex];
                _driverName = driverInfo.Name;
                //Debug.WriteLine($"standings overlay - car class {_ownClass} driver name {_driverLastName}");
            }
        }
    }

    private void SplitEntryList(List<KeyValuePair<int, CarInfo>> cars)
    {
        ClearCarClassEntryList();

        foreach (KeyValuePair<int, CarInfo> kvp in cars)
        {
            if (kvp.Value == null)
            {
                return;
            }

            CarInfo carInfo = kvp.Value;
            string carClass = carInfo.CarClass;

            GetEntryListForClass(carClass).Add(kvp);

        }
    }

    private string GetDriverClass()
    {
        return SimDataProvider.LocalCar.CarModel.CarClass;
    }

    /// <summary>
    /// Compares the cup postion. Some cars might have position 0, which means they are ordered after other cars.
    /// </summary>    
    private int CompareToCupPosition(int cupPositionA, int cupPositionB)
    {
        if (cupPositionA == 0)
        {
            return cupPositionB == 0 ? 0 : 1;
        }
        else if (cupPositionB == 0)
        {
            return -1;
        }
        else
        {
            return BigInteger.Compare(cupPositionA, cupPositionB);
        }
    }

    public class StandingsTableRow
    {
        public int CarIdx { get; set; }
        public int Position { get; set; }
        public int RaceNumber { get; set; }
        public string DriverName { get; set; }

        // e.g. "A3.43 4155" for A class, 3.43 safety rating 4155 irating
        public string LicenseInfo { get; set; }

        // gap to car in front that's in the same class
        public LapInfo IntervalMs { get; set; }
        public LapInfo LastLapTime { get; set; }
        public LapInfo FastestLapTime { get; set; }
        public string AdditionalInfo { get; set; }
    }

    public class OverlayStandingsTable
    {
        public int Height { get; set; }
        private readonly int _x;
        private readonly int _y;
        // pixels between columns
        private readonly int _columnGap = 5;
        // pixels between rows
        private readonly int _rowGap = 3;
        private readonly int _fontSize = 0;

        private readonly SolidBrush _oddBackground = new(Color.FromArgb(100, Color.Black));
        private readonly SolidBrush _evenBackground = new(Color.FromArgb(180, Color.Black));
        private readonly SolidBrush _driversCarBackground = new(Color.FromArgb(180, Color.DarkSeaGreen));

        public OverlayStandingsTable(int x, int y, int fontSize)
        {
            _x = x;
            _y = y;
            _fontSize = fontSize;
        }

        public void Draw(Graphics g, int y, List<StandingsTableRow> tableData, int splitRowIndex, SolidBrush classBackground, string header, string ownName /*, bool showDeltaRow*/, bool showInvalidLapIndicator)
        {
            var rowPosY = _y + y;

            if (tableData.Count == 0) return;

            OverlayStandingTableHeaderLabel tableHeader = new(g, _x, rowPosY, classBackground, FontUtil.FontSegoeMono(_fontSize));
            tableHeader.Draw(g, Brushes.White, header);
            rowPosY += tableHeader.Height + _rowGap;

            for (int i = 0; i < tableData.Count; i++)
            {

                var columnPosX = _x;

                SolidBrush backgroundColor = _oddBackground;
                if (i % 2 == 0)
                {
                    backgroundColor = _evenBackground;
                }

                if (tableData[i].DriverName.Equals(ownName))
                {
                    backgroundColor = _driversCarBackground;
                }


                OverlayStandingsTablePositionLabel position = new(g, columnPosX, rowPosY, backgroundColor, classBackground, FontUtil.FontSegoeMono(_fontSize));
                position.Draw(g, tableData[i].Position.ToString());

                columnPosX += position.Width + _columnGap;
                OverlayStandingsTableTextLabel raceNumber = new(g, columnPosX, rowPosY, 4, FontUtil.FontSegoeMono(_fontSize));
                raceNumber.Draw(g, backgroundColor, (SolidBrush)Brushes.White, Brushes.White, "#" + tableData[i].RaceNumber.ToString(), false);

                columnPosX += raceNumber.Width + _columnGap;
                OverlayStandingsTableTextLabel driverName = new(g, columnPosX, rowPosY, 20, FontUtil.FontSegoeMono(_fontSize));
                driverName.Draw(g, backgroundColor, (SolidBrush)Brushes.Purple, Brushes.White, tableData[i].DriverName, false);

                columnPosX += driverName.Width + _columnGap;
                OverlayStandingsTableTextLabel licenseInfo = new(g, columnPosX, rowPosY, 11, FontUtil.FontSegoeMono(_fontSize));
                licenseInfo.Draw(g, backgroundColor, (SolidBrush)Brushes.Purple, Brushes.White, tableData[i].LicenseInfo, false);

                columnPosX += licenseInfo.Width + _columnGap;
                OverlayStandingsTableTextLabel interval = new(g, columnPosX, rowPosY, 6, FontUtil.FontSegoeMono(_fontSize));
                string intervalString = GetTimeDiff(tableData[i].IntervalMs);
                if (tableData[i].IntervalMs.LaptimeMS < -100)
                {
                    interval.Draw(g, backgroundColor, (SolidBrush)Brushes.DarkGreen, Brushes.White, "-" + intervalString, true);
                }
                else if (tableData[i].IntervalMs.LaptimeMS > 100)
                {
                    interval.Draw(g, backgroundColor, (SolidBrush)Brushes.DarkRed, Brushes.White, "+" + intervalString, true);
                }
                else
                {
                    interval.Draw(g, backgroundColor, (SolidBrush)Brushes.Red, Brushes.White, " " + intervalString, false);
                }

                // TODO: hightling if improved or purple
                columnPosX += interval.Width + _columnGap;
                OverlayStandingsTableTextLabel lastLaptTime = new(g, columnPosX, rowPosY, 9, FontUtil.FontSegoeMono(_fontSize));
                lastLaptTime.Draw(g, backgroundColor, (SolidBrush)Brushes.Purple, Brushes.White, GetLapTime(tableData[i].LastLapTime), false);

                columnPosX += lastLaptTime.Width + _columnGap;
                OverlayStandingsTableTextLabel fastestLaptTime = new(g, columnPosX, rowPosY, 9, FontUtil.FontSegoeMono(_fontSize));
                fastestLaptTime.Draw(g, backgroundColor, (SolidBrush)Brushes.Purple, Brushes.White, GetLapTime(tableData[i].FastestLapTime), false);

                if (tableData[i].AdditionalInfo != "")
                {
                    if (tableData[i].AdditionalInfo == "X")
                    {
                        if (showInvalidLapIndicator) fastestLaptTime.DrawAdditionalInfo(g, Brushes.DarkRed, "");
                    }
                    else
                    {
                        fastestLaptTime.DrawAdditionalInfo(g, Brushes.DarkGreen, tableData[i].AdditionalInfo);
                    }

                }

                rowPosY += position.Height + _rowGap;

                if (i == splitRowIndex - 1) rowPosY += 10;

            }
            Height = rowPosY;
        }
    }

    public class OverlayStandingTableHeaderLabel
    {
        private readonly int _x;
        private readonly int _y;
        private CachedBitmap _cachedBackground;
        private SolidBrush _backgroundBrush;
        public int Height { get; }
        private readonly Font _fontFamily;

        public OverlayStandingTableHeaderLabel(Graphics g, int x, int y, SolidBrush backgroundBrush, Font font)
        {
            _x = x;
            _y = y;
            _fontFamily = font;
            _backgroundBrush = backgroundBrush;
            var fontSize = g.MeasureString(" ", _fontFamily);
            Height = (int)fontSize.Height;

        }

        public void Draw(Graphics g, Brush fontBruch, string text)
        {
            var fontSize = g.MeasureString(text, _fontFamily);

            if (_cachedBackground == null || fontSize.Width > _cachedBackground.Width)
            {
                if (_cachedBackground != null) _cachedBackground.Dispose();
                _cachedBackground = new CachedBitmap((int)(fontSize.Width + 10), (int)fontSize.Height, gr =>
                {
                    var rectanle = new Rectangle(_x, _y, (int)(fontSize.Width + 10), (int)fontSize.Height);
                    g.FillRoundedRectangle(_backgroundBrush, rectanle, 3);
                });
            }

            _cachedBackground.Draw(g, _x, _y);
            var textOffset = 2;
            g.DrawStringWithShadow(text, _fontFamily, fontBruch, new PointF(_x, _y + textOffset));
        }
    }

    public class OverlayStandingsTableTextLabel
    {
        private readonly int _x;
        private readonly int _y;
        private readonly int _height;
        private readonly int _maxStringLength;
        private readonly Font _fontFamily;

        private readonly int _spacing = 10; // possible to set value in contructor

        public int Width { get; }


        public OverlayStandingsTableTextLabel(Graphics g, int x, int y, int maxStringLength, Font font)
        {
            _fontFamily = font;
            _maxStringLength = maxStringLength;
            _x = x;
            _y = y;

            string maxString = new('K', _maxStringLength);
            var fontSize = g.MeasureString(maxString, _fontFamily);
            Width = (int)fontSize.Width + _spacing;
            _height = (int)fontSize.Height;

        }

        public void Draw(Graphics g, SolidBrush backgroundBrush, SolidBrush highlightBrush, Brush fontBruch, string text, bool highlight)
        {
            Rectangle graphRect = new(_x - _spacing / 2, _y, Width, _height);

            if (highlight)
            {
                LinearGradientBrush lgb = new(graphRect, backgroundBrush.Color, highlightBrush.Color, 0f, true);
                lgb.SetBlendTriangularShape(.5f, .6f);
                g.FillRectangle(lgb, graphRect);
                Rectangle hightlightRect = new(_x - _spacing / 2, _y + _height - 4, Width, 4);
                g.FillRoundedRectangle(highlightBrush, hightlightRect, 3);
            }
            else
            {
                g.FillRoundedRectangle(backgroundBrush, graphRect, 3);
            }

            var textOffset = 2;
            g.DrawStringWithShadow(TruncateString(text), _fontFamily, fontBruch, new PointF(_x, _y + textOffset));

        }

        public void DrawAdditionalInfo(Graphics g, Brush brush, string text)
        {
            var fontSize = g.MeasureString(text, _fontFamily);

            var rectanle = new Rectangle(_x + Width, _y, (int)(fontSize.Width + 10), _height);
            using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rectanle, 0, _height / 4, _height / 4, 0);
            g.FillPath(brush, path);
            var textOffset = 2;
            g.DrawString(text, _fontFamily, Brushes.White, new PointF(_x + (Width + 5), _y + textOffset));

        }

        private string TruncateString(string text)
        {
            return text.Length <= _maxStringLength ? text : text.Substring(0, _maxStringLength);
        }
    }


    public class OverlayStandingsTablePositionLabel
    {
        private readonly int _spacing = 10;
        private readonly Font _fontFamily;

        private readonly int _x;
        private readonly int _y;
        public int Width { get; }
        public int Height { get; }
        private readonly int _maxFontWidth;
        private CachedBitmap _cachedBackground;

        private readonly GraphicsPath _path;

        public OverlayStandingsTablePositionLabel(Graphics g, int x, int y, SolidBrush background, SolidBrush highlight, Font font)
        {

            _x = x;
            _y = y;
            _fontFamily = font;

            string maxString = new('K', 2); // max two figures are allowed :-)
            var fontSize = g.MeasureString(maxString, _fontFamily);
            _maxFontWidth = (int)fontSize.Width;
            Width = (int)fontSize.Width + _spacing;
            Height = (int)fontSize.Height;


            var rectanle = new Rectangle(_x, _y, Width - _spacing / 2, Height);
            _path = GraphicsExtensions.CreateRoundedRectangle(rectanle, 0, 0, Height / 3, 0);

            _cachedBackground = new CachedBitmap((int)(fontSize.Width + _spacing), (int)fontSize.Height, gr =>
            {
                LinearGradientBrush lgb = new(new Point() { X = _x - _spacing, Y = _y }, new Point() { X = Width + _spacing, Y = _y }, highlight.Color, background.Color);
                g.FillPath(lgb, _path);

                var highlightRectanle = new Rectangle(_x, _y, 4, Height);
                g.FillRectangle(highlight, highlightRectanle);
            });

        }

        public void Draw(Graphics g, string number)
        {

            _cachedBackground.Draw(g, _x, _y);
            var numberWidth = g.MeasureString(number, _fontFamily).Width;
            var textOffset = 2;
            g.DrawString(number, _fontFamily, Brushes.White, new PointF(_x + _spacing / 2 + _maxFontWidth / 2 - numberWidth / 2, _y + textOffset));

        }
    }
}
