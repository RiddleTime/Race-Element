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
            this.RefreshRateHz = 1;

            float fontSize = 9;
            var font = FontUtil.FontUnispace(fontSize);
            _table = new InfoTable(fontSize, new int[] { (int)(font.Size * 4), 500 });

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
                case BroadcastingCarEventType.None: break;
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

                default: break;
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
            foreach (KeyValuePair<int, CarData> kv in _entryListCars)
            {
                if (kv.Value.CarInfo != null)
                {
                    _table.AddRow(kv.Value.CarInfo.GetCurrentDriverName().Trim(), new string[] { $"{kv.Value.CarInfo.RaceNumber}", $"{ConversionFactory.GetCarName(kv.Value.CarInfo.CarModelType)}" });

                    if (kv.Value.RealtimeCarUpdate.LastLap != null)
                    {
                        if (kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.HasValue)
                        {
                            TimeSpan lastLapTime = TimeSpan.FromMilliseconds(kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.Value);

                            _table.AddRow(String.Empty, new string[] { "", $"Last lap: {lastLapTime:mm\\:ss\\.fff}" });
                        }
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
