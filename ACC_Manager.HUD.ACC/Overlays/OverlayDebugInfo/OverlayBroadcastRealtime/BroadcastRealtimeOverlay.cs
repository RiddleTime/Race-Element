using ACCManager.Data.ACC.EntryList;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.Util;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using static ACCManager.Data.ACC.EntryList.EntryListTracker;
using static ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.DebugInfoHelper;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayBroadcastRealtime
{
#if DEBUG
    [Overlay(Name = "Broadcast Realtime", Version = 1.00, OverlayType = OverlayType.Debug,
        Description = "A panel showing live broadcast realtime data.")]
#endif
    internal sealed class BroadcastRealtimeOverlay : AbstractOverlay
    {
        private DebugConfig _config = new DebugConfig();

        private InfoTable _table;

        public BroadcastRealtimeOverlay(Rectangle rectangle) : base(rectangle, "Broadcast Realtime")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 10;
            this.Width = 300;
            this.Height = 300;

            _table = new InfoTable(9, new int[] { 200 });
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
            foreach (KeyValuePair<int, CarData> carData in EntryListTracker.Instance.Cars)
            {
                if (carData.Key == broadCastRealTime.FocusedCarIndex)
                {
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

        public sealed override bool ShouldRender()
        {
            return true;
        }
    }
}
