using ACC_Manager.Util.SystemExtensions;
using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Data;
using ACCManager.Data.ACC.EntryList;
using ACCManager.Data.ACC.Tracker;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using ACCManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.ACC.EntryList.EntryListTracker;
using static ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.DebugInfoHelper;
using static ACCManager.HUD.Overlay.OverlayUtil.InfoTable;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayEntryList
{
    internal sealed class EntryListOverlay : AbstractOverlay
    {
        private readonly EntryListDebugConfig _config = new EntryListDebugConfig();
        private class EntryListDebugConfig : OverlayConfiguration
        {
            internal bool ShowExtendedData { get; set; } = false;

            [ToolTip("Allows you to reposition this debug panel.")]
            internal bool Undock { get; set; } = false;

            public EntryListDebugConfig()
            {
                AllowRescale = true;
            }
        }

        private readonly InfoTable _table;

        public EntryListOverlay(Rectangle rect) : base(rect, "Debug EntryList Overlay")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 5;

            float fontSize = 9;
            var font = FontUtil.FontUnispace(fontSize);
            _table = new InfoTable(fontSize, new int[] { (int)(font.Size * 5), (int)(font.Size * 13), (int)(font.Size * 8) });

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
            if (this._config.Undock)
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
            if (!this._config.Undock)
            {
                DebugInfoHelper.Instance.RemoveOverlay(this);
                DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
            }

        }

        public sealed override void Render(Graphics g)
        {
            List<KeyValuePair<int, CarData>> cars = EntryListTracker.Instance.Cars;

            SortEntryList(cars);

            foreach (KeyValuePair<int, CarData> kv in cars)
            {
                if (kv.Value.CarInfo != null)
                {
                    AddCarFirstRow(kv);

                    if (this._config.ShowExtendedData)
                    {
                        string speed = $"{kv.Value.RealtimeCarUpdate.Kmh} km/h".FillStart(8, ' ');
                        _table.AddRow(String.Empty, new string[] { String.Empty, $"Lap {kv.Value.RealtimeCarUpdate.Laps}` {kv.Value.RealtimeCarUpdate.SplinePosition:F3}", speed });
                    }
                }
            }

            _table._headerWidthSet = false;

            _table.Draw(g);
        }

        private void AddCarFirstRow(KeyValuePair<int, CarData> kv)
        {
            string[] firstRow = new string[] { String.Empty, String.Empty, String.Empty };
            Color[] firstRowColors = new Color[] { Color.White, Color.White, Color.White };
            firstRow[0] = $"{kv.Value.CarInfo.RaceNumber}";



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
                                        firstRowColors[1] = Color.FromArgb(255, 207, 97, 255);

                                TimeSpan fastestLapTime = TimeSpan.FromMilliseconds(kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.Value);
                                firstRow[1] = $"{fastestLapTime:mm\\:ss\\.fff}";
                            }
                            else
                                firstRow[1] = $"--:--.---";
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
                                        firstRowColors[1] = Color.FromArgb(255, 207, 97, 255);

                                TimeSpan fastestLapTime = TimeSpan.FromMilliseconds(kv.Value.RealtimeCarUpdate.BestSessionLap.LaptimeMS.Value);
                                firstRow[1] = $"{fastestLapTime:mm\\:ss\\.fff}";
                            }
                            else
                                firstRow[1] = $"--:--.---";
                        break;
                    }
                default: break;
            }

            switch (kv.Value.RealtimeCarUpdate.CarLocation)
            {
                case CarLocationEnum.PitEntry:
                    {
                        firstRow[1] += " (PI)";
                        break;
                    }
                case CarLocationEnum.PitExit:
                    {
                        firstRow[1] += " (PE)";
                        break;
                    }
                case CarLocationEnum.Pitlane:
                    {
                        firstRow[1] += " (P)";
                        break;
                    }

                case CarLocationEnum.Track:
                    {
                        firstRow[2] = $"{kv.Value.RealtimeCarUpdate.Delta / 1000f:F2}".FillStart(6, ' ');
                        firstRowColors[2] = kv.Value.RealtimeCarUpdate.Delta > 0 ? Color.OrangeRed : Color.LimeGreen;
                        break;
                    }
                default: break;
            }

            string raceNumber = $"{kv.Value.CarInfo.RaceNumber}".FillEnd(3, ' ');
            string firstName = kv.Value.CarInfo.Drivers[kv.Value.CarInfo.CurrentDriverIndex].FirstName;
            if (firstName.Length > 0) firstName = firstName.First() + ".";
            string cupPosition = $"{kv.Value.RealtimeCarUpdate.CupPosition}".FillStart(2, ' ');
            TableRow row = new TableRow()
            {
                Header = $"{cupPosition} {firstName} {kv.Value.CarInfo.GetCurrentDriverName().Trim()}",
                Columns = firstRow,
                ColumnColors = firstRowColors,
                HeaderBackground = Color.FromArgb(70, Color.Black)
            };

            if (kv.Key == pageGraphics.PlayerCarID) row.HeaderBackground = Color.FromArgb(120, Color.Red);
            else
                if (kv.Key == broadCastRealTime.FocusedCarIndex) row.HeaderBackground = Color.FromArgb(70, Color.Red);

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
                                        var aSpline = a.Value.RealtimeCarUpdate.SplinePosition;
                                        var bSpline = b.Value.RealtimeCarUpdate.SplinePosition;
                                        float aPosition = a.Value.RealtimeCarUpdate.Laps + aSpline / 10;
                                        float bPosition = b.Value.RealtimeCarUpdate.Laps + bSpline / 10;
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

        public sealed override bool ShouldRender()
        {
            return true;
        }

    }
}
