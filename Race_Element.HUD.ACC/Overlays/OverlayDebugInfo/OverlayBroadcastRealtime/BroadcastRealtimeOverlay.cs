using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.EntryList.TrackPositionGraph;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using static RaceElement.Data.ACC.EntryList.EntryListTracker;
using static RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.DebugInfoHelper;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayBroadcastRealtime
{
    [Overlay(Name = "Broadcast Realtime", Version = 1.00, OverlayType = OverlayType.Debug,
        Description = "A panel showing live broadcast realtime data.")]
    internal sealed class BroadcastRealtimeOverlay : AbstractOverlay
    {
        private DebugConfig _config = new DebugConfig();

        private InfoTable _table;

        public BroadcastRealtimeOverlay(Rectangle rectangle) : base(rectangle, "Broadcast Realtime")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 5;
            this.Width = 370;
            this.Height = 280;

            _table = new InfoTable(9, new int[] { 200 });
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
            foreach (KeyValuePair<int, CarData> carData in EntryListTracker.Instance.Cars)
            {
                if (carData.Key == broadCastRealTime.FocusedCarIndex)
                {
                    if (carData.Value.CarInfo == null)
                        continue;

                    string driverName = carData.Value.CarInfo.GetCurrentDriverName();

                    string firstName = carData.Value.CarInfo.Drivers[carData.Value.CarInfo.CurrentDriverIndex].FirstName;
                    _table.AddRow("Name", new string[] { $"{firstName} {driverName}" });

                    FieldInfo[] members = carData.Value.RealtimeCarUpdate.GetType().GetRuntimeFields().ToArray();
                    foreach (FieldInfo member in members)
                    {
                        var value = member.GetValue(carData.Value.RealtimeCarUpdate);
                        value = ReflectionUtil.FieldTypeValue(member, value);

                        if (value != null)
                            _table.AddRow($"{member.Name.Replace("<", "").Replace(">k__BackingField", "")}", new string[] { value.ToString() });
                    }

                    _table.Draw(g);
                }
            }
        }

        public sealed override bool ShouldRender() => true;
    }
}
