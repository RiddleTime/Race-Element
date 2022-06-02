using ACCManager.Broadcast.Structs;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.DebugInfoHelper;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayBroadcastRealtime
{
    internal sealed class BroadcastCarUpdateOverlay : AbstractOverlay
    {
        private DebugConfig _config = new DebugConfig();
        private Font _inputFont = FontUtil.FontUnispace((float)9);

        public BroadcastCarUpdateOverlay(Rectangle rectangle) : base(rectangle, "Debug BroadcastCarUpdate Overlay")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 5;
            this.Width = 300;
            this.Height = 300;
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
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            int xMargin = 5;
            int y = 0;
            FieldInfo[] members = broadCastLocalCar.GetType().GetRuntimeFields().ToArray();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(broadCastLocalCar);
                value = ReflectionUtil.FieldTypeValue(member, value);
                g.DrawString($"{member.Name.Replace("<", "").Replace(">k__BackingField", "")}: {value}", _inputFont, Brushes.White, 0 + xMargin, y);
                y += (int)_inputFont.Size + 4;
            }
        }

        public sealed override bool ShouldRender()
        {
            return true;
        }
    }
}
