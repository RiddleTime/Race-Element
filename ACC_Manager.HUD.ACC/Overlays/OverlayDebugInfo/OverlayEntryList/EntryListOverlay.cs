using ACC_Manager.Util.SystemExtensions;
using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Data;
using ACCManager.Data.ACC.EntryList;
using ACCManager.Data.ACC.EntryList.TrackPositionGraph;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static ACCManager.Data.ACC.EntryList.EntryListTracker;
using static ACCManager.Data.SetupConverter;
using static ACCManager.HUD.Overlay.OverlayUtil.InfoTable;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayEntryList
{
#if DEBUG
    [Overlay(Name = "Entrylist Overlay", Version = 1.00, OverlayType = OverlayType.Debug,
Description = "A panel showing live broadcast track data.")]
#endif
    internal sealed class EntryListOverlay : AbstractOverlay
    {
        private readonly EntryListDebugConfig _config = new EntryListDebugConfig();
        private class EntryListDebugConfig : OverlayConfiguration
        {
            [ConfigGrouping("EntryList", "Provides settings for overlay docking.")]
            public EntryListGrouping Entrylist { get; set; } = new EntryListGrouping();
            public class EntryListGrouping
            {
                [ToolTip("Show extended data, adds a new row for each car.")]
                public bool ExtendedData { get; set; } = false;
            }

            [ConfigGrouping("Dock", "Provides settings for overlay docking.")]
            public DockConfigGrouping Dock { get; set; } = new DockConfigGrouping();
            public class DockConfigGrouping
            {
                [ToolTip("Allows you to reposition this debug panel.")]
                public bool Undock { get; set; } = false;
            }

            public EntryListDebugConfig()
            {
                AllowRescale = true;
            }
        }

        private readonly InfoTable _table;

        private readonly Color Gt3Color = Color.FromArgb(255, Color.Black);
        private readonly Color Gt4Color = Color.FromArgb(255, 24, 24, 72);
        private readonly Color CupColor = Color.FromArgb(255, 30, 61, 26);
        private readonly Color TcxColor = Color.FromArgb(255, 0, 96, 136);
        private readonly Color StColor = Color.FromArgb(255, 0, 96, 136);
        private readonly Color ChlColor = Color.FromArgb(255, 112, 110, 0);



        public EntryListOverlay(Rectangle rect) : base(rect, "Entrylist Overlay")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 10;

            float fontSize = 9;
            var font = FontUtil.FontUnispace(fontSize);
            _table = new InfoTable(fontSize, new int[] { (int)(font.Size * 13), (int)(font.Size * 13), (int)(font.Size * 8), (int)(font.Size * 8) });

            this.Width = 415;
            this.Height = 800;
        }

        private void Instance_WidthChanged(object sender, bool e)
        {
            if (e)
                this.X = DebugInfoHelper.Instance.GetX(this);
        }

        public sealed override void BeforeStart()
        {
            if (this._config.Dock.Undock)
                this.AllowReposition = true;
            else
            {
                DebugInfoHelper.Instance.WidthChanged += Instance_WidthChanged;
                DebugInfoHelper.Instance.AddOverlay(this);
                this.X = DebugInfoHelper.Instance.GetX(this);
                this.Y = 0;
            }
        }

        public sealed override void BeforeStop()
        {
            if (!this._config.Dock.Undock)
            {
                DebugInfoHelper.Instance.RemoveOverlay(this);
                DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
            }

        }

        public sealed override void Render(Graphics g)
        {
            List<KeyValuePair<int, CarData>> cars = EntryListTracker.Instance.Cars;

            if (cars.Count == 0)
                return;

            SortEntryList(cars);

            CarData firstCar = GetFirstPositionCar();
            Car carAhead = null;

            if (firstCar.CarInfo != null)
                carAhead = PositionGraph.Instance.GetCar(firstCar.CarInfo.CarIndex);


            foreach (KeyValuePair<int, CarData> kv in cars)
            {
                if (kv.Value.CarInfo != null)
                {
                    AddCarFirstRow(kv);

                    if (_config.Entrylist.ExtendedData)
                    {
                        string speed = $"{kv.Value.RealtimeCarUpdate.Kmh} km/h".FillStart(8, ' ');

                        string distanceText = string.Empty;

                        if (carAhead != null)
                        {
                            Car carCar = PositionGraph.Instance.GetCar(kv.Value.CarInfo.CarIndex);
                            if (carCar != null && carCar != carAhead)
                            {

                                float carAheadDistance = 0;
                                if (carAhead != null) carAheadDistance = carAhead.LapIndex * broadCastTrackData.TrackMeters + broadCastTrackData.TrackMeters * carAhead.SplinePosition;
                                float carDistance = carCar.LapIndex * broadCastTrackData.TrackMeters + carCar.SplinePosition * broadCastTrackData.TrackMeters;

                                if (carAheadDistance - carDistance < broadCastTrackData.TrackMeters)
                                {
                                    distanceText = $"+{carAheadDistance - carDistance:F0}".FillStart(4, ' ') + "m";
                                }
                                else
                                {
                                    distanceText = $"+{carAhead.LapIndex - carCar.LapIndex} laps";
                                }
                            }

                            carAhead = carCar;
                        }

                        _table.AddRow(String.Empty, new string[] { String.Empty, $"{distanceText}", speed });
                    }
                }
            }

            _table._headerWidthSet = false;

            _table.Draw(g);
        }

        private void AddCarFirstRow(KeyValuePair<int, CarData> kv)
        {
            string[] firstRow = new string[] { String.Empty, String.Empty, String.Empty, String.Empty };
            Color[] firstRowColors = new Color[] { Color.White, Color.White, Color.White, Color.White };

            DriverInfo currentDriver = kv.Value.CarInfo.Drivers[kv.Value.CarInfo.CurrentDriverIndex];
            string firstName = currentDriver.FirstName;
            if (firstName.Length > 0) firstName = firstName.First() + "";
            firstRow[0] = $"{firstName}. {currentDriver.LastName}";

            firstRow[1] = $"{kv.Value.CarInfo.RaceNumber}";

            int bestSessionLapMS = -1;
            if (broadCastRealTime.BestSessionLap != null)
                bestSessionLapMS = broadCastRealTime.BestSessionLap.LaptimeMS.GetValueOrDefault(-1);
            switch (broadCastRealTime.SessionType)
            {
                case RaceSessionType.Race:
                    {
                        if (kv.Value.RealtimeCarUpdate.LastLap != null)
                            if (kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.HasValue)
                            {
                                if (broadCastRealTime.BestSessionLap != null)
                                    if (kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS == bestSessionLapMS)
                                        firstRowColors[2] = Color.FromArgb(255, 207, 97, 255);

                                TimeSpan fastestLapTime = TimeSpan.FromMilliseconds(kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.Value);
                                firstRow[2] = $"{fastestLapTime:mm\\:ss\\.fff}";
                            }
                            else
                                firstRow[2] = $"--:--.---";
                        break;
                    }

                case RaceSessionType.Practice:
                case RaceSessionType.Qualifying:
                    {
                        if (kv.Value.RealtimeCarUpdate.BestSessionLap != null)
                            if (kv.Value.RealtimeCarUpdate.BestSessionLap.LaptimeMS.HasValue)
                            {
                                if (broadCastRealTime.BestSessionLap != null)
                                    if (kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS == bestSessionLapMS)
                                        firstRowColors[2] = Color.FromArgb(255, 207, 97, 255);

                                TimeSpan fastestLapTime = TimeSpan.FromMilliseconds(kv.Value.RealtimeCarUpdate.BestSessionLap.LaptimeMS.Value);
                                firstRow[2] = $"{fastestLapTime:mm\\:ss\\.fff}";
                            }
                            else
                                firstRow[2] = $"--:--.---";
                        break;
                    }
                default: break;
            }

            switch (kv.Value.RealtimeCarUpdate.CarLocation)
            {
                case CarLocationEnum.PitEntry:
                    {
                        firstRow[3] += " (PI)";
                        break;
                    }
                case CarLocationEnum.PitExit:
                    {
                        firstRow[3] += " (PE)";
                        break;
                    }
                case CarLocationEnum.Pitlane:
                    {
                        firstRow[3] += " (P)";
                        break;
                    }

                case CarLocationEnum.Track:
                    {
                        firstRow[3] = $"{kv.Value.RealtimeCarUpdate.Delta / 1000f:F2}".FillStart(6, ' ');
                        firstRowColors[3] = kv.Value.RealtimeCarUpdate.Delta > 0 ? Color.OrangeRed : Color.LimeGreen;
                        break;
                    }
                default: break;
            }

            var carModel = ConversionFactory.GetCarModels(kv.Value.CarInfo.CarModelType);
            var carClass = ConversionFactory.GetConversion(carModel).CarClass;

            Color headerBackgroundColor = Color.FromArgb(90, Color.Black);
            switch (carClass)
            {
                case CarClasses.GT3:
                    headerBackgroundColor = Gt3Color;
                    break;
                case CarClasses.GT4:
                    headerBackgroundColor = Gt4Color;
                    break;
                case CarClasses.CUP:
                    headerBackgroundColor = CupColor;
                    break;
                case CarClasses.ST:
                    headerBackgroundColor = StColor;
                    break;
                case CarClasses.TCX:
                    headerBackgroundColor = TcxColor;
                    break;
                case CarClasses.CHL:
                    headerBackgroundColor = ChlColor;
                    break;
                default:
                    break;

            }

            string cupPosition = $"{kv.Value.RealtimeCarUpdate.CupPosition}";
            TableRow row = new TableRow()
            {
                Header = $"  {cupPosition}",
                Columns = firstRow,
                ColumnColors = firstRowColors,
                HeaderBackground = headerBackgroundColor
            };

            if (kv.Key == pageGraphics.PlayerCarID)
                row.HeaderBackground = Color.FromArgb(120, Color.Red);
            else
                if (kv.Key == broadCastRealTime.FocusedCarIndex) row.HeaderBackground = Color.FromArgb(90, Color.Red);

            _table.AddRow(row);
        }

        private void SortEntryList(List<KeyValuePair<int, CarData>> cars)
        {

            switch (broadCastRealTime.SessionType)
            {
                case RaceSessionType.Practice:
                case RaceSessionType.Race:
                    {
                        switch (broadCastRealTime.Phase)
                        {
                            case SessionPhase.SessionOver:
                            case SessionPhase.PreSession:
                            case SessionPhase.PreFormation:
                                {
                                    cars.Sort((a, b) =>
                                    {
                                        return a.Value.RealtimeCarUpdate.CupPosition.CompareTo(b.Value.RealtimeCarUpdate.CupPosition);
                                    });
                                    break;
                                }
                            default:
                                {
                                    cars.Sort((a, b) =>
                                    {
                                        if (a.Value.CarInfo == null)
                                            return -1;

                                        if (b.Value.CarInfo == null)
                                            return 1;

                                        Car carCarA = PositionGraph.Instance.GetCar(a.Value.CarInfo.CarIndex);
                                        Car carCarb = PositionGraph.Instance.GetCar(b.Value.CarInfo.CarIndex);

                                        if (carCarA == null) return -1;
                                        if (carCarb == null) return 1;

                                        var aSpline = carCarA.SplinePosition;
                                        var bSpline = carCarb.SplinePosition;

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


                case RaceSessionType.Qualifying:
                    {
                        cars.Sort((a, b) =>
                        {
                            return a.Value.RealtimeCarUpdate.CupPosition.CompareTo(b.Value.RealtimeCarUpdate.CupPosition);
                        });
                        break;
                    }

                default: break;
            }
        }

        public CarData GetFirstPositionCar()
        {
            List<KeyValuePair<int, CarData>> cars = EntryListTracker.Instance.Cars;
            cars.Sort((a, b) =>
            {
                if (a.Value.CarInfo == null)
                    return -1;

                if (b.Value.CarInfo == null)
                    return -1;

                var aSpline = PositionGraph.Instance.GetCar(a.Value.CarInfo.CarIndex)?.SplinePosition;
                var bSpline = PositionGraph.Instance.GetCar(b.Value.CarInfo.CarIndex)?.SplinePosition;

                var aLaps = PositionGraph.Instance.GetCar(a.Value.CarInfo.CarIndex)?.LapIndex;
                var bLaps = PositionGraph.Instance.GetCar(b.Value.CarInfo.CarIndex)?.LapIndex;

                float aPosition = aLaps.GetValueOrDefault(0) * 10 + aSpline.GetValueOrDefault(0);
                float bPosition = bLaps.GetValueOrDefault(0) * 10 + bSpline.GetValueOrDefault(0);
                return aPosition.CompareTo(bPosition);
            });
            cars.Reverse();


            return cars.First().Value;


        }

        public sealed override bool ShouldRender()
        {
            return true;
        }

    }
}
