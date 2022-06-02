using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Data;
using ACCManager.Data.ACC.Tracker;
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
using static ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.DebugInfoHelper;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayEntryList
{

    internal class CarData
    {
        public CarInfo CarInfo { get; set; }
        public RealtimeCarUpdate RealtimeCarUpdate { get; set; }


    }

    internal sealed class EntryListOverlay : AbstractOverlay
    {
        private DebugConfig _config = new DebugConfig();

        private Dictionary<int, CarData> _entryListCars = new Dictionary<int, CarData>();
        private readonly InfoTable _table;

        public EntryListOverlay(Rectangle rect) : base(rect, "Debug EntryList Overlay")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 10;

            float fontSize = 10;
            var font = FontUtil.FontUnispace(fontSize);
            _table = new InfoTable(fontSize, new int[] { (int)(font.Size * 5), 500 });

            this.Width = 600;
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

            BroadcastTracker.Instance.OnRealTimeCarUpdate += RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate += EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent += Broadcast_EventHandler;

        }

        public sealed override void BeforeStop()
        {
            if (!this._config.Undock)
            {
                DebugInfoHelper.Instance.RemoveOverlay(this);
                DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
            }

            BroadcastTracker.Instance.OnRealTimeCarUpdate -= RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate -= EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent -= Broadcast_EventHandler;
        }

        private void Broadcast_EventHandler(object sender, BroadcastingEvent broadcastingEvent)
        {
            switch (broadcastingEvent.Type)
            {
                case BroadcastingCarEventType.LapCompleted:
                    {
                        if (broadcastingEvent.CarData == null)
                            break;

                        CarData carData;
                        if (_entryListCars.TryGetValue(broadcastingEvent.CarData.CarIndex, out carData))
                        {
                            carData.CarInfo = broadcastingEvent.CarData;
                        }
                        else
                        {
                            Debug.WriteLine($"BroadcastingCarEventType.LapCompleted car index: {broadcastingEvent.CarData.CarIndex} not found in entry list");
                            carData = new CarData();
                            carData.CarInfo = broadcastingEvent.CarData;
                            _entryListCars.Add(broadcastingEvent.CarData.CarIndex, carData);
                        }
                        break;
                    }


                case BroadcastingCarEventType.Accident:
                    {
                        if (broadcastingEvent.CarData == null)
                            break;

                        Debug.WriteLine($"Car: {broadcastingEvent.CarData.RaceNumber} had an accident");
                        break;
                    }
                default:
                    {
                        if (broadcastingEvent.CarData == null)
                            break;

                        Debug.WriteLine($"{broadcastingEvent.Type} - {broadcastingEvent.CarData}");
                        break;
                    }
            }
        }

        private void EntryListUpdate_EventHandler(object sender, CarInfo carInfo)
        {
            CarData carData;
            if (_entryListCars.TryGetValue(carInfo.CarIndex, out carData))
            {
                carData.CarInfo = carInfo;
            }
            else
            {
                carData = new CarData();
                carData.CarInfo = carInfo;
                _entryListCars.Add(carInfo.CarIndex, carData);
            }

        }

        private void RealTimeCarUpdate_EventHandler(object sender, RealtimeCarUpdate carUpdate)
        {
            CarData carData;
            if (_entryListCars.TryGetValue(carUpdate.CarIndex, out carData))
            {
                carData.RealtimeCarUpdate = carUpdate;
            }
            else
            {
                Debug.WriteLine($"RealTimeCarUpdate_EventHandler car index: {carUpdate.CarIndex} not found in entry list");
                carData = new CarData();
                carData.RealtimeCarUpdate = carUpdate;
                _entryListCars.Add(carUpdate.CarIndex, carData);
            }

        }



        public sealed override void Render(Graphics g)
        {
            if (pageGraphics.SessionType == ACCSharedMemory.AcSessionType.AC_UNKNOWN)
                _entryListCars.Clear();

            List<KeyValuePair<int, CarData>> cars = _entryListCars.ToList();

            cars.Sort((x, y) => { return x.Value.RealtimeCarUpdate.SplinePosition.CompareTo(y.Value.RealtimeCarUpdate.SplinePosition); });
            cars.Reverse();

            foreach (KeyValuePair<int, CarData> kv in cars)
            {
                if (kv.Value.CarInfo != null)
                {
                    string[] firstRow = new string[2] { String.Empty, String.Empty };

                    firstRow[0] = $"{kv.Value.RealtimeCarUpdate.CupPosition}";

                    if (kv.Value.RealtimeCarUpdate.LastLap != null)
                        if (kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.HasValue)
                        {
                            TimeSpan lastLapTime = TimeSpan.FromMilliseconds(kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.Value);
                            firstRow[1] = $"{lastLapTime:mm\\:ss\\.fff}";

                            if (kv.Value.RealtimeCarUpdate.BestSessionLap.LaptimeMS.HasValue)
                            {
                                TimeSpan fastestLapTime = TimeSpan.FromMilliseconds(kv.Value.RealtimeCarUpdate.BestSessionLap.LaptimeMS.Value);
                                firstRow[1] += $" - {fastestLapTime:mm\\:ss\\.fff}";
                            }
                        }
                        else
                            firstRow[1] = $"--:--.---";
                    _table.AddRow($"{kv.Value.CarInfo.RaceNumber} - {kv.Value.CarInfo.GetCurrentDriverName().Trim()}", firstRow, new Color[] { Color.OrangeRed });

                    LapType currentLapType = LapType.ERROR;
                    if (kv.Value.RealtimeCarUpdate.CurrentLap != null)
                    {
                        currentLapType = kv.Value.RealtimeCarUpdate.CurrentLap.Type;
                    }
                    _table.AddRow(String.Empty, new string[] { String.Empty, $"{currentLapType} - {kv.Value.RealtimeCarUpdate.SplinePosition * 100:F2}% - {kv.Value.RealtimeCarUpdate.Kmh} km/h" });
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
