using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.DebugInfoHelper;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayBroadcastRealtime
{

    [Overlay(Name = "Broadcast Track Data", Version = 1.00, OverlayType = OverlayType.Debug,
        Description = "A panel showing live broadcast track data.")]
    internal sealed class BroadcastTrackDataOverlay : AbstractOverlay
    {
        private readonly DebugConfig _config = new DebugConfig();
        private InfoTable _table;

        public BroadcastTrackDataOverlay(Rectangle rectangle) : base(rectangle, "Debug BroadcastTrackData Overlay")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 5;
            this.Width = 300;
            this.Height = 80;

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
            FieldInfo[] members = broadCastTrackData.GetType().GetRuntimeFields().ToArray();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(broadCastTrackData);
                value = ReflectionUtil.FieldTypeValue(member, value);

                if (value != null)
                    _table.AddRow($"{member.Name.Replace("<", "").Replace(">k__BackingField", "")}", new string[] { value.ToString() });
            }

            _table.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
            return true;
        }
    }
}
