using ACCManager.Broadcast.Structs;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayDebugInfo.OverlayBroadcastRealtime
{
    internal sealed class BroadcastTrackDataOverlay : AbstractOverlay
    {
        private Font inputFont = new Font("Arial", 10);

        public BroadcastTrackDataOverlay(Rectangle rectangle) : base(rectangle, "Debug BroadcastTrackData Overlay")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 5;

            this.X = 0;
            this.Y = 0;
            this.Width = 300;
            this.Height = 50;

            DebugInfoHelper.Instance.WidthChanged += (sender, args) =>
            {
                if (args)
                {
                    this.X = DebugInfoHelper.Instance.GetX(this);
                }
            };
        }

        public override void BeforeStart()
        {
            DebugInfoHelper.Instance.AddOverlay(this);
            this.X = DebugInfoHelper.Instance.GetX(this);
        }

        public override void BeforeStop()
        {
            DebugInfoHelper.Instance.RemoveOverlay(this);
        }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            int xMargin = 5;
            int y = 0;
            FieldInfo[] members = broadCastTrackData.GetType().GetRuntimeFields().ToArray();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(broadCastTrackData);
                value = ReflectionUtil.FieldTypeValue(member, value);
                g.DrawString($"{member.Name.Replace("<", "").Replace(">k__BackingField", "")}: {value}", inputFont, Brushes.White, 0 + xMargin, y);
                y += (int)inputFont.Size + 2;
            }
        }

        public override bool ShouldRender()
        {
            return true;
        }
    }
}
