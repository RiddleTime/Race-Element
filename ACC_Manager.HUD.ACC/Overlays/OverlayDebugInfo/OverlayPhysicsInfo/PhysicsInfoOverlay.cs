using ACCManager.HUD.ACC.Overlays.OverlayDebugInfo;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
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

namespace ACCManager.HUD.ACC.Overlays.OverlayPhysicsInfo
{

    [Overlay(Name = "Physics Info", Version = 1.00,
        Description = "Shared Memory Physics Page", OverlayType = OverlayType.Debug)]
    internal sealed class PhysicsInfoOverlay : AbstractOverlay
    {
        private readonly DebugConfig _config = new DebugConfig();
        private readonly InfoTable _table;

        public PhysicsInfoOverlay(Rectangle rectangle) : base(rectangle, "Debug Physics Overlay")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 5;
            this.Width = 600;
            this.Height = 700;

            _table = new InfoTable(9, new int[] { 450 });
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
            FieldInfo[] members = pagePhysics.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pagePhysics);
                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }

                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = ReflectionUtil.FieldTypeValue(member, value);
                    _table.AddRow($"{member.Name}", new string[] { value.ToString() });
                }
            }

            _table.Draw(g);
        }

        public sealed override bool ShouldRender()
        {
            return true;
        }
    }
}
