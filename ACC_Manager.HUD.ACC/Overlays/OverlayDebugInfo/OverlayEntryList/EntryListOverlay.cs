using ACC_Manager.Util.NumberExtensions;
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

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayEntryList
{
    internal sealed class EntryListOverlay : AbstractOverlay
    {
        private EntryListDebugConfig _config = new EntryListDebugConfig();
        private class EntryListDebugConfig : OverlayConfiguration
        {
            internal bool ShowExtendedData { get; set; } = false;

            [ToolTip("Allows you to reposition this debug panel.")]
            internal bool Undock { get; set; } = false;
        }

        private readonly InfoTable _table;

        public EntryListOverlay(Rectangle rect) : base(rect, "Debug EntryList Overlay")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 10;

            float fontSize = 10;
            var font = FontUtil.FontUnispace(fontSize);
            _table = new InfoTable(fontSize, new int[] { (int)(font.Size * 5), 300 });

            this.Width = 500;
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

            switch (broadCastRealTime.SessionType)
            {
                case RaceSessionType.Race:
                    {
                        if (broadCastRealTime.Phase == SessionPhase.PreSession || broadCastRealTime.Phase == SessionPhase.PreFormation)
                            cars.Sort((a, b) =>
                            {
                                return a.Value.RealtimeCarUpdate.CupPosition.CompareTo(b.Value.RealtimeCarUpdate.CupPosition);
                            });
                        else
                        {
                            cars.Sort((a, b) =>
                            {
                                float aPosition = a.Value.RealtimeCarUpdate.Laps + a.Value.RealtimeCarUpdate.SplinePosition / 10;
                                float bPosition = b.Value.RealtimeCarUpdate.Laps + b.Value.RealtimeCarUpdate.SplinePosition / 10;
                                return aPosition.CompareTo(bPosition);
                            });
                            cars.Reverse();
                        }
                        break;
                    }

                case RaceSessionType.Practice:
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

            foreach (KeyValuePair<int, CarData> kv in cars)
            {
                if (kv.Value.CarInfo != null)
                {
                    string[] firstRow = new string[2] { String.Empty, String.Empty };
                    firstRow[0] = $"{kv.Value.RealtimeCarUpdate.CupPosition}";

                    switch (broadCastRealTime.SessionType)
                    {
                        case RaceSessionType.Race:
                            {
                                if (kv.Value.RealtimeCarUpdate.LastLap != null)
                                    if (kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.HasValue)
                                    {
                                        TimeSpan fastestLapTime = TimeSpan.FromMilliseconds(kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.Value);
                                        firstRow[1] = $"{fastestLapTime:mm\\:ss\\.fff}";
                                    }
                                    else
                                        firstRow[1] = $"--:--.---";
                                break;
                            }

                        case RaceSessionType.Qualifying:
                            {
                                if (kv.Value.RealtimeCarUpdate.BestSessionLap != null)
                                    if (kv.Value.RealtimeCarUpdate.BestSessionLap.LaptimeMS.HasValue)
                                    {
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

                        default: break;
                    }

                    string raceNumber = $"{kv.Value.CarInfo.RaceNumber}".FillEnd(3, ' ');
                    _table.AddRow($"{raceNumber} {kv.Value.CarInfo.GetCurrentDriverName().Trim()}", firstRow, new Color[] { Color.OrangeRed });


                    if (this._config.ShowExtendedData)
                    {
                        LapType currentLapType = LapType.ERROR;
                        if (kv.Value.RealtimeCarUpdate.CurrentLap != null)
                            currentLapType = kv.Value.RealtimeCarUpdate.CurrentLap.Type;
                        _table.AddRow(String.Empty, new string[] { String.Empty, $"Lap {kv.Value.RealtimeCarUpdate.Laps} @ {kv.Value.RealtimeCarUpdate.SplinePosition * 100:F0}% - {kv.Value.RealtimeCarUpdate.Kmh} km/h" });
                    }
                }
            }

            _table._headerWidthSet = false;

            _table.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
            return true;
        }

    }
}
