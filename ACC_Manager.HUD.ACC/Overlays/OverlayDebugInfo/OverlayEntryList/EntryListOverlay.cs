using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Data;
using ACCManager.Data.ACC.Tracker;
using ACCManager.HUD.Overlay.Internal;
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
        private Font _inputFont = FontUtil.FontUnispace((float)9);

        Dictionary<int, CarData> EntryListCars = new Dictionary<int, CarData>();

        public EntryListOverlay(Rectangle rect) : base(rect, "Debug EntryList Overlay")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 1;
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
            if (broadcastingEvent.Type.Equals(BroadcastingCarEventType.LapCompleted))
            {
                CarData carData;
                if (EntryListCars.TryGetValue(broadcastingEvent.CarData.CarIndex, out carData))
                {
                    carData.CarInfo = broadcastingEvent.CarData;
                }
                else
                {
                    Debug.WriteLine($"BroadcastingCarEventType.LapCompleted car index: {broadcastingEvent.CarData.CarIndex} not found in entry list");
                    carData = new CarData();
                    carData.CarInfo = broadcastingEvent.CarData;
                    EntryListCars.Add(broadcastingEvent.CarData.CarIndex, carData);

                }
            }
        }

        private void EntryListUpdate_EventHandler(object sender, CarInfo carInfo)
        {
            CarData carData;
            if (EntryListCars.TryGetValue(carInfo.CarIndex, out carData))
            {
                carData.CarInfo = carInfo;
            }
            else
            {
                carData = new CarData();
                carData.CarInfo = carInfo;
                EntryListCars.Add(carInfo.CarIndex, carData);
            }

        }

        private void RealTimeCarUpdate_EventHandler(object sender, RealtimeCarUpdate carUpdate)
        {
            CarData carData;
            if (EntryListCars.TryGetValue(carUpdate.CarIndex, out carData))
            {
                carData.RealtimeCarUpdate = carUpdate;
            }
            else
            {
                Debug.WriteLine($"RealTimeCarUpdate_EventHandler car index: {carUpdate.CarIndex} not found in entry list");
                carData = new CarData();
                carData.RealtimeCarUpdate = carUpdate;
                EntryListCars.Add(carUpdate.CarIndex, carData);
            }

        }



        public sealed override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));
            int xMargin = 5;
            int y = 0;

            g.DrawString($"entry list size: {EntryListCars.Count}", _inputFont, Brushes.White, 0 + xMargin, y);
            y += (int)_inputFont.Size + 4;

            foreach (KeyValuePair<int, CarData> kv in EntryListCars)
            {
                if (kv.Value.CarInfo != null)
                {
                    g.DrawString($"> {kv.Value.CarInfo.CarIndex} - {kv.Value.CarInfo.RaceNumber} - {ConversionFactory.GetCarName(kv.Value.CarInfo.CarModelType)} - {kv.Value.CarInfo.GetCurrentDriverName()}", _inputFont, Brushes.White, 0 + xMargin, y);
                }

                y += (int)_inputFont.Size + 4;
                if (kv.Value.RealtimeCarUpdate.LastLap != null)
                {
                    string LaptimeString = $"{TimeSpan.FromMilliseconds(kv.Value.RealtimeCarUpdate.LastLap.LaptimeMS.Value):mm\\:ss\\.fff}";
                    g.DrawString($"  last lap time: {LaptimeString}", _inputFont, Brushes.White, 0 + xMargin, y);
                }

                y += (int)_inputFont.Size + 4;
            }

        }

        public sealed override bool ShouldRender()
        {
            return true;
        }

    }
}
